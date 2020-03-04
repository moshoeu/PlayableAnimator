/**
 *Created on 2020.3
 *Author:ZhangYuhao
 *Title: Unity AnimatorController 转换到 PlayableAnimator Asset 工具类
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace CostumeAnimator
{
    public class AnimCtrlTransUtil
    {
        #region Create Asset
        public AssetStateController CreateControllerAsset(string name)
        {
            AssetStateController asset = ScriptableObject.CreateInstance<AssetStateController>();
            AssetDatabase.CreateAsset(asset, GetNewAssetPath(name));
            AssetDatabase.Refresh();
            return asset;
        }

        public AssetStateLayer CreateLayerAsset(string name)
        {
            AssetStateLayer asset = ScriptableObject.CreateInstance<AssetStateLayer>();
            AssetDatabase.CreateAsset(asset, GetNewAssetPath(name));
            AssetDatabase.Refresh();
            return asset;
        }

        public AssetStateGroup CreateGroupAsset(string name)
        {
            AssetStateGroup asset = ScriptableObject.CreateInstance<AssetStateGroup>();
            AssetDatabase.CreateAsset(asset, GetNewAssetPath(name));
            AssetDatabase.Refresh();
            return asset;
        }

        public AssetBlendTree CreateBlendTreeAsset(string name)
        {
            AssetBlendTree asset = ScriptableObject.CreateInstance<AssetBlendTree>();
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


    }
}
