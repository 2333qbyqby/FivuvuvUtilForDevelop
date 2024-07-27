using System;

namespace FivuvuvUtil.Common
{
    public class MyEvent<T> : IEvent where T : new()
    {
        private Action<T> _myEvent;

        public IUnRegister Register(Action<T> e)
        {
            _myEvent += e;
            return new CustomUnRegister<T>(this, e);
        }

        public void UnRegister(Action<T> e)
        {
            _myEvent -= e;
        }

        public void Trigger()
        {
            _myEvent?.Invoke(new T());
        }

        public void Trigger(T t)
        {
            _myEvent?.Invoke(t);
        }
    }
}
