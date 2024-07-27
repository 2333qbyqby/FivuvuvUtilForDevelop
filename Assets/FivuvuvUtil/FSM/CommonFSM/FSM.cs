using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace FivuvuvUtil.CommonFSM
{
    /// <summary>����Ԫ״̬��</summary>
    public class FSM
    {
        public State LastState { get; private set; }
        public string LastStateName { get => LastState != null ? LastState.Name : null; }
        /// <summary>��ǰ״̬</summary>
        public State CurrentState { get; private set; }
        /// <summary>��ǰ״̬����</summary>
        public string CurrentStateName { get { if (CurrentState != null) return CurrentState.Name; else return null; } }

        private List<State> states;
        private List<CorountineAction> coroutineActions;
        private List<CorountineAction> conRemoveList;

        internal void AddContinuousAct(CorountineAction act)
        {
            coroutineActions.Add(act);
        }

        internal void RemoveContinuousAct(CorountineAction act)
        {
            conRemoveList.Add(act);
        }

        internal void Interrupt()
        {
            foreach (CorountineAction ca in coroutineActions)
                ca.Stop();
        }

        /// <summary>״̬�ı��¼�</summary>
        public event Action<State, State> onStateChanged_detailed;

        /// <summary>״̬�ı��¼�</summary>
        public event Action<string, string> onStateChanged;

        /// <summary>
        /// ����״̬��������ֱ�ӵ��ã�
        /// </summary>
        /// <param name="newState">�µ�״̬</param>
        public void ChangeCurrentState(State newState)
        {
            if (newState == null || !states.Contains(newState))
                return;
            if (onStateChanged_detailed != null && CurrentState != null)
                onStateChanged_detailed.Invoke(CurrentState, newState);
            if (onStateChanged != null && CurrentState != null)
                onStateChanged.Invoke(CurrentStateName, newState.Name);
            if (CurrentStateName != newState.Name)
            {
                LastState = CurrentState;
                LastState?.TriggerLeaveEvent();
                CurrentState = newState;
                newState.LastEnterTime = Time.time;
                newState.LastEnterRealTime = Time.realtimeSinceStartup;
                CurrentState?.TriggerEnterEvent();
            }
        }

        /// <summary>
        /// ����״̬��������ֱ�ӵ��ã�
        /// </summary>
        /// <param name="stateName">�µ�״̬����</param>
        public void ChangeCurrentState(string stateName)
        {
            ChangeCurrentState(GetState(stateName));
        }

        /// <summary>ִ��һ�Σ�����Update��������ô˷���</summary>
        public void Tick()
        {
            if (CurrentState == null) // ����ǰ״̬Ϊ�գ��Զ��л���״̬�б��һ��״̬
            {
                if (states.Count > 0)
                    CurrentState = states[0];
                else
                    return;
            }
            foreach (CorountineAction cact in conRemoveList)
            {
                coroutineActions.Remove(cact);
            }
            conRemoveList.Clear();
            foreach (CorountineAction act in coroutineActions)
            {
                act.DoCoroutine();
            }
            State nextState = CurrentState.Tick();
            ChangeCurrentState(nextState);
        }

        /// <summary>ִ��һ�δ������ᰴ���򴥷���ǰ״̬�¿ɱ���������Ϊ��</summary>
        public void Trigger()
        {
            if (CurrentState == null) // ����ǰ״̬Ϊ�գ��Զ��л���״̬�б��һ��״̬
            {
                if (states.Count > 0)
                    CurrentState = states[0];
                else
                    return;
            }
            State nextState = CurrentState.Trigger();
            ChangeCurrentState(nextState);
        }

        /// <summary>
        /// ��״̬�������µ�״̬
        /// </summary>
        /// <param name="name">״̬����</param>
        /// <param name="acts">��Ϊ�б�</param>
        /// <returns>c������״̬</returns>
        public State NewState(string name, MonoAction[] acts = null)
        {
            State newState = new State(name, this, acts);
            return newState;
        }

        /// <summary>
        /// ��״̬������µ�״̬
        /// </summary>
        /// <param name="state">״̬</param>
        public void AddState(State state)
        {
            if (state == null)
            {
                return;
            }
            if (state.ParentFSM != this)
            {
                state.SetParentFSM(this);
                return;
            }
            states.Add(state);
            if (CurrentState == null)
            {
                CurrentState = state;
            }
        }

        /// <summary>
        /// ��״̬������µ�״̬
        /// </summary>
        /// <param name="stateArray">״̬�б�</param>
        public void AddState(State[] stateArray)
        {
            foreach (State state in stateArray)
                AddState(state);
        }

        /// <summary>
        /// �Ƴ�״̬
        /// </summary>
        /// <param name="name">״̬����</param>
        public void RemoveState(string name)
        {
            for (int i = 0; i < states.Count; i++)
            {
                State state = states[i];
                if (state.Name == name)
                {
                    states.RemoveAt(i);
                    state.SetParentFSM(null);
                    break;
                }
            }
        }

        /// <summary>
        /// ����Ԫ״̬��
        /// </summary>
        /// <param name="stateArray"></param>
        public FSM(State[] stateArray = null)
        {
            CurrentState = null;
            states = new List<State>();
            coroutineActions = new List<CorountineAction>();
            conRemoveList = new List<CorountineAction>();
            if (stateArray != null)
                AddState(stateArray);
        }

        /// <summary>
        /// ��ȡ״̬�����������򷵻�null��
        /// </summary>
        /// <param name="name">״̬����</param>
        /// <returns>��ȡ����״̬</returns>
        public State GetState(string name)
        {
            foreach (State state in states)
                if (state.Name == name)
                    return state;
            return null;
        }
    }
}
