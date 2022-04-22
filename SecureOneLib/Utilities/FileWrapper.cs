using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SecureOneLib.Utilities
{
    public class FileWrapper
    {
        public FileWrapper(string filepath)
        {
            if (filepath.Length == 0)
                throw new ArgumentException("Empty file name");

            FilePathString = filepath;
            FInfo = new FileInfo(filepath);
        }

        public string FilePathString { get; }

        public FileStream OpenRead()
        {
            return File.OpenRead(FilePathString);
        }

        public FileInfo FInfo { get; }

        /// <summary>
        /// Возвращает строку представляющую файл
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return FilePathString;
        }
    }

    public class PackageWrapper : FileWrapper
    {
        public PackageWrapper(string filepath)
            : base(filepath)
        {
            using (FileStream fs = File.OpenRead(filepath))
            {
                byte[] magic = new byte[sizeof(UInt32)];
                fs.Read(magic, 0, sizeof(UInt32));

                if (BitConverter.ToUInt32(magic, 0) != Crypto.SessionKey._magic)
                    throw new SOInvalidFormatException("Invalid stream format. Unknown magic value.");
            }
        }

    }
}
