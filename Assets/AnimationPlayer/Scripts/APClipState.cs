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
    public class APClipState : APStateBase
    {
        /// <summary>
        /// 切片playable
        /// </summary>
        private AnimationClipPlayable m_clipPlayable;

        /// <summary>
        /// 资源句柄
        /// </summary>
        private IResourceHandle m_resourceHandle;

        public APClipState(EAPStateID stateID) : base(stateID)
        {

        }

        public override void OnCreate(PlayableGraph gragh, IResourceHandle resourceHandle)
        {
            m_resourceHandle = resourceHandle;

            AnimationClip clip = resourceHandle.GetResource<AnimationClip>();
            m_clipPlayable = AnimationClipPlayable.Create(gragh, clip);

            Output = m_clipPlayable;
        }

        public override void OnDestroy()
        {
            m_clipPlayable.Destroy();

            m_resourceHandle.ReleaseResource();
        }

        public override void SetApplyFootIK(bool enable)
        {
            m_clipPlayable.SetApplyFootIK(enable);
        }

    }
}
