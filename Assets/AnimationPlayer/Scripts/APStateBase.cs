/******************************************************************
** 文件名:  APStateBase.cs
** 版  权:  (C)  
** 创建人:  moshoeu
** 日  期:  2021/9/29
** 描  述:  状态基类
**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
*******************************************************************/

using UnityEngine.Playables;

namespace AnimationPlayer
{
    public abstract class APStateBase
    {
        /// <summary>
        /// 输出节点
        /// </summary>
        public Playable Output
        {
            protected set;
            get;
        }

        /// <summary>
        /// 状态ID
        /// </summary>
        public EAPStateID StateID
        {
            private set;
            get;
        }

        /// <summary>
        /// 是否空闲
        /// </summary>
        public bool m_IsFree;

        /// <summary>
        /// 上次进入空闲的时间
        /// </summary>
        public double m_LstEnterFreeTime;

        /// <summary>
        /// 已经释放掉
        /// </summary>
        public bool m_IsDisposed;

        /// <summary>
        /// 进入状态
        /// </summary>
        public virtual void OnEnter()
        {
            Output.Play();
        }

        /// <summary>
        /// 帧更新
        /// </summary>
        /// <param name="deltaTime"></param>
        public virtual void OnUpdate(float deltaTime)
        {

        }

        /// <summary>
        /// 退出状态
        /// </summary>
        public virtual void OnExit()
        {
            Output.Pause();
            Output.SetTime(0);
        }

        /// <summary>
        /// 设置时间
        /// </summary>
        /// <param name="time"></param>
        public virtual void SetTime(float time)
        {
            Output.SetTime(time);
        }

        /// <summary>
        /// 设置速度
        /// </summary>
        /// <param name="speed"></param>
        public virtual void SetSpeed(float speed)
        {
            Output.SetSpeed(speed);
        }

        /// <summary>
        /// 创建状态
        /// </summary>
        public abstract void OnCreate(PlayableGraph gragh, IResourceHandle resourceHandle);

        /// <summary>
        /// 销毁状态
        /// </summary>
        public abstract void OnDestroy();

        /// <summary>
        /// 开关IK
        /// </summary>
        /// <param name="enable"></param>
        public abstract void SetApplyFootIK(bool enable);

        public APStateBase (EAPStateID stateID)
        {
            StateID = stateID;
        }
    }
}
