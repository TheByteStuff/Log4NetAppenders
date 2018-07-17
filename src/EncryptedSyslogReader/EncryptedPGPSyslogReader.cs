using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Security.Cryptography;
// using Bouncy Castle library: http://www.bouncycastle.org/csharp/
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace EncryptedSyslogReader
{
    public partial class EncryptedPGPSyslogReader : Form
    {

        private List<string> SyslogFile = new List<string>();

        private Boolean PGPPrivateKeySet = false;
        private Boolean PGPPassphraseSet = false;
        private Boolean SyslogFileRead = false;

        private Boolean configurationFileRead = false;
        private Boolean configurationModified = false;

        public EncryptedPGPSyslogReader()
        {
            InitializeComponent();
            SetMenuOptions();
        }

        private void SetMenuOptions()
        {
            menuOpenToolStripMenuItem.Enabled = PGPPassphraseSet && PGPPrivateKeySet;
            menuClearToolStripMenuItem.Enabled = SyslogFileRead;
            menuCloseToolStripMenuItem.Enabled = SyslogFileRead;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //TODO Prompt to save file if edited, exit
            Application.Exit();
        }

        private void lblConfigFile_Click(object sender, EventArgs e)
        {

        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openSyslogFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SyslogFile = ReadSyslogFile(openSyslogFileDialog.FileName);
                buildSyslogBox();
                txtLogFileName.Text = openSyslogFileDialog.FileName;

                SyslogFileRead = true;
                configurationFileRead = true;
                configurationModified = false;

                SetMenuOptions();
            }
        }

        private List<string> ReadSyslogFile(string fileNamepath)
        {
            try
            {
                List<string> SyslogFile = new List<string>();

                using (StreamReader sr = new StreamReader(fileNamepath))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        SyslogFile.Add(line);
                    }
                }

                return SyslogFile;
            }
            catch (Exception ex)
            {
                //LogHelper.GetInstance().WriteLogError(String.Format("Error reading history file {0}.", fileNamepath), ex);
                return null;
            }
        }

        private void buildSyslogBox()
        {
            txtDecryptedLog.Clear();
            txtRawSyslog.Clear();

            Boolean PGPBeginFlag = false;
            string PGPBeginMessage = "-----BEGIN PGP MESSAGE-----";

            foreach (var lineitem in SyslogFile)
            {
                txtRawSyslog.AppendText(lineitem + Environment.NewLine);

                if (PGPBeginFlag)
                {
                    PGPBeginFlag = false;
                    try
                    {
                        string data = DecryptPgpData(lineitem);
                        txtDecryptedLog.AppendText(data + Environment.NewLine);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error decrypting line in file." + Environment.NewLine + ex.Message, "Decrypt Error!!");
                        txtDecryptedLog.AppendText("Error Decrypting:" + lineitem + Environment.NewLine);
                    }
                    continue;
                }

                if (lineitem.Contains(PGPBeginMessage))
                {
                    PGPBeginFlag = true;
                    txtDecryptedLog.AppendText(lineitem.Replace(PGPBeginMessage, ""));
                    continue;
                }

                if (lineitem.Contains("-----END PGP MESSAGE-----"))
                {
                    continue;
                }

                txtDecryptedLog.AppendText(lineitem + Environment.NewLine);

                Console.WriteLine(lineitem);
                //parse
                //gridSyslogFile.Rows.Add("1", "2", "3", "4", "5");
            }
        }

        private void setPGPKeyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (setPrivateKeyFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtPrivateKeyFileNameAndPath.Text = setPrivateKeyFileDialog.FileName;
                PGPPrivateKeySet = true;
                menuOpenToolStripMenuItem.Enabled = PGPPassphraseSet && PGPPrivateKeySet;
            }
        }

        private void menuClearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            txtPrivateKeyFileNameAndPath.Text = string.Empty;
            txtLogFileName.Text = string.Empty;
            txtPassphrase.Text = string.Empty;

            PGPPrivateKeySet = false;
            PGPPassphraseSet = false;
            SyslogFileRead = false;

            SetMenuOptions();

            txtRawSyslog.Clear();
            txtDecryptedLog.Clear();
        }

        private void menuCloseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            txtRawSyslog.Clear();
            txtDecryptedLog.Clear();
            txtLogFileName.Text = string.Empty;
            SyslogFileRead = false;

            SetMenuOptions();
        }


        private void txtPassphrase_TextChanged(object sender, EventArgs e)
        {
            PGPPassphraseSet = true;
            menuOpenToolStripMenuItem.Enabled = PGPPassphraseSet && PGPPrivateKeySet;
        }


        //PGP Decryption Logic
        private string DecryptPgpData(string inputData)
        {
            string output;
            using (Stream inputStream = IoHelper.GetStream(inputData))
            {
                using (Stream keyIn = File.OpenRead(txtPrivateKeyFileNameAndPath.Text))
                {
                    output = DecryptPgpData(inputStream, keyIn, txtPassphrase.Text);
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

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

    }
}
