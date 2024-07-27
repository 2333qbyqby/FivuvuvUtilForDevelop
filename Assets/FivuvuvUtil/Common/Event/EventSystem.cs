using System;
using System.Collections.Generic;
using UnityEngine;

namespace FivuvuvUtil.Common
{
    public class EventSystem_QF
    {
        public static Dictionary<Type, IEvent> EventDict = new Dictionary<Type, IEvent>();
        
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