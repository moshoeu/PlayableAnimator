/**
 *Created on 2020.2.1
 *Author:ZhangYuhao
 *Title: 动画控制器状态管理
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace PlayableAnimator
{
    // todo: 暂时放在这儿
    public struct BlendTreeConfig
    {
        public float threshold;
        public float speed;
        public int clipIndex;
        public float weight;
    }

    public class PlayableStateController : IPlayableAnimatorNode
    {
        private class StateInfo
        {
            #region method
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

            public void SetPlayable(Playable playable)
            {
                m_Playable = playable;
            }

            public void ResetDirtyFlags()
            {
                m_EnabledDirty = false;
                m_WeightDirty = false;
            }

            public void SetBlendTreeNodeCount(int count)
            {
                blendTreeConfigs = new BlendTreeConfig[count];
            }

            public void SetBlendTreeConfig(int index, float threshold, float speed, int clipIndex)
            {
                if (blendTreeConfigs == null)
                {
                    Debug.LogErrorFormat("state:{0} blendTreeConfigs is not initialized!", m_StateName);
                    return;
                }

                if (index >= blendTreeConfigs.Length || index < 0)
                {
                    Debug.LogErrorFormat("state:{0} blendTreeConfigs index is out of range!", m_StateName);
                    return;
                }

                blendTreeConfigs[index].clipIndex = clipIndex;
                blendTreeConfigs[index].threshold = threshold;
                blendTreeConfigs[index].speed = speed;

                m_BlendTreeDirty = true;
            }

            /// <summary>
            /// 对混合树结点排序
            /// </summary>
            public void SortBlendTreeConfig()
            {
                System.Array.Sort(blendTreeConfigs, (a, b) =>
                {
                    if (a.threshold < b.threshold)
                    {
                        return 1;
                    }
                    else if (a.threshold == b.threshold)
                    {
                        return 0;
                    }
                    else
                    {
                        return -1;
                    }
                });
            }

            /// <summary>
            /// 按照参数大小计算混合树各结点权重
            /// </summary>
            public void CalBlendTreeWeight(Animator animator)
            {
                float param = animator.GetFloat(blendTreeParameter);
                for (int i = 0; i < blendTreeConfigs.Length - 1; i++)
                {
                    float curThreshold = blendTreeConfigs[i].threshold;
                    float nextThreshold = blendTreeConfigs[i + 1].threshold;
                    if (param >= curThreshold && param < nextThreshold)    // 位于两点之间
                    {
                        float curWeight = Mathf.Clamp01((nextThreshold - param) / (nextThreshold - curThreshold));
                        blendTreeConfigs[i].weight = curWeight;
                        blendTreeConfigs[i + 1].weight = 1 - curWeight;
                    }
                    else
                    {
                        blendTreeConfigs[i].weight = 0;
                    }
                }

                if (param < blendTreeConfigs[0].threshold)
                {
                    blendTreeConfigs[0].weight = 1;
                }

                if (param >= blendTreeConfigs[blendTreeConfigs.Length - 1].threshold)
                {
                    blendTreeConfigs[blendTreeConfigs.Length - 1].weight = 1;
                }
            }

            #endregion

            #region property

            #region BlendTree
            public bool isBlendTree;
            public string blendTreeParameter;
            private bool m_BlendTreeDirty = false;
            private BlendTreeConfig[] blendTreeConfigs;
            #endregion

            #region Fading 
            public bool fading
            {
                get { return m_Fading; }
            }
            private bool m_Fading;

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
            #endregion

            #region Base
            public string stateGroupName;   // 归属状态集合名字

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

            private float m_Time;

            public float speed
            {
                get { return (float)m_Playable.GetSpeed(); }
                set { m_Playable.SetSpeed(value); }
            }

            public float playableDuration
            {
                get { return (float)m_Playable.GetDuration(); }
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

            private bool m_WeightDirty;
            private bool m_EnabledDirty;

            public void InvalidateTime() { m_TimeIsUpToDate = false; }
            private bool m_TimeIsUpToDate;
            #endregion

            #endregion
        }

        private class StateLayer : IPlayableAnimatorNode
        {
            private List<StateInfo> m_States;
            private PlayableGraph m_Graph;

            private AnimationMixerPlayable m_Mixer;

            private AvatarMask m_AvatarMask;
            public AvatarMask avatarMask
            {
                get { return m_AvatarMask; }
                set { m_AvatarMask = value; }
            }

            public int layerIndex;
            public float weight;
            public bool isAdditive;
            public bool isLayerDirty;

            public StateInfo this[int i]
            {
                get
                {
                    return m_States[i];
                }
            }

            public int Count { get { return m_Count; } }
            private int m_Count;

            public StateLayer(int layerIndex, PlayableGraph graph)
            {
                this.layerIndex = layerIndex;
                m_States = new List<StateInfo>();
                m_Graph = graph;
                m_Mixer = AnimationMixerPlayable.Create(m_Graph, 1, true);
                isLayerDirty = true;
            }

            private void DisconnectInput(int index)
            {
                m_Graph.Disconnect(m_Mixer, index);
            }

            private void ConnectInput(int index)
            {
                StateInfo state = m_States[index];
                m_Graph.Connect(state.playable, 0, m_Mixer, state.index);
            }

            private void UpdateStates(float deltaTime)
            {
                bool mustUpdateWeights = false;
                float totalWeight = 0f;
                int count = m_States.Count;
                for (int i = 0; i < count; i++)
                {
                    StateInfo state = m_States[i];
                    if (state == null)
                        continue;

                    state.InvalidateTime();

                    if (state.fading)
                    {
                        state.SetWeight(Mathf.MoveTowards(state.weight, state.targetWeight, state.fadeSpeed * deltaTime));
                        if (Mathf.Approximately(state.weight, state.targetWeight))
                        {
                            state.ForceWeight(state.targetWeight);
                            if (state.weight == 0f)
                            {
                                state.Stop();
                            }
                        }
                    }

                    if (state.enabledDirty)
                    {
                        if (state.enabled)
                            state.Play();
                        else
                            state.Pause();


                        Playable input = m_Mixer.GetInput(i);
                        //if state is disabled but the corresponding input is connected, disconnect it
                        if (input.IsValid() && !state.enabled)
                        {
                            DisconnectInput(i);
                        }
                        else if (state.enabled && !input.IsValid())
                        {
                            ConnectInput(state.index);
                        }

                    }

                    if (state.enabled && state.wrapMode == WrapMode.Once)
                    {
                        bool stateIsDone = state.isDone;
                        float speed = state.speed;
                        float time = state.GetTime();
                        float duration = state.playableDuration;

                        stateIsDone |= speed < 0f && time < 0f;
                        stateIsDone |= speed >= 0f && time >= duration;
                        if (stateIsDone)
                        {
                            state.Stop();
                            state.Disable();
                            DisconnectInput(state.index);

                        }
                    }

                    totalWeight += state.weight;
                    if (state.weightDirty)
                    {
                        mustUpdateWeights = true;
                    }
                    state.ResetDirtyFlags();
                }

                if (mustUpdateWeights)
                {
                    bool hasAnyWeight = totalWeight > 0.0f;
                    for (int i = 0; i < m_States.Count; i++)
                    {
                        StateInfo state = m_States[i];
                        float weight = hasAnyWeight ? state.weight / totalWeight : 0.0f;
                        m_Mixer.SetInputWeight(state.index, weight);
                    }
                }
            }

            public void RemoveStateGroup(string groupName)
            {
                for (int i = 0; i < m_States.Count; i++)
                {
                    var state = m_States[i];
                    if (state == null)
                    {
                        continue;
                    }

                    if (state.stateGroupName == groupName)
                    {
                        state.DestroyPlayable();
                        m_States[state.index] = null;
                        m_Count--;
                    }
                }
            }

            public void AddState(string stateName, bool isBlendTree, string groupName = null)
            {
                if (FindState(stateName) != null)
                {
                    Debug.LogErrorFormat("Add state fail, state:{0} has existed!", stateName);
                    return;
                }

                StateInfo state = new StateInfo();
                state.stateName = stateName;
                state.isBlendTree = isBlendTree;
                state.stateGroupName = groupName;

                int emptyIndex = m_States.FindIndex(s => s == null);
                if (emptyIndex != -1)
                {
                    m_States[emptyIndex] = state;
                    state.index = emptyIndex;
                }
                else
                {
                    state.index = m_States.Count;
                    m_States.Add(state);

                    m_Mixer.SetInputCount(m_States.Count);     // 增加输入端口
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

            public void Update(float deltaTime)
            {
                UpdateStates(deltaTime);
            }

            public void Release()
            {
                for (int i = 0; i < m_States.Count; i++)
                {
                    var state = m_States[i];
                    if (state == null)
                    {
                        continue;
                    }

                    state.DestroyPlayable();
                }

                m_Mixer.GetGraph().DestroySubgraph(m_Mixer);
            }

            public void SetPlayableOutput(int outputPort, int desInputPort, Playable inputNode)
            {
                m_Graph.Connect(m_Mixer, 0, inputNode, desInputPort);
            }
        }

        private List<StateLayer> m_StateLayers;
        private PlayableGraph m_Graph;
        private AnimationLayerMixerPlayable m_LayerMixer;

        public PlayableStateController(PlayableGraph graph)
        {
            m_StateLayers = new List<StateLayer>();
            m_StateLayers.Add(new StateLayer(0, graph));     // 添加默认层

            m_LayerMixer = AnimationLayerMixerPlayable.Create(graph, 1);

            m_Graph = graph;
        }

        public void Update(float deltaTime)
        {
            UpdateStates(deltaTime);
            UpdateLayers(deltaTime);
        }

        public void SetPlayableOutput(int outputPort, int desInputPort, Playable inputNode)
        {
            m_Graph.Connect(m_LayerMixer, 0, inputNode, desInputPort);
        }

        #region Layer
        public int AddLayer()
        {
            int emptyIndex = m_StateLayers.FindIndex(l => l == null);
            int newLayerIndex = 0;
            if (emptyIndex != -1)
            {
                newLayerIndex = emptyIndex;
                m_StateLayers[newLayerIndex] = new StateLayer(newLayerIndex, m_Graph);
            }
            else
            {
                newLayerIndex = m_StateLayers.Count;
                m_StateLayers.Add(new StateLayer(newLayerIndex, m_Graph));

                m_LayerMixer.SetInputCount(m_StateLayers.Count);
            }

            return newLayerIndex;
        }

        public void RemoveLayer(int layerIndex)
        {
            int removeIndex = m_StateLayers.FindIndex(l => l != null && l.layerIndex == layerIndex);
            m_StateLayers[removeIndex].Release();
            m_StateLayers[removeIndex] = null;
        }

        public void SetLayerMask(int layerIndex, AvatarMask mask)
        {
            if (!CheckLayerIfExist(layerIndex)) return;

            StateLayer layer = m_StateLayers[layerIndex];
            layer.avatarMask = mask;
            m_LayerMixer.SetLayerMaskFromAvatarMask((uint)layerIndex, mask);    // todo: 观察是否可以放这里，效果不好则放在UpdateLayers里
        }

        public void SetLayerAddtive(int layerIndex, bool isAdditive)
        {
            if (!CheckLayerIfExist(layerIndex)) return;

            StateLayer layer = m_StateLayers[layerIndex];
            layer.isAdditive = isAdditive;
            m_LayerMixer.SetLayerAdditive((uint)layerIndex, isAdditive);        // todo: 观察是否可以放这里，效果不好则放在UpdateLayers里
        }

        private bool CheckLayerIfExist(int layerIndex)
        {
            if (layerIndex >= m_StateLayers.Count || layerIndex < 0)
            {
                Debug.LogErrorFormat("Layer index:{0} is out of range!", layerIndex);
                return false;
            }

            StateLayer layer = m_StateLayers[layerIndex];
            if (layer == null)
            {
                Debug.LogErrorFormat("Layer index:{0} has destroyed!", layerIndex);
                return false;
            }

            return true;
        }

        private void UpdateLayers(float deltaTime)
        {
            for (int i = 0; i < m_StateLayers.Count; i++)
            {
                StateLayer layer = m_StateLayers[i];
                if (layer == null) continue;

                if (layer.isLayerDirty)
                {
                    layer.SetPlayableOutput(0, layer.layerIndex, m_LayerMixer);
                    layer.isLayerDirty = false;
                }
            }
        }
        #endregion

        #region State
        private void UpdateStates(float deltaTime)
        {
            for (int i = 0; i < m_StateLayers.Count; i++)
            {
                if (m_StateLayers[i] == null) continue;
                m_StateLayers[i].Update(deltaTime);
            }
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

        public void AddState(string stateName, bool isBlendTree, string groupName = null, int layer = 0)
        {
            StateLayer stateLayer = GetStateLayer(layer);
            if (stateLayer != null) { AddState(stateName, isBlendTree, groupName); }
        }

        public void RemoveState(string stateName, string groupName = null, int layer = 0)
        {
            StateLayer stateLayer = GetStateLayer(layer);
            if (stateLayer != null) { RemoveState(stateName); }
        }

        public void EnableState(string stateName, string groupName = null, int layer = 0)
        {
            StateLayer stateLayer = GetStateLayer(layer);
            if (stateLayer != null) { EnableState(stateName); }
        }

        public void DisableState(string stateName, string groupName = null, int layer = 0)
        {
            StateLayer stateLayer = GetStateLayer(layer);
            if (stateLayer != null) { DisableState(stateName); }
        }

        public void SetInputWeight(string stateName, float weight, string groupName = null, int layer = 0)
        {
            StateLayer stateLayer = GetStateLayer(layer);
            if (stateLayer != null) { SetInputWeight(stateName, weight); }
        }
        #endregion
    }

    // 暂时不需要这个设计，过于冗杂，直接保存状态归属哪个集合
    /*private class StateGroup
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

            int emptyIndex = m_States.FindIndex(s => s == null);
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

        public List<StateInfo> GetStates()
        {
            return m_States;
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
    }*/
}


