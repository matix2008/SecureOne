using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureOneLib
{
    [Serializable]
    public class SecureOneBaseException : Exception
    {
        public SecureOneBaseException() { }

        public SecureOneBaseException(string message)
            : base(message) { }

        public SecureOneBaseException(string message, Exception inner)
            : base(message, inner) { }
    }

    [Serializable]
    public class SOCertificateNotFoundException : SecureOneBaseException
    {
        public SOCertificateNotFoundException() { }

        public SOCertificateNotFoundException(string message)
            : base(message) { }

        public SOCertificateNotFoundException(string message, Exception inner)
            : base(message, inner) { }
    }

    [Serializable]
    public class SOInvalidFormatException : SecureOneBaseException
    {
        public SOInvalidFormatException() { }

        public SOInvalidFormatException(string message)
            : base(message) { }

        public SOInvalidFormatException(string message, Exception inner)
            : base(message, inner) { }
    }
}
