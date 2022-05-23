using System;

namespace SecureOneLib
{
    /// <summary>
    /// Реализует базовое исключение
    /// </summary>
    [Serializable]
    public class SecureOneBaseException : Exception
    {
        public SecureOneBaseException() { }

        public SecureOneBaseException(string message)
            : base(message) { }

        public SecureOneBaseException(string message, Exception inner)
            : base(message, inner) { }
    }
    /// <summary>
    /// Реализует исключение - ошибка поиска сертификата
    /// </summary>
    [Serializable]
    public class SOCertificateNotFoundException : SecureOneBaseException
    {
        public SOCertificateNotFoundException() { }

        public SOCertificateNotFoundException(string message)
            : base(message) { }

        public SOCertificateNotFoundException(string message, Exception inner)
            : base(message, inner) { }
    }
    /// <summary>
    /// Реализует исключение - ошибка формата данных
    /// </summary>
    [Serializable]
    public class SOInvalidFormatException : SecureOneBaseException
    {
        public SOInvalidFormatException() { }

        public SOInvalidFormatException(string message)
            : base(message) { }

        public SOInvalidFormatException(string message, Exception inner)
            : base(message, inner) { }
    }
    /// <summary>
    /// Реализует исключение - ошибка криптографической операции
    /// </summary>
    [Serializable]
    public class SOCryptographicException : SecureOneBaseException
    {
        public SOCryptographicException() { }

        public SOCryptographicException(string message)
            : base(message) { }

        public SOCryptographicException(string message, Exception inner)
            : base(message, inner) { }
    }
}
