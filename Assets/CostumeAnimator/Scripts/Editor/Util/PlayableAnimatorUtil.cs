﻿/**
 *Created on 2020.3
 *Author:ZhangYuhao
 *Title: Unity AnimatorController 转换到 PlayableAnimator Asset 工具类
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

namespace CostumeAnimator
{
    public class PlayableAnimatorUtil
    {
        #region Create Asset
        public AssetStateController CreateControllerAsset(string name)
        {
            AssetStateController asset = ScriptableObject.CreateInstance<AssetStateController>();
            asset.name = name;
            AssetDatabase.CreateAsset(asset, GetNewAssetPath(name));
            AssetDatabase.Refresh();
            return asset;
        }

        public AssetStateLayer CreateLayerAsset(string name)
        {
            AssetStateLayer asset = ScriptableObject.CreateInstance<AssetStateLayer>();
            asset.name = name;
            AssetDatabase.CreateAsset(asset, GetNewAssetPath(name));
            AssetDatabase.Refresh();
            return asset;
        }

        public AssetStateGroup CreateGroupAsset(string name)
        {
            AssetStateGroup asset = ScriptableObject.CreateInstance<AssetStateGroup>();
            asset.name = name;
            AssetDatabase.CreateAsset(asset, GetNewAssetPath(name));
            AssetDatabase.Refresh();
            return asset;
        }

        public AssetBlendTree CreateBlendTreeAsset(string name)
        {
            AssetBlendTree asset = ScriptableObject.CreateInstance<AssetBlendTree>();
            asset.name = name;
            AssetDatabase.CreateAsset(asset, GetNewAssetPath(name));
            AssetDatabase.Refresh();
            return asset;
        }

        /// <summary>
        /// 获取新建Asset的路径
        /// </summary>
        /// <param name="ctrlName"></param>
        /// <returns></returns>
        private string GetNewAssetPath(string name)
        {
            string dir = null;

            if (Selection.activeObject != null)
            {
                dir = AssetDatabase.GetAssetPath(Selection.activeObject);
                if (Directory.Exists(dir) == false)
                {
                    var di = Directory.GetParent(dir);
                    dir = di.ToString();
                }
            }
            else
            {
                dir = "Assets";
            }

            return string.Format("{0}/{1}.asset", dir, name);
        }
        #endregion

        #region Trans
        /// <summary>
        /// 将AniamtorController转换为Asset
        /// </summary>
        /// <param name="path"></param>
        public void TransAnimator2Asset(string path)
        {
            // 生成动画控制器
            AnimatorController animCtrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            AssetStateController assetCtrl = CreateControllerAsset(animCtrl.name);

            // 生成动画层
            AnimatorControllerLayer[] animCtrlLayers = animCtrl.layers;
            assetCtrl.stateLayers = new AssetStateLayer[animCtrlLayers.Length];
            for (int i = 0; i < animCtrlLayers.Length; i++)
            {
                string layerName = string.Format("{0}_layer{1}", animCtrl.name, i );
                AssetStateLayer assetLayer = CreateLayerAsset(layerName);
                assetCtrl.stateLayers[i] = assetLayer;

                AnimatorControllerLayer animCtrlLayer = animCtrlLayers[i];
                assetLayer.avatarMask = animCtrlLayer.avatarMask;
                assetLayer.isAdditive = animCtrlLayer.blendingMode == AnimatorLayerBlendingMode.Additive;

                // 生成动画组
                List<AssetStateGroup> assetGroups = new List<AssetStateGroup>();

                // 生成默认动画组
                string defaultGroupName = string.Format("{0}_group_default", layerName);
                AnimatorStateMachine animCtrlStateMachine = animCtrlLayer.stateMachine;
                AssetStateGroup assetDefaultGroup = CreateGroupAsset(defaultGroupName);
                TransStateGroup(animCtrlStateMachine, assetDefaultGroup);
                assetGroups.Add(assetDefaultGroup);

                // 生成子动画组（子控制器）

                ChildAnimatorStateMachine[] subMachines = animCtrlStateMachine.stateMachines;
                for (int j = 0; j < subMachines.Length; j++)
                {
                    string subGroupName = string.Format("{0}_group_{1}", layerName, j);
                    AnimatorStateMachine originSubStateMachine = subMachines[j].stateMachine;
                    AssetStateGroup assetSubGroup = CreateGroupAsset(subGroupName);

                    TransStateGroup(originSubStateMachine, assetSubGroup, subGroupName);
                    assetGroups.Add(assetSubGroup);

                    if (originSubStateMachine.stateMachines != null && originSubStateMachine.stateMachines.Length > 0)
                    {
                        Debug.LogErrorFormat("not support more then 1 layer subMachines, subMachine name:{0}", originSubStateMachine.name);
                    }
                }

                assetLayer.stateGroups = assetGroups.ToArray();
            }
        }

        /// <summary>
        /// 把 AnimatorStateMachine 转换为 AssetStateGroup
        /// </summary>
        /// <param name="originStateMachine"></param>
        /// <param name="assetStateGroup"></param>
        /// <param name="groupName"></param>
        private void TransStateGroup(AnimatorStateMachine originStateMachine, AssetStateGroup assetStateGroup, string groupName = null)
        {
            ChildAnimatorState[] animCtrlStates = originStateMachine.states;
            List<AssetStateController.Motion> assetMotions = new List<AssetStateController.Motion>();
            List<AssetBlendTree> assetBlendTrees = new List<AssetBlendTree>();
            for (int j = 0; j < animCtrlStates.Length; j++)
            {
                Motion motion = animCtrlStates[j].state.motion;
                bool isBlendTree = motion is BlendTree;
                if (isBlendTree)
                {
                    BlendTree originBlendTree = motion as BlendTree;
                    string blendTreeName = string.Format("{0}_blendTree_{1}", assetStateGroup.name, originBlendTree.name);
                    AssetBlendTree assetBlendTree = CreateBlendTreeAsset(blendTreeName);
                    TransBlendTree(originBlendTree, assetBlendTree);
                    assetBlendTrees.Add(assetBlendTree);
                }
                else
                {
                    AnimationClip originClip = motion as AnimationClip;
                    AssetStateController.Motion newMotion = new AssetStateController.Motion();
                    
                    if (originClip != null)
                    {
                        newMotion.clip = originClip;
                        newMotion.speed = originClip.apparentSpeed;
                        newMotion.stateName = originClip.name;
                    }

                    assetMotions.Add(newMotion);
                }
            }

            assetStateGroup.groupName = groupName;
            assetStateGroup.motions = assetMotions.ToArray();
            assetStateGroup.blendTrees = assetBlendTrees.ToArray();
        }

        /// <summary>
        /// 把 BlendTree 转换为 AssetBlendTree
        /// </summary>
        /// <param name="originBlendTree"></param>
        /// <param name="assetBlendTree"></param>
        private void TransBlendTree(BlendTree originBlendTree, AssetBlendTree assetBlendTree)
        {
            assetBlendTree.stateName = originBlendTree.name;
            assetBlendTree.parameter = originBlendTree.blendParameter;
            if( originBlendTree.blendType == BlendTreeType.Simple1D)
            {
                assetBlendTree.blendTreeType = AssetBlendTree.BlendTreeType._1D;    // 目前只支持1D混合树
            }
            else
            {
                Debug.LogError("not support blendTree type except Simple1D");
                return;
            }

            ChildMotion[] originChilds = originBlendTree.children;
            assetBlendTree.motions = new AssetStateController.Motion[originChilds.Length];
            for (int i = 0; i < originChilds.Length; i++)
            {
                AssetStateController.Motion newMotion = new AssetStateController.Motion();
                AnimationClip clip = (originChilds[i].motion as AnimationClip);     // 混合树子节点 转换为clip类型
                newMotion.blendTreeType = assetBlendTree.blendTreeType;
                newMotion.clip = clip;
                newMotion.threshold = originChilds[i].threshold;
                newMotion.speed = clip.apparentSpeed;

                assetBlendTree.motions[i] = newMotion;
            }
            assetBlendTree.Sort();  // 按照threshold排序
        }
        #endregion
    }
}