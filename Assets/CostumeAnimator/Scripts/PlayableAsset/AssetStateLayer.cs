using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CostumeAnimator
{
    [CreateAssetMenu(fileName = "StateLayer", menuName = "PlayableAnimator/StateLayer", order = 3)]
    public class AssetStateLayer : ScriptableObject
    {
        public AssetStateGroup[] stateGroups;
        public AvatarMask avatarMask;
        public bool isAdditive;

        public void AddStates(PlayableAnimator playableAnimator, int layer)
        {
            if (stateGroups != null)
            {
                for (int i = 0; i < stateGroups.Length; i++)
                {
                    stateGroups[i].AddStates(playableAnimator, layer);
                }
            }
        }

        public void AddLayer(PlayableAnimator playableAnimator, int layer)
        {
            var ctrl = playableAnimator.StateController;
            if (layer > 0)  // 0 为自动创建的默认层
            {
                ctrl.AddLayer(layer);
            }

            if (avatarMask != null)
            {
                ctrl.SetLayerMask(layer, avatarMask);
            }
            
            ctrl.SetLayerAddtive(layer, isAdditive);
        }
    }
}

