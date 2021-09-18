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
        /// <summary>
        /// 过渡速度
        /// </summary>
        private float m_transitionSpeed;

        /// <summary>
        /// 过渡权重
        /// </summary>
        private List<float> m_inputWeights = new List<float>();

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

            for (int i = 0; i < m_inputWeights.Count; i++)
            {
                Playable input = StateMixer.GetInput(i);
                if (false == input.IsValid())
                {
                    continue;
                }

                float inputWeight = m_inputWeights[i];
                inputWeight += m_transitionSpeed * deltatime * (m_crtPlayingInputIdx == i ? 1 : -1);
                inputWeight = Mathf.Clamp01(inputWeight);

                sumWeight += inputWeight;
            }

            // 归一化权重 
            for (int i = 0; i < m_inputWeights.Count; i++)
            {
                Playable input = StateMixer.GetInput(i);
                if (false == input.IsValid())
                {
                    continue;
                }

                m_inputWeights[i] /= sumWeight;
                float weight = m_inputWeights[i];
                StateMixer.SetInputWeight(i, weight);

                // 权重为0 断开连接
                if (weight <= Mathf.Epsilon)
                {
                    StateMixer.DisconnectInput(i);

                    // TODO: 根据策略 判断是否销毁该Playable 释放资源
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
            IAPState state = m_states.Find(s => s.GetStateID() == stateID);
            if (state == null)
            {
                // 如果找不到状态 同步加载状态
                if (false == AddState(stateID, true, (s) => state = s))
                {
                    Debug.LogError($"APLayer.cs: 过渡到状态[{stateID}]失败！ID为[{LayerID}]的层里不存在该状态，也无法从资源库中加载该状态！");
                    return;
                }
            }

            int freeIdx = FindFreeMixerInputIndex();

            // 过渡时间为0 则直接将权重设置为1 
            if (transtionTime <= Mathf.Epsilon)
            {
                for(int i = 0; i < m_inputWeights.Count; i++)
                {
                    m_inputWeights[i] = i == freeIdx ? 1 : 0;
                }
                m_transitionSpeed = 0;
            }
            else
            {
                m_inputWeights[freeIdx] = 0;
                m_transitionSpeed = 1 / transtionTime;
            }

            m_crtPlayingInputIdx = freeIdx;

            Playable statePlayable = state.GetOutputPlayable();
            statePlayable.SetTime(startTime);

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

            for (int i = 0; i < m_inputWeights.Count; i++)
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
                m_inputWeights.Add(0);
                int inputCount = m_inputWeights.Count;

                StateMixer.SetInputCount(inputCount);
                findIdx = inputCount - 1;
            }

            return findIdx;
        }
    }
}
