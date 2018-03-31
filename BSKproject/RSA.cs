using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BSKproject
{
    class RSA
    {

        public static byte[] Encrypte(byte[] data, string username)
        {
            var rsa = new RSACryptoServiceProvider();

            string publicPath = Path.Combine(Constants.KEY_FOLDER_PATH, Constants.PUBLIC_KEY_FOLDER, username);
            string publicKeyFile = Path.Combine(publicPath, Constants.PUBLIC_KEY_FILENAME);

            using (var reader = new StreamReader(publicKeyFile))
            {
                string input = reader.ReadToEnd();
                rsa.FromXmlString(input);
            }

            return rsa.Encrypt(data, true);
        }

        public static byte[] Decrypte(byte[] key, string username, string keyPharse)
        {
            var rsa = new RSACryptoServiceProvider();

            string privatePath = Path.Combine(Constants.KEY_FOLDER_PATH, Constants.PRIVATE_KEY_FOLDER, username);
            string privateKeyFile = Path.Combine(privatePath, Constants.PRIVATE_KEY_FILENAME);

            using (var reader = new StreamReader(privateKeyFile))
            {
                string input = reader.ReadToEnd();
                rsa.FromXmlString(input);
            }

            return rsa.Decrypt(key, true);

        }

        public static void GenerateKey(string username, string keyPharse)
        {
            User user = new User(username);
            var rsa = new RSACryptoServiceProvider();
            string publicPath = Path.Combine(Constants.KEY_FOLDER_PATH, Constants.PUBLIC_KEY_FOLDER, username);
            string privatePath = Path.Combine(Constants.KEY_FOLDER_PATH, Constants.PRIVATE_KEY_FOLDER, username);

            string publicKeyFile = Path.Combine(publicPath, Constants.PUBLIC_KEY_FILENAME);
            string privateKeyFile = Path.Combine(privatePath, Constants.PRIVATE_KEY_FILENAME);

            if (Directory.Exists(publicPath))
            {
                Directory.Delete(publicPath, true);
            }

            if (Directory.Exists(privatePath))
            {
                Directory.Delete(privatePath, true);
            }

            Directory.CreateDirectory(publicPath);
            Directory.CreateDirectory(privatePath);

            using (var file = new StreamWriter(publicKeyFile))
            {
                string publicKey = rsa.ToXmlString(false);
                file.Write(publicKey);
            }

            using (var file = new StreamWriter(privateKeyFile))
            {
                string privateKey = rsa.ToXmlString(true);
                file.Write(privateKey);
            }
        }
    }
}
