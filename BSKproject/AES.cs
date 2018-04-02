using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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

        public CryptoStream DecrypteStream(Stream stream, string userName, string keyPharse)
        {
            AesManaged myAes = new AesManaged();
            myAes.Mode = cipherMode;
            myAes.IV = iV;
            var user = (from u in recipentsList
                        where u.Name.Equals(userName)
                        select u).FirstOrDefault();
            myAes.Key = user.LoadKey(keyPharse);

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
