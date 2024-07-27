using System;
using System.Collections;
using System.Collections.Generic;

namespace FivuvuvUtil.CommonFSM
{
    //״̬
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

        /// <summary>״̬����</summary>
        public string Name { get; private set; }
        /// <summary>��״̬��</summary>
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
        /// ��״̬�����Ϊ
        /// </summary>
        /// <param name="act">��Ϊ</param>
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
        /// ��״̬�����Ϊ
        /// </summary>
        /// <param name="actArray">��Ϊ�б�</param>
        public void AddMonoActions(MonoAction[] actArray)
        {
            foreach (MonoAction act in actArray)
                AddMonoAction(act);
        }

        /// <summary>
        /// ��״̬�����µı�׼��Ϊ
        /// </summary>
        /// <param name="action">��Ϊ</param>
        /// <param name="condition">��������</param>
        /// <returns>��������Ϊ</returns>
        public StandardAction NewStandardAction(Action action = null, Func<bool> condition = null)
        {
            StandardAction newAct = new StandardAction(action, false, condition);
            AddMonoAction(newAct);
            return newAct;
        }

        /// <summary>
        /// ��״̬�����µĴ�������Ϊ
        /// </summary>
        /// <param name="action">��Ϊ</param>
        /// <param name="condition">��������</param>>
        /// <returns>��������Ϊ</returns>
        public StandardAction NewTriggerAction(Action action = null, Func<bool> condition = null)
        {
            StandardAction newAct = new StandardAction(action, true, condition);
            AddMonoAction(newAct);
            return newAct;
        }

        /// <summary>
        /// ��״̬�����µ�ת����Ϊ
        /// </summary>
        /// <param name="nextState">��һ��״̬</param>
        /// <param name="useTrigger">�Ƿ�ʹ�ô�����</param>
        /// <param name="condition">��������</param>
        /// <param name="action">��Ϊ</param>
        /// <returns>��������Ϊ</returns>
        public StandardAction NewTransferAction(State nextState, bool useTrigger = true, Func<bool> condition = null, Action action = null)
        {
            StandardAction newAct = new StandardAction(action, useTrigger, condition, nextState);
            AddMonoAction(newAct);
            return newAct;
        }

        /// <summary>
        /// ��״̬�����µĳ�����Ϊ
        /// </summary>
        /// <param name="ienumGenerator">Э�̺���</param>
        /// <param name="condition">��������</param>
        /// <param name="next">��һ��״̬</param>
        /// <param name="useTrigger">�Ƿ�ʹ�ô�����</param>
        /// <param name="interrupt">�Ƿ����������Ϊ</param>
        /// <param name="block">�Ƿ�����������Ϊ����</param>
        /// <returns>��������Ϊ</returns>
        public CorountineAction NewCoroutineAction(Func<IEnumerator> ienumGenerator, bool useTrigger = true, Func<bool> condition = null,
            State next = null, bool interrupt = false, bool block = false)
        {
            CorountineAction newAct = new CorountineAction(ienumGenerator, useTrigger, condition, next, interrupt, block);
            AddMonoAction(newAct);
            return newAct;
        }

        /// <summary>
        /// ��״̬�����µĳ�����Ϊ
        /// </summary>
        /// <param name="enumerator">Э�̶��󣨲����Ǵ�Э�̺����д����ģ���Ӧ�Ǽ̳���IEnumerator����д��Reset�����������</param>
        /// <param name="condition">��������</param>
        /// <param name="next">��һ��״̬</param>
        /// <param name="useTrigger">�Ƿ�ʹ�ô�����</param>
        /// <param name="interrupt">�Ƿ����������Ϊ</param>
        /// <param name="block">�Ƿ�����������Ϊ����</param>
        /// <returns>��������Ϊ</returns>
        public CorountineAction NewCoroutineAction(IEnumerator enumerator, bool useTrigger = true, State next = null,
            Func<bool> condition = null, bool interrupt = false, bool block = false)
        {
            CorountineAction newAct = new CorountineAction(enumerator, useTrigger, condition, next, interrupt, block);
            AddMonoAction(newAct);
            return newAct;
        }

        /// <summary>
        /// ״̬
        /// </summary>
        /// <param name="name">����</param>
        /// <param name="parent">��״̬��</param>
        /// <param name="acts">��Ϊ�б�</param>
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
