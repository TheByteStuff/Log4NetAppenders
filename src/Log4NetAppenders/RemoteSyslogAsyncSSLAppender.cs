using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Util;
using log4net.Repository.Hierarchy;

using System.IO;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace TheByteStuff.log4net.Appenders
{
    public class RemoteSyslogAsyncSSLAppender : AppenderSkeleton, IDisposable
    {
        #region Public Instance Constructors

        /// <summary>
        /// This appender was built and tested to deliver Syslog messages to a Synology Log Center in BSD (legacy Syslog) format over SSL.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This appender was built and tested to deliver Syslog messages to a Synology Log Center in BSD (legacy Syslog) format over SSL.
        /// </para>
        /// <para>
        /// https://syslog-ng.com/documents/html/syslog-ng-ose-latest-guides/en/syslog-ng-ose-guide-admin/html/concepts-message-bsdsyslog.html
        /// 
        /// https://tools.ietf.org/search/rfc3164
        /// </para>
        /// <para>
        /// Various Log4net Appender implementations were consulted/copied while developing this Appender including
        /// UDPAppender (base apache log4net source code)
        /// RemoteSyslogAppender (base apache log4net source code)
        /// RemoteSyslog5424Appender (https://github.com/cityindex/log4net.Appenders.Contrib)
        /// </para>
        /// <para>
        /// To view the logging results, a custom application can be developed that listens for logging 
        /// events.
        /// </para>
        /// </remarks>
        /// <example>
        /// <para>SysLogFacility may have the following values:
        /// Kernel, User, Mail, Daemons, Authorization, Syslog, Printer, News, 
        /// Uucp, Clock, Authorization2, Ftp, Ntp, Audit, Alert, Clock2, 
        /// Local0, Local1, Local2, Local3, Local4, Local5, Local6, Local7
        /// </para>
        /// </remarks>
        /// <example>
        /// <para>
        /// An example configuration section to log information using this appender to the 
        /// IP 127.0.0.1 on port 6514 using cert C:\ApplicationName\Certs\localhost.cert:
        /// </para>
        /// <code lang="XML" escaped="true">
        /// <appender name="RemoteSyslogSSLAppender" type="TheByteStuff.log4net.Appenders.RemoteSyslogAsyncSSLAppender,Log4NetAppenders">
        ///    <threshold value = "ALL" />
        ///    <AppName value="ApplicationName"/>
        ///    <RemoteHost value = "127.0.0.1" />
        ///    <RemotePort value="6514"/>
        ///    <CertificatePath value = "C:\ApplicationName\Certs\localhost.cert" />
        ///    <SysLogFacility value="Local0"/>
        ///    <EnableDiagnosticInfo value = "true" />
        ///    <EnableRemoteDiagnosticInfo value="true"/>
        ///    <layout type = "log4net.Layout.PatternLayout" >
        ///       <conversionPattern value="%.255message"/>
        ///    </layout>
        /// </appender>
        /// </code>
        /// </example>
        /// <author>The Byte Stuff</author>
        public RemoteSyslogAsyncSSLAppender()
        {
            FormattedHostName = FormatHostName();

            _senderThread = new Thread(SenderThreadEntry)
            {
                Name = "RemoteSyslogAsyncSSLSenderThread",
                IsBackground = true,
            };
        }

        string Domain = "test"; // System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
        //string test = System.Environment. .UserDomainName;
        string hostName = Dns.GetHostName();
        string FormattedHostName = String.Empty;


        public RemoteSyslogAsyncSSLAppender(string server, int port, string certificatePath)
            : this()
        {
            RemoteHost = server;
            RemotePort = port;
            CertificatePath = certificatePath;
        }

        private string FormatHostName()
        {
            //TODO - allow HostName to be configured from Log Definition?
            string LocalIp = string.Empty;
            //System.Net.IPHostEntry host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            Task<IPHostEntry> GetHostTask = System.Net.Dns.GetHostEntryAsync(System.Net.Dns.GetHostName());
            Task.WaitAll(GetHostTask);
            System.Net.IPHostEntry host = GetHostTask.Result;

            foreach (System.Net.IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    LocalIp = ip.ToString();
                    break;
                }
            }
            if (LocalIp.Length > 0)
            {
                LocalIp = "(" + LocalIp + ")";
            }

            string localDomain = "";
            if (this.Domain.Trim().Length > 0)
            {
                localDomain = "/" + Domain;
            }

            return hostName + localDomain + LocalIp;
        }


#endregion Public Instance Constructors

        private Socket _socket;
        private SslStream _stream;
        private TextWriter _writer;
        private volatile bool _disposed;
        private volatile bool _closing;

        private readonly Thread _senderThread;
        private TimeSpan _sendingPeriod;
        private readonly TimeSpan _defaultSendingPeriod = TimeSpan.FromSeconds(5);
        private readonly TimeSpan _maxSendingPeriod = TimeSpan.FromMinutes(10);

        private readonly int TaskWaitTime = 2000;
        private readonly Queue<string> _messageQueue = new Queue<string>();
        private readonly object _connectionSync = new object();
        private readonly object _sendingSync = new object();

        private readonly ILog _log = LogManager.GetLogger(typeof(RemoteSyslogAsyncSSLAppender));

        public int MaxQueueSize = 1024 * 1024;

        public bool EnableDiagnosticInfo
        {
            get { return _enableRemoteDiagnosticInfo; }
            set { _enableRemoteDiagnosticInfo = value; }
        }
        private bool _enableDiagnosticInfo = false;

        public bool EnableRemoteDiagnosticInfo
        {
            get { return _enableRemoteDiagnosticInfo; }
            set { _enableRemoteDiagnosticInfo = value; }
        }

        private volatile bool _enableRemoteDiagnosticInfo = true;
        private bool _disableSSL = false;

        #region Public Instance Properties

        public bool DisableSSL
        {
            get { return _disableSSL; }
            set { _disableSSL = value; }
        }

        public string AppName
        {
            get { return _appName ?? "-"; }
            set { _appName = value; }
        }

        public string RemoteHost
        {
            get { return m_remoteHost; }
            set { m_remoteHost = value; }
        }

        public string CertificatePath
        {
            get { return m_CertificatePath; }
            set { m_CertificatePath = value; }
        }

        public string Certificate
        {
            get { return m_Certificate; }
            set { m_Certificate = value; }
        }

        public string DomainName
        {
            get { return m_DomainName; }
            set { m_DomainName = value; }
        }

        public string SysLogFacility
        {
            get { return m_SysLogFacility; }
            set { m_SysLogFacility = value; }
        }

        private SyslogFacility SysLogFacilityValue
        {
            get { return m_SysLogFacilityValue; }
            set { m_SysLogFacilityValue = value; }
        }

        /// <summary>
        /// Gets or sets the TCP port number of the remote host or multicast group to which 
        /// the underlying appender should sent the logging event.
        /// </summary>
        public int RemotePort
        {
            get { return m_remotePort; }
            set
            {
                if (value < IPEndPoint.MinPort || value > IPEndPoint.MaxPort)
                {
                    throw new ArgumentOutOfRangeException(
                        String.Format("The value specified {0} is less than {1} or greater than {2}.",
                        value, 
                        IPEndPoint.MinPort.ToString(NumberFormatInfo.InvariantInfo),
                        IPEndPoint.MaxPort.ToString(NumberFormatInfo.InvariantInfo)
                        ));
                }
                else
                {
                    m_remotePort = value;
                }
            }
        }
        #endregion Public Instance Properties


        #region Implementation of IOptionHandler

        /// <summary>
        /// Initialize the appender based on the options set.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is part of the <see cref="IOptionHandler"/> delayed object
        /// activation scheme. The <see cref="ActivateOptions"/> method must 
        /// be called on this object after the configuration properties have
        /// been set. Until <see cref="ActivateOptions"/> is called this
        /// object is in an undefined state and must not be used. 
        /// </para>
        /// <para>
        /// If any of the configuration properties are modified then 
        /// <see cref="ActivateOptions"/> must be called again.
        /// </para>
        /// <para>
        /// The appender will be ignored if no <see cref="RemoteHost" /> was specified or 
        /// an invalid remote or local TCP port number was specified.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The required property <see cref="RemoteAddress" /> was not specified.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The TCP port number assigned to <see cref="LocalPort" /> or <see cref="RemotePort" /> is less than <see cref="IPEndPoint.MinPort" /> or greater than <see cref="IPEndPoint.MaxPort" />.</exception>
        public override void ActivateOptions()
        {
            base.ActivateOptions();

            //Need error checking
            if (this.RemoteHost == null || (this.RemoteHost.Trim().Length == 0))
            {
                throw new ArgumentNullException("The required property 'RemoteHost' was not specified.");
            }

            if (Uri.CheckHostName(RemoteHost) == UriHostNameType.Unknown)
            {
                throw new ArgumentOutOfRangeException("Host specified by property 'RemoteHost' is not a valid URI.");
            }

            if (!this.DisableSSL && this.CertificatePath == null)
            {
                throw new ArgumentNullException("The required property 'CertificatePath' was not specified.");
            }

            if (!this.DisableSSL && !File.Exists(CertificatePath))
            {
                throw new ArgumentOutOfRangeException("File specified by property 'CertificatePath' was not found.");
            }

            if (this.RemotePort == 0)
            {
                throw new ArgumentNullException("The required property 'RemotePort' was not specified.");
            }

            if ((this.SysLogFacility == null) || (SysLogFacility.Trim().Length == 0))
            {
                throw new ArgumentNullException("The required property 'SysLogFacility' was not specified.");
            }

            switch (SysLogFacility.Trim())
            {
                case "Kernel": SysLogFacilityValue = SyslogFacility.Kernel; break;
                case "User": SysLogFacilityValue = SyslogFacility.User; break;
                case "Mail": SysLogFacilityValue = SyslogFacility.Mail; break;
                case "Daemons": SysLogFacilityValue = SyslogFacility.Daemons; break;
                case "Authorization": SysLogFacilityValue = SyslogFacility.Authorization; break;
                case "Syslog": SysLogFacilityValue = SyslogFacility.Syslog; break;
                case "Printer": SysLogFacilityValue = SyslogFacility.Printer; break;
                case "News": SysLogFacilityValue = SyslogFacility.News; break;
                case "Uucp": SysLogFacilityValue = SyslogFacility.Uucp; break;
                case "Clock": SysLogFacilityValue = SyslogFacility.Clock; break;
                case "Authorization2": SysLogFacilityValue = SyslogFacility.Authorization2; break;
                case "Ftp": SysLogFacilityValue = SyslogFacility.Ftp; break;
                case "Ntp": SysLogFacilityValue = SyslogFacility.Ntp; break;
                case "Audit": SysLogFacilityValue = SyslogFacility.Audit; break;
                case "Alert": SysLogFacilityValue = SyslogFacility.Alert; break;
                case "Clock2": SysLogFacilityValue = SyslogFacility.Clock2; break;
                case "Local0": SysLogFacilityValue = SyslogFacility.Local0; break;
                case "Local1": SysLogFacilityValue = SyslogFacility.Local1; break;
                case "Local2": SysLogFacilityValue = SyslogFacility.Local2; break;
                case "Local3": SysLogFacilityValue = SyslogFacility.Local3; break;
                case "Local4": SysLogFacilityValue = SyslogFacility.Local4; break;
                case "Local5": SysLogFacilityValue = SyslogFacility.Local5; break;
                case "Local6": SysLogFacilityValue = SyslogFacility.Local6; break;
                case "Local7": SysLogFacilityValue = SyslogFacility.Local7; break;
                default:
                    throw new ArgumentOutOfRangeException("The value specified by property 'SyslogFacility' is not valid.");
            }

            this.EnsureConnected(); //SSL Logic

            try
            {
                // TODO fix -- Thread.MemoryBarrier();
                _senderThread.Start();
            }
            catch (Exception exc)
            {
                ErrorHandler.Error(exc.ToString());
            }
        }
        #endregion


        #region enums

        /// <summary>
        /// Syslog severity levels per https://en.wikipedia.org/wiki/Syslog & https://syslog-ng.com/documents/html/syslog-ng-ose-latest-guides/en/syslog-ng-ose-guide-admin/html/bsdsyslog-pri.html.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The syslog severities.
        /// </para>
        /// </remarks>
        public enum SyslogSeverity
        {
            Emergency = 0,
            Alert = 1,
            Critical = 2,
            Error = 3,
            Warning = 4,
            Notice = 5,
            Informational = 6,
            Debug = 7
        };

        /// <summary>
        /// Syslog facility per https://en.wikipedia.org/wiki/Syslog & https://syslog-ng.com/documents/html/syslog-ng-ose-latest-guides/en/syslog-ng-ose-guide-admin/html/bsdsyslog-pri.html.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The syslog facilities.
        /// </para>
        /// </remarks>
        public enum SyslogFacility
        {
            Kernel = 0,
            User = 1,
            Mail = 2,
            Daemons = 3,
            Authorization = 4,
            Syslog = 5,
            Printer = 6,
            News = 7,
            Uucp = 8,
            Clock = 9,
            Authorization2 = 10,
            Ftp = 11,
            Ntp = 12,
            Audit = 13,
            Alert = 14,
            Clock2 = 15,
            Local0 = 16,
            Local1 = 17,
            Local2 = 18,
            Local3 = 19,
            Local4 = 20,
            Local5 = 21,
            Local6 = 22,
            Local7 = 23
        }
        #endregion


        public int GeneratePriority(SyslogSeverity severity)
        {
            return GeneratePriority(SysLogFacilityValue, severity);
        }

        public static int GeneratePriority(SyslogFacility facility, SyslogSeverity severity)
        {
            if (facility < SyslogFacility.Kernel || facility > SyslogFacility.Local7)
            {
                throw new ArgumentException("SyslogFacility out of range", "facility");
            }

            if (severity < SyslogSeverity.Emergency || severity > SyslogSeverity.Debug)
            {
                throw new ArgumentException("SyslogSeverity out of range", "severity");
            }

            unchecked
            {
                return ((int)facility * 8) + (int)severity;
            }
        }


        /// <summary>
        /// Translates a log4net level to a syslog severity.
        /// </summary>
        /// <param name="level">A log4net level.</param>
        /// <returns>A syslog severity.</returns>
        /// <remarks>
        /// <para>
        /// Translates a log4net level to a syslog severity.
        /// </para>
        /// </remarks>
        virtual protected SyslogSeverity GetSeverity(Level level)
        {
            LevelSeverity levelSeverity = m_levelMapping.Lookup(level) as LevelSeverity;
            if (levelSeverity != null)
            {
                return levelSeverity.Severity;
            }

            //
            // Fallback to sensible default values
            //

            if (level >= Level.Alert)
            {
                return SyslogSeverity.Alert;
            }
            else if (level >= Level.Critical)
            {
                return SyslogSeverity.Critical;
            }
            else if (level >= Level.Error)
            {
                return SyslogSeverity.Error;
            }
            else if (level >= Level.Warn)
            {
                return SyslogSeverity.Warning;
            }
            else if (level >= Level.Notice)
            {
                return SyslogSeverity.Notice;
            }
            else if (level >= Level.Info)
            {
                return SyslogSeverity.Informational;
            }
            // Default setting
            return SyslogSeverity.Debug;
        }

        #region LevelSeverity LevelMapping Entry
        /// <summary>
        /// A class to act as a mapping between the level that a logging call is made at and
        /// the syslog severity that is should be logged at.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A class to act as a mapping between the level that a logging call is made at and
        /// the syslog severity that is should be logged at.
        /// </para>
        /// </remarks>
        public class LevelSeverity : LevelMappingEntry
        {
            private SyslogSeverity m_severity;

            /// <summary>
            /// The mapped syslog severity for the specified level
            /// </summary>
            /// <remarks>
            /// <para>
            /// Required property.
            /// The mapped syslog severity for the specified level
            /// </para>
            /// </remarks>
            public SyslogSeverity Severity
            {
                get { return m_severity; }
                set { m_severity = value; }
            }
        }
        #endregion // LevelSeverity LevelMapping Entry


        #region Override implementation of AppenderSkeleton

        protected override void Append(LoggingEvent loggingEvent)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            try
            {
                var frame = FormatMessage(loggingEvent);

                lock (_messageQueue)
                {
                    if (_messageQueue.Count == MaxQueueSize - 1)
                    {
                        var warningMessage = string.Format(
                            "Message queue size ({0}) is exceeded. Not sending new messages until the queue backlog has been sent.", MaxQueueSize);
                        LogDiagnosticInfo(warningMessage, Level.Warn);
                    }
                    if (_messageQueue.Count >= MaxQueueSize)
                        return;
                    _messageQueue.Enqueue(frame);
                }
            }
            catch (Exception exc)
            {
                LogError(exc);
            }
        }


        private string FormatMessage(LoggingEvent loggingEvent)
        {
            /* 
             * https://syslog-ng.com/documents/html/syslog-ng-ose-latest-guides/en/syslog-ng-ose-guide-admin/html/concepts-message-bsdsyslog.html
             * Message format is Priority Header Message
             * Priority is "<(Facility*8)+Serverity>"
             * Header is "TimeStamp(yyyy-MM-dd'T'HH:mm:ss) HostName"
             * Message is "program[pid] message-text"
             * 
             */

            int priority = GeneratePriority(SysLogFacilityValue, GetSeverity(loggingEvent.Level));

            string LogTimeStamp = loggingEvent.TimeStamp.ToString("yyyy-MM-dd'T'HH:mm:ss");

            //Byte[] buffer = m_encoding.GetBytes(RenderLoggingEvent(loggingEvent).ToCharArray());
            //Char[] buffer = ("<" + priority.ToString() + "> " + LogTimeStamp + " " + this.FormattedHostName + " " + this._appName + " " + RenderLoggingEvent(loggingEvent)).ToCharArray();
            return ("<" + priority.ToString() + "> " + LogTimeStamp + " " + this.FormattedHostName + " " + this._appName + " " + RenderLoggingEvent(loggingEvent));
        }

        /// <summary>
        /// This appender requires a <see cref="Layout"/> to be set.
        /// </summary>
        /// <value><c>true</c></value>
        /// <remarks>
        /// <para>
        /// This appender requires a <see cref="Layout"/> to be set.
        /// </para>
        /// </remarks>
        override protected bool RequiresLayout
        {
            get { return true; }
            //get { return false; }
        }

        /// <summary>
        /// Closes the connection and releases all resources associated with 
        /// this <see cref="TCPSSLAppender" /> instance.
        /// </summary>
        /// <remarks>
        /// <para>
        /// </para>
        /// </remarks>
        override protected void OnClose()
        {
            base.OnClose();

            this.Disconnect();
        }
        #endregion Override implementation of AppenderSkeleton


        #region Private Instance Fields

        /// <summary>
        /// Mapping from level object to syslog severity
        /// </summary>
        private LevelMapping m_levelMapping = new LevelMapping();

        /// <summary>
        /// The hostname of the remote host to which the logging event will be sent.
        /// </summary>
        private string m_remoteHost;

        /// <summary>
        /// The path and name of the Certificate to use.
        /// </summary>
        private string m_CertificatePath;

        /// <summary>
        /// The Certificate.
        /// </summary>
        private string m_Certificate;

        /// <summary>
        /// The DomainName of the host logger.
        /// </summary>
        private string m_DomainName=String.Empty;

        /// <summary>
        /// The SysLog Facility to encode on the logging event will be sent.
        /// </summary>
        private string m_SysLogFacility;

        private SyslogFacility m_SysLogFacilityValue = SyslogFacility.Kernel;

        /// <summary>
        /// The TCP port number of the remote host or multicast group to 
        /// which the logging event will be sent.
        /// </summary>
        private int m_remotePort;

        private string _appName;

        #endregion Private Instance Fields

        //https://stackoverflow.com/questions/2661764/how-to-check-if-a-socket-is-connected-disconnected-in-c
        private bool SocketConnected(Socket s)
        {
            bool part1 = s.Poll(1000, SelectMode.SelectRead);
            bool part2 = (s.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }

        private void EnsureConnected()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            lock (_connectionSync)
            {
                if ((_socket != null) && (SocketConnected(_socket)))
                    return;

                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(RemoteHost, RemotePort);

                var rawStream = new NetworkStream(socket);

                /*
                 * Open the stream without the "Encoding.UTF8" to avoid the Byte Order Mark/ ef:bb:bf
                 * from being sent.  For the Synology Log Center, if not all Syslog, this will cause
                 * the initial message to be incorrectly logged.
                 * 
                 * SSL is active by default
                 */
                if (_disableSSL)
                {
                    //_writer = new StreamWriter(rawStream, Encoding.UTF8);
                    _writer = new StreamWriter(rawStream);

                }
                else
                {
                    _stream = new SslStream(rawStream, false, VerifyServerCertificate);
                    var certificate = (string.IsNullOrEmpty(CertificatePath))
                        ? new X509Certificate(Encoding.ASCII.GetBytes(Certificate.Trim()))
                        : new X509Certificate(CertificatePath);
                    var certificates = new X509CertificateCollection(new[] { certificate });
                    Task clientAuthenticationTask = _stream.AuthenticateAsClientAsync(RemoteHost, certificates, SslProtocols.Tls, false);
                    Task.WaitAll(clientAuthenticationTask);
                    //_writer = new StreamWriter(_stream, Encoding.UTF8);
                    _writer = new StreamWriter(_stream);
                }
                //_writer.Flush();
                _socket = socket;
            }
        }

        private static bool VerifyServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }


        public void Dispose()
        {
            try
            {
                LogDiagnosticInfo("RemoteSyslogAsyncSSLAppender.Dispose()");

                _closing = true;

                // give the sender thread some time to flush the messages
                _senderThread.Join(TaskWaitTime);

                // TODO fix -- _senderThread.Interrupt();
                _senderThread.Join(TaskWaitTime);

                // TODO fix -- _senderThread.Abort();

                _disposed = true;
            }
            catch (ThreadStateException)
            {
            }
            catch (Exception exc)
            {
                LogError(exc);
            }
            try
            {
                Disconnect();
            }
            catch (Exception exc)
            {
                LogError(exc);
            }
        }

        private static readonly SocketError[] IgnoreSocketErrors = {
            SocketError.TimedOut, SocketError.ConnectionRefused
        };

        void Disconnect()
        {
            lock (_connectionSync)
            {
                if (_writer != null)
                {
                    try
                    {
                        _writer.Dispose();
                    }
                    catch (Exception exc)
                    {
                        //LogError(exc);
                        //TO DO -- add logging
                    }
                    _writer = null;
                }

                if (_stream != null)
                {
                    _stream.Dispose();
                    _stream = null;
                }

                if (_socket != null)
                {
                    try
                    {
                        if (_socket.Connected)
                        {
                            // TODO fix -- _socket.Disconnect(true);
                        }
                    }
                    catch (Exception exc)
                    {
                        //LogError(exc);
                        //TO DO - add logging
                    }

                    _socket.Dispose();
                    _socket = null;
                }
            }
        }

        static bool IsConnected(Socket socket)
        {
            return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
        }

        private void LogStartupInfo()
        {
            Assembly entryAssembly = typeof(RemoteSyslogSSLAppender).GetTypeInfo().Assembly;
            //var entryAssembly = Assembly.GetEntryAssembly();
            var message = string.Format("Starting '{0}' '{1}",
                (entryAssembly != null) ? entryAssembly.FullName : Process.GetCurrentProcess().MainModule.FileName,
                entryAssembly.FullName);
            LogDiagnosticInfo(message);
        }

        void LogDiagnosticInfo(string format, params object[] args)
        {
            if (_enableDiagnosticInfo)
            { 
                try
                {
                    var message = string.Format(format, args);
                    if (_closing)
                        Trace.WriteLine(message);
                    else
                        _log.Info(message);

                    RemoteLog(message, Level.Info);
                }
                catch (Exception exc)
                {
                    Trace.WriteLine(exc.ToString());
                }
            }
        }

        void LogError(string format, params object[] args)
        {
            var message = string.Format(format, args);
            if (_closing)
                Trace.WriteLine(message);
            else
                _log.Error(message);

            RemoteLog(message, Level.Error);
        }

        void LogError(Exception exc)
        {
            LogError(exc.ToString());
        }

        //Cloned from original implementation, remove for now
        private void RemoteLog(string message, Level level)
        {
            //if (EnableRemoteDiagnosticInfo)
            //{
            //    lock (_messageQueue)
            //    {
            //        var loggingEvent = CreateLoggingEvent(message, level);
            //        var renderedMessage = FormatMessage(loggingEvent, _diagFields);
            //        _messageQueue.Enqueue(renderedMessage);
            //    }
            //}
        }

        //Thread Logic for sending messages to remote host
        private void SenderThreadEntry()
        {
            try
            {
                while (!_disposed)
                {
                    if (_log.IsErrorEnabled)
                    {
                        LogStartupInfo();
                        break;
                    }
                    // TODO fix -- Thread.Yield();
                }

                while (!_disposed)
                {
                    TrySendMessages();
                    if (_closing)
                        break;

                    var startTime = DateTime.UtcNow;
                    while (DateTime.UtcNow - startTime < _sendingPeriod && !_closing)
                        Thread.Sleep(10);
                }
            }
            // TODO fix -- 
            //catch (ThreadInterruptedException)
            //{
            //}
            catch (Exception exc)
            {
                LogError(exc);
            }
        }

        private void TrySendMessages()
        {
            try
            {
                SendMessages();
            }
            // TODO fix -- 
            //catch (ThreadInterruptedException)
            //{
            //}
            //catch (ThreadAbortException)
            //{
            //}
            catch (ObjectDisposedException)
            {
            }
            catch (Exception exc)
            {
                LogError(exc);
            }
        }

        void SendMessages()
        {
            lock (_sendingSync)
            {
                Socket socket = null;

                try
                {
                    EnsureConnected();

                    TextWriter writer;
                    lock (_connectionSync)
                    {
                        if (_socket == null || _writer == null)
                            return;
                        socket = _socket;
                        writer = _writer;
                    }

                    _sendingPeriod = _defaultSendingPeriod;

                    while (true)
                    {
                        string frame;

                        lock (_messageQueue)
                        {
                            if (_messageQueue.Count == 0)
                                break;
                            frame = _messageQueue.Peek();
                        }

                        writer.WriteLine(frame);
                        writer.Flush();

                        lock (_messageQueue)
                        {
                            _messageQueue.Dequeue();
                        }
                    }

                    return;
                }
                catch (SocketException exc)
                {
                    if (!IgnoreSocketErrors.Contains(exc.SocketErrorCode))
                        LogError(exc);
                }
                catch (IOException exc)
                {
                    if ((uint)exc.HResult != 0x80131620) // COR_E_IO
                        LogError(exc);
                }

                if (socket != null && IsConnected(socket))
                    return;

                var newPeriod = Math.Min(_sendingPeriod.TotalSeconds * 2, _maxSendingPeriod.TotalSeconds);
                _sendingPeriod = TimeSpan.FromSeconds(newPeriod);

                LogDiagnosticInfo(string.Format("Connection to the server lost. Re-try in {0} seconds.", newPeriod));

                Disconnect();
            }
        }

        public void Flush(double maxTimeSecs = 10)
        {
            LogDiagnosticInfo("RemoteSyslogAsyncSSLAppender.Flush({0}, {1})", Name, maxTimeSecs);

            var thread = new Thread(TrySendMessages);
            thread.Start();

            if (!thread.Join(TaskWaitTime))
            {
                // TODO fix -- 
                //thread.Interrupt();
                //if (!thread.Join(TaskWaitTime))
                //    thread.Abort();
            }
        }
    }
}
