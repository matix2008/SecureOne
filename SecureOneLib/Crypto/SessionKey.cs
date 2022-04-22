using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace SecureOneLib.Crypto
{
    public class SessionKey
    {
        public static readonly UInt32 _magic = 0xABBAFEDE;

        byte[] buffer = null;

        public SessionKey(byte[] iv, byte[] key)
        {
            if (iv == null)
                throw new ArgumentNullException("iv");
            if (key == null)
                throw new ArgumentNullException("key");

            buffer = new byte[iv.Length + key.Length + sizeof(Int32) * 2 + sizeof(UInt32)];

            byte[] magic = BitConverter.GetBytes(_magic);
            byte[] iv_len = BitConverter.GetBytes(iv.Length);
            byte[] key_len = BitConverter.GetBytes(key.Length);

            int ofsset = 0;
            magic.CopyTo(buffer, ofsset);
            ofsset += sizeof(UInt32);

            iv_len.CopyTo(buffer, ofsset);
            ofsset += sizeof(Int32);

            iv.CopyTo(buffer, ofsset);
            ofsset += iv.Length;

            key_len.CopyTo(buffer, ofsset);
            ofsset += sizeof(Int32);

            key.CopyTo(buffer, ofsset);

            IV = iv;
            Key = key;
            StreamTail = null;
        }

        public SessionKey(Stream inputStream)
        {
            if (inputStream == null)
                throw new ArgumentNullException("outputStream");

            byte[] magic = new byte[sizeof(UInt32)];
            inputStream.Read(magic, 0, sizeof(UInt32));

            if( BitConverter.ToUInt32(magic,0) != _magic )
                throw new SOInvalidFormatException("Invalid stream format. Unknown magic value.");

            byte[] buffer = new byte[sizeof(Int32)];
            inputStream.Read(buffer, 0, sizeof(Int32));
            int iv_len = BitConverter.ToInt32(buffer, 0);

            byte[] iv = new byte[iv_len];
            inputStream.Read(iv, 0, iv_len);

            inputStream.Read(buffer, 0, sizeof(Int32));
            int key_len = BitConverter.ToInt32(buffer, 0);


            byte[] key = new byte[key_len];
            inputStream.Read(key, 0, key_len);

            IV = iv;
            Key = key;

            MemoryStream output = new MemoryStream();
            inputStream.CopyTo(output);
            StreamTail = output;
            StreamTail.Position = 0;
        }

        public byte[] IV { get;  }
        public byte[] Key { get; }
        public Stream StreamTail { get;  }

        public Stream CopyTo(Stream outputFileStream)
        {
            MemoryStream sessionKeyStream = new MemoryStream();
            sessionKeyStream.Write(buffer, 0, buffer.Length);
            outputFileStream.CopyTo(sessionKeyStream);
            sessionKeyStream.Position = 0;
            return sessionKeyStream;
        }
    }
}
