using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Xml;
using System.Threading;
using Microsoft.Win32;

namespace BSKproject
{
    public partial class MainWindow : Window
    {
        AES aes;
        List<string> users = new List<string>();
        List<string> pendingRecipents = new List<string>();
        string outputPath = "";
        string inputPath = "";
        string loggedUser = "";

        static Random random = new Random();

        public MainWindow()
        {
            InitializeComponent();
            aes = new AES();
            aes.cipherMode = CipherMode.CBC;
            aes.blockSize = 64;
            radioButton_CBC.IsChecked = true;
            blockSize64.IsChecked = true;
            if (File.Exists(Path.Combine(Constants.KEY_FOLDER_PATH, Constants.PUBLIC_KEY_FOLDER)))
            {
                string[] directories = Directory.GetDirectories(Path.Combine(Constants.KEY_FOLDER_PATH, Constants.PUBLIC_KEY_FOLDER));

                foreach (var directory in directories)
                {
                    userList.Items.Add(Path.GetFileName(directory));
                    users.Add(Path.GetFileName(directory));
                }
            }
        }


        private void CBC_Checked(object sender, RoutedEventArgs e)
        {
            aes.cipherMode = CipherMode.CBC;
        }

        private void ECB_Checked(object sender, RoutedEventArgs e)
        {
            aes.cipherMode = CipherMode.ECB;
        }

        private void CFB_Checked(object sender, RoutedEventArgs e)
        {
            aes.cipherMode = CipherMode.CFB;
        }

        private void OFB_Checked(object sender, RoutedEventArgs e)
        {
            aes.cipherMode = CipherMode.OFB;
        }

        private void MenuItem_Open_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                inputPath = dlg.FileName;
                InputFileLabel.Content = inputPath;
            }
        }

        private void MenuItem_Choose_Click(object sender, RoutedEventArgs e)
        {
            // Configure save file dialog box
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            //dlg.FileName = "Document"; // Default file name
            //dlg.DefaultExt = ".text"; // Default file extension
            //dlg.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                outputPath = dlg.FileName;
                OutputFileLabel.Content = outputPath;
            }
        }

        public static byte[] ByteArrayFromString(string str)
        {
            var octets = str.Split('.');
            byte[] t = new byte[octets.Length];
            int i = 0;
            foreach (var octet in octets)
            {
                byte.TryParse(octet, out t[i++]);
            }
            return t;
        }


        private async void Cypher_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string errors = "";
                if (inputPath == "")
                    errors += "No input file specified\n";

                if (outputPath == "")
                    errors += "No output file specified\n";

                if (aes.cipherMode == 0)
                    errors += "No cipher mode specified\n";

                if (pendingRecipents == null || pendingRecipents.Count == 0)
                    errors += "No Recipient was assigned\n";

                if (errors != "")
                {
                    MessageBox.Show(errors, "Error", MessageBoxButton.OK);
                    return;
                }

                //INITIALIZE

                FileInfo info = new FileInfo(Path.GetTempFileName());
                int randomValue;

                byte[] buffer = new byte[4];
                random.NextBytes(buffer);
                randomValue = BitConverter.ToInt32(buffer,0);

                string basePassword = String.Format("{0}{1}", DateTime.Now, randomValue);

                byte[] salt1 = new byte[8];
                using (RNGCryptoServiceProvider rngCsp = new
                RNGCryptoServiceProvider())
                {
                    // Fill the array with a random value.
                    rngCsp.GetBytes(salt1);
                }
                const int Iterations = 400;
                var keyGenerator = new Rfc2898DeriveBytes(basePassword, salt1, Iterations);
                aes.key = keyGenerator.GetBytes(aes.keySize);

                keyGenerator = new Rfc2898DeriveBytes(basePassword.Reverse().ToString(), salt1, Iterations);
                buffer = new byte[16];
                random.NextBytes(buffer);
                aes.iV = buffer;

                // END OF INITIALIZE

                foreach (string nickname in pendingRecipents)
                {
                    aes.addUser(nickname);
                }



                using (var output = File.Open(outputPath, FileMode.Create))
                {

                    using (var xmlOutput = XmlWriter.Create(output))
                    {
                        xmlOutput.WriteStartDocument();

                        FilesManager.WriteToXml(xmlOutput, aes);

                        xmlOutput.Flush();

                        xmlOutput.WriteEndDocument();
                    }
                    using (var source = File.Open(inputPath, FileMode.Open))
                    {
                        using (var sourceEncypted = aes.EncrypteStream(source))
                        {
                            Task copyTask = sourceEncypted.CopyToAsync(output);

                            //new Thread(() => StartUpdatingprogress(source, copyTask)).Start();
                            long length = source.Length;
                            do
                            {
                                double position = (double)source.Position;
                                pbStatus.Value = (position / length) * 100;
                            }
                            while (!copyTask.IsCompleted);
                            pbStatus.Value = 100;

                            await copyTask;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
            }
        }


        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SubmitUser_Click(object sender, RoutedEventArgs e)
        {

            using (MD5 md5Hash = MD5.Create())
            {
                byte[] keyPharseHash = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(Password.Text));
                RSA.GenerateKey(Login.Text, keyPharseHash);
            }
            userList.Items.Add(Login.Text);
            users.Add(Login.Text);
        }

        private async void Decypher_Click(object sender, RoutedEventArgs e)
        {
            string errors = "";
            if (inputPath == "")
                errors += "No input file specified\n";

            if (outputPath == "")
                errors += "No output file specified\n";

            if (loggedUser == "")
                errors += "No logged user\n";

            if (errors != "")
            {
                MessageBox.Show(errors, "Error", MessageBoxButton.OK);
                return;
            }

            string tempFile = System.IO.Path.GetTempFileName();

            using (var input = File.OpenRead(inputPath))
            {
                using (var reader = XmlReader.Create(input))
                {
                    aes = FilesManager.FromXml(reader, aes);

                    using (var output = File.OpenWrite(tempFile))
                    {
                        input.Position = (reader as IXmlLineInfo).LinePosition + "</Header>".Length;//Set proper position beacues xmlReader loads to much

                        Task copyTask = input.CopyToAsync(output);

                        //new Thread(() => StartUpdatingprogress(source, copyTask)).Start();
                        long length = input.Length;
                        do
                        {
                            double position = (double)input.Position;
                            pbStatus.Value = (position / length) * 100;
                        }
                        while (!copyTask.IsCompleted);
                        pbStatus.Value = 100;

                        copyTask.Wait();
                    }
                }
            }
                
            using (var input = File.OpenRead(tempFile))
            {
                using (var decryptedStream = aes.DecrypteStream(input, loggedUser, LogInPassword.Text))
                {
                    if (decryptedStream == null)
                        return;
                    using (var output = File.OpenWrite(outputPath))
                    {
                        Task copyTask = decryptedStream.CopyToAsync(output);

                        long length = input.Length;
                        do
                        {
                            double position = (double)input.Position;
                            pbStatus.Value = (position / length) * 100;
                        }
                        while (!copyTask.IsCompleted);
                        pbStatus.Value = 100;

                        await copyTask;
                    }
                }
            }

        }

        private void LogIn_Click(object sender, RoutedEventArgs e)
        {
            loggedUser = LoginNickname.Text;
            LoggedUserLabel.Content = loggedUser;
        }

        private void blockSize64_Checked(object sender, RoutedEventArgs e)
        {
                aes.blockSize = Int32.Parse((string)blockSize64.Content);
        }

        private void blockSize128_Checked(object sender, RoutedEventArgs e)
        {
            aes.blockSize = Int32.Parse((string)blockSize128.Content);
        }

        private void blockSize256_Checked(object sender, RoutedEventArgs e)
        {
            aes.blockSize = Int32.Parse((string)blockSize256.Content);
        }

        private void removeRecipent_Click(object sender, RoutedEventArgs e)
        {
            pendingRecipents.Remove((string)userList.SelectedItem);
            recipentListBox.Items.Remove((string)userList.SelectedItem);
        }

        private void addRecipent_Click(object sender, RoutedEventArgs e)
        {
            foreach (string user in users)
            {
                if (user == (string)userList.SelectedItem)
                {
                    pendingRecipents.Add(user);
                    recipentListBox.Items.Add(Login.Text);
                }
            }

        }

    }
}
