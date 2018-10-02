# Log4NetAppenders

This project supplies Appenders for Apache log4net as well as some sample/reference applications.

Use of this software is free.  

Suggestions or donations are both appreciated and welcome can be made by using the "Contact" tab at https://www.thebytestuff.com/


RemoteSyslogSSLAppender and RemoteSyslogAsyncSSLAppender: Appenders specifically to allow remote SSL logging to a syslog running on a Synology NAS. This should work with any syslog conforming to the BSD specification.

EncryptedForwardingAppender: ForwardingAppender to allow encryption of logged messages (both the message text and exception text).  Current implementation uses PGP/GPG encryption.


Note: certificates and encryption keys are supplied in the code base for use with the unit tests and to provide a functioning example.  New entities should be created to use with your deployment.