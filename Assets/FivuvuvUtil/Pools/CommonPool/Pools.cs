using System.Collections.Generic;
using UnityEngine;
using System;

namespace FivuvuvUtil.CommonPool
{
    /// <summary>初始化参数</summary>
    public class PoolArgs
    {
        #region 变量定义

        /// <summary>初始位置</summary>
        public Vector3 initialPos;
        /// <summary>初始旋转</summary>
        public Quaternion initialRotation;
        /// <summary>初始速度(3D)</summary>
        public Vector3 initialVelocity;
        /// <summary>初始速度(2D)</summary>
        public Vector2 initialVelocity2D;

        #endregion

        #region 构造函数

        public PoolArgs()
        {
            initialPos = Vector3.zero;
            initialRotation = Quaternion.identity;
            initialVelocity = Vector3.zero;
            initialVelocity2D = Vector2.zero;
        }

        public PoolArgs(Vector3 pos)
        {
            initialPos = pos;
            initialRotation = Quaternion.identity;
            initialVelocity = Vector3.zero;
            initialVelocity2D = Vector2.zero;
        }

        public PoolArgs(Vector3 pos, Vector3 initialSpeed)
        {
            initialPos = pos;
            initialRotation = Quaternion.identity;
            initialVelocity = initialSpeed;
            initialVelocity2D = Vector2.zero;
        }

        public PoolArgs(Vector3 pos, Vector2 initialSpeed2D)
        {
            initialPos = pos;
            initialRotation = Quaternion.identity;
            initialVelocity = Vector3.zero;
            initialVelocity2D = initialSpeed2D;
        }

        public PoolArgs(Vector3 pos, Quaternion rotation)
        {
            initialPos = pos;
            initialRotation = rotation;
            initialVelocity = Vector3.zero;
            initialVelocity2D = Vector2.zero;
        }

        public PoolArgs(Vector3 pos, Quaternion rotation, Vector3 initialSpeed)
        {
            initialPos = pos;
            initialRotation = rotation;
            initialVelocity = initialSpeed;
            initialVelocity2D = Vector2.zero;
        }

        public PoolArgs(Vector3 pos, Quaternion rotation, Vector2 initialSpeed2D)
        {
            initialPos = pos;
            initialRotation = rotation;
            initialVelocity = Vector3.zero;
            initialVelocity2D = initialSpeed2D;
        }

        #endregion
    }

    /// <summary>被对象池管理的池对象接口</summary>
    public interface IPoolManaged<ArgsType> where ArgsType : PoolArgs
    {
        /// <summary>加载进场景前进行初始化</summary>
        void Init_Pool(ArgsType args);
        /// <summary>移除回调</summary>
        Action OnRemove_Pool { get; set; }
        /// <summary>从场景中消失</summary>
        void Remove_Pool();
    }

    /// <summary>对象池,继承这个</summary>
    /// <typeparam name="ObjectType">池对象类型</typeparam>
    /// <typeparam name="ArgsType">参数类型</typeparam>
    public class Pool<ObjectType, ArgsType> : MonoBehaviour
        where ObjectType : MonoBehaviour, IPoolManaged<ArgsType>
        where ArgsType : PoolArgs, new()
    {
        #region 变量定义

        [Header("池对象预制体")]
        [SerializeField]
        private List<GameObject> prefabs;

        /// <summary>原形字典</summary>
        private Dictionary<string, GameObject> prototypes;
        /// <summary>队列字典</summary>
        private Dictionary<string, Queue<ObjectType>> queues;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            prototypes = new Dictionary<string, GameObject>();
            queues = new Dictionary<string, Queue<ObjectType>>();
            foreach (GameObject t in prefabs)
            {

                AddNewPrototype(t);
            }
        }

        #endregion

        #region 编辑模式接口(不建议在运行时调用)

        /// <summary>添加新的池对象预制体(不建议在运行时调用)</summary>
        /// <param name="prefab">预制体</param>
        /// <returns>是否成功添加</returns>
        public bool AddNewPrefab(GameObject prefab)
        {
            if (prefab == null)
            {
                Debug.LogWarning("向对象池" + gameObject.name + "添加的预制体为空");
                return false;
            }
            string name = prefab.name;
            if (prefab.GetComponent<ObjectType>() == null)
            {
                Debug.LogWarning("向对象池" + gameObject.name + "添加的名为" + name + "的预制体不含" + typeof(ObjectType).Name + "组件");
                return false;
            }
            int index = prefabs.FindIndex((GameObject item) => { return item.name == name; });
            if (index < 0)
            {
                prefabs.Add(prefab);
                return true;
            }
            else
            {
                Debug.LogWarning("向对象池" + gameObject.name + "已经存在名为" + name + "的预制体");
                return false;
            }
        }

        #endregion

        #region 运行时管理接口

        /// <summary>添加新的池对象模板(不建议在外部调用此函数)</summary>
        /// <param name="prototype">模板</param>
        /// <param name="name">名称</param>
        /// <returns>是否成功添加</returns>
        public bool AddNewPrototype(GameObject prototype, string name = null)
        {
            if (prototypes == null || queues == null)
            {
                Debug.LogWarning(gameObject.name + "的对象池尚未初始化，禁止在此时添加原型");
                return false;
            }
            if (name == null)
            {
                name = prototype.name;
            }
            if (prototype.GetComponent<ObjectType>() == null)
            {
                Debug.LogWarning(gameObject.name + "的对象池中名为" + name + "的原型不含" + typeof(ObjectType).Name + "组件");
                return false;
            }
            if (prototypes == null)
            {
                return true;
            }
            if (prototypes.ContainsKey(name))
            {
                Debug.LogWarning(gameObject.name + "的对象池中名为" + name + "的原型已存在");
                return false;
            }
            else
            {
                prototypes.Add(name, prototype);
                queues.Add(name, new Queue<ObjectType>());
                return true;
            }
        }

        #endregion

        #region 对象生成接口

        /// <summary>生成指定物体</summary>
        /// <param name="name">物体名称</param>
        /// <param name="args">初始化参数</param>
        /// <returns>物体</returns>
        public ObjectType Instantiate(string name, ArgsType args = null)
        {
            Queue<ObjectType> itemQueue;
            bool nameExist = queues.TryGetValue(name, out itemQueue);
            if (!nameExist)
            {
                Debug.LogError("对象池中名为" + name + "的物体不存在");
                return null;
            }
            else
            {
                ObjectType newItem;
                if (itemQueue.Count > 0)
                {
                    newItem = itemQueue.Dequeue();
                }
                else
                {
                    newItem = GameObject.Instantiate(prototypes[name], args.initialPos, args.initialRotation).GetComponent<ObjectType>();
                    newItem.OnRemove_Pool += () => { itemQueue.Enqueue(newItem); };
                }
                if (args == null)
                {
                    args = new ArgsType();
                }
                newItem.Init_Pool(args);
                return newItem;
            }
        }

        /// <summary>生成指定物体，并获取指定组件</summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <param name="name">物体名称</param>
        /// <param name="args">初始化参数</param>
        /// <returns>物体组件</returns>
        public T InstantiateWithComponent<T>(string name, ArgsType args = null)
        {
            return Instantiate(name, args).GetComponent<T>();
        }

        #endregion
    }
}