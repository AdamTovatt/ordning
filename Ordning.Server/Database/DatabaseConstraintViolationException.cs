namespace Ordning.Server.Database
{
    /// <summary>
    /// Exception thrown when a database constraint violation occurs.
    /// This exception provides user-friendly error messages that can be displayed in the frontend.
    /// </summary>
    public class DatabaseConstraintViolationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseConstraintViolationException"/> class.
        /// </summary>
        /// <param name="message">The user-friendly error message.</param>
        public DatabaseConstraintViolationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseConstraintViolationException"/> class.
        /// </summary>
        /// <param name="message">The user-friendly error message.</param>
        /// <param name="innerException">The inner exception that caused this exception.</param>
        public DatabaseConstraintViolationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
