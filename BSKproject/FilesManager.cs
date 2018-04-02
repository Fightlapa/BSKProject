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

        static public void WriteToXml(XmlWriter output, AES aes)
        {
            output.WriteStartElement("Header");

            output.WriteElementString("KeySize", aes.keySize.ToString());
            output.WriteElementString("BlockSize", aes.blockSize.ToString());
            output.WriteElementString("CipherMode", aes.cipherMode.ToString());

            string iVConverted = string.Join(".", new List<byte>(aes.iV).ConvertAll(i => ((int)i).ToString()).ToArray());

            output.WriteElementString("IV", iVConverted);

            output.WriteStartElement("Users");
            foreach (User user in aes.recipentsList)
            {
                user.WriteToXml(output, aes.key);
            }

            output.WriteEndElement();
            output.WriteEndElement();
        }

        static public AES FromXml(XmlReader reader, AES aes)
        {
            reader.ReadToFollowing("KeySize");

            aes.keySize = reader.ReadElementContentAsInt();

            aes.blockSize = reader.ReadElementContentAsInt();

            aes.cipherMode = AES.CipherFromString(reader.ReadElementContentAsString());

            aes.iV = ByteArrayFromString(reader.ReadElementContentAsString());


            do
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "User")
                {
                    var user = User.FromXml(reader);
                    aes.recipentsList.Add(user);
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
