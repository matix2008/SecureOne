using System;

namespace SecureOneLib
{
    /// <summary>
    /// Реализует состояние криптографической операции
    /// </summary>
    public class SOState
    {
        //public enum CryptoOpration { Unknown, Sign, Encrypt, SignEncrypt, Verify, VerifyDecrypt, Decrypt };
        public enum CryptoState { Unknown, Start, Pending, InProgress, Completed, Error, Cancel };

        public SOState()
        {
            //Operation = CryptoOpration.Unknown;
            State = CryptoState.Unknown;
            Message = String.Empty;
            Exception = null;
        }

        //public CryptoOpration Operation { get; set; }
        public CryptoState State { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }
    }
}
