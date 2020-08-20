using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Log4NetAppenders")]
[assembly: AssemblyDescription("Appenders for Apache log4net\r\n\r\nRemoteSyslogSSLAppender and RemoteSyslogAsyncSSLAppender: Appenders specifically to allow remote SSL logging to a syslog running on a Synology NAS. This should work with any syslog conforming to the BSD specification.\r\n\r\nEncryptedForwardingAppender: ForwardingAppender to allow encryption of logged messages (both the message text and exception text).  Current implementation uses PGP/GPG encryption.\r\n\t\t\r\nReference project on GitHub for SampleApp to write logs for these appenders as well as a sample application for reading an encrypted file log.\r\n\r\nUse of this software is free.  \r\n\r\nSuggestions or donations are both appreciated and welcome can be made by using the \"Contact\" tab at https://www.thebytestuff.com/")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("The Byte Stuff, LLC")]
[assembly: AssemblyProduct("Log4NetAppenders")]
[assembly: AssemblyCopyright("Copyright ©  2018")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("7efa7c68-107d-493e-b9ad-e4c2fd9f59a7")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("3.0.0.0")]
[assembly: AssemblyFileVersion("3.0.0.0")]
