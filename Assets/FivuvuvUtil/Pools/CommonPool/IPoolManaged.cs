using FivuvuvUtil.CommonPool;
using System;
namespace FivuvuvUtil.CommonPool
{
    /// <summary>被对象池管理的池对象接口</summary>
    public interface IPoolManaged<ArgsType> where ArgsType : PoolArgs
    {
        /// <summary>加载进场景前进行初始化</summary>
        void InitPool(ArgsType args);
        /// <summary>移除回调</summary>
        Action OnRemoveFromPool { get; set; }
        /// <summary>从场景中消失</summary>
        void RemoveFromPool();
    }

}