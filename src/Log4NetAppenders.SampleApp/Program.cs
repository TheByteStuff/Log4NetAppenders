using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.xml", Watch = true)]
namespace Log4NetAppenders.SampleApp
{
    /*
     * This program demonstrates file logging using the Appenders in this solution
     * using a log4net.xml.
     * 
     * The reference may be useful to see how the PublicKeyInline for the EncryptedForwardingAppender
     * is configured.
     * 
     * The tests defined in the UnitTest package can be referenced for examples
     * of dynamically creating the Loggers.
     * 
     */
    class Program
    {
        static void Main(string[] args)
        {
            //RemoteSysLogAppender();

            EncryptedAppender();

            Thread.Sleep(TimeSpan.FromSeconds(16));
        }


        private static void EncryptedAppender()
        {
            Console.WriteLine("EncryptedAppender SampleApp.");

            log4net.Config.XmlConfigurator.Configure();

            Exception ex = new Exception("Test encrypted exception.");

            /*
            ILog logger = LogManager.GetLogger("ApplicationEncrypted");
            logger.Debug("This is an encryption debug test.");
            logger.Debug("This is an encryption debug test with exception.", ex);
            /*/
            ILog logger = LogManager.GetLogger("ApplicationEncrypted2");
            logger.Debug("his is an encryption debug test using PublicKeyFileNameAndPath.");
            //*/
        }


        private static void RemoteSysLogAppender()
        {
            Console.WriteLine("RemoteSysLogSSLAppender SampleApp.");

            log4net.Config.XmlConfigurator.Configure();

            /*
             * Application the is RemoteSyslogSSLAppender, 
             * Application2 is the RemoteSyslogAsyncSSLAppender
             */
            /*
            ILog logger = LogManager.GetLogger("Application");
            logger.Debug("This is a debug test.");
            /*/
            ILog logger = LogManager.GetLogger("Application2");
            logger.Debug("This is a debug test.");
            //*/
        }
    }
}
