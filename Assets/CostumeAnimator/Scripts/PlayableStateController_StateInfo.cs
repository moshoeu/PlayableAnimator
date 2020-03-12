/**
 *Created on 2020.2
 *Author:ZhangYuhao
 *Title: 动画控制器状态实例
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
        public class StateInfo
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
                if (m_Clip == null && m_BlendTreeConfigs == null) return;
                if (m_Enabled) return;

                m_EnabledDirty = true;
                m_Enabled = true;
            }

            public void Disable()
            {
                if (m_Enabled == false)  return;

                m_EnabledDirty = true;
                m_Enabled = false;
            }

            public void Pause()
            {
                m_Playable.Pause();
            }

            public void Play()
            {
                if (m_Clip == null && m_BlendTreeConfigs == null) return;
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
                if (m_Clip == null && m_BlendTreeConfigs == null) return;
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

                if (m_BlendTreeConfigs != null)
                {
                    for (int i = 0; i < m_BlendTreeConfigs.Length; i++)
                    {
                        var playable = m_BlendTreeConfigs[i].playable;
                        if (playable.IsValid())
                        {
                            playable.GetGraph().DestroySubgraph(playable);
                        }
                    }
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

            public void SetBlendTreePlayable(BlendTreeConfig[] configs)
            {
                m_BlendTreeConfigs = configs;

                if (configs != null && configs.Length > 0)
                {
                    m_ClipLength = configs[0].clip.length;
                }

                if (!m_IsBlendTreeRootInit)
                {
                    // 创建混合树混合节点，与状态playable相连
                    var graph = m_Playable.GetGraph();
                    m_BlendTreeMixer = AnimationMixerPlayable.Create(graph, m_BlendTreeConfigs.Length, true);
                    graph.Connect(m_BlendTreeMixer, 0, m_Playable, 0);
                    m_Playable.SetInputWeight(0, 1f);
                    m_IsBlendTreeRootInit = true;
                }
                else if (m_BlendTreeMixer.GetInputCount() != m_BlendTreeConfigs.Length)
                {
                    m_BlendTreeMixer.SetInputCount(m_BlendTreeConfigs.Length);
                }

                m_IsBlendTreeDirty = true;
            }

            /// <summary>
            /// 按照参数大小计算混合树各结点权重
            /// </summary>
            public void CalBlendTreeWeight(float param)
            {
                for (int i = 0; i < m_BlendTreeConfigs.Length - 1; i++)
                {
                    float curThreshold = m_BlendTreeConfigs[i].threshold;
                    float nextThreshold = m_BlendTreeConfigs[i + 1].threshold;
                    if (param >= curThreshold && param < nextThreshold)    // 位于两点之间
                    {
                        float curWeight = Mathf.Clamp01((nextThreshold - param) / (nextThreshold - curThreshold));
                        m_BlendTreeConfigs[i].weight = curWeight;
                        m_BlendTreeConfigs[i + 1].weight = 1 - curWeight;
                        i++;
                    }
                    else
                    {
                        m_BlendTreeConfigs[i].weight = 0;
                    }
                }

                if (param < m_BlendTreeConfigs[0].threshold)
                {
                    m_BlendTreeConfigs[0].weight = 1;
                }

                if (param >= m_BlendTreeConfigs[m_BlendTreeConfigs.Length - 1].threshold)
                {
                    m_BlendTreeConfigs[m_BlendTreeConfigs.Length - 1].weight = 1;
                }

                // 改变playable权重
                for (int i = 0; i < m_BlendTreeConfigs.Length; i++)
                {
                    var playable = m_BlendTreeConfigs[i].playable;
                    m_BlendTreeMixer.SetInputWeight(i, m_BlendTreeConfigs[i].weight);
                    playable.SetSpeed(m_BlendTreeConfigs[i].speed);

                    if (!m_IsBlendTreeInit)
                    {
                        var graph = m_Playable.GetGraph();
                        graph.Connect(playable, 0, m_BlendTreeMixer, i);
                    }
                }

                m_IsBlendTreeInit = true;
                m_IsBlendTreeDirty = false;
            }

            #endregion

            #region property

            #region BlendTree
            public bool isBlendTree;


            private bool m_IsBlendTreeRootInit = false;     // 混合树根节点playable初始化
            private bool m_IsBlendTreeDirty = false;
            private bool m_IsBlendTreeInit = false;         // 混合树子节点初始化
            public bool isBlendTreeDirty { get { return m_IsBlendTreeDirty; } }     // todo: 暂时不用 混合树参数改变无法通知到每个状态
            public string blendTreeParameter;
            private AnimationMixerPlayable m_BlendTreeMixer;

            private BlendTreeConfig[] m_BlendTreeConfigs;
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

            private AnimationClip m_Clip;
            public AnimationClip Clip
            {
                set
                {
                    m_Clip = value;
                    m_ClipLength = m_Clip.length;
                }
            }

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

            private float m_ClipLength;
            public float clipLength
            {
                get { return m_ClipLength; }
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
    }
}


