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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region field
        AES aes;
        /// <summary>
        /// List of approved users.
        /// </summary>
        List<string> users = new List<string>();
        string outputPath;
        string inputPath;
        string loggedUser;

        static Random random = new Random();
        //
        // Summary:
        //     Specifies the block cipher mode to use for encryption.
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            aes = new AES();
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
            //dlg.FileName = "Document"; // Default file name
            //dlg.DefaultExt = ".txt"; // Default file extension
            //dlg.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                inputPath = dlg.FileName;
            }
        }

        private void MenuItem_Choose_Click(object sender, RoutedEventArgs e)
        {
            // Configure save file dialog box
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Document"; // Default file name
            dlg.DefaultExt = ".text"; // Default file extension
            dlg.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                outputPath = dlg.FileName;
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
                const int Iterations = 300;
                var keyGenerator = new Rfc2898DeriveBytes(basePassword, salt1, Iterations);
                aes.key = keyGenerator.GetBytes(aes.keySize);

                keyGenerator = new Rfc2898DeriveBytes(basePassword.Reverse().ToString(), salt1, Iterations);
                aes.iV = keyGenerator.GetBytes(16);



                if (aes.recipentsList == null || aes.recipentsList.Count == 0)
                {
                    MessageBox.Show("No Recipient was assigned", "Error", MessageBoxButton.OK);
                    return;
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
            RSA.GenerateKey(Login.Text, Password.Text);
            userList.Items.Add(Login.Text);
            users.Add(Login.Text);
        }

        private void addRecipent_Click(object sender, RoutedEventArgs e)
        {
            foreach (string user in users)
            {
                if (user == (string)userList.SelectedItem)
                {
                    aes.addUser(user);
                }
            }

        }

        private async void Decypher_Click(object sender, RoutedEventArgs e)
        {
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
                using (var decryptedStream = aes.DecrypteStream(input, loggedUser, "Test"))
                {
                    using (var output = File.OpenWrite(outputPath))
                    {
                        Task copyTask = decryptedStream.CopyToAsync(output);

                        //new Thread(() => StartUpdatingprogress(source, copyTask)).Start();
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
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem typeItem = (ComboBoxItem)comboBox.SelectedItem;
            aes.blockSize = Int32.Parse((string)typeItem.Content);
        }
    }
}
