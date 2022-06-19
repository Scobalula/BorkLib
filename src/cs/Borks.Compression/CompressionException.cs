
namespace Borks.Compression
{
    /// <summary>
    /// The exception that is thrown when a compression error occurs.
    /// </summary>
    public class CompressionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompressionException"/> class.
        /// </summary>
        public CompressionException() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompressionException"/> class.
        /// </summary>
        /// <param name="message">The error that has occured.</param>
        public CompressionException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompressionException"/> class.
        /// </summary>
        /// <param name="message">The error that has occured.</param>
        /// <param name="inner">The exception that is the cause of the current exception.</param>
        public CompressionException(string message, Exception inner) : base(message, inner) { }
    }
}
