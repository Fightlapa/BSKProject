using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static BSKproject.MainWindow;

namespace BSKproject
{
    class FilesManager
    {

        static public void WriteToXml(XmlWriter output, int keySize, int blockSize, CipherMode cipherMode, byte[] iV, List<User> users)
        {
            output.WriteStartElement("Header");

            output.WriteElementString("KeySize", keySize.ToString());
            output.WriteElementString("BlockSize", blockSize.ToString());
            output.WriteElementString("CipherMode", cipherMode.ToString());

            string iVConverted = string.Join(".", new List<byte>(iV).ConvertAll(i => ((int)i).ToString()).ToArray());

            output.WriteElementString("IV", iVConverted);

            output.WriteStartElement("Users");
            foreach (User user in users)
            {
                user.WriteToXml(output);
            }

            output.WriteEndElement();
            output.WriteEndElement();
        }

        public AES FromXml(XmlReader reader, int keySize, int blockSize, CipherMode cipherMode, byte[] iV, List<User> users)
        {
            AES aes = new AES();
            reader.ReadToFollowing("KeySize");
            keySize = reader.ReadElementContentAsInt();

            blockSize = reader.ReadElementContentAsInt();

            aes = new AES();

            aes.cipherMode = AES.CipherFromString(reader.ReadElementContentAsString());

            aes.iV = ByteArrayFromString(reader.ReadElementContentAsString());


            do
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "User")
                {
                    var user = User.FromXml(reader);
                    users.Add(user);
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Header")
                {
                    break;
                }

            }
            while (reader.Read());
            return aes;
        }
    }
}
