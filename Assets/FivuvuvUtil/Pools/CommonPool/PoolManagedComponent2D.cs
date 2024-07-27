using UnityEngine;
using System;

namespace FivuvuvUtil.CommonPool
{
    /// <summary>被对象池管理的2D物体类,用来被需要管理的物体继承的，最好再写一个基类</summary>
    /// <typeparam name="ArgsType">参数类型</typeparam>
    public class PoolManagedComponent2D<ArgsType> : MonoBehaviour, IPoolManaged<ArgsType>
        where ArgsType : PoolArgs
    {
        public Action OnRemoveFromPool { get; set; }

        protected Rigidbody2D m_rb2D;

        protected virtual void Awake()
        {
            if (m_rb2D == null)
            {
                m_rb2D = GetComponent<Rigidbody2D>();
            }
        }

        protected virtual void Start()
        {

        }

        public virtual void InitPool(ArgsType args)
        {
            transform.position = args.initialPos;
            transform.rotation = args.initialRotation;
            if (m_rb2D == null)
            {
                m_rb2D = GetComponent<Rigidbody2D>();
            }
            if (m_rb2D != null)
            {
                m_rb2D.bodyType = RigidbodyType2D.Dynamic;
                m_rb2D.velocity = args.initialVelocity2D;
            }
            gameObject.SetActive(true);
        }

        public virtual void RemoveFromPool()
        {
            transform.position = new Vector3(9999, 9999);
            transform.rotation = Quaternion.identity;
            if (m_rb2D != null && m_rb2D.bodyType != RigidbodyType2D.Static)
            {
                m_rb2D.velocity = Vector2.zero;
                m_rb2D.bodyType = RigidbodyType2D.Static;
            }
            gameObject.SetActive(false);
            OnRemoveFromPool?.Invoke();
        }

        protected virtual void OnDestroy()
        {
            OnRemoveFromPool = null;
        }
    }
}
