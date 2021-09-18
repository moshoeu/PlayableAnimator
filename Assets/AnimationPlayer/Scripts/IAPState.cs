/******************************************************************
** 文件名:  IAPState.cs
** 版  权:  (C)  
** 创建人:  moshoeu
** 日  期:  2021/9/10
** 描  述:  状态运行时接口
**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
*******************************************************************/

using UnityEngine.Playables;

namespace AnimationPlayer
{
    public interface IAPState
    {
        /// <summary>
        /// 获取输出的Playable
        /// </summary>
        /// <returns></returns>
        Playable GetOutputPlayable();

        /// <summary>
        /// 进入状态
        /// </summary>
        void OnEnter();

        /// <summary>
        /// 帧更新
        /// </summary>
        /// <param name="deltaTime"></param>
        void OnUpdate(float deltaTime);

        /// <summary>
        /// 退出状态
        /// </summary>
        void OnExit();

        /// <summary>
        /// 创建状态
        /// </summary>
        void OnCreate(PlayableGraph gragh, IResourceHandle resourceHandle, EAPStateID stateID);

        /// <summary>
        /// 销毁状态
        /// </summary>
        void OnDestroy();

        /// <summary>
        /// 设置时间
        /// </summary>
        /// <param name="time"></param>
        void SetTime(float time);

        /// <summary>
        /// 设置速度
        /// </summary>
        /// <param name="speed"></param>
        void SetSpeed(float speed);

        /// <summary>
        /// 开关IK
        /// </summary>
        /// <param name="enable"></param>
        void SetApplyFootIK(bool enable);

        /// <summary>
        /// 获取状态ID
        /// </summary>
        /// <returns></returns>
        EAPStateID GetStateID();
    }
}
