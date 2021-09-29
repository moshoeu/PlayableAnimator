/******************************************************************
** 文件名:  APLayer.cs
** 版  权:  (C)  
** 创建人:  moshoeu
** 日  期:  2021/9/15 11:41:02
** 描  述:  运行时动画层
**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
*******************************************************************/

using UnityEngine.Animations;
using UnityEngine.Playables;
using System.Collections.Generic;
using UnityEngine;

namespace AnimationPlayer
{
    public partial class APLayer
    {
        private class StateWeight
        {
            /// <summary>
            /// 状态索引
            /// </summary>
            public EAPStateID m_StateID;

            /// <summary>
            /// 权重
            /// </summary>
            public float m_Weight;
        }

        /// <summary>
        /// 过渡速度
        /// </summary>
        private float m_transitionSpeed;

        /// <summary>
        /// 过渡权重
        /// </summary>
        private List<StateWeight> m_inputStates = new List<StateWeight>();

        /// <summary>
        /// 当前播放的状态的输入索引
        /// </summary>
        private int m_crtPlayingInputIdx;

        /// <summary>
        /// 更新状态
        /// </summary>
        /// <param name="deltatime"></param>
        public void UpdateTranstion(float deltatime)
        {
            float sumWeight = 0;

            for (int i = 0; i < m_inputStates.Count; i++)
            {
                Playable input = StateMixer.GetInput(i);
                if (false == input.IsValid())
                {
                    continue;
                }

                float inputWeight = m_inputStates[i].m_Weight;
                inputWeight += m_transitionSpeed * deltatime * (m_crtPlayingInputIdx == i ? 1 : -1);
                inputWeight = Mathf.Clamp01(inputWeight);

                sumWeight += inputWeight;
            }

            // 归一化权重 
            for (int i = 0; i < m_inputStates.Count; i++)
            {
                Playable input = StateMixer.GetInput(i);
                if (false == input.IsValid())
                {
                    continue;
                }

                StateWeight stateWeight = m_inputStates[i];
                stateWeight.m_Weight /= sumWeight;
                float weight = stateWeight.m_Weight;
                StateMixer.SetInputWeight(i, weight);

                // 权重为0 断开连接
                if (weight <= Mathf.Epsilon)
                {
                    StateMixer.DisconnectInput(i);

                    APStateBase inputState = GetState(stateWeight.m_StateID);
                    if (inputState == null)
                    {
                        Debug.LogError($"UpdateTranstion: 断开连接时 状态[{stateWeight.m_StateID}]不存在");
                    }
                    else
                    {
                        inputState.m_IsFree = true;
                        inputState.m_LstEnterFreeTime = StateMixer.GetTime();
                    }
                }
            }
        }

        /// <summary>
        /// 过渡到状态
        /// </summary>
        /// <param name="stateID">状态ID</param>
        /// <param name="startTime">新状态开始时间</param>
        /// <param name="transtionTime">过渡时间</param>
        /// <param name="transtionCurve">过渡曲线</param>
        public void TranstionState(EAPStateID stateID, float startTime, float transtionTime, AnimationCurve transtionCurve = null)
        {
            APStateBase state = GetState(stateID);

            int freeIdx = FindFreeMixerInputIndex();

            // 过渡时间为0 则直接将权重设置为1 
            if (transtionTime <= Mathf.Epsilon)
            {
                for(int i = 0; i < m_inputStates.Count; i++)
                {
                    m_inputStates[i].m_Weight = i == freeIdx ? 1 : 0;
                }
                m_transitionSpeed = 0;
            }
            else
            {
                m_inputStates[freeIdx].m_Weight = 0;
                m_transitionSpeed = 1 / transtionTime;
            }

            m_inputStates[freeIdx].m_StateID = stateID;
            m_crtPlayingInputIdx = freeIdx;

            Playable statePlayable = state.Output;
            state.SetTime(startTime);

            // 状态已经连接上
            state.m_IsFree = false;

            StateMixer.ConnectInput(freeIdx, statePlayable, 0);
            StateMixer.SetInputWeight(freeIdx, 0);
        }

        /// <summary>
        /// 找到空闲的输入索引
        /// </summary>
        /// <returns></returns>
        private int FindFreeMixerInputIndex()
        {
            int findIdx = -1;

            for (int i = 0; i < m_inputStates.Count; i++)
            {
                Playable input = StateMixer.GetInput(i);
                if (false == input.IsValid())
                {
                    findIdx = i;
                    break;
                }
            }

            if (findIdx == -1)
            {
                m_inputStates.Add(new StateWeight());
                int inputCount = m_inputStates.Count;

                StateMixer.SetInputCount(inputCount);
                findIdx = inputCount - 1;
            }

            return findIdx;
        }
    }
}
