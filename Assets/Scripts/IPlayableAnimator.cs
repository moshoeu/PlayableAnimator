/**
 *Created on 2020.2
 *Author:ZhangYuhao
 *Title: 动画控制器接口
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace CostumeAnimator
{
    public interface IPlayableAnimatorNode
    {
        void SetPlayableOutput(int outputPort, int inputPort, Playable inputNode);
    }

    public interface IPlayableBlendTree
    {
        void Sort();
        void SetBlendTreeType();
    }


    public interface IAnimatorParameter
    {
        int GetInt(string name);
        int GetInt(int id);
        void SetInt(string name, int val);
        void SetInt(int id, int val);
        int AddInt(string name);

        float GetFloat(string name);
        float GetFloat(int id);
        void SetFloat(string name, float val);
        void SetFloat(int id, float val);
        int AddFloat(string name);

        bool GetBool(string name);
        bool GetBool(int id);
        void SetBool(string name, bool val);
        void SetBool(int id, bool val);
        int AddBool(string name);

    }
}

