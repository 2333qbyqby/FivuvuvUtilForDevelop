using System.Collections.Generic;
using UnityEngine;
using System;

namespace FivuvuvUtil.CommonPool
{
    /// <summary>��ʼ������</summary>
    public class PoolArgs
    {
        #region ��������

        /// <summary>��ʼλ��</summary>
        public Vector3 initialPos;
        /// <summary>��ʼ��ת</summary>
        public Quaternion initialRotation;
        /// <summary>��ʼ�ٶ�(3D)</summary>
        public Vector3 initialVelocity;
        /// <summary>��ʼ�ٶ�(2D)</summary>
        public Vector2 initialVelocity2D;

        #endregion

        #region ���캯��

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

    /// <summary>������ع���ĳض���ӿ�</summary>
    public interface IPoolManaged<ArgsType> where ArgsType : PoolArgs
    {
        /// <summary>���ؽ�����ǰ���г�ʼ��</summary>
        void Init_Pool(ArgsType args);
        /// <summary>�Ƴ��ص�</summary>
        Action OnRemove_Pool { get; set; }
        /// <summary>�ӳ�������ʧ</summary>
        void Remove_Pool();
    }

    /// <summary>�����,�̳����</summary>
    /// <typeparam name="ObjectType">�ض�������</typeparam>
    /// <typeparam name="ArgsType">��������</typeparam>
    public class Pool<ObjectType, ArgsType> : MonoBehaviour
        where ObjectType : MonoBehaviour, IPoolManaged<ArgsType>
        where ArgsType : PoolArgs, new()
    {
        #region ��������

        [Header("�ض���Ԥ����")]
        [SerializeField]
        private List<GameObject> prefabs;

        /// <summary>ԭ���ֵ�</summary>
        private Dictionary<string, GameObject> prototypes;
        /// <summary>�����ֵ�</summary>
        private Dictionary<string, Queue<ObjectType>> queues;

        #endregion

        #region Unity��������

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

        #region �༭ģʽ�ӿ�(������������ʱ����)

        /// <summary>����µĳض���Ԥ����(������������ʱ����)</summary>
        /// <param name="prefab">Ԥ����</param>
        /// <returns>�Ƿ�ɹ����</returns>
        public bool AddNewPrefab(GameObject prefab)
        {
            if (prefab == null)
            {
                Debug.LogWarning("������" + gameObject.name + "��ӵ�Ԥ����Ϊ��");
                return false;
            }
            string name = prefab.name;
            if (prefab.GetComponent<ObjectType>() == null)
            {
                Debug.LogWarning("������" + gameObject.name + "��ӵ���Ϊ" + name + "��Ԥ���岻��" + typeof(ObjectType).Name + "���");
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
                Debug.LogWarning("������" + gameObject.name + "�Ѿ�������Ϊ" + name + "��Ԥ����");
                return false;
            }
        }

        #endregion

        #region ����ʱ����ӿ�

        /// <summary>����µĳض���ģ��(���������ⲿ���ô˺���)</summary>
        /// <param name="prototype">ģ��</param>
        /// <param name="name">����</param>
        /// <returns>�Ƿ�ɹ����</returns>
        public bool AddNewPrototype(GameObject prototype, string name = null)
        {
            if (prototypes == null || queues == null)
            {
                Debug.LogWarning(gameObject.name + "�Ķ������δ��ʼ������ֹ�ڴ�ʱ���ԭ��");
                return false;
            }
            if (name == null)
            {
                name = prototype.name;
            }
            if (prototype.GetComponent<ObjectType>() == null)
            {
                Debug.LogWarning(gameObject.name + "�Ķ��������Ϊ" + name + "��ԭ�Ͳ���" + typeof(ObjectType).Name + "���");
                return false;
            }
            if (prototypes == null)
            {
                return true;
            }
            if (prototypes.ContainsKey(name))
            {
                Debug.LogWarning(gameObject.name + "�Ķ��������Ϊ" + name + "��ԭ���Ѵ���");
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

        #region �������ɽӿ�

        /// <summary>����ָ������</summary>
        /// <param name="name">��������</param>
        /// <param name="args">��ʼ������</param>
        /// <returns>����</returns>
        public ObjectType Instantiate(string name, ArgsType args = null)
        {
            Queue<ObjectType> itemQueue;
            bool nameExist = queues.TryGetValue(name, out itemQueue);
            if (!nameExist)
            {
                Debug.LogError("���������Ϊ" + name + "�����岻����");
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

        /// <summary>����ָ�����壬����ȡָ�����</summary>
        /// <typeparam name="T">�������</typeparam>
        /// <param name="name">��������</param>
        /// <param name="args">��ʼ������</param>
        /// <returns>�������</returns>
        public T InstantiateWithComponent<T>(string name, ArgsType args = null)
        {
            return Instantiate(name, args).GetComponent<T>();
        }

        #endregion
    }
}