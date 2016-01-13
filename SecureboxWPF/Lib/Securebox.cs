using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using RestSharp;
using System.Web;
using System.Management;

namespace SecureboxWPF
{
    public class Securebox
    {
        

        public static string secureboxExtension = ".box";
        public static void DeleteCurrentFile(string inputFile)
        {
            File.Delete(inputFile);
        }

        public static String GetVersion()
        {
            return "0.0.1";
        }

        private static void _getUpdate(String updateURL = "http://securebox.io/registration-desktop/")
        {
            //open browser to our site
            Process.Start(updateURL);
        }

        public static void CheckIfNeedUpdate()
        {
            Securebox._CheckIfNeedUpdate();
        }

        private static String GetUniqueSystemIdentifier()
        {
            var search = new ManagementObjectSearcher("SELECT * FROM Win32_baseboard");
            var mobos = search.Get();
            var motherboardSerial ="";
            foreach (var m in mobos)
            {
                motherboardSerial = m["SerialNumber"].ToString(); // ProcessorID if you use Win32_CPU
            }
            String path = Path.GetPathRoot(Environment.SystemDirectory);
            String drive = path.Split(':')[0];
            ManagementObject dsk = new ManagementObject(@"win32_logicaldisk.deviceid=""" + drive + @":""");
            dsk.Get();
            string volumeSerial = dsk["VolumeSerialNumber"].ToString();
           
            string cpuInfo = string.Empty;
            ManagementClass mc = new ManagementClass("win32_processor");
            ManagementObjectCollection moc = mc.GetInstances();

            foreach (ManagementObject mo in moc)
            {
                if (cpuInfo == "")
                {
                    //Get only the first CPU's ID
                    cpuInfo = mo.Properties["processorID"].Value.ToString();
                    break;
                }
            }

            return cpuInfo + motherboardSerial + volumeSerial;
        }

        private static void _CheckIfNeedUpdate()
        {
            String serverAddress = "http://192.168.1.161:8000/";
            //String serverAddress = "https://securebox.io/";
            var client = new RestClient(serverAddress);
            var request = new RestRequest("check-update/", Method.POST);
            var uniqueId = Securebox.GetUniqueSystemIdentifier();
            var currentSecureboxVersion = Securebox.GetVersion();
            request.AddParameter("os", "windows");
            request.AddParameter("uuid", uniqueId);
            request.AddParameter("current_version", currentSecureboxVersion);
            string osVersion = Environment.OSVersion.ToString();
            request.AddParameter("os_version", osVersion); 
            IRestResponse<MyToken> response = client.Execute<MyToken>(request);

            try
            {
                //var token = response.Content.ToString();
                RestSharp.Deserializers.JsonDeserializer deserial = new RestSharp.Deserializers.JsonDeserializer();
                MyToken token = deserial.Deserialize<MyToken>(response);  
                if ( token != null )
                {
                    String updateURL = token.Url;
                    if( currentSecureboxVersion != token.Version )
                    {
                        Securebox._getUpdate(serverAddress + updateURL);
                    }
                    
                }
                else
                {
                    throw new HttpException(404, "Are you sure you're in the right place?");
                }
            }
            catch(Exception ex)
            {
                Trace.TraceError(ex.Message);
            }
        }

        private static void _PostProcessEncryptFileMethod(string inputFile, string outputFile, Guid GUID)
        {
            byte[] newByte = GUID.ToByteArray();
            int numberOfBytes = newByte.Length;
            using (var newFile = new FileStream(outputFile, FileMode.CreateNew, FileAccess.Write))
            {
                for (var i = 0; i < numberOfBytes; i++)
                {
                    newFile.WriteByte(newByte[i]);
                }
                using (var oldFile = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
                {
                    oldFile.CopyTo(newFile);
                }
            }
        }

        public static Boolean PostProcessEncryptFile(string inputFile, string outputFile, Guid GUID)
        {
            Boolean isCancelEncryptionProcess = false;
            isCancelEncryptionProcess = Securebox._PostProcessEncryptFile(inputFile, outputFile, GUID);
            return isCancelEncryptionProcess;
        }

        private static Boolean _PostProcessEncryptFile(string inputFile, string outputFile, Guid GUID)
        {
            Boolean isCancelEncryptionProcess = false;
            if (File.Exists(outputFile))
            {
                DialogResult dialogResult = System.Windows.Forms.MessageBox.Show("Would you like to overwrite an existing file?\nSelecting 'No' will append the current file name with a - and a numeral for the current file.\nSelecting 'Cancel' will skip all remaining files as well as this file", "Overwrite confirmation", MessageBoxButtons.YesNoCancel);
                if (dialogResult == DialogResult.Yes)
                {
                    Securebox.DeleteCurrentFile(outputFile);
                    Securebox._PostProcessEncryptFileMethod(inputFile, outputFile, GUID);
                }
                isCancelEncryptionProcess = dialogResult == DialogResult.Cancel;               
            }
            else
            {
                Securebox._PostProcessEncryptFileMethod(inputFile, outputFile, GUID);
            }
            Securebox.DeleteCurrentFile(inputFile);
            return isCancelEncryptionProcess;
        }
        public static void DecryptPreprocess(string inputFile)
        {
            Securebox._DecryptPreprocess(inputFile);
        }
        private static void _DecryptPreprocess(string inputFile)
        {
            string outputFile = inputFile + ".tmp";

            int numberOfBytes = 16;
            byte[] bytes = new byte[numberOfBytes];
            //get GUID from file
            using(var fs = new FileStream( inputFile, FileMode.Open, FileAccess.Read))
            {
                fs.Read(bytes, 0, bytes.Length);    
                
            }
           // var buffer2 = File.ReadAllBytes(inputFile);
            Guid guid = new Guid(bytes);
            string GUID = guid.ToString();
            //create new file without GUID

            char[] delimiterChars = { '.' };
            string[] newFile = outputFile.Split(delimiterChars);
            string newFileFullPathName = String.Join(".", newFile.Take(newFile.Length - 2));

            using(var fs = new FileStream( inputFile, FileMode.Open, FileAccess.Read))
            {
                byte[] fullfile = new byte[fs.Length];
                int toRead = (int)fs.Length - 1, bytesRead;
                fs.Seek(16, SeekOrigin.Begin);
                while (toRead > 0 && (bytesRead = fs.Read(fullfile, 0, toRead)) > 0)
                {
                    toRead -= bytesRead;
                    numberOfBytes += bytesRead;
                }
                using (var newFS = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
                {
                    newFS.Write(fullfile, 0, fullfile.Length);
                    
                }


            }
            //get IV and key from SQLite DB
            SecureboxKey el = DatabaseManagement.GetSecureboxKey(GUID);
            if (el != null
                && el.IV != null
                && el.IV.Length > 0)
            {

                Securebox._DecryptFile(outputFile, newFileFullPathName, el);
            }
            Securebox.DeleteCurrentFile(outputFile);
        }

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        public static void EncryptFile(string inputFile, string outputFile, Guid GUID)
        {
           Securebox._EncryptFile(inputFile, outputFile, GUID);
        }

        public static void DecryptFile(string inputFile)
        {
            Securebox._DecryptPreprocess(inputFile);
        }

        private static void _EncryptFile(string inputFile, string outputFile, Guid GUID)
        {
            try
            {
                using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
                {

                     /* This is for demostrating purposes only. 
                     * Ideally you will want the IV key to be different from your key and you should always generate a new one for each encryption in other to achieve maximum security*/
                    byte[] IV = GUID.ToByteArray();

                    byte[] key = GUID.ToByteArray();



                    DatabaseManagement.InsertKey(GUID.ToString(), IV, key);

                    using (FileStream fsCrypt = new FileStream(outputFile, FileMode.Create))
                    {
                        using (ICryptoTransform encryptor = aes.CreateEncryptor(key, IV))
                        {
                            using (CryptoStream cs = new CryptoStream(fsCrypt, encryptor, CryptoStreamMode.Write))
                            {
                                using (FileStream fsIn = new FileStream(inputFile, FileMode.Open))
                                {
                                    int data;
                                    while ((data = fsIn.ReadByte()) != -1)
                                    {
                                        cs.WriteByte((byte)data);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            Securebox.DeleteCurrentFile(inputFile);
        }

        private static void _DecryptFile(string inputFile, string outputFile, SecureboxKey skey)
        {
            try
            {
                using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
                {
                    // byte[] key = ASCIIEncoding.UTF8.GetBytes(skey.KEY);
                    byte[] key = skey.KEY;
                    /* This is for demostrating purposes only. 
                     * Ideally you will want the IV key to be different from your key and you should always generate a new one for each encryption in other to achieve maximum security*/
                    byte[] IV = skey.IV;

                    using (FileStream fsCrypt = new FileStream(inputFile, FileMode.Open))
                    {
                        using (FileStream fsOut = new FileStream(outputFile, FileMode.Create))
                        {
                            using (ICryptoTransform decryptor = aes.CreateDecryptor(key, IV))
                            {
                                using (CryptoStream cs = new CryptoStream(fsCrypt, decryptor, CryptoStreamMode.Read))
                                {
                                    int data;
                                    while ((data = cs.ReadByte()) != -1)
                                    {
                                        fsOut.WriteByte((byte)data);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }
        }

      

        public static void CenterWindowOnScreen(Window wnd)
        {
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            double windowWidth = wnd.Width;
            double windowHeight = wnd.Height;
            wnd.Left = (screenWidth / 2) - (windowWidth / 2);
            wnd.Top = (screenHeight / 2) - (windowHeight / 2);
        }

        bool invalid = false;

        public bool IsValidEmail(string strIn)
        {
            invalid = false;
            if (String.IsNullOrEmpty(strIn))
                return false;

            // Use IdnMapping class to convert Unicode domain names. 
            try
            {
                strIn = Regex.Replace(strIn, @"(@)(.+)$", this.DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }

            if (invalid)
                return false;

            // Return true if strIn is in valid e-mail format. 
            try
            {
                return Regex.IsMatch(strIn,
                      @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                      @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                      RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        private string DomainMapper(Match match)
        {
            // IdnMapping class with default property values.
            IdnMapping idn = new IdnMapping();

            string domainName = match.Groups[2].Value;
            try
            {
                domainName = idn.GetAscii(domainName);
            }
            catch (ArgumentException)
            {
                invalid = true;
            }
            return match.Groups[1].Value + domainName;
        }
    }
}
