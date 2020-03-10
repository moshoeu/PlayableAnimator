using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CostumeAnimator
{
    [CreateAssetMenu(fileName = "StateGroup", menuName = "PlayableAnimator/StateGroup", order = 2)]
    public class AssetStateGroup : ScriptableObject
    {
        public string groupName;
        public AssetStateController.Motion[] motions;
        public AssetBlendTree[] blendTrees;

        public void AddStates(PlayableAnimator playableAnimator, int layer)
        {
            if (blendTrees != null)
            {
                for (int i = 0; i < blendTrees.Length; i++)
                {
                    blendTrees[i].AddStates(playableAnimator, layer, groupName);
                }
            }

            if (motions != null)
            {
                for (int i = 0; i < motions.Length; i++)
                {
                    var motion = motions[i];
                    string stateName = string.IsNullOrEmpty(motion.stateName) ? motion.clip.name : motion.stateName;
                    playableAnimator.AddState(motion.clip, stateName, groupName, layer);
                }
            }
        }
    }
}

