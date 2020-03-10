/**
 *Created on 2020.2
 *Author:ZhangYuhao
 *Title: 动画控制器状态层
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace CostumeAnimator
{
    public partial class PlayableStateController
    {
        private class StateLayer : IPlayableAnimatorNode
        {
            private List<StateInfo> m_States;
            private PlayableGraph m_Graph;

            private AnimationMixerPlayable m_Mixer;
            private PlayableAnimatorParameter m_Params;

            private AvatarMask m_AvatarMask;
            public AvatarMask avatarMask
            {
                get { return m_AvatarMask; }
                set { m_AvatarMask = value; }
            }

            public int layerIndex;
            public float weight;
            public bool isAdditive;
            public bool IKPass;
            public bool isLayerDirty;

            private Dictionary<string, float> _lastParamValue;

            public StateInfo this[int i]
            {
                get
                {
                    return m_States[i];
                }
            }

            public int Count { get { return m_Count; } }
            private int m_Count;

            public StateLayer(int layerIndex, PlayableGraph graph, PlayableAnimatorParameter param)
            {
                this.layerIndex = layerIndex;
                m_States = new List<StateInfo>();
                m_Graph = graph;
                m_Params = param;
                m_Mixer = AnimationMixerPlayable.Create(m_Graph, 1, true);
                //isLayerDirty = true;

                _lastParamValue = new Dictionary<string, float>();
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

            /// <summary>
            /// 组装插值速度
            /// </summary>
            /// <param name="state"></param>
            /// <param name="targetWeight"></param>
            /// <param name="time"></param>
            private void SetupLerp(StateInfo state, float targetWeight, float time)
            {
                float travel = Mathf.Abs(state.weight - targetWeight);
                float newSpeed = time != 0f ? travel / time : Mathf.Infinity;

                // If we're fading to the same target as before but slower, assume CrossFade was called multiple times and ignore new speed
                if (state.fading && Mathf.Approximately(state.targetWeight, targetWeight) && newSpeed < state.fadeSpeed)
                    return;

                state.FadeTo(targetWeight, newSpeed);
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

                    // 处理混合树状态
                    if (state.isBlendTree)
                    {
                        bool isChangeParam = false;
                        float lastParam = 0;
                        float param = m_Params.GetFloat(state.blendTreeParameter);
                        isChangeParam = !_lastParamValue.TryGetValue(state.blendTreeParameter, out lastParam);
                        if (!isChangeParam)
                        {
                            isChangeParam = lastParam != param;
                        }
                        if (isChangeParam)
                        {
                            state.CalBlendTreeWeight(param);
                            _lastParamValue[state.blendTreeParameter] = param;
                        }
                    }

                    // 处理状态过渡 设置权重
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

                    // 处理状态开关
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

                    // 处理非循环状态的自然关闭
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

                // 有状态权重变化 更改playable权重
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

            public void AddState(string stateName, bool isBlendTree, Playable playable, AnimationClip clip = null, string groupName = null, BlendTreeConfig[] blendTreePlayables = null, string blendTreeParam = null)
            {
                if (FindState(stateName) != null)
                {
                    Debug.LogErrorFormat("Add state fail, state:{0} has existed!", stateName);
                    return;
                }

                if (isBlendTree && (blendTreePlayables == null || blendTreeParam == null))
                {
                    Debug.LogError("BlendTreePlayables or blendTreeParam is null but isBlendTree is true!");
                    return;
                }

                StateInfo state = new StateInfo();
                state.stateName = stateName;
                state.isBlendTree = isBlendTree;
                state.stateGroupName = groupName;

                state.SetPlayable(playable);
                if (isBlendTree)
                {
                    state.SetBlendTreePlayable(blendTreePlayables);
                    state.blendTreeParameter = blendTreeParam;
                }
                else
                {
                    state.Clip = clip;
                }
                state.Pause();

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
                    StopAllState();
                    state.Enable();
                    state.ForceWeight(1.0f);
                }
            }

            public void EnableState(string stateName, float fixedTime)
            {
                StateInfo state = FindState(stateName);
                if (state != null)
                {
                    StopAllState();
                    state.Enable();

                    if (fixedTime >= 0)
                    {
                        state.SetTime(fixedTime);
                    }
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

            public void StopAllState()
            {
                for (int i = 0; i < m_States.Count; i++)
                {
                    if (m_States[i] == null) continue;
                    m_States[i].Stop();
                }
            }

            public void Crossfade(string stateName, float time, bool isNormalnize)
            {
                if (time == 0)
                {
                    EnableState(stateName);
                    return;
                }

                int stateIndex = -1;
                for (int i = 0; i < m_States.Count; i++)
                {
                    StateInfo state = m_States[i];
                    if (state == null)
                        continue;

                    if (state.stateName == stateName)
                    {
                        state.Enable();
                        stateIndex = state.index;
                    }

                    if (state.enabled == false)     // 跳过停止的状态
                        continue;

                    float targetWeight = state.index == stateIndex ? 1.0f : 0.0f;   // 还在运行的旧状态，权重设置为0，新状态的权重设置为1

                    time = isNormalnize ? (Mathf.Clamp01(time) * state.playableDuration) : time;
                    SetupLerp(state, targetWeight, time);
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

            public void SetPlayableOutput(int outputPort, int inputPort, Playable inputNode)
            {
                m_Graph.Connect(m_Mixer, 0, inputNode, inputPort);
            }
        }
    }
}


