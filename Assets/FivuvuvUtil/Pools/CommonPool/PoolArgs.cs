using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace FivuvuvUtil.CommonPool
{
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
}
/// <summary>��ʼ������</summary>