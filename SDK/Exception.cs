namespace ActiveCollabSDK.SDK
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Exception" />
    abstract class Exception : System.Exception
    {
        /// <param name="code">string</param>
        public Exception(string message) : base(message)
        {
        }
    }
}
