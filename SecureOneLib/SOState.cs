using System;

namespace SecureOneLib
{
    /// <summary>
    /// Реализует состояние криптографической операции
    /// </summary>
    public class SOState
    {
        public enum CryptoState { Unknown, Start, Pending, InProgress, Completed, Error, Cancel };

        public SOState()
        {
            State = CryptoState.Unknown;
            Message = String.Empty;
            Exception = null;
        }

        public SOState(CryptoState state, string message, Exception ex = null)
        {
            State = state;
            Message = message;
            Exception = ex;
        }
        public CryptoState State { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }

        public static SOState Create(CryptoState state, string message, Exception ex = null)
        {
            return new SOState(state, message, ex);
        }
    }
}
