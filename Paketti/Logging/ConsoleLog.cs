using System;

namespace Paketti.Logging
{
    public class ConsoleLog : ILog
    {
        private int depth = 0;

        public IDisposable LogStep(string msg)
        {
            Console.Write(msg.PadLeft(msg.Length + depth));
            depth += 1;
            return new LogOnDispose(Decrement);
        }

        private void Decrement()
            => depth -= 1;

        private class LogOnDispose :
            IDisposable
        {
            private readonly Action _onDispose;
            private readonly DateTime _start;

            public LogOnDispose(Action onDispose)
            {
                _start = DateTime.Now;
                _onDispose = onDispose;
            }

            public void Dispose()
            {
                var time = (DateTime.Now - _start).ToString();
                Console.SetCursorPosition(Console.WindowWidth - 17, Console.CursorTop);
                Console.WriteLine(time);
                _onDispose();
            }
        }
    }
}