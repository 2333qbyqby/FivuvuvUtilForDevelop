using System;
using System.Collections.Generic;
using UnityEngine;

namespace FivuvuvUtil.common
{
    public class EventSystem_QF
    {
        public static Dictionary<Type, IEvent> EventDict = new Dictionary<Type, IEvent>();
        //
        public static void Send<T>() where T : new()
        {
            if (EventDict.TryGetValue(typeof(T), out var v))
            {
                var myEvent = v as MyEvent<T>;
                myEvent.Trigger();
            }
        }

        public static void Send<T>(T t) where T : new()
        {
            if (EventDict.TryGetValue(typeof(T), out var v))
            {
                var myEvent = v as MyEvent<T>;
                myEvent.Trigger(t);
            }
        }

        public static IUnRegister Register<T>(Action<T> e) where T : new()
        {
            if (EventDict.TryGetValue(typeof(T), out var v))
            {
                var myEvent = v as MyEvent<T>;
                return myEvent.Register(e);
            }
            else
            {
                var myEvent = new MyEvent<T>();
                var unRegister = myEvent.Register(e);
                EventDict.Add(typeof(T), myEvent);
                return unRegister;
            }
        }

        public static void UnRegister<T>(Action<T> e) where T : new()
        {
            if (EventDict.TryGetValue(typeof(T), out var v))
            {
                var myEvent = v as MyEvent<T>;
                myEvent.UnRegister(e);
            }
        }
    }

    public interface IEvent { }

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

    public interface IUnRegister
    {
        void UnRegister();
    }

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

    public class UnRegisterWhenDestroyTrigger : MonoBehaviour
    {
        private HashSet<IUnRegister> _unRegisterList = new HashSet<IUnRegister>();

        public void AddUnRegister(IUnRegister ur)
        {
            _unRegisterList.Add(ur);
        }

        public void RemoveUnRegister(IUnRegister ur)
        {
            _unRegisterList.Remove(ur);
        }

        private void OnDestroy()
        {
            foreach (var unRegister in _unRegisterList)
            {
                unRegister.UnRegister();
            }
            _unRegisterList.Clear();
        }
    }

    public static class UnRegisterExtension
    {
        public static void UnRegisterWhenGameObjectDestroy(this IUnRegister self, GameObject go)
        {
            var component = go.GetComponent<UnRegisterWhenDestroyTrigger>();
            if (component == null)
                component = go.AddComponent<UnRegisterWhenDestroyTrigger>();
            component.AddUnRegister(self);
        }

        public static void UnRegisterWhenGameObjectDestroy<T>(this IUnRegister unRegister, T component) where T : Component
        {
            unRegister.UnRegisterWhenGameObjectDestroy(component.gameObject);
        }

    }


}