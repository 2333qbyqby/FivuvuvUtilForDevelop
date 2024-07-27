using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace FivuvuvUtil.CommonFSM
{
    /// <summary>行为</summary>
    public abstract class MonoAction
    {
        /// <summary>父状态集合，允许一个行为有多个父状态，以便于复用</summary>
        internal List<State> ParentStates { get; private set; }
        /// <summary>执行条件</summary>
        public Func<bool> condition;
        /// <summary>下一个状态，若为null或指向的状态不在状态机中则保持原样</summary>
        public State nextState;
        ///<summary>是否使用触发器
        ///（若是则不Tick函数无效而需要通过调用Trigger函数手动触发,反之则Tick函数生效Trigger函数无效）</summary>
        public bool useTrigger;
        /// <summary>该行为是否会打断其他行为（主要用于打断ContinuousAct）</summary>
        public bool interrupt;
        /// <summary>该行为是否会阻断其他行为执行（主要由ContinuousAct所使用）</summary>
        public bool block;

        public event Action onTriggered;

        /// <summary>
        /// 执行一次行为（建议不要直接调用，而是在Update方法中调用状态机的Tick函数）
        /// </summary>
        /// <returns>是否成功执行</returns>
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

        /// <summary>执行行为</summary>
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

        /// <summary>用于触发器</summary>
        public virtual bool Trigger()
        {
            if (useTrigger && (condition == null || condition.Invoke()) &&  // 检查是否满足触发条件
                CheckState())  // 检查状态机的当前状态是否为行为的父状态
            {
                if (onTriggered != null)
                    onTriggered.Invoke();
                DoAct();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 创建行为
        /// </summary>
        /// <param name="useTrigger">是否使用触发器
        /// （若是则不Tick函数无效而需要通过调用Trigger函数手动触发,反之则Tick函数生效Trigger函数无效）</param>
        /// <param name="condition">触发条件</param>
        /// <param name="next">下一个状态，若为null则保持原样</param>
        /// <param name="interrupt">该行为是否会打断其他行为（主要用于打断ContinuousAct）</param>
        /// <param name="block">该行为是否会阻断其他行为执行（主要由ContinuousAct所使用）</param>
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
        /// 设置父状态
        /// </summary>
        /// <param name="parents">父状态集合</param>
        public void SetParentState(State[] parents)
        {
            ParentStates = new List<State>(parents);
            if (parents == null)
            {
                Debug.LogWarning("将行为的父状态设置为了空，该行为将无法被执行");
                return;
            }
            foreach (State state in parents)
            {
                state.AddMonoAction(this);
            }
        }

        /// <summary>
        /// 添加父状态
        /// </summary>
        /// <param name="parent">父状态</param>
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

    /// <summary>标准行为</summary>
    public class StandardAction : MonoAction
    {
        /// <summary>具体行为动作</summary>
        public Action act;

        protected override void DoAct()
        {
            if (act != null)
            {
                act.Invoke();
            }
        }

        /// <summary>创建标准行为</summary>
        /// <param name="act">具体行为动作</param>
        /// <param name="useTrigger">是否使用触发器
        /// （若是则不Tick函数无效而需要通过调用Trigger函数手动触发,反之则Tick函数生效Trigger函数无效）</param>
        /// <param name="condition">触发条件</param>
        /// <param name="next">下一个状态，若为null则保持原样</param>
        /// <param name="interrupt">该行为是否会打断其他行为（主要用于打断ContinuousAct）</param>
        /// <param name="block">该行为是否会阻断其他行为执行（主要由ContinuousAct所使用）</param>
        public StandardAction(Action act = null, bool useTrigger = false, Func<bool> condition = null,
            State next = null, bool interrupt = false, bool block = false) :
            base(useTrigger, condition, next, interrupt, block)
        {
            this.act = act;
        }
    }

    /// <summary>持续行为</summary>
    public class CorountineAction : MonoAction
    {
        /// <summary>协程函数</summary>
        public Func<IEnumerator> ienumGenerator;

        /// <summary>协程是否开启</summary>
        private bool onCoroutine;
        /// <summary>协程迭代器</summary>
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

        /// <summary>执行一次行为（不建议直接调用，而是在Update方法中调用状态机的Tick函数）</summary>
        /// <returns>是否执行成功</returns>
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
