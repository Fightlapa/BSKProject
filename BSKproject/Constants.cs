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
        #region Const
        /// <summary>
        /// Folder for keys.
        /// </summary>
        public static readonly string KEY_FOLDER_PATH = Directory.GetCurrentDirectory();
        /// <summary>
        /// Subfolder for private keys.
        /// </summary>
        public const string PRIVATE_KEY_FOLDER = "private";
        /// <summary>
        /// SubFolder for public keys.
        /// </summary>
        public const string PUBLIC_KEY_FOLDER = "public";
        /// <summary>
        /// 
        /// </summary>
        public const string PRIVATE_KEY_FILENAME = "PrivateKey.txt";
        /// <summary>
        /// 
        /// </summary>
        public const string PUBLIC_KEY_FILENAME = "PublicKey.txt";
        /// <summary>
        /// Salt for genereting byte of Array.
        /// </summary>
        public static readonly byte[] SALT = new byte[] { 12, 20, 30, 45, 50, 60, 70, 80 };
        #endregion
    }
}
