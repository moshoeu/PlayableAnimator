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
        /// 输出节点
        /// </summary>
        private Playable m_output;

        /// <summary>
        /// 切片playable
        /// </summary>
        private AnimationClipPlayable m_clipPlayable;

        /// <summary>
        /// 资源句柄
        /// </summary>
        private IResourceHandle m_resourceHandle;

        #region 接口
        public Playable GetOutputPlayable()
        {
            return m_output;
        }

        public void OnCreate(PlayableGraph gragh, IResourceHandle resourceHandle)
        {
            m_resourceHandle = resourceHandle;

            m_output = Playable.Create(gragh, 1);

            AnimationClip clip = resourceHandle.GetResource<AnimationClip>();
            m_clipPlayable = AnimationClipPlayable.Create(gragh, clip);

            gragh.Connect(m_clipPlayable, 0, m_output, 0);
            m_output.SetInputWeight(0, 1);
        }

        public void OnDestroy()
        {
            m_clipPlayable.Destroy();
            m_output.Destroy();

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
