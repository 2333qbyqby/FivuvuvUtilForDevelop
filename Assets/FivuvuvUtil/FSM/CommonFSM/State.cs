using System;
using System.Collections;
using System.Collections.Generic;

namespace FivuvuvUtil.CommonFSM
{
    //状态
    public class State
    {
        public event Action onStateEntered;
        public event Action onStateaLeave;

        private float lastEnterTime;
        private float lastEnterRealTime;

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is State)
            {
                State state = obj as State;
                return Name == state.Name;
            }
            else
            {
                return base.Equals(obj);
            }
        }

        /// <summary>状态名称</summary>
        public string Name { get; private set; }
        /// <summary>父状态机</summary>
        public FSM ParentFSM { get; private set; }

        public float LastEnterTime
        {
            get => lastEnterTime;
            internal set { lastEnterTime = value; }
        }
        public float LastEnterRealTime
        {
            get => lastEnterRealTime;
            internal set { lastEnterRealTime = value; }
        }

        private List<MonoAction> actions;

        internal State Tick()
        {
            foreach (MonoAction act in actions)
            {
                bool acted = act.Tick();
                if (acted)
                {
                    if (act.nextState != null)
                        return act.nextState;
                    if (act.block)
                        return null;
                    if (act.interrupt)
                        Interrupt();
                }
            }
            return null;
        }

        private void Interrupt()
        {
            ParentFSM.Interrupt();
        }

        internal void TriggerEnterEvent()
        {
            onStateEntered?.Invoke();
        }

        internal void TriggerLeaveEvent()
        {
            onStateaLeave?.Invoke();
        }

        internal State Trigger()
        {
            foreach (MonoAction act in actions)
            {
                if (!act.useTrigger)
                    continue;
                bool acted = act.Trigger();
                if (acted)
                {
                    if (act.nextState != null)
                        return act.nextState;
                    if (act.block)
                        return null;
                }
            }
            return null;
        }

        internal void SetParentFSM(FSM fsm)
        {
            ParentFSM = fsm;
            if (fsm == null)
                return;
            fsm.AddState(this);
        }

        /// <summary>
        /// 向状态添加行为
        /// </summary>
        /// <param name="act">行为</param>
        public void AddMonoAction(MonoAction act)
        {
            if (!act.ParentStates.Contains(this))
            {
                act.AddParentState(this);
                return;
            }
            if (act is CorountineAction)
            {
                CorountineAction a = act as CorountineAction;
                a.onCoroutineStateChanged += (bool started) => { if (started) ParentFSM.AddContinuousAct(a); else ParentFSM.RemoveContinuousAct(a); };
            }
            if (act.useTrigger)
            {
                act.onTriggered += () => { if (act.interrupt) Interrupt(); ParentFSM.ChangeCurrentState(act.nextState); };
            }
            actions.Add(act);
        }

        /// <summary>
        /// 向状态添加行为
        /// </summary>
        /// <param name="actArray">行为列表</param>
        public void AddMonoActions(MonoAction[] actArray)
        {
            foreach (MonoAction act in actArray)
                AddMonoAction(act);
        }

        /// <summary>
        /// 向状态创建新的标准行为
        /// </summary>
        /// <param name="action">行为</param>
        /// <param name="condition">条件函数</param>
        /// <returns>创建的行为</returns>
        public StandardAction NewStandardAction(Action action = null, Func<bool> condition = null)
        {
            StandardAction newAct = new StandardAction(action, false, condition);
            AddMonoAction(newAct);
            return newAct;
        }

        /// <summary>
        /// 向状态创建新的触发器行为
        /// </summary>
        /// <param name="action">行为</param>
        /// <param name="condition">条件函数</param>>
        /// <returns>创建的行为</returns>
        public StandardAction NewTriggerAction(Action action = null, Func<bool> condition = null)
        {
            StandardAction newAct = new StandardAction(action, true, condition);
            AddMonoAction(newAct);
            return newAct;
        }

        /// <summary>
        /// 向状态创建新的转换行为
        /// </summary>
        /// <param name="nextState">下一个状态</param>
        /// <param name="useTrigger">是否使用触发器</param>
        /// <param name="condition">条件函数</param>
        /// <param name="action">行为</param>
        /// <returns>创建的行为</returns>
        public StandardAction NewTransferAction(State nextState, bool useTrigger = true, Func<bool> condition = null, Action action = null)
        {
            StandardAction newAct = new StandardAction(action, useTrigger, condition, nextState);
            AddMonoAction(newAct);
            return newAct;
        }

        /// <summary>
        /// 向状态创建新的持续行为
        /// </summary>
        /// <param name="ienumGenerator">协程函数</param>
        /// <param name="condition">条件函数</param>
        /// <param name="next">下一个状态</param>
        /// <param name="useTrigger">是否使用触发器</param>
        /// <param name="interrupt">是否会打断其他行为</param>
        /// <param name="block">是否会阻断其他行为触发</param>
        /// <returns>创建的行为</returns>
        public CorountineAction NewCoroutineAction(Func<IEnumerator> ienumGenerator, bool useTrigger = true, Func<bool> condition = null,
            State next = null, bool interrupt = false, bool block = false)
        {
            CorountineAction newAct = new CorountineAction(ienumGenerator, useTrigger, condition, next, interrupt, block);
            AddMonoAction(newAct);
            return newAct;
        }

        /// <summary>
        /// 向状态创建新的持续行为
        /// </summary>
        /// <param name="enumerator">协程对象（不能是从协程函数中创建的，而应是继承了IEnumerator并重写了Reset函数的类对象）</param>
        /// <param name="condition">条件函数</param>
        /// <param name="next">下一个状态</param>
        /// <param name="useTrigger">是否使用触发器</param>
        /// <param name="interrupt">是否会打断其他行为</param>
        /// <param name="block">是否会阻断其他行为触发</param>
        /// <returns>创建的行为</returns>
        public CorountineAction NewCoroutineAction(IEnumerator enumerator, bool useTrigger = true, State next = null,
            Func<bool> condition = null, bool interrupt = false, bool block = false)
        {
            CorountineAction newAct = new CorountineAction(enumerator, useTrigger, condition, next, interrupt, block);
            AddMonoAction(newAct);
            return newAct;
        }

        /// <summary>
        /// 状态
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="parent">父状态机</param>
        /// <param name="acts">行为列表</param>
        public State(string name, FSM parent = null, MonoAction[] acts = null)
        {
            actions = new List<MonoAction>();
            Name = name;
            if (acts != null)
                foreach (MonoAction act in acts)
                    actions.Add(act);
            if (parent != null)
                SetParentFSM(parent);
        }
    }
}
