using System;
using System.IO;
using System.Text;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;

using System.Security.Cryptography;
// using Bouncy Castle library: http://www.bouncycastle.org/csharp/
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

using TheByteStuff.log4net.Appenders;

namespace Log4NetAppenders.UnitTest
{
    [TestClass]
    public class EncryptedForwardingAppenderTest
    {
        private static string testServerCertificate = @".\Certificate\localhost.pfx";
        private static string testClientCertificate = @".\Certificate\localhost.cert";

        private static string testPGPPublicKey = @".\Encryption\testuserPublic.asc";
        private static string testPGPPrivateKey = @".\Encryption\testuserPrivate.asc";
        private static string testPGPPrivateKeyPassphrase = "O8v2Q*gMDvOJKI";
        private static string testPGPPublicKeyInline = "----- BEGIN PGP PUBLIC KEY BLOCK-----\n Version: GnuPG v2\n\n mQENBFsNrLoBCADL7Z/hDdMFm18oO9u8o/QKCcJWtp9RxkI+gOANX03gBLv8+Ad1\n LpDR4/ptZACyz4lE9nzBYjZJKmjgoV8SEoGX3lHGmBPetaEh4EdzYOYG7Ty7XiN0\n VZ7BIuEwgQ32TvQ0gwJuIyEIXYEPpX6UK044BLFZFjSXA8ooqHIY5pj9P+kVblmw\n i/SOi2pMga5q1t0r+jnnrvFy6Z85/p4TxNhSBaUJzaXTIzEt85ujv3+z/lAf5FEf\n GoOaxinKSgchyAWAN1+piEaNssVEigVcTTplxm6BUKobA6naGq0n3E6oX0m8ENow\n fd9DM76BYi/hicludvt0lwNVZbLPGy4W2S7PABEBAAG0MUdQRyBUZXN0VXNlciAo\n VGVzdCBVc2UgT25seSkgPHRlc3R1c2VyQGdtYWlsLmNvbT6JATkEEwEIACMFAlsN\n rLoCGwMHCwkIBwMCAQYVCAIJCgsEFgIDAQIeAQIXgAAKCRD4pnzEGHdCDErnB/43\n JE8BVHewDemPIrNkAHWlwJOPvohDyIB2JTu7meTcSvNmLyNw3QlUMQSbmKPhulzF\n i6kx4Bu3oi398CzGRAu06a/9rj8aTjbVzEFYXH8R6YnmXKashzbxmJLKniDNEUnm\n reAMz4PcFnhdWI2ADSnmhry84VW83netwcyKYqTHJisYuPeU+YF9kvohOrKZ+P34\n YF/sM3mnApCQQcEhPcHhG05+mP0vGahQkFn3nfR9RMKCE6ixKx/K0LFbI9u6+ckN\n vOIQ01kEfExHf/78EN5kMYR9IYc82KIz5wAngVUKPD8YE3daHvj19r98O4NNrRnR\n      6qbR+ovqjMsUCWYHxEZpuQENBFsNrLoBCADU0m0AYu3ZHn/zx01e/YEO9YtSWUBo\n V3w+piradZI6ryR2qftciWY5cjmFFtsDXeluHx+/fm02HGoIWjdREqLElxd4/8io\n YC3lXLYUhmIK6CMKOuV5MOWOwx0o17JPly8+T62sEdn1b1qrhx8bceXuBkRSedrc\n eHt014PNgyG3f8T8NkuafUPy+fzhIlaTLIXbHZ2GXeXaxf0EqO36idofWKvkeLfT\n sSijKZNRFV1smrS758HCrPzERcnfTWESAU2hsY3r4NipWsK2r2CJYP310y+KrGPf\n      /yHt6vRo4N/yO1IdMOywQu0SsJJ8Nb9GhuNp22lNcNgBscHP1MHbzP3VABEBAAGJ\n AR8EGAEIAAkFAlsNrLoCGwwACgkQ+KZ8xBh3QgzObAgAum+4QgfGe/QuW3sdvHKw\n      7qlxhlMC2+ZGFMCgWJpwQ2PsHmiH/8nkIgIRj90HNdqaajaRiIy7iYAmGwuPitUJ\n HE6HgenKcm1c9y+t+xb3rnRpCTZrldrQOHlvOZTsV1pKnU+86DBrbTcbGoWlAdYH\n HNoi9FTwNrLi5QisSfNRGEm7Mv2U++CyRJqSUd9kINOruCfzpgLWq4fvmqJ0lR3f\n Mxa8O8PgtPT98BollUztuGpQ7k2K1u+TlOdpZamGBK+7frJYqoPYkXA9LKnb1RqA\n BPbo9OmNNQ2GgaHsPyibI0GRjKvibA7iZIf02/M+Usq+WqwdnTwPUJHiZ+dBVr4L\n uA==\n      =HaYM\n      -----END PGP PUBLIC KEY BLOCK-----\n    ";

        private static string localHost = "127.0.0.1";
        private const int Port = 40004;

        private static EncryptedForwardingAppender _appender;
        private static ILog _log;

        private static MemoryAppender _memoryAppender = new MemoryAppender();

        private static string FormatMessage(string id)
        {
            var message = id + "_" + Guid.NewGuid();
            return message;
        }

        static void CreateAppender()
        {
            var layout = new PatternLayout("%.255message");
            layout.ActivateOptions();

            var appender = new EncryptedForwardingAppender()
            {
                Layout = layout,
                PublicKeyFileNameAndPath = testPGPPublicKey,
                Name = "EncryptedForwardingAppender",
            };
            appender.ActivateOptions();

            appender.AddAppender(_memoryAppender);

            var diagAppender = new TraceAppender
            {
                Layout = layout,
                Name = "EncryptedForwardingAppenderDiagLogger",
            };
            diagAppender.ActivateOptions();

            BasicConfigurator.Configure(diagAppender, appender);

            _appender = appender;
            _log = LogManager.GetLogger("EncryptedForwardingAppender");
        }


        [ClassInitialize]
        //Runs once 
        public static void SuiteSetUp(TestContext context)
        {
            CreateAppender();
        }

        [ClassCleanup]
        //Runs once 
        public static void SuiteTearDown()
        {
        }

        [TestInitialize]
        //Runs before each test 
        public void TestSetUp()
        {
            _memoryAppender.Clear();
        }

        [TestCleanup]
        //Runs after each test 
        public void TestTearDown()
        {

        }

        //Appender initialization Exception Checks
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "File specified by property 'PublicKeyFileNameAndPath' was not found.")]
        public void TestPublicKeyFileNameAndPathInvalidException()
        {
            var appender = new EncryptedForwardingAppender()
            {
                PublicKeyFileNameAndPath = "c:\badfilename.txt",
                Name = "EncryptedForwardingAppender",
            };

            appender.ActivateOptions();
            Assert.Fail("Expected Exception not hit.");
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "The required property 'PublicKey' was not specified.")]
        public void TestPGPPublicKeyInvalidException()
        {
            //use valid file with no key
            var appender = new EncryptedForwardingAppender()
            {
                PublicKeyFileNameAndPath = testServerCertificate,
                Name = "EncryptedForwardingAppender",
            };

            appender.ActivateOptions();
            Assert.Fail("Expected Exception not hit.");
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "Data specified by property 'PublicKeyInline' is not a valid PGP Public Key.")]
        public void TestPublicKeyInlineInvalidException()
        {
            var appender = new EncryptedForwardingAppender()
            {
                PublicKeyInline = "BadPublicKey",
                Name = "EncryptedForwardingAppender",
            };

            appender.ActivateOptions();
            Assert.Fail("Expected Exception not hit.");
        }


        [TestMethod]
        public void TestConstructor()
        {
            var appender = new EncryptedForwardingAppender(testPGPPublicKey, null);
            appender.ActivateOptions();

            var appender2 = new EncryptedForwardingAppender(null, testPGPPublicKeyInline);
            appender2.ActivateOptions();
        }


        [TestMethod]
        public void TestMessageReceipt()
        {
            var sentMessages = new List<string>();

            Assert.AreEqual(0, _memoryAppender.GetEvents().Length);    //Confirm messages cleared
            int expectedMessages = 0;

            var message = FormatMessage("This a log info message ");
            _log.Info(message);
            sentMessages.Add(message);
            expectedMessages++;
            Thread.Sleep(TimeSpan.FromSeconds(1));

            message = FormatMessage("This a log warning message ");
            _log.Warn(message);
            sentMessages.Add(message);
            expectedMessages++;
            Thread.Sleep(TimeSpan.FromSeconds(1));

            message = FormatMessage("This a log error message ");
            _log.Error(message);
            sentMessages.Add(message);
            expectedMessages++;
            Thread.Sleep(TimeSpan.FromSeconds(1));

            message = FormatMessage("This is a log debug message ");
            _log.Debug(message);
            sentMessages.Add(message);
            expectedMessages++;
            Thread.Sleep(TimeSpan.FromSeconds(1));

            message = FormatMessage("This is a log fatal message ");
            _log.Fatal(message);
            expectedMessages++;
            sentMessages.Add(message);
            Thread.Sleep(TimeSpan.FromSeconds(1));

            log4net.Core.LoggingEvent[] events = _memoryAppender.GetEvents();

            Assert.AreEqual(expectedMessages, events.Count());
        }


        [TestMethod]
        public void TestMessagesProcessedPublicKeyInline()
        {
            var sentMessages = new List<string>();
            var i = 0;

            MemoryAppender memoryAppender = new MemoryAppender();
            var layout = new PatternLayout("%.255message");
            layout.ActivateOptions();

            EncryptedForwardingAppender appenderPublicKeyInline = new EncryptedForwardingAppender()
            {
                Layout = layout,
                PublicKeyInline = testPGPPublicKeyInline,
                Name = "EncryptedForwardingAppenderPublicKeyInline",
            };
            appenderPublicKeyInline.ActivateOptions();
            appenderPublicKeyInline.AddAppender(memoryAppender);

            BasicConfigurator.Configure(appenderPublicKeyInline);
            ILog  logPublicKeyInline = LogManager.GetLogger("EncryptedForwardingAppenderPublicKeyInline");

            Assert.AreEqual(0, memoryAppender.GetEvents().Length);    //Confirm messages cleared

            var message = FormatMessage("This a log info message " + i++);
            logPublicKeyInline.Info(message);
            sentMessages.Add(message);

            message = FormatMessage("This a log warning message " + i++);
            logPublicKeyInline.Warn(message);
            sentMessages.Add(message);

            message = FormatMessage("This a log error message " + i++);
            logPublicKeyInline.Error(message);
            sentMessages.Add(message);

            log4net.Core.LoggingEvent[] events = memoryAppender.GetEvents();
            var receivedMessages = new List<string>();
            foreach (log4net.Core.LoggingEvent logevent in events)
            {
                receivedMessages.Add(FormatAndDecryptedLogMessage(logevent.RenderedMessage));
            }
            var missingMessages = sentMessages.Count(sentMessage => !receivedMessages.Any(rcvmessage => rcvmessage.Contains(sentMessage)));
            Assert.IsTrue(missingMessages < sentMessages.Count * 0.01);
        }


        [TestMethod]
        public void TestMessagesProcessed()
        {
            var sentMessages = new List<string>();
            var i = 0;

            Assert.AreEqual(0, _memoryAppender.GetEvents().Length);    //Confirm messages cleared

            for (; i < 100; i++)
            {
                var message = FormatMessage("This a log info message " + i);
                _log.Info(message);
                sentMessages.Add(message);

                message = FormatMessage("This a log warning message " + i);
                _log.Warn(message);
                sentMessages.Add(message);

                message = FormatMessage("This a log error message " + i);
                _log.Error(message);
                sentMessages.Add(message);
            }

            for (; i < 200; i++)
            {
                var message = FormatMessage("This is a log debug message " + i);
                _log.Debug(message);
                sentMessages.Add(message);
                message = FormatMessage("This is a log fatal message " + i);
                _log.Fatal(message);
                sentMessages.Add(message);
            }

            log4net.Core.LoggingEvent[] events = _memoryAppender.GetEvents();
            var receivedMessages = new List<string>();
            foreach (log4net.Core.LoggingEvent logevent in events)
            {
                receivedMessages.Add(FormatAndDecryptedLogMessage(logevent.RenderedMessage)); 
            }
            var missingMessages = sentMessages.Count(sentMessage => !receivedMessages.Any(message => message.Contains(sentMessage)));
            Assert.IsTrue(missingMessages < sentMessages.Count * 0.01);
        }


        private string FormatAndDecryptedLogMessage(string inputdata)
        {
            string temp = inputdata;
            temp = temp.Replace("-----BEGIN PGP MESSAGE-----\r\n", "");
            temp = temp.Replace("\r\n-----END PGP MESSAGE-----", "");
            temp = temp.Replace("\r\n", "");
            return DecryptPgpData(temp);
        }

        //PGP Decryption Logic
        private string DecryptPgpData(string inputData)
        {
            string output;
            using (Stream inputStream = IoHelper.GetStream(inputData))
            {
                using (Stream keyIn = File.OpenRead(testPGPPrivateKey))
                {
                    output = DecryptPgpData(inputStream, keyIn, testPGPPrivateKeyPassphrase);
                }
            }
            return output;
        }

        private string DecryptPgpData(Stream inputStream, Stream privateKeyStream, string passPhrase)
        {
            string output;

            PgpObjectFactory pgpFactory = new PgpObjectFactory(PgpUtilities.GetDecoderStream(inputStream));
            // find secret key
            PgpSecretKeyRingBundle pgpKeyRing = new PgpSecretKeyRingBundle(PgpUtilities.GetDecoderStream(privateKeyStream));

            PgpObject pgp = null;
            if (pgpFactory != null)
            {
                pgp = pgpFactory.NextPgpObject();
            }

            // the first object might be a PGP marker packet.
            PgpEncryptedDataList encryptedData = null;
            if (pgp is PgpEncryptedDataList)
            {
                encryptedData = (PgpEncryptedDataList)pgp;
            }
            else
            {
                encryptedData = (PgpEncryptedDataList)pgpFactory.NextPgpObject();
            }

            // decrypt
            PgpPrivateKey privateKey = null;
            PgpPublicKeyEncryptedData pubKeyData = null;
            foreach (PgpPublicKeyEncryptedData pubKeyDataItem in encryptedData.GetEncryptedDataObjects())
            {
                privateKey = FindSecretKey(pgpKeyRing, pubKeyDataItem.KeyId, passPhrase.ToCharArray());

                if (privateKey != null)
                {
                    pubKeyData = pubKeyDataItem;
                    break;
                }
            }

            if (privateKey == null)
            {
                throw new ArgumentException("Secret key for message not found.");
            }

            PgpObjectFactory plainFact = null;
            using (Stream clear = pubKeyData.GetDataStream(privateKey))
            {
                plainFact = new PgpObjectFactory(clear);
            }

            PgpObject message = plainFact.NextPgpObject();

            if (message is PgpCompressedData)
            {
                PgpCompressedData compressedData = (PgpCompressedData)message;
                PgpObjectFactory pgpCompressedFactory = null;

                using (Stream compDataIn = compressedData.GetDataStream())
                {
                    pgpCompressedFactory = new PgpObjectFactory(compDataIn);
                }

                message = pgpCompressedFactory.NextPgpObject();
                PgpLiteralData literalData = null;
                if (message is PgpOnePassSignatureList)
                {
                    message = pgpCompressedFactory.NextPgpObject();
                }

                literalData = (PgpLiteralData)message;
                using (Stream unc = literalData.GetInputStream())
                {
                    output = IoHelper.GetString(unc);
                }

            }
            else if (message is PgpLiteralData)
            {
                PgpLiteralData literalData = (PgpLiteralData)message;
                using (Stream unc = literalData.GetInputStream())
                {
                    output = IoHelper.GetString(unc);
                }
            }
            else if (message is PgpOnePassSignatureList)
            {
                throw new PgpException("Encrypted message contains a signed message - not literal data.");
            }
            else
            {
                throw new PgpException("Message is not a simple encrypted file - type unknown.");
            }

            return output;
        }

        private PgpPrivateKey FindSecretKey(PgpSecretKeyRingBundle pgpSec, long keyId, char[] pass)
        {
            PgpSecretKey pgpSecKey = pgpSec.GetSecretKey(keyId);
            if (pgpSecKey == null)
            {
                return null;
            }

            return pgpSecKey.ExtractPrivateKey(pass);
        }

    }
}
