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
        /// 状态混合器
        /// </summary>
        public AnimationMixerPlayable StateMixer { get; private set; }

        /// <summary>
        /// 层ID = 层混合器上的输入索引
        /// </summary>
        public uint LayerID { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        private AvatarMask m_avatarMask;

        /// <summary>
        /// 
        /// </summary>
        public AvatarMask AvatarMask 
        { 
            get
            {
                return m_avatarMask;
            }
            set
            {
                m_avatarMask = value;

                if (value != null)
                {
                    m_layerMixer.SetLayerMaskFromAvatarMask(LayerID, value);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private bool m_applyFootIK;

        /// <summary>
        /// 
        /// </summary>
        public bool ApplyFootIK
        {
            get
            {
                return m_applyFootIK;
            }
            set
            {
                m_applyFootIK = value;
                foreach (var state in m_states)
                {
                    state.SetApplyFootIK(value);
                }
            }
        }

        /// <summary>
        /// 是否是叠加层
        /// </summary>
        private bool m_isAdditive;

        /// <summary>
        /// 混合模式是否是叠加 - 设置为false则为override模式
        /// </summary>
        public bool IsAdditive
        {
            get
            {
                return m_isAdditive;
            }
            set
            {
                m_isAdditive = value;
                m_layerMixer.SetLayerAdditive(LayerID, value);
            }
        }

        /// <summary>
        /// 所有状态
        /// </summary>
        private List<IAPState> m_states = new List<IAPState>();

        /// <summary>
        /// 层混合器
        /// </summary>
        private AnimationLayerMixerPlayable m_layerMixer;

        public APLayer(uint layerIdx, PlayableGraph gragh, AnimationLayerMixerPlayable layerMixer,
            bool applyFootIK = false, bool isAddtive = false, AvatarMask avatarMask = null)
        {
            LayerID = layerIdx;
            m_applyFootIK = applyFootIK;
            m_isAdditive = isAddtive;
            m_avatarMask = avatarMask;
            m_layerMixer = layerMixer;

            StateMixer = AnimationMixerPlayable.Create(gragh);
        }

        /// <summary>
        /// 添加一个状态
        /// </summary>
        /// <param name="stateID"></param>
        /// <param name="stateResHandle"></param>
        public bool AddState(EAPStateID stateID, IResourceHandle stateResHandle)
        {
            return true;
        }

        /// <summary>
        /// 添加一个状态 根据ID去资源库加载
        /// </summary>
        /// <param name="stateID"></param>
        /// <param name="isSyncLoad"></param>
        public bool AddState(EAPStateID stateID, bool isSyncLoad, System.Action<IAPState> onLoad = null)
        {
            return true;
        }
    }
}
