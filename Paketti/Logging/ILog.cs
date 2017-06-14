using System;

namespace Paketti.Logging
{
    public interface ILog
    {
        IDisposable LogStep(string msg);
    }
}