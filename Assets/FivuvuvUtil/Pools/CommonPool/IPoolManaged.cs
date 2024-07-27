using FivuvuvUtil.CommonPool;
using System;
namespace FivuvuvUtil.CommonPool
{
    /// <summary>������ع���ĳض���ӿ�</summary>
    public interface IPoolManaged<ArgsType> where ArgsType : PoolArgs
    {
        /// <summary>���ؽ�����ǰ���г�ʼ��</summary>
        void InitPool(ArgsType args);
        /// <summary>�Ƴ��ص�</summary>
        Action OnRemoveFromPool { get; set; }
        /// <summary>�ӳ�������ʧ</summary>
        void RemoveFromPool();
    }

}