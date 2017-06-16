using System;
using System.Collections.Generic;

namespace Paketti.Logging
{
    public class ConsoleLog : ILog
    {
        private int _depth = 0;

        public IDisposable LogStep(string msg)
        {
            var top = Console.CursorTop;
            Console.WriteLine(msg.PadLeft(2 * _depth + msg.Length));
            _depth += 1;

            return new WriteOnDispose(top, Decrement);
        }

        private void Decrement()
            => _depth -= 1;

        private class WriteOnDispose : IDisposable
        {
            private readonly DateTime _start = DateTime.UtcNow;
            private readonly int _oldTop;
            private Action _onDispose;

            public WriteOnDispose(int top, Action onDispose)
            {
                _oldTop = top;
                _onDispose = onDispose;
            }

            public void Dispose()
            {
                var diff = (DateTime.UtcNow - _start).ToString();

                //backup
                var newLeft = Console.CursorLeft;
                var newTop = Console.CursorTop;

                //write higher up in the console if needed
                Console.CursorTop = _oldTop;
                Console.CursorLeft = Console.WindowWidth - 17;
                Console.WriteLine(diff);

                //restore
                Console.CursorLeft = newLeft;
                Console.CursorTop = newTop;

                _onDispose();
            }
        }
    }
}