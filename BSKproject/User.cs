using System;
using System.Collections.Generic;
using System.Xml;
using System.Security.Cryptography;
using System.Text;

namespace BSKproject
{
     class User
    {
        private string name;
        public byte[] key;

        public string Name
        {
            get
            {
                return name;
            }
        }

        private User()
        {
        }

        public User(string name)
        {
            this.name = name;
        }

        public void WriteToXml(XmlWriter output, byte[] key)
        {
            output.WriteStartElement("User");

            output.WriteElementString("Username", name);

            key = RSA.Encrypte(key,name);
            string keyConverted = string.Join(".", new List<byte>(key).ConvertAll(i => ((int)i).ToString()).ToArray());
            output.WriteElementString("Key", keyConverted);

            output.WriteEndElement();
        }

        public static User FromXml(XmlReader input)
        {
            var user = new User();

            input.ReadToFollowing("Username");
            user.name = input.ReadElementContentAsString();

            user.key = MainWindow.ByteArrayFromString(input.ReadElementContentAsString());

            return user;
        }

        public void StoreKey(byte[] key)
        {
            this.key = RSA.Encrypte(key, this.name);
        }

        public byte[] LoadKey(string keyPhrase)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] keyPharseHash = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(keyPhrase));
                return RSA.Decrypte(this.key, this.name, keyPharseHash);
            }
        }

    }
}