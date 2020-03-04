/**
 *Created on 2020.2
 *Author:ZhangYuhao
 *Title: 动画控制器参数
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CostumeAnimator
{
    public class PlayableAnimatorParameter : IAnimatorParameter
    {
        private class Parameter<T>
        {
            public string name;
            public T val;
            public bool isVaild;

            public Parameter(string name)
            {
                this.name = name;
                val = default(T);
                isVaild = true;
            }
        }

        private List<Parameter<int>> m_IntParams;
        private List<Parameter<float>> m_FloatParams;
        private List<Parameter<bool>> m_BoolParams;

        public int AddBool(string name)
        {
            if (m_BoolParams == null)
            {
                m_BoolParams = new List<Parameter<bool>>();
            }

            var param = new Parameter<bool>(name);

            int emptyIndex = m_BoolParams.FindIndex(b => b.isVaild == false);
            if (emptyIndex == -1)
            {
                emptyIndex = m_BoolParams.Count;
                m_BoolParams.Add(param);
            }
            else
            {
                m_BoolParams[emptyIndex] = param;
            }

            return emptyIndex;
        }

        public int AddFloat(string name)
        {
            if (m_FloatParams == null)
            {
                m_FloatParams = new List<Parameter<float>>();
            }

            var param = new Parameter<float>(name);

            int emptyIndex = m_FloatParams.FindIndex(b => b.isVaild == false);
            if (emptyIndex == -1)
            {
                emptyIndex = m_FloatParams.Count;
                m_FloatParams.Add(param);
            }
            else
            {
                m_FloatParams[emptyIndex] = param;
            }

            return emptyIndex;
        }

        public int AddInt(string name)
        {
            if (m_IntParams == null)
            {
                m_IntParams = new List<Parameter<int>>();
            }

            var param = new Parameter<int>(name);

            int emptyIndex = m_IntParams.FindIndex(b => b.isVaild == false);
            if (emptyIndex == -1)
            {
                emptyIndex = m_IntParams.Count;
                m_IntParams.Add(param);
            }
            else
            {
                m_IntParams[emptyIndex] = param;
            }

            return emptyIndex;
        }

        public bool GetBool(string name)
        {
            bool res = default(bool);
            int find = m_BoolParams.FindIndex(b => b.isVaild && b.name == name);
            if (find == -1)
            {
                Debug.LogErrorFormat("cant find name: {0} bool parameter", name);
            }
            else
            {
                res = m_BoolParams[find].val;
            }

            return res;
        }

        public bool GetBool(int id)
        {
            bool res = default(bool);
            if (id >= m_BoolParams.Count)
            {
                Debug.LogErrorFormat("cant find id: {0} bool parameter", id);
            }

            else
            {
                var param = m_BoolParams[id];
                if (param.isVaild == false)
                {
                    Debug.LogErrorFormat("id: {0} bool parameter is Invalid", id);
                }
                else
                {
                    res = param.val;
                }
            }

            return res;
        }

        public float GetFloat(string name)
        {
            float res = default(float);
            int find = m_FloatParams.FindIndex(b => b.isVaild && b.name == name);
            if (find == -1)
            {
                Debug.LogErrorFormat("cant find name: {0} float parameter", name);
            }
            else
            {
                res = m_FloatParams[find].val;
            }

            return res;
        }

        public float GetFloat(int id)
        {
            float res = default(float);
            if (id >= m_BoolParams.Count)
            {
                Debug.LogErrorFormat("cant find id: {0} bool parameter", id);
            }

            else
            {
                var param = m_FloatParams[id];
                if (param.isVaild == false)
                {
                    Debug.LogErrorFormat("id: {0} bool parameter is Invalid", id);
                }
                else
                {
                    res = param.val;
                }
            }

            return res;
        }

        public int GetInt(string name)
        {
            int res = default(int);
            int find = m_IntParams.FindIndex(b => b.isVaild && b.name == name);
            if (find == -1)
            {
                Debug.LogErrorFormat("cant find name: {0} int parameter", name);
            }
            else
            {
                res = m_IntParams[find].val;
            }

            return res;
        }

        public int GetInt(int id)
        {
            int res = default(int);
            if (id >= m_BoolParams.Count)
            {
                Debug.LogErrorFormat("cant find id: {0} bool parameter", id);
            }

            else
            {
                var param = m_IntParams[id];
                if (param.isVaild == false)
                {
                    Debug.LogErrorFormat("id: {0} bool parameter is Invalid", id);
                }
                else
                {
                    res = param.val;
                }
            }

            return res;
        }

        public void SetBool(string name, bool val)
        {
            int find = m_BoolParams.FindIndex(b => b.isVaild && b.name == name);
            if (find == -1)
            {
                Debug.LogErrorFormat("cant find name: {0} bool parameter!", name);
                return;
            }

            m_BoolParams[find].val = val;
        }

        public void SetBool(int id, bool val)
        {
            if (id >= m_BoolParams.Count)
            {
                Debug.LogErrorFormat("cant find id: {0} bool parameter", id);
                return;
            }

            var param = m_BoolParams[id];
            if (param.isVaild == false)
            {
                Debug.LogErrorFormat("id: {0} bool parameter is Invalid", id);
                return;
            }

            param.val = val;
        }

        public void SetFloat(string name, float val)
        {
            int find = m_FloatParams.FindIndex(b => b.isVaild && b.name == name);
            if (find == -1)
            {
                Debug.LogErrorFormat("cant find name: {0} bool parameter!", name);
                return;
            }

            m_FloatParams[find].val = val;
        }

        public void SetFloat(int id, float val)
        {
            if (id >= m_FloatParams.Count)
            {
                Debug.LogErrorFormat("cant find id: {0} bool parameter", id);
                return;
            }

            var param = m_FloatParams[id];
            if (param.isVaild == false)
            {
                Debug.LogErrorFormat("id: {0} bool parameter is Invalid", id);
                return;
            }

            param.val = val;
        }

        public void SetInt(string name, int val)
        {
            int find = m_IntParams.FindIndex(b => b.isVaild && b.name == name);
            if (find == -1)
            {
                Debug.LogErrorFormat("cant find name: {0} bool parameter!", name);
                return;
            }

            m_IntParams[find].val = val;
        }

        public void SetInt(int id, int val)
        {
            if (id >= m_IntParams.Count)
            {
                Debug.LogErrorFormat("cant find id: {0} bool parameter", id);
                return;
            }

            var param = m_IntParams[id];
            if (param.isVaild == false)
            {
                Debug.LogErrorFormat("id: {0} bool parameter is Invalid", id);
                return;
            }

            param.val = val;
        }

        public bool ContainsFloat(string name)
        {
            if (m_FloatParams == null) return false;

            return (m_FloatParams.FindIndex(p => p.isVaild && p.name == name) != -1);
        }
    }
}


