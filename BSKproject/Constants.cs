using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSKproject
{
    class Constants
    {
        public static readonly string KEY_FOLDER_PATH = Directory.GetCurrentDirectory();
        public const string PRIVATE_KEY_FOLDER = "Private";
        public const string PUBLIC_KEY_FOLDER = "Public";
        public const string PRIVATE_KEY_FILENAME = "PrivateKey.txt";
        public const string PUBLIC_KEY_FILENAME = "PublicKey.txt";
    }
}
