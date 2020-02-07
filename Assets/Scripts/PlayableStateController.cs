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

namespace PlayableAnimator
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
            public float threshold;
            public float speed;
            public float weight;
        }

        public PlayableStateController(PlayableGraph graph, Animator animator)
        {
            m_Params = new PlayableAnimatorParameter();

            m_StateLayers = new List<StateLayer>();
            m_StateLayers.Add(new StateLayer(0, graph, m_Params));     // 添加默认层

            m_LayerMixer = AnimationLayerMixerPlayable.Create(graph, 1);
            m_LayerMixer.SetInputWeight(0, 1f);

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
        public int AddLayer()
        {
            int emptyIndex = m_StateLayers.FindIndex(l => l == null);
            int newLayerIndex = 0;
            if (emptyIndex != -1)
            {
                newLayerIndex = emptyIndex;
                m_StateLayers[newLayerIndex] = new StateLayer(newLayerIndex, m_Graph, m_Params);
            }
            else
            {
                newLayerIndex = m_StateLayers.Count;
                m_StateLayers.Add(new StateLayer(newLayerIndex, m_Graph, m_Params));

                m_LayerMixer.SetInputCount(m_StateLayers.Count);
            }

            return newLayerIndex;
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
                    layer.SetPlayableOutput(0, layer.layerIndex, m_LayerMixer);
                    layer.isLayerDirty = false;
                }
            }
        }
        #endregion

        #region State
        public void AddState(string stateName, Playable playable, string groupName = null, int layer = 0)
        {
            StateLayer stateLayer = GetStateLayer(layer);
            if (stateLayer != null) { stateLayer.AddState(stateName, false, playable, groupName); }
        }

        public void AddBlendTree(string stateName, Playable playable, BlendTreeConfig[] configs, string blendTreeParam, string groupName = null, int layer = 0)
        {
            StateLayer stateLayer = GetStateLayer(layer);
            if (stateLayer != null) { stateLayer.AddState(stateName, true, playable, groupName, configs, blendTreeParam); }
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
            if (layer >= m_StateLayers.Count)
            {
                Debug.LogErrorFormat("Get state layer fail, state group:{0} is not existing!", layer);
            }

            else
            {
                stateLayer = m_StateLayers[layer];
            }

            return stateLayer;
        }
        #endregion
    }
}


