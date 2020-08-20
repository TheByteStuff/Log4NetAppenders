using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;

using TheByteStuff.log4net.Appenders;

namespace Log4NetAppenders.UnitTest
{
    [TestClass]
    public class RemoteSyslogSSLAppenderTest
    {
        //*
        private static string testServerCertificate = @".\Certificate\localhost.pfx";
        private static string testClientCertificate = @".\Certificate\localhost.cert";
        private static string localHost = "127.0.0.1";
        private const int Port = 40004;
        /*/
        //stub for alternate configuration
        private static string testClientCertificate = @"C:\xxxx\xxxxxx.crt";
        private static string localHost = "www.xxxxxxx.com";
        private const int Port = 6514;
        //*/

        private static RemoteSyslogSSLAppender _appender;
        private static ILog _log;

        readonly static MockServer _server = new MockServer();

        private static string FormatMessage(string id)
        {
            var message = id + "_" + Guid.NewGuid();
            return message;
        }

        static void CreateAppender()
        {
            var layout = new PatternLayout("%.255message");
            layout.ActivateOptions();

            var appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                Layout = layout,
                //SysLogFacility = "Daemons",
                SysLogFacility = "Authorization",
                EnableDiagnosticInfo = true,
                EnableRemoteDiagnosticInfo = false,
                DisableSSL = false,
                Name = "RemoteSyslogSSLAppender",
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();

            var diagAppender = new TraceAppender
            {
                Layout = layout,
                Name = "RemoteSyslogSSLAppenderDiagLogger",
            };
            diagAppender.ActivateOptions();

            //BasicConfigurator.Configure(new IAppender[] { diagAppender, appender });
            BasicConfigurator.Configure(diagAppender, appender);

            _appender = appender;
            _log = LogManager.GetLogger("RemoteSyslogSSLAppender");
        }


        [ClassInitialize]
        //Runs once 
        public static void SuiteSetUp(TestContext context)
        {
            //_server = new MockServer();
            _server.Start(Port, testServerCertificate);
            CreateAppender();
        }

        [ClassCleanup]
        //Runs once 
        public static void SuiteTearDown()
        {
            _server.Dispose();
        }

        [TestInitialize]
        //Runs before each test 
        public void TestSetUp()
        {

        }

        [TestCleanup]
        //Runs after each test 
        public void TestTearDown()
        {

        }

        //Appender initialization Exception Checks
        [TestMethod]
        public void TestCertPathNotDefinedSkipDisableSSL()
        {
            string expectedFacility = "Kernel";
            var appender = new RemoteSyslogSSLAppender(localHost, Port, null)
            {
                SysLogFacility = expectedFacility,
                DisableSSL=true,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            int priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Emergency);
            Assert.AreEqual(0, priority, String.Format("Expected priority not set for facility {0}.", expectedFacility));
        }

        [TestMethod]
        public void TestCertPathNotValidSkipDisableSSL()
        {
            string expectedFacility = "Kernel";
            var appender = new RemoteSyslogSSLAppender(localHost, Port, "")
            {
                SysLogFacility = expectedFacility,
                DisableSSL = true,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            int priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Emergency);
            Assert.AreEqual(0, priority, String.Format("Expected priority not set for facility {0}.", expectedFacility));
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "The required property 'SysLogFacility' was not specified.")]
        public void TestSysLogFacilityNotDefinedException()
        {
            var appender = new RemoteSyslogSSLAppender()
            {
                RemoteHost = localHost,
                RemotePort=1,
                CertificatePath = testClientCertificate,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            int priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Emergency);
            Assert.Fail("Expected Exception not hit.");
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "The required property 'RemoteHost' was not specified.")]
        public void TestRemoteHostNotDefinedException()
        {
            string expectedFacility = "Kernel";
            var appender = new RemoteSyslogSSLAppender()
            {
                RemoteHost = null,
                RemotePort = 1,
                CertificatePath = testClientCertificate,
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            int priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Emergency);
            Assert.Fail("Expected Exception not hit.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "Host specified by property 'RemoteHost' is not a valid URI.")]
        public void TestRemoteHostNotValidException()
        {
            string expectedFacility = "Kernel";
            var appender = new RemoteSyslogSSLAppender("192.168.252 .1", Port, testClientCertificate)
            {
                Name = "TestRemoteHostNotValidException",
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            int priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Emergency);
            Assert.Fail("Expected Exception not hit.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "The value specified -1 is less than 0 or greater than 65535.")]
        public void TestRemotePortOutOfRange_BelowException()
        {
            string expectedFacility = "Kernel";
            var appender = new RemoteSyslogSSLAppender()
            {
                RemoteHost = localHost,
                RemotePort = -1,
                CertificatePath = testClientCertificate,
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "The value specified 65536 is less than 0 or greater than 65535.")]
        public void TestRemotePortOutOfRange_AboveException()
        {
            string expectedFacility = "Kernel";
            var appender = new RemoteSyslogSSLAppender()
            {
                RemoteHost = localHost,
                RemotePort = 65536,
                CertificatePath = testClientCertificate,
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "The required property 'CertificatePath' was not specified.")]
        public void TestCertPathNotDefinedException()
        {
            string expectedFacility = "Kernel";
            var appender = new RemoteSyslogSSLAppender(localHost, Port, null)
            {
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            int priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Emergency);
            Assert.Fail("Expected Exception not hit.");
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "File specified by property 'CertificatePath' was not found.")]
        public void TestCertPathNotValidException()
        {
            string expectedFacility = "Kernel";
            var appender = new RemoteSyslogSSLAppender(localHost, Port, "")
            {
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            int priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Emergency);
            Assert.Fail("Expected Exception not hit.");
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "The required property 'SysLogFacility' was not specified.")]
        public void TestSysLogFacilityNotDefined1Exception()
        {
            var appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = null,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            int priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Emergency);
            Assert.Fail("Expected Exception not hit.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "The required property 'SysLogFacility' was not specified.")]
        public void TestSysLogFacilityNotDefined2Exception()
        {
            var appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = "",
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            int priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Emergency);
            Assert.Fail("Expected Exception not hit.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "The value specified by property 'SyslogFacility' is not valid.")]
        public void TestSysLogFacilityInvalidException()
        {
            var appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = "Local9",
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            int priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Emergency);
            Assert.Fail("Expected Exception not hit.");
        }


        [TestMethod]
        public void TestDomainName()
        {
            string expectedFacility = "Kernel";
            var appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = expectedFacility,
                DomainName="TestDomain",
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };

            Assert.AreEqual("TestDomain", appender.DomainName);

            var appender2 = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            Assert.AreEqual(String.Empty, appender2.DomainName);
        }


        [TestMethod]
        public void TestAppName()
        {
            string expectedFacility = "Kernel";
            var appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            Assert.AreEqual(typeof(RemoteSyslogSSLAppenderTest).Name, appender.AppName);

            var appender2 = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = expectedFacility
            };
            Assert.AreEqual("-", appender2.AppName);
        }

        [TestMethod]
        public void TestDiagnosticFlags()
        {
            string expectedFacility = "Kernel";
            var appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = expectedFacility,
                EnableDiagnosticInfo=true,
                EnableRemoteDiagnosticInfo=true,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            Assert.AreEqual(true, appender.EnableDiagnosticInfo);
            Assert.AreEqual(true, appender.EnableRemoteDiagnosticInfo);

            var appender2 = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = expectedFacility,
                EnableDiagnosticInfo = false,
                EnableRemoteDiagnosticInfo = false
            };
            Assert.AreEqual(false, appender2.EnableDiagnosticInfo);
            Assert.AreEqual(false, appender2.EnableRemoteDiagnosticInfo);
        }


        [TestMethod]
        public void TestSysLogFacility()
        {
            string expectedFacility = "Kernel";
            var appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            int priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Emergency);
            Assert.AreEqual(0, priority, String.Format("Expected priority not set for facility {0}.", expectedFacility));

            expectedFacility = "User";
            appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Emergency);
            Assert.AreEqual(8, priority, String.Format("Expected priority not set for facility {0}.", expectedFacility));

            expectedFacility = "Mail";
            appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Emergency);
            Assert.AreEqual(16, priority, String.Format("Expected priority not set for facility {0}.", expectedFacility));

            expectedFacility = "Daemons";
            appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Emergency);
            Assert.AreEqual(24, priority, String.Format("Expected priority not set for facility {0}.", expectedFacility));

            expectedFacility = "Authorization";
            appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Emergency);
            Assert.AreEqual(32, priority, String.Format("Expected priority not set for facility {0}.", expectedFacility));

            expectedFacility = "Syslog";
            appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Emergency);
            Assert.AreEqual(40, priority, String.Format("Expected priority not set for facility {0}.", expectedFacility));

            expectedFacility = "Printer";
            appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Emergency);
            Assert.AreEqual(48, priority, String.Format("Expected priority not set for facility {0}.", expectedFacility));

            expectedFacility = "News";
            appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Emergency);
            Assert.AreEqual(56, priority, String.Format("Expected priority not set for facility {0}.", expectedFacility));

            expectedFacility = "Uucp";
            appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Emergency);
            Assert.AreEqual(64, priority, String.Format("Expected priority not set for facility {0}.", expectedFacility));

            expectedFacility = "Clock";
            appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Emergency);
            Assert.AreEqual(72, priority, String.Format("Expected priority not set for facility {0}.", expectedFacility));

            expectedFacility = "Authorization2";
            appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Emergency);
            Assert.AreEqual(80, priority, String.Format("Expected priority not set for facility {0}.", expectedFacility));

            expectedFacility = "Ftp";
            appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Emergency);
            Assert.AreEqual(88, priority, String.Format("Expected priority not set for facility {0}.", expectedFacility));

            expectedFacility = "Ntp";
            appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Emergency);
            Assert.AreEqual(96, priority, String.Format("Expected priority not set for facility {0}.", expectedFacility));

            expectedFacility = "Audit";
            appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Emergency);
            Assert.AreEqual(104, priority, String.Format("Expected priority not set for facility {0}.", expectedFacility));

            expectedFacility = "Alert";
            appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Emergency);
            Assert.AreEqual(112, priority, String.Format("Expected priority not set for facility {0}.", expectedFacility));

            expectedFacility = "Clock2";
            appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Emergency);
            Assert.AreEqual(120, priority, String.Format("Expected priority not set for facility {0}.", expectedFacility));

            expectedFacility = "Local0";
            appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Emergency);
            Assert.AreEqual(128, priority, String.Format("Expected priority not set for facility {0}.", expectedFacility));

            expectedFacility = "Local1";
            appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Emergency);
            Assert.AreEqual(136, priority, String.Format("Expected priority not set for facility {0}.", expectedFacility));

            expectedFacility = "Local2";
            appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Emergency);
            Assert.AreEqual(144, priority, String.Format("Expected priority not set for facility {0}.", expectedFacility));

            expectedFacility = "Local3";
            appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Emergency);
            Assert.AreEqual(152, priority, String.Format("Expected priority not set for facility {0}.", expectedFacility));

            expectedFacility = "Local4";
            appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Emergency);
            Assert.AreEqual(160, priority, String.Format("Expected priority not set for facility {0}.", expectedFacility));

            expectedFacility = "Local5";
            appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Emergency);
            Assert.AreEqual(168, priority, String.Format("Expected priority not set for facility {0}.", expectedFacility));

            expectedFacility = "Local6";
            appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Emergency);
            Assert.AreEqual(176, priority, String.Format("Expected priority not set for facility {0}.", expectedFacility));

            expectedFacility = "Local7";
            appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Emergency);
            Assert.AreEqual(184, priority, String.Format("Expected priority not set for facility {0}.", expectedFacility));
        }


        [TestMethod]
        public void TestSysLogLevel()
        {
            string expectedFacility = "Kernel";
            string expectedLogServerity = "Emergency";
            var appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            int priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Emergency);
            Assert.AreEqual(0, priority, String.Format("Expected priority not set for severity {0}.", expectedLogServerity));

            expectedFacility = "Kernel";
            expectedLogServerity = "Alert";
            appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Alert);
            Assert.AreEqual(1, priority, String.Format("Expected priority not set for severity {0}.", expectedFacility));

            expectedFacility = "Kernel";
            expectedLogServerity = "Critical";
            appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Critical);
            Assert.AreEqual(2, priority, String.Format("Expected priority not set for severity {0}.", expectedFacility));

            expectedFacility = "Kernel";
            expectedLogServerity = "Error";
            appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Error);
            Assert.AreEqual(3, priority, String.Format("Expected priority not set for severity {0}.", expectedFacility));

            expectedFacility = "Kernel";
            expectedLogServerity = "Warning";
            appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Warning);
            Assert.AreEqual(4, priority, String.Format("Expected priority not set for severity {0}.", expectedFacility));

            expectedFacility = "Kernel";
            expectedLogServerity = "Notice";
            appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Notice);
            Assert.AreEqual(5, priority, String.Format("Expected priority not set for severity {0}.", expectedFacility));

            expectedFacility = "Kernel";
            expectedLogServerity = "Informational";
            appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Informational);
            Assert.AreEqual(6, priority, String.Format("Expected priority not set for severity {0}.", expectedFacility));

            expectedFacility = "Kernel";
            expectedLogServerity = "Debug";
            appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Debug);
            Assert.AreEqual(7, priority, String.Format("Expected priority not set for severity {0}.", expectedFacility));

            expectedFacility = "Mail";
            expectedLogServerity = "Warning";
            appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            {
                SysLogFacility = expectedFacility,
                AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            };
            appender.ActivateOptions();
            priority = appender.GeneratePriority(RemoteSyslogSSLAppender.SyslogSeverity.Warning);
            Assert.AreEqual(20, priority, String.Format("Expected priority not set for severity {0}.", expectedFacility));
        }

        [TestMethod]
        public void TestMessageReceipt()
        {
            var sentMessages = new List<string>();
            _server.ClearMessages();

            Assert.AreEqual(0, _server.GetMessages().Count);    //Confirm messages cleared
            int expectedMessages = 0;

            var message = FormatMessage("This a log info message ");
            _log.Info(message);
            sentMessages.Add(message);
            expectedMessages++;
            Thread.Sleep(TimeSpan.FromSeconds(2));

            message = FormatMessage("This a log warning message ");
            _log.Warn(message);
            sentMessages.Add(message);
            expectedMessages++;
            Thread.Sleep(TimeSpan.FromSeconds(2));

            message = FormatMessage("This a log error message ");
            _log.Error(message);
            sentMessages.Add(message);
            expectedMessages++;
            Thread.Sleep(TimeSpan.FromSeconds(2));

            message = FormatMessage("This is a log debug message ");
            _log.Debug(message);
            sentMessages.Add(message);
            expectedMessages++;
            Thread.Sleep(TimeSpan.FromSeconds(2));

            message = FormatMessage("This is a log fatal message ");
            _log.Fatal(message);
            expectedMessages++;
            sentMessages.Add(message);

            Thread.Sleep(TimeSpan.FromSeconds(15));
            //_server.CloseConnections();

            var messages = _server.GetMessages();

            Assert.AreEqual(expectedMessages, messages.Count());
        }


        [TestMethod]
        public void TestMessagesProcessed()
        {
            var sentMessages = new List<string>();
            var i = 0;

            //var layout = new PatternLayout("%.255message");
            //layout.ActivateOptions();

            //ILog _log;
            //string expectedFacility = "Mail";
            //RemoteSyslogSSLAppender appender = new RemoteSyslogSSLAppender(localHost, Port, testClientCertificate)
            //{
            //    SysLogFacility = expectedFacility,
            //    Name= "RemoteSyslogSSLAppenderTest",
            //    //Threshold = log4net.Core.Level.All,
            //    AppName = typeof(RemoteSyslogSSLAppenderTest).Name
            //};
            //appender.ActivateOptions();

            //var diagAppender = new TraceAppender
            //{
            //    Layout = layout,
            //    Name = "RemoteSyslogSSLAppenderDiagLogger",
            //};
            //diagAppender.ActivateOptions();

            //BasicConfigurator.Configure(diagAppender, appender);

            //_log = LogManager.GetLogger(typeof(RemoteSyslogSSLAppenderTest));
            ////_log = LogManager.GetLogger("RemoteSyslogSSLAppenderTest");

            //was 100
            for (; i < 2; i++)
            {
                var message = FormatMessage("This a log info message " + i);
                _log.Info(message);
                message = FormatMessage("This a log warning message " + i);
                _log.Warn(message);
                message = FormatMessage("This a log error message " + i);
                _log.Error(message);
                sentMessages.Add(message);
            }

            Thread.Sleep(TimeSpan.FromSeconds(11));
            //_server.CloseConnections();

            // was 200
            for (; i < 3; i++)
            {
                var message = FormatMessage("This is a log debug message " + i);
                _log.Debug(message);
                sentMessages.Add(message);
            }

            Thread.Sleep(TimeSpan.FromSeconds(16));

            var messages = _server.GetMessages();
            var missingMessages = sentMessages.Count(sentMessage => !messages.Any(message => message.Contains(sentMessage)));

            Assert.IsTrue(missingMessages < sentMessages.Count * 0.01);
        }
    }
}
