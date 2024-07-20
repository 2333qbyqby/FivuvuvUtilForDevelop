using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
namespace FivuvuvUtil
{
    abstract public class MonoSingleton<Type> : MonoBehaviour where Type : MonoBehaviour
    {
        [Header("该物体是否跨场景")]
        [SerializeField]
        protected bool permanent = false;
        protected bool destroyed = false;
        static private Type instance;
        GameObject permanentObject;
        /// <summary>
        /// 单例
        /// </summary>
        public static Type Instance
        {
            get
            {
                if (instance == null)
                {
                    (FindObjectOfType<Type>() as MonoSingleton<Type>)?.InitializeInstance();
                }
                return instance;
            }
            protected set
            {
                instance = value;
            }
        }

        private void InitializeInstance()
        {
            if (permanent)
            {
                permanentObject = gameObject;
                while (permanentObject.transform.parent != null)
                {
                    permanentObject = permanentObject.transform.parent.gameObject;
                }
            }
            if (instance == null)
            {
                instance = this as Type;
                if (permanent)
                {
                    DontDestroyOnLoad(permanentObject);
                }
            }
            else if (permanent && instance != this)
            {
                destroyed = true;
                if (permanentObject != null)
                {
                    Destroy(permanentObject);
                }
            }
        }

        protected virtual void Awake()
        {
            if (permanent)
            {
                permanentObject = gameObject;
                while (permanentObject.transform.parent != null)
                {
                    permanentObject = permanentObject.transform.parent.gameObject;
                }
            }
            if (instance == null)
            {
                instance = this as Type;
                if (permanent)
                {
                    DontDestroyOnLoad(permanentObject);
                }
            }
            else if (permanent && instance != this)
            {
                destroyed = true;
                if (permanentObject != null)
                {
                    Destroy(permanentObject);
                }
            }
        }
    }
}



