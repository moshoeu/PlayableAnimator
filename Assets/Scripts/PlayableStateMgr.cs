/**
 *Created on 2020.2.1
 *Author:ZhangYuhao
 *Title: 动画控制器状态管理
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class PlayableStateMgr
{
    private class StateInfo
    {
        public void Initialize(string name, WrapMode wrapMode)
        {
            m_StateName = name;
            m_WrapMode = wrapMode;
        }

        public float GetTime()
        {
            if (m_TimeIsUpToDate)
                return m_Time;

            m_Time = (float)m_Playable.GetTime();
            m_TimeIsUpToDate = true;
            return m_Time;
        }

        public void SetTime(float newTime)
        {
            m_Time = newTime;
            m_Playable.ResetTime(m_Time);
            m_Playable.SetDone(m_Time >= m_Playable.GetDuration());
        }

        public void Enable()
        {
            if (m_Enabled)
                return;

            m_EnabledDirty = true;
            m_Enabled = true;
        }

        public void Disable()
        {
            if (m_Enabled == false)
                return;

            m_EnabledDirty = true;
            m_Enabled = false;
        }

        public void Pause()
        {
            m_Playable.Pause();
        }

        public void Play()
        {
            m_Playable.Play();
        }

        public void Stop()
        {
            m_FadeSpeed = 0f;
            ForceWeight(0.0f);
            Disable();
            SetTime(0.0f);
            m_Playable.SetDone(false);
        }

        public void ForceWeight(float weight)
        {
            m_TargetWeight = weight;
            m_Fading = false;
            m_FadeSpeed = 0f;
            SetWeight(weight);
        }

        public void SetWeight(float weight)
        {
            m_Weight = weight;
            m_WeightDirty = true;
        }

        public void FadeTo(float weight, float speed)
        {
            m_Fading = Mathf.Abs(speed) > 0f;
            m_FadeSpeed = speed;
            m_TargetWeight = weight;
        }

        public void DestroyPlayable()
        {
            if (m_Playable.IsValid())
            {
                m_Playable.GetGraph().DestroySubgraph(m_Playable);
            }
        }

        #region 属性
        public bool isBlendTree;

        public bool enabled
        {
            get { return m_Enabled; }
        }
        private bool m_Enabled;

        public int index
        {
            get { return m_Index; }
            set
            {
                Debug.Assert(m_Index == 0, "Should never reassign Index");
                m_Index = value;
            }
        }

        private int m_Index;

        public string stateName
        {
            get { return m_StateName; }
            set { m_StateName = value; }
        }
        private string m_StateName;

        public bool fading
        {
            get { return m_Fading; }
        }
        private bool m_Fading;


        private float m_Time;

        public float targetWeight
        {
            get { return m_TargetWeight; }
        }
        private float m_TargetWeight;

        public float weight
        {
            get { return m_Weight; }
        }
        float m_Weight;

        public float fadeSpeed
        {
            get { return m_FadeSpeed; }
        }
        float m_FadeSpeed;

        public float speed
        {
            get { return (float)m_Playable.GetSpeed(); }
            set { m_Playable.SetSpeed(value); }
        }

        public float playableDuration
        {
            get { return (float)m_Playable.GetDuration(); }
        }

        public void SetPlayable(Playable playable)
        {
            m_Playable = playable;
        }

        public bool isDone { get { return m_Playable.IsDone(); } }

        public Playable playable
        {
            get { return m_Playable; }
        }
        private Playable m_Playable;

        public WrapMode wrapMode
        {
            get { return m_WrapMode; }
        }

        private WrapMode m_WrapMode;

        public bool enabledDirty { get { return m_EnabledDirty; } }
        public bool weightDirty { get { return m_WeightDirty; } }

        public void ResetDirtyFlags()
        {
            m_EnabledDirty = false;
            m_WeightDirty = false;
        }

        private bool m_WeightDirty;
        private bool m_EnabledDirty;

        public void InvalidateTime() { m_TimeIsUpToDate = false; }
        private bool m_TimeIsUpToDate;
        #endregion
    }

    private class StateGroup
    {
        private List<StateInfo> m_States;

        public string groupName;

        public StateInfo this[int i]
        {
            get
            {
                return m_States[i];
            }
        }

        public int Count { get { return m_Count; } }
        private int m_Count;

        public StateGroup(string groupName)
        {
            this.groupName = groupName;
            m_States = new List<StateInfo>();
        }

        public void AddState(string stateName, bool isBlendTree)
        {
            StateInfo state = new StateInfo();
            state.stateName = stateName;
            state.isBlendTree = isBlendTree;

            if (FindState(stateName) != null)
            {
                Debug.LogErrorFormat("Add state fail, state:{0} has existed!", stateName);
                return;
            }

            int emptyIndex = m_States.FindIndex(state => state == null);
            if (emptyIndex != -1)
            {
                m_States[emptyIndex] = state;
                state.index = emptyIndex;
            }
            else
            {
                m_States.Add(state);
                state.index = m_States.Count;
            }

            m_Count++;
        }

        public void RemoveState(string stateName)
        {
            StateInfo removeState = FindState(stateName);
            if (removeState == null)
            {
                Debug.LogErrorFormat("Remove state fail, state:{0} is not existing!", stateName);
                return;
            }

            removeState.DestroyPlayable();
            m_States[removeState.index] = null;
            m_Count--;
        }

        public void EnableState(string stateName)
        {
            StateInfo state = FindState(stateName);
            if (state != null)
            {
                state.Enable();
            }
        }

        public void DisableState(string stateName)
        {
            StateInfo state = FindState(stateName);
            if (state != null)
            {
                state.Disable();
            }
        }

        public void SetInputWeight(string stateName, float weight)
        {
            StateInfo state = FindState(stateName);
            if (state != null)
            {
                state.SetWeight(weight);
            }
        }

        public StateInfo FindState(string stateName)
        {
            int index = m_States.FindIndex(state => state != null && state.stateName == stateName);
            if (index == -1)
            {
                return null;
            }
            return m_States[index];
        }

        public void Release()
        {
            for (int i = 0; i < m_States.Count; i++)
            {
                StateInfo state = m_States[i];
                if (state != null)
                {
                    RemoveState(state.stateName);
                }
            }
        }
    }

    private class StateLayer
    {
        public int layerIndex;
        private StateGroup m_DefaultStateGroup;
        private Dictionary<string, StateGroup> m_StateGroups;

        public StateLayer(int layerIndex)
        {
            m_DefaultStateGroup = new StateGroup("default");
            this.layerIndex = layerIndex;
        }

        private StateGroup GetStateGroup(string groupName)
        {
            StateGroup group = null;
            if (groupName != null)
            {
                if (m_StateGroups == null)
                {
                    Debug.LogError("No state groups here!");
                }

                else if (false == m_StateGroups.TryGetValue(groupName, out group))
                {
                    Debug.LogErrorFormat("Add state fail, state group:{0} is not existing!", groupName);
                }
            }
            else
            {
                group = m_DefaultStateGroup;
            }

            return group;
        }

        public void AddStateGroup(string groupName)
        {
            if (m_StateGroups == null)
            {
                m_StateGroups = new Dictionary<string, StateGroup>();
            }

            if (m_StateGroups.ContainsKey(groupName))
            {
                Debug.LogErrorFormat("Add state group fail, state group:{0} has existed!", groupName);
                return;
            }

            m_StateGroups.Add(groupName, new StateGroup(groupName));
        }

        public void RemoveStateGroup(string groupName)
        {
            StateGroup group = GetStateGroup(groupName);
            group.Release();
            m_StateGroups.Remove(groupName);
        }

        public void AddState(string stateName, bool isBlendTree, string groupName = null)
        {
            StateGroup group = GetStateGroup(groupName);
            group.AddState(stateName, isBlendTree);
        }

        public void RemoveState(string stateName, string groupName = null)
        {
            StateGroup group = GetStateGroup(groupName);
            group.RemoveState(stateName);
        }

        public void EnableState(string stateName, string groupName = null)
        {
            StateGroup group = GetStateGroup(groupName);
            group.EnableState(stateName);
        }

        public void DisableState(string stateName, string groupName = null)
        {
            StateGroup group = GetStateGroup(groupName);
            group.DisableState(stateName);
        }

        public void SetInputWeight(string stateName, float weight, string groupName = null)
        {
            StateGroup group = GetStateGroup(groupName);
            group.SetInputWeight(stateName, weight);
        }

        public StateInfo FindState(string stateName, string groupName = null)
        {
            StateGroup group = GetStateGroup(groupName);
            return group.FindState(stateName);
        }
    }

    private List<StateLayer> m_StateLayers;

    public PlayableStateMgr()
    {
        m_StateLayers = new List<StateLayer>();
        m_StateLayers.Add(new StateLayer(0));     // 添加默认层
    }

    private StateLayer GetStateLayer(int layer)
    {
        StateLayer stateLayer = null;
        if (layer >= m_StateLayers.Count)
        {
            Debug.LogErrorFormat("Get state layer fail, state group:{0} is not existing!", layer);
        }

        else
        {
            stateLayer = m_StateLayers[layer];
        }

        return stateLayer;
    }

    public void AddStateGroup(string groupName, int layer = 0)
    {
        StateLayer stateLayer = GetStateLayer(layer);
        stateLayer.AddStateGroup(groupName);
    }

    public void RemoveStateGroup(string groupName, int layer = 0)
    {
        StateLayer stateLayer = GetStateLayer(layer);
        stateLayer.RemoveStateGroup(groupName);
    }

    public void AddState(string stateName, bool isBlendTree, string groupName = null, int layer = 0)
    {
        StateLayer stateLayer = GetStateLayer(layer);
        stateLayer.AddState(stateName, isBlendTree, groupName);
    }

    public void RemoveState(string stateName, string groupName = null, int layer = 0)
    {
        StateLayer stateLayer = GetStateLayer(layer);
        stateLayer.RemoveState(stateName, groupName);
    }

    public void EnableState(string stateName, string groupName = null, int layer = 0)
    {
        StateLayer stateLayer = GetStateLayer(layer);
        stateLayer.EnableState(stateName, groupName);
    }

    public void DisableState(string stateName, string groupName = null, int layer = 0)
    {
        StateLayer stateLayer = GetStateLayer(layer);
        stateLayer.DisableState(stateName, groupName);
    }

    public void SetInputWeight(string stateName, float weight, string groupName = null, int layer = 0)
    {
        StateLayer stateLayer = GetStateLayer(layer);
        stateLayer.SetInputWeight(stateName, weight, groupName);
    }
}


