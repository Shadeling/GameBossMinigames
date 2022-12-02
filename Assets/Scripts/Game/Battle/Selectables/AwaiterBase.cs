
using System.Runtime.CompilerServices;
using System;


namespace MyGame.Utils
{

    public abstract class AwaiterBase<TAwaited> : IAwaiter<TAwaited>
    {
        private Action _continuation;
        private bool _isCompleted;
        private TAwaited _result;

        public bool IsCompleted => _isCompleted;

        public TAwaited GetResult() => _result;

        public void OnCompleted(Action continuation)
        {
            if (_isCompleted)
            {
                continuation?.Invoke();
            }
            else
            {
                _continuation = continuation;
            }
        }

        protected void ONWaitFinish(TAwaited result)
        {
            _result = result;
            _isCompleted = true;
            _continuation?.Invoke();
        }
    }

    public interface IAwaitable<T>
    {
        IAwaiter<T> GetAwaiter();
    }

    public interface IAwaiter<TAwaited> : INotifyCompletion
    {
        bool IsCompleted { get; }
        TAwaited GetResult();
    }

}
