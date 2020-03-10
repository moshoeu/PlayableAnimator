/**
 *Created on 2020.2
 *Author:ZhangYuhao
 *Title: 动画控制器状态管理
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace CostumeAnimator
{
    public partial class PlayableStateController : IPlayableAnimatorNode
    {
        private List<StateLayer> m_StateLayers;
        private PlayableGraph m_Graph;
        private AnimationLayerMixerPlayable m_LayerMixer;
        private PlayableAnimatorParameter m_Params;
        public PlayableAnimatorParameter Params { get { return m_Params; } }

        public struct BlendTreeConfig
        {
            public Playable playable;
            public AnimationClip clip;
            public float threshold;
            public float speed;
            public float weight;
        }

        public PlayableStateController(PlayableGraph graph)
        {
            m_Params = new PlayableAnimatorParameter();

            m_StateLayers = new List<StateLayer>();

            m_LayerMixer = AnimationLayerMixerPlayable.Create(graph, 1);
            m_LayerMixer.SetInputWeight(0, 1f);

            var layer = new StateLayer(0, graph, m_Params);
            layer.SetPlayableOutput(0, layer.layerIndex, m_LayerMixer);
            m_StateLayers.Add(layer);     // 添加默认层

            m_Graph = graph;
        }

        public void Update(float deltaTime)
        {
            UpdateStates(deltaTime);
            UpdateLayers(deltaTime);
        }

        public void SetPlayableOutput(int outputPort, int inputPort, Playable inputNode)
        {
            m_Graph.Connect(m_LayerMixer, 0, inputNode, inputPort);
        }

        #region Layer
        public void AddLayer(int layerIndex)
        {
            if (-1 != m_StateLayers.FindIndex(l=> l!=null && l.layerIndex == layerIndex))
            {
                Debug.LogErrorFormat("layer:{0} has existed!", layerIndex);
                return;
            }

            var layer = new StateLayer(layerIndex, m_Graph, m_Params);

            int emptyIndex = m_StateLayers.FindIndex(l => l == null);
            if (emptyIndex != -1)
            {
                m_StateLayers[emptyIndex] = layer;
            }
            else
            {
                m_StateLayers.Add(layer);
                m_LayerMixer.SetInputCount(m_StateLayers.Count);
            }
            layer.SetPlayableOutput(0, layer.layerIndex, m_LayerMixer);
        }

        public void RemoveLayer(int layerIndex)
        {
            int removeIndex = m_StateLayers.FindIndex(l => l != null && l.layerIndex == layerIndex);
            m_StateLayers[removeIndex].Release();
            m_StateLayers[removeIndex] = null;
        }

        public void SetLayerMask(int layerIndex, AvatarMask mask)
        {
            if (!CheckLayerIfExist(layerIndex)) return;

            StateLayer layer = m_StateLayers[layerIndex];
            layer.avatarMask = mask;
            m_LayerMixer.SetLayerMaskFromAvatarMask((uint)layerIndex, mask);    // todo: 观察是否可以放这里，效果不好则放在UpdateLayers里
        }

        public void SetLayerAddtive(int layerIndex, bool isAdditive)
        {
            if (!CheckLayerIfExist(layerIndex)) return;

            StateLayer layer = m_StateLayers[layerIndex];
            layer.isAdditive = isAdditive;
            m_LayerMixer.SetLayerAdditive((uint)layerIndex, isAdditive);        // todo: 观察是否可以放这里，效果不好则放在UpdateLayers里
        }

        public void SetLayerIKPass(int layerIndex, bool _IKPass)
        {
            if (!CheckLayerIfExist(layerIndex)) return;

            StateLayer layer = m_StateLayers[layerIndex];
            layer.IKPass = _IKPass;     // 只是个标志位
        }

        public void SetLayerWeight(int layerIndex, float weight)
        {
            if (!CheckLayerIfExist(layerIndex)) return;
            StateLayer layer = m_StateLayers[layerIndex];
            layer.weight = Mathf.Clamp01(weight);
            layer.isLayerDirty = true;
        }

        private bool CheckLayerIfExist(int layerIndex)
        {
            if (layerIndex >= m_StateLayers.Count || layerIndex < 0)
            {
                Debug.LogErrorFormat("Layer index:{0} is out of range!", layerIndex);
                return false;
            }

            StateLayer layer = m_StateLayers[layerIndex];
            if (layer == null)
            {
                Debug.LogErrorFormat("Layer index:{0} has destroyed!", layerIndex);
                return false;
            }

            return true;
        }

        private void UpdateLayers(float deltaTime)
        {
            for (int i = 0; i < m_StateLayers.Count; i++)
            {
                StateLayer layer = m_StateLayers[i];
                if (layer == null) continue;

                if (layer.isLayerDirty)
                {
                    m_LayerMixer.SetInputWeight(layer.layerIndex, layer.weight);
                    layer.isLayerDirty = false;
                }
            }
        }
        #endregion

        #region State
        public void AddState(string stateName, Playable playable, AnimationClip clip, string groupName = null, int layer = 0)
        {
            StateLayer stateLayer = GetStateLayer(layer);
            if (stateLayer != null) { stateLayer.AddState(stateName, false, playable, clip, groupName); }
        }

        public void AddBlendTree(string stateName, Playable playable, BlendTreeConfig[] configs, string blendTreeParam, string groupName = null, int layer = 0)
        {
            StateLayer stateLayer = GetStateLayer(layer);
            if (stateLayer != null) { stateLayer.AddState(stateName, true, playable, null, groupName, configs, blendTreeParam); }
        }

        public void RemoveState(string stateName, int layer = 0)
        {
            StateLayer stateLayer = GetStateLayer(layer);
            if (stateLayer != null) { stateLayer.RemoveState(stateName); }
        }

        public void EnableState(string stateName, int layer = 0)
        {
            StateLayer stateLayer = GetStateLayer(layer);
            if (stateLayer != null) { stateLayer.EnableState(stateName); }
        }

        public void EnableState(string stateName, float fixedTime, int layer = 0)
        {
            StateLayer stateLayer = GetStateLayer(layer);
            if (stateLayer != null) { stateLayer.EnableState(stateName, fixedTime); }
        }

        public void DisableState(string stateName, int layer = 0)
        {
            StateLayer stateLayer = GetStateLayer(layer);
            if (stateLayer != null) { stateLayer.DisableState(stateName); }
        }

        public void SetInputWeight(string stateName, float weight, int layer = 0)
        {
            StateLayer stateLayer = GetStateLayer(layer);
            if (stateLayer != null) { stateLayer.SetInputWeight(stateName, weight); }
        }

        public void Crossfade(string stateName, float fixedTime, bool isNormalnize, int layer = 0)
        {
            StateLayer stateLayer = GetStateLayer(layer);
            if (stateLayer != null) { stateLayer.Crossfade(stateName, fixedTime, isNormalnize); }
        }

        private void UpdateStates(float deltaTime)
        {
            for (int i = 0; i < m_StateLayers.Count; i++)
            {
                if (m_StateLayers[i] == null) continue;
                m_StateLayers[i].Update(deltaTime);
            }
        }

        private StateLayer GetStateLayer(int layer)
        {
            StateLayer stateLayer = null;

            int index = m_StateLayers.FindIndex((l => l != null && l.layerIndex == layer));
            stateLayer = (index == -1) ? stateLayer : m_StateLayers[index];

            return stateLayer;
        }
        #endregion
    }
}


