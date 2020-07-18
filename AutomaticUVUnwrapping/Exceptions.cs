using System;
using System.Runtime.Serialization;

namespace AutomaticUVUnwrapping
{
    [Serializable()]
    class FindInverseMatrixException : Exception
    {
        public FindInverseMatrixException() : base()
        {
        }

        public FindInverseMatrixException(string message) : base(message)
        {
        }

        public FindInverseMatrixException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected FindInverseMatrixException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
