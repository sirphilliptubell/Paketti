using System;

namespace Paketti.Logging
{
    /// <summary>
    /// Represents a log.
    /// </summary>
    public interface ILog
    {
        /// <summary>
        /// Logs that some process has begun.
        /// </summary>
        /// <param name="msg">The message.</param>
        /// <returns></returns>
        IDisposable LogStep(string msg);
    }
}