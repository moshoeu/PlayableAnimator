/******************************************************************
** 文件名:  IResourceHandle.cs
** 版  权:  (C)  
** 创建人:  moshoeu
** 日  期:  2021/9/10 17:19:43
** 描  述:  资源句柄接口
**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
*******************************************************************/

using UnityEngine;

namespace AnimationPlayer
{
    public interface IResourceHandle
    {
        /// <summary>
        /// 获取资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetResource<T>() where T : Object;

        /// <summary>
        /// 释放资源
        /// </summary>
        void ReleaseResource();
    }
}
