namespace ActiveCollabSDK.SDK.Exceptions
{
    class FileNotReadable : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileNotReadable" /> class.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="message">The message.</param>
        public FileNotReadable(string path, string message = null) : base(GetMessage(path))
        {
        }

        /// <summary>Gets the message.</summary>
        /// <param name="path">The path.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        private static string GetMessage(string path, string message = null)
        {
            if (string.IsNullOrEmpty(message))
            {
                message = "File " + path + " is not readable";
            }

            return message;
        }
    }
}
