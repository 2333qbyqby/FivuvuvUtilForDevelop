using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace FivuvuvUtil.CommonFSM
{
    /// <summary>有限元状态机</summary>
    public class FSM
    {
        public State LastState { get; private set; }
        public string LastStateName { get => LastState != null ? LastState.Name : null; }
        /// <summary>当前状态</summary>
        public State CurrentState { get; private set; }
        /// <summary>当前状态名称</summary>
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

        /// <summary>状态改变事件</summary>
        public event Action<State, State> onStateChanged_detailed;

        /// <summary>状态改变事件</summary>
        public event Action<string, string> onStateChanged;

        /// <summary>
        /// 更改状态（不建议直接调用）
        /// </summary>
        /// <param name="newState">新的状态</param>
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
        /// 更改状态（不建议直接调用）
        /// </summary>
        /// <param name="stateName">新的状态名称</param>
        public void ChangeCurrentState(string stateName)
        {
            ChangeCurrentState(GetState(stateName));
        }

        /// <summary>执行一次，请在Update函数里调用此方法</summary>
        public void Tick()
        {
            if (CurrentState == null) // 若当前状态为空，自动切换到状态列表第一个状态
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

        /// <summary>执行一次触发（会按次序触发当前状态下可被触发的行为）</summary>
        public void Trigger()
        {
            if (CurrentState == null) // 若当前状态为空，自动切换到状态列表第一个状态
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
        /// 向状态机创建新的状态
        /// </summary>
        /// <param name="name">状态名称</param>
        /// <param name="acts">行为列表</param>
        /// <returns>c创建的状态</returns>
        public State NewState(string name, MonoAction[] acts = null)
        {
            State newState = new State(name, this, acts);
            return newState;
        }

        /// <summary>
        /// 向状态机添加新的状态
        /// </summary>
        /// <param name="state">状态</param>
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
        /// 向状态机添加新的状态
        /// </summary>
        /// <param name="stateArray">状态列表</param>
        public void AddState(State[] stateArray)
        {
            foreach (State state in stateArray)
                AddState(state);
        }

        /// <summary>
        /// 移除状态
        /// </summary>
        /// <param name="name">状态名称</param>
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
        /// 有限元状态机
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
        /// 获取状态（若不存在则返回null）
        /// </summary>
        /// <param name="name">状态名称</param>
        /// <returns>获取到的状态</returns>
        public State GetState(string name)
        {
            foreach (State state in states)
                if (state.Name == name)
                    return state;
            return null;
        }
    }
}
