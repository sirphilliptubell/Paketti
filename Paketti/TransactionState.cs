using System;
using System.Collections.Generic;
using System.Text;

namespace Paketti
{
    public static class TransactionState
    {
        public static TransactionState<T> New<T>(T state)
            => new TransactionState<T>(state);
    }

    public class TransactionState<T>
    {
        private T _oldState;
        private T _newState;
        private bool _useNewState;

        public T State
            => _useNewState
            ? _newState
            : _oldState;

        public TransactionState(T state)
        {
            _oldState = state;
            _useNewState = false;
        }

        public void Update(T newState)
        {
            _newState = newState;
            _useNewState = true;
        }

        public void Rollback()
        {
            _useNewState = false;
            _newState = default(T);
        }

        public void Commit()
        {
            _oldState = _newState;
            _useNewState = false;
            _newState = default(T);
        }
    }
}