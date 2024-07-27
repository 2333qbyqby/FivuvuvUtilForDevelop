using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace FivuvuvUtil.CommonFSM
{
    /// <summary>��Ϊ</summary>
    public abstract class MonoAction
    {
        /// <summary>��״̬���ϣ�����һ����Ϊ�ж����״̬���Ա��ڸ���</summary>
        internal List<State> ParentStates { get; private set; }
        /// <summary>ִ������</summary>
        public Func<bool> condition;
        /// <summary>��һ��״̬����Ϊnull��ָ���״̬����״̬�����򱣳�ԭ��</summary>
        public State nextState;
        ///<summary>�Ƿ�ʹ�ô�����
        ///��������Tick������Ч����Ҫͨ������Trigger�����ֶ�����,��֮��Tick������ЧTrigger������Ч��</summary>
        public bool useTrigger;
        /// <summary>����Ϊ�Ƿ����������Ϊ����Ҫ���ڴ��ContinuousAct��</summary>
        public bool interrupt;
        /// <summary>����Ϊ�Ƿ�����������Ϊִ�У���Ҫ��ContinuousAct��ʹ�ã�</summary>
        public bool block;

        public event Action onTriggered;

        /// <summary>
        /// ִ��һ����Ϊ�����鲻Ҫֱ�ӵ��ã�������Update�����е���״̬����Tick������
        /// </summary>
        /// <returns>�Ƿ�ɹ�ִ��</returns>
        public virtual bool Tick()
        {
            if (useTrigger)
            {
                return false;
            }
            else if (condition == null || condition.Invoke())
            {
                DoAct();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>ִ����Ϊ</summary>
        protected abstract void DoAct();

        protected bool CheckState()
        {
            if (ParentStates == null)
            {
                return false;
            }
            else
            {
                foreach (State state in ParentStates)
                {
                    if (state.ParentFSM != null && state == state.ParentFSM.CurrentState)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>���ڴ�����</summary>
        public virtual bool Trigger()
        {
            if (useTrigger && (condition == null || condition.Invoke()) &&  // ����Ƿ����㴥������
                CheckState())  // ���״̬���ĵ�ǰ״̬�Ƿ�Ϊ��Ϊ�ĸ�״̬
            {
                if (onTriggered != null)
                    onTriggered.Invoke();
                DoAct();
                return true;
            }
            return false;
        }

        /// <summary>
        /// ������Ϊ
        /// </summary>
        /// <param name="useTrigger">�Ƿ�ʹ�ô�����
        /// ��������Tick������Ч����Ҫͨ������Trigger�����ֶ�����,��֮��Tick������ЧTrigger������Ч��</param>
        /// <param name="condition">��������</param>
        /// <param name="next">��һ��״̬����Ϊnull�򱣳�ԭ��</param>
        /// <param name="interrupt">����Ϊ�Ƿ����������Ϊ����Ҫ���ڴ��ContinuousAct��</param>
        /// <param name="block">����Ϊ�Ƿ�����������Ϊִ�У���Ҫ��ContinuousAct��ʹ�ã�</param>
        public MonoAction(bool useTrigger = false, Func<bool> condition = null, State next = null,
            bool interrupt = false, bool block = false)
        {
            ParentStates = new List<State>();
            this.condition = condition;
            this.nextState = next;
            this.useTrigger = useTrigger;
            this.interrupt = interrupt;
            this.block = block;
        }

        /// <summary>
        /// ���ø�״̬
        /// </summary>
        /// <param name="parents">��״̬����</param>
        public void SetParentState(State[] parents)
        {
            ParentStates = new List<State>(parents);
            if (parents == null)
            {
                Debug.LogWarning("����Ϊ�ĸ�״̬����Ϊ�˿գ�����Ϊ���޷���ִ��");
                return;
            }
            foreach (State state in parents)
            {
                state.AddMonoAction(this);
            }
        }

        /// <summary>
        /// ��Ӹ�״̬
        /// </summary>
        /// <param name="parent">��״̬</param>
        public void AddParentState(State parent)
        {
            if (ParentStates == null)
            {
                ParentStates = new List<State>();
            }
            ParentStates.Add(parent);
            parent.AddMonoAction(this);
        }
    }

    /// <summary>��׼��Ϊ</summary>
    public class StandardAction : MonoAction
    {
        /// <summary>������Ϊ����</summary>
        public Action act;

        protected override void DoAct()
        {
            if (act != null)
            {
                act.Invoke();
            }
        }

        /// <summary>������׼��Ϊ</summary>
        /// <param name="act">������Ϊ����</param>
        /// <param name="useTrigger">�Ƿ�ʹ�ô�����
        /// ��������Tick������Ч����Ҫͨ������Trigger�����ֶ�����,��֮��Tick������ЧTrigger������Ч��</param>
        /// <param name="condition">��������</param>
        /// <param name="next">��һ��״̬����Ϊnull�򱣳�ԭ��</param>
        /// <param name="interrupt">����Ϊ�Ƿ����������Ϊ����Ҫ���ڴ��ContinuousAct��</param>
        /// <param name="block">����Ϊ�Ƿ�����������Ϊִ�У���Ҫ��ContinuousAct��ʹ�ã�</param>
        public StandardAction(Action act = null, bool useTrigger = false, Func<bool> condition = null,
            State next = null, bool interrupt = false, bool block = false) :
            base(useTrigger, condition, next, interrupt, block)
        {
            this.act = act;
        }
    }

    /// <summary>������Ϊ</summary>
    public class CorountineAction : MonoAction
    {
        /// <summary>Э�̺���</summary>
        public Func<IEnumerator> ienumGenerator;

        /// <summary>Э���Ƿ���</summary>
        private bool onCoroutine;
        /// <summary>Э�̵�����</summary>
        public IEnumerator enumerator;

        private bool reusable;
        private float lastStop;

        public event Action<bool> onCoroutineStateChanged;
        public bool OnCourountine => onCoroutine;

        public void DoCoroutine()
        {
            if (!onCoroutine)
                return;
            if (!reusable)
            {
                if (enumerator != null && enumerator.Current is float)
                {
                    float seconds = (float)enumerator.Current;
                    if (lastStop < 0)
                    {
                        lastStop = Time.time;
                        return;
                    }
                    else if (Time.time - lastStop < seconds)
                    {
                        return;
                    }
                }
                else if (enumerator != null && enumerator.Current is IEnumerator)
                {
                    enumerator = enumerator.Current as IEnumerator;

                }
                lastStop = -1;
            }
            if (!enumerator.MoveNext())
            {
                Stop();
            }
        }

        /// <summary>ִ��һ����Ϊ��������ֱ�ӵ��ã�������Update�����е���״̬����Tick������</summary>
        /// <returns>�Ƿ�ִ�гɹ�</returns>
        public override bool Tick()
        {
            if (onCoroutine)
                return false;
            else
                return base.Tick();
        }

        public void Stop()
        {
            onCoroutine = false;
            if (onCoroutineStateChanged != null)
                onCoroutineStateChanged.Invoke(false);
        }

        protected override void DoAct()
        {
            if (ienumGenerator != null && !reusable)
            {
                enumerator = ienumGenerator.Invoke();
            }
            else if (enumerator != null && reusable)
                enumerator.Reset();
            else
                return;
            onCoroutine = true;
            if (onCoroutineStateChanged != null)
                onCoroutineStateChanged.Invoke(true);
        }

        public override bool Trigger()
        {
            return !onCoroutine ? base.Trigger() : false;
        }

        public CorountineAction(Func<IEnumerator> ienumGenerator = null, bool useTrigger = false, Func<bool> condition = null,
            State next = null, bool interrupt = false, bool block = false) :
            base(useTrigger, condition, next, interrupt, block)
        {
            this.ienumGenerator = ienumGenerator;
            onCoroutine = false;
            this.enumerator = null;
            reusable = false;
            lastStop = -1;
        }

        public CorountineAction(IEnumerator enumerator, bool useTrigger = false, Func<bool> condition = null,
            State next = null, bool interrupt = false, bool block = false) :
            base(useTrigger, condition, next, interrupt, block)
        {
            this.ienumGenerator = null;
            onCoroutine = false;
            this.enumerator = enumerator;
            reusable = true;
            lastStop = -1;
        }
    }
}
