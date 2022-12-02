using System;
using UniRx;
using UnityEngine;

namespace MyGame.Utils
{

    public abstract class StatefulScriptableObjectValueBase<T> : ScriptableObjectValueBase<T>, IObservable<T>
    {
        private ReactiveProperty<T> _innerDataSource = new ReactiveProperty<T>();
        public override void SetValue(T value)
        {
            base.SetValue(value);
            _innerDataSource.Value = value;
        }
        public IDisposable Subscribe(IObserver<T> observer) => _innerDataSource.Subscribe(observer);
    }


    public abstract class ScriptableObjectValueBase<T> : ScriptableObject, IAwaitable<T>
    {
        public T CurrentValue { get; private set; }
        public Action<T> OnNewValue;

        public virtual void SetValue(T value)
        {
            CurrentValue = value;
            OnNewValue?.Invoke(value);
        }

        public IAwaiter<T> GetAwaiter()
        {
            return new NewValueNotifier<T>(this);
        }
    }

    public class NewValueNotifier<TAwaited> : AwaiterBase<TAwaited>
    {
        private readonly ScriptableObjectValueBase<TAwaited> _scriptableObjectValueBase;

        public NewValueNotifier(ScriptableObjectValueBase<TAwaited> scriptableObjectValueBase)
        {
            _scriptableObjectValueBase = scriptableObjectValueBase;
            _scriptableObjectValueBase.OnNewValue += ONNewValue;
        }

        private void ONNewValue(TAwaited obj)
        {
            _scriptableObjectValueBase.OnNewValue -= ONNewValue;
            ONWaitFinish(obj);
        }
    }
}