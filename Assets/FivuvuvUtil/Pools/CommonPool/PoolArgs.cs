using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace FivuvuvUtil.CommonPool
{
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
}
/// <summary>初始化参数</summary>