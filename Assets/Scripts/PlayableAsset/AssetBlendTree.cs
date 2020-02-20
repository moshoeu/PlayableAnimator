using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CostumeAnimator
{
    [CreateAssetMenu(fileName = "BlendTree", menuName = "PlayableAnimator/BlendTree", order =1)]
    public class AssetBlendTree : ScriptableObject, IPlayableBlendTree
    {                                     

        public AssetStateController.Motion[] motions;
        public string stateName;
        public string parameter;

        public enum BlendTreeType
        {
            None = 0,
            _1D,
            //_2D
        }
        public BlendTreeType blendTreeType = BlendTreeType._1D;

        public void Sort()
        {
            if (motions == null) return;

            System.Array.Sort(motions, (a, b) =>
            {
                if (a.threshold > b.threshold) return 1;
                else if (a.threshold == b.threshold) return 0;
                else return -1;
            });
        }

        public void SetBlendTreeType()
        {
            if (motions == null) return;

            for (int i = 0; i < motions.Length; i++)
            {
                motions[i].blendTreeType = blendTreeType;
            }
        }

        public void AddStates(PlayableAnimator playableAnimator, int layer, string groupName = null)
        {
            PlayableStateController.BlendTreeConfig[] configs = new PlayableStateController.BlendTreeConfig[motions.Length];
            for (int i = 0; i < configs.Length; i++)
            {
                configs[i].clip = motions[i].clip;
                configs[i].speed = motions[i].speed;
                configs[i].threshold = motions[i].threshold;
            }
            if (playableAnimator.StateController.Params.ContainsFloat(parameter) == false)
            {
                playableAnimator.StateController.Params.AddFloat(parameter);
            }
            
            playableAnimator.AddBlendTree(configs, parameter, stateName, groupName, layer);
        }
    }
}


