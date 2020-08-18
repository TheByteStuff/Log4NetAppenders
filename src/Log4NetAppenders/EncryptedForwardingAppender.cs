using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Util;
using log4net.Repository.Hierarchy;

using System.Security.Cryptography;
// using Bouncy Castle library: http://www.bouncycastle.org/csharp/
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace TheByteStuff.log4net.Appenders
{
    public class EncryptedForwardingAppender : ForwardingAppender
    {

        public EncryptedForwardingAppender()
        {
        }


        public EncryptedForwardingAppender(string PublicKeyFileNameAndPath, string PublicKey) : this()
        {
            if (PublicKeyFileNameAndPath != null && PublicKeyFileNameAndPath.Trim().Length > 0)
            {
                this.PublicKeyFileNameAndPath = PublicKeyFileNameAndPath;
            }

            if (PublicKey != null && PublicKey.Trim().Length > 0)
            {
                this.PublicKeyInline = PublicKey;
            }
        }


        private string m_PublicKey; // = String.Empty;
        private string m_PublicKeyFileNameAndPath; // = String.Empty;

        private PgpPublicKey PGPPublicKey;

        public string PublicKeyFileNameAndPath
        {
            get { return m_PublicKeyFileNameAndPath; }
            set { m_PublicKeyFileNameAndPath = value; }
        }

        public string PublicKeyInline
        {
            get { return m_PublicKey; }
            set { m_PublicKey = value; }
        }


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
        /// The appender will be ignored if no PGP Public Key can be determined from the supplied
        /// parameters <see cref="PublicKeyFileNameAndPath" /> or <see cref="PublicKeyInline" />.
        /// </para>
        /// <para>Referenced https://github.com/ArtisanCode/Log4NetMessageEncryptor as part of
        /// building this solution.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The required PGPPublicKey could not be determined.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The file specified by <see cref="PublicKeyFileNameAndPath" /> was not found.</exception>
        public override void ActivateOptions()
        {
            base.ActivateOptions();

            //Need error checking

            if ((this.PublicKeyFileNameAndPath != null) && (this.PublicKeyFileNameAndPath.Trim().Length>0))
            {
                if (!File.Exists(this.PublicKeyFileNameAndPath))
                {
                    throw new ArgumentOutOfRangeException("File specified by property 'PublicKeyFileNameAndPath' was not found.");
                }
                else
                {
                    using (Stream publicKeyStream = File.OpenRead(PublicKeyFileNameAndPath))
                    {
                        this.PGPPublicKey = ReadPublicKeyFile(publicKeyStream);
                    }
                }
            }

            if (this.PublicKeyInline != null && this.PublicKeyInline.Trim().Length > 0)
            {
                try
                {
                    this.PGPPublicKey = ReadPublicKey(this.PublicKeyInline);
                }
                catch (Exception ex)
                {
                    throw new ArgumentOutOfRangeException("Data specified by property 'PublicKeyInline' is not a valid PGP Public Key.");
                }
            }

            if (this.PGPPublicKey == null)
            {
                throw new ArgumentNullException("The required 'PGPPublicKey' was not able to be determined from input parameters.");
            }

        }


        private PgpPublicKey ReadPublicKey(string PublicKeyInText)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(PublicKeyInText);
            Stream stream = new MemoryStream(byteArray);
            stream.Position = 0;

            return ReadPublicKeyFile(stream);
        }


        private PgpPublicKey ReadPublicKeyFile(Stream inputStream)
        {
            inputStream = PgpUtilities.GetDecoderStream(inputStream);
            PgpPublicKeyRingBundle pgpPub = new PgpPublicKeyRingBundle(inputStream);

            foreach (PgpPublicKeyRing keyRing in pgpPub.GetKeyRings())
            {
                foreach (PgpPublicKey key in keyRing.GetPublicKeys())
                {
                    if (key.IsEncryptionKey)
                    {
                        return key;
                    }
                }
            }

            throw new ArgumentOutOfRangeException("Can't find encryption key in key ring.");
        }


        private string EncryptPgpData(string inputData)
        {
            // use armor: yes, use integrity check? yes?
            string encryptedData = EncryptPgpData(inputData, true, true);

            //remove version
            string CRLF = "\r\n";
            int start = encryptedData.IndexOf("Version: ");
            int end = encryptedData.IndexOf(CRLF, start + 1);
            if (end > start)
            {
                encryptedData = encryptedData.Remove(start, (end - start) + 4);
            }

            string beginMessageTag = "-----BEGIN PGP MESSAGE-----\r\n";
            int beginMessageTagPosition = encryptedData.IndexOf(beginMessageTag);

            string endMessageTag = "\r\n-----END PGP MESSAGE-----\r\n";
            int endMessageTagPosition = encryptedData.IndexOf(endMessageTag);

            beginMessageTagPosition = beginMessageTag.Length;
            start = encryptedData.IndexOf(CRLF, beginMessageTagPosition);
            while ((start > 0) && (start < endMessageTagPosition))
            {
                encryptedData = encryptedData.Remove(start, CRLF.Length);
                start = encryptedData.IndexOf(CRLF, beginMessageTagPosition);
                endMessageTagPosition = encryptedData.IndexOf(endMessageTag);
            }

            return encryptedData;
        }


        public string EncryptPgpData(string inputData, bool armor, bool withIntegrityCheck)
        {
            byte[] processedData = Compress(Encoding.ASCII.GetBytes(inputData), PgpLiteralData.Console, CompressionAlgorithmTag.Uncompressed);

            MemoryStream bOut = new MemoryStream();
            Stream output = bOut;

            if (armor)
                output = new ArmoredOutputStream(output);

            PgpEncryptedDataGenerator encGen = new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.Cast5, withIntegrityCheck, new SecureRandom());
            encGen.AddMethod(PGPPublicKey);

            Stream encOut = encGen.Open(output, processedData.Length);

            encOut.Write(processedData, 0, processedData.Length);
            // TODO fix -- encOut.Close();
            // try flush, not Flush?
            encOut.Flush();
            encOut.Dispose();

            if (armor)
            {

                output.Flush();
                output.Dispose();
                //output.Close();
            }

            return System.Text.Encoding.UTF8.GetString(bOut.ToArray());
        }


        private static byte[] Compress(byte[] clearData, string fileName, CompressionAlgorithmTag algorithm)
        {
            MemoryStream bOut = new MemoryStream();

            PgpCompressedDataGenerator comData = new PgpCompressedDataGenerator(algorithm);
            Stream cos = comData.Open(bOut); // open it with the final destination
            PgpLiteralDataGenerator lData = new PgpLiteralDataGenerator();

            // we want to Generate compressed data. This might be a user option later,
            // in which case we would pass in bOut.
            Stream pOut = lData.Open(
            cos,                    // the compressed output stream
            PgpLiteralData.Binary,
            fileName,               // "filename" to store
            clearData.Length,       // length of clear data
            DateTime.UtcNow         // current time
            );

            pOut.Write(clearData, 0, clearData.Length);
            //TODO - try this to fix no close
            pOut.Flush();
            pOut.Dispose();
            // pOut.Close();

            comData.Close();

            return bOut.ToArray();
        }


        /// <summary>
        /// Generates the encrypted log event.
        /// </summary>
        /// <param name="source">The source logging event.</param>
        /// <returns>The source logging event with the message encrypted accordingly</returns>
        public virtual LoggingEvent GenerateEncryptedLogEvent(LoggingEvent source)
        {
            LoggingEvent result;

            try
            {
                LoggingEventData data = new LoggingEventData();
                data.Domain = source.Domain;
                data.Level = source.Level;
                data.LoggerName = "TheByteStuff.EncryptedForwardingAppender";
                data.ThreadName = source.ThreadName;
                data.TimeStampUtc = source.TimeStampUtc;
                data.UserName = source.UserName;

                data.Message = EncryptPgpData(source.RenderedMessage);

                string exceptionString = source.GetExceptionString();
                if (!string.IsNullOrWhiteSpace(exceptionString))
                {
                    data.ExceptionString = EncryptPgpData(exceptionString);
                }

                result = new LoggingEvent(data);
            }
            catch (Exception ex)
            {
                throw ex;
                // Ensure that the logging encryption never fails with an unexpected exception, rather, create an error
                // log event so that can be logged instead. This is to ensure that we aren't inadvertently leaking
                // sensitive data in our logs if an error occurs, better to log nothing than leak data!
                //result = LogEventFactory.CreateErrorEvent(ex.Message);
            }

            return result;
        }


        /// <summary>
        /// Actions the append.
        /// </summary>
        /// <param name="loggingEvent">The logging event.</param>
        public virtual void ActionAppend(LoggingEvent loggingEvent)
        {
            var eventWithEncryptedMessage = GenerateEncryptedLogEvent(loggingEvent);

            base.Append(eventWithEncryptedMessage);
        }

        /// <summary>
        /// Actions the append.
        /// </summary>
        /// <param name="loggingEvents">The logging events.</param>
        public virtual void ActionAppend(LoggingEvent[] loggingEvents)
        {
            var encryptedEvents = loggingEvents.Select(x => GenerateEncryptedLogEvent(x)).ToArray();

            base.Append(encryptedEvents);
        }

        /// <summary>
        /// Forward the logging event to the attached appenders
        /// </summary>
        /// <param name="loggingEvent">The event to log.</param>
        /// <remarks>
        /// Delivers the logging event to all the attached appenders.
        /// </remarks>
        protected override void Append(LoggingEvent loggingEvent)
        {
            ActionAppend(loggingEvent);
        }

        /// <summary>
        /// Forward the logging events to the attached appenders
        /// </summary>
        /// <param name="loggingEvents">The array of events to log.</param>
        /// <remarks>
        /// Delivers the logging events to all the attached appenders.
        /// </remarks>
        protected override void Append(LoggingEvent[] loggingEvents)
        {
            ActionAppend(loggingEvents);
        }

    }
}
