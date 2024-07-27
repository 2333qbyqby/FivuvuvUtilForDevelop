using System;

namespace FivuvuvUtil.Common
{
    public class CustomUnRegister<T> : IUnRegister where T : new()
    {
        private MyEvent<T> _myEvent;
        private Action<T> _myAction;

        public CustomUnRegister(MyEvent<T> myEvent, Action<T> e)
        {
            _myEvent = myEvent;
            _myAction = e;
        }

        public void UnRegister()
        {
            _myEvent.UnRegister(_myAction);
        }
    }
}
