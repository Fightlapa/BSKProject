using System;
using System.Collections.Generic;
using System.Xml;

namespace BSKproject
{
    internal class User
    {
        /// <summary>
        /// Username.
        /// </summary>
        private string name;
        /// <summary>
        /// Encrypted AES key by user RSA public key.
        /// </summary>
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

        #region Methods
        /// <summary>
        /// Writes user form xml file.
        /// </summary>
        /// <param name="output">Opened xml file</param>
        public void WriteToXml(XmlWriter output, byte[] key)
        {
            output.WriteStartElement("User");

            output.WriteElementString("Username", name);

            key = RSA.Encrypte(key,name);
            string keyConverted = string.Join(".", new List<byte>(key).ConvertAll(i => ((int)i).ToString()).ToArray());
            output.WriteElementString("Key", keyConverted);

            output.WriteEndElement();
        }

        /// <summary>
        /// Load config for user from xml file.
        /// </summary>
        /// <param name="input">Opened xml file</param>
        /// <returns></returns>
        public static User FromXml(XmlReader input)
        {
            var user = new User();

            input.ReadToFollowing("Username");
            user.name = input.ReadElementContentAsString();

            user.key = MainWindow.ByteArrayFromString(input.ReadElementContentAsString());

            return user;
        }

        /// <summary>
        /// Store key with encryption of public key
        /// </summary>
        /// <param name="key"></param>
        internal void StoreKey(byte[] key)
        {
            this.key = RSA.Encrypte(key, this.name);
        }



        /// <summary>
        /// TODO
        /// Shows the dialog for pharse input.
        /// </summary>
        /// <returns></returns>
        internal byte[] LoadKey(string keyPharse)
        {
            return RSA.Decrypte(this.key, this.name, keyPharse);
        }


        #endregion
    }
}