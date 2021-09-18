/******************************************************************
** 文件名:  APClipState.cs
** 版  权:  (C)  
** 创建人:  moshoeu
** 日  期:  2021/9/10 16:58:12
** 描  述:  切片状态
**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
*******************************************************************/

using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine;

namespace AnimationPlayer
{
    public class APClipState : IAPState
    {
        /// <summary>
        /// 切片playable
        /// </summary>
        private AnimationClipPlayable m_clipPlayable;

        /// <summary>
        /// 资源句柄
        /// </summary>
        private IResourceHandle m_resourceHandle;

        /// <summary>
        /// 状态ID
        /// </summary>
        private EAPStateID m_stateID;

        #region 接口
        public Playable GetOutputPlayable()
        {
            return m_clipPlayable;
        }

        public EAPStateID GetStateID()
        {
            return m_stateID;
        }

        public void OnCreate(PlayableGraph gragh, IResourceHandle resourceHandle, EAPStateID stateID)
        {
            m_resourceHandle = resourceHandle;

            AnimationClip clip = resourceHandle.GetResource<AnimationClip>();
            m_clipPlayable = AnimationClipPlayable.Create(gragh, clip);

            m_stateID = stateID;
        }

        public void OnDestroy()
        {
            m_clipPlayable.Destroy();

            m_resourceHandle.ReleaseResource();
        }

        public void OnEnter()
        {
            m_clipPlayable.Play();
        }

        public void OnExit()
        {
            m_clipPlayable.Pause();
            m_clipPlayable.SetTime(0);
        }

        public void OnUpdate(float deltaTime)
        {
            
        }

        public void SetApplyFootIK(bool enable)
        {
            m_clipPlayable.SetApplyFootIK(enable);
        }

        public void SetSpeed(float speed)
        {
            m_clipPlayable.SetSpeed(speed);
        }

        public void SetTime(float time)
        {
            m_clipPlayable.SetTime(time);
        }

        #endregion
    }
}
