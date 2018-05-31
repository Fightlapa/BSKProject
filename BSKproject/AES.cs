using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BSKproject
{
    class AES
    {
        public CipherMode cipherMode;
        public byte[] key;
        public byte[] iV;
        //hardcore 128 for now
        public int blockSize = 128;
        public int keySize = 32;
        public List<User> recipentsList = new List<User>();

        public CryptoStream EncrypteStream(Stream stream)
        {
            AesManaged myAes = new AesManaged();
            myAes.Mode = cipherMode;
            myAes.IV = iV;
            myAes.Key = this.key;

            ICryptoTransform encryptor = myAes.CreateEncryptor();

            return new CryptoStream(stream, encryptor, CryptoStreamMode.Read);
        }

        public CryptoStream EncrypteStream(Stream stream, byte[] password)
        {
            AesManaged myAes = new AesManaged();
            myAes.Mode = cipherMode;
            myAes.IV = new byte[16];
            myAes.Key = password;
            myAes.BlockSize = this.blockSize;

            ICryptoTransform encryptor = myAes.CreateEncryptor();

            return new CryptoStream(stream, encryptor, CryptoStreamMode.Write);
        }

        public CryptoStream DecrypteStream(Stream stream, string login, string keyPharse)
        {
            AesManaged myAes = new AesManaged();
            myAes.Mode = cipherMode;
            myAes.IV = iV;
            var user = (from u in recipentsList
                        where u.Name.Equals(login)
                        select u).FirstOrDefault();
            if (user == null)
            {
                MessageBox.Show("User not in recipents!", "Error", MessageBoxButton.OK);
                return null;
            }
            byte[] aesKey = user.LoadKey(keyPharse);
            if (aesKey == null)
            {
                return null;
            }
            myAes.Key = aesKey;

            ICryptoTransform decryptor = myAes.CreateDecryptor();

            return new CryptoStream(stream, decryptor, CryptoStreamMode.Read);
        }

        public CryptoStream DecrypteStream(Stream stream, byte[] password)
        {
            AesManaged myAes = new AesManaged();
            myAes.Mode = cipherMode;
            myAes.IV = new byte[16];
            myAes.Key = password;

            ICryptoTransform decryptor = myAes.CreateDecryptor();

            return new CryptoStream(stream, decryptor, CryptoStreamMode.Read);
        }


        public static CipherMode CipherFromString(string str)
        {
            switch (str)
            {
                case "CBC":
                    return CipherMode.CBC;
                case "ECB":
                    return CipherMode.ECB;
                case "OFB":
                    return CipherMode.OFB;
                case "CFB":
                    return CipherMode.CFB;
            }
            return 0;
        }

        public bool addUser(string name)
        {
            string userPath = Path.Combine(Constants.KEY_FOLDER_PATH, Constants.PUBLIC_KEY_FOLDER, name);

            if (Directory.Exists(userPath) && this.key != null)
            {
                var user = new User(name);
                user.StoreKey(this.key);
                recipentsList.Add(user);

                return true;
            }
            return false;
        }

    }
}
