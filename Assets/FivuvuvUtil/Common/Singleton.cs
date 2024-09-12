using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace FivuvuvUtil
{
    public class Singleton<T> where T : class, new()
    {
        protected Singleton() { }

        private static T instance = null;

        public static T Instance => instance ?? (instance = new T());

        public static void Clear()
        {
            instance = null;
        }
    }
}
