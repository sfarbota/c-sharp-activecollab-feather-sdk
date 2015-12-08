namespace ActiveCollabSDK.SDK.Exceptions
{
    class WrapperNotInitialized : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WrapperNotInitialized" /> class.
        /// </summary>
        /// <param name="message"></param>
        public WrapperNotInitialized(string message = null) : base(GetMessage(message))
        {
        }

        /// <summary>Gets the message.</summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        private static string GetMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                message = "API wrapper is not initialized. Please set proper API URL and key";
            }

            return message;
        }
    }
}
