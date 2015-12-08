namespace ActiveCollabSDK.SDK.Exceptions
{
    class IssueTokenException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IssueTokenException" /> class.
        /// </summary>
        /// <param name="code">string</param>
        public IssueTokenException(string code) : base(GetMessage(code))
        {
        }

        /// <summary>Gets the message.</summary>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        private static string GetMessage(string code)
        {
            switch (code)
            {
                case "1":
                    return "Client details not set";
                case "2":
                    return "Unknown user";
                case "3":
                    return "Invalid Password";
                case "4":
                    return "Not allowed for given User and their System Role";
                default:
                    return "Unknown error";
            }
        }
    }
}
