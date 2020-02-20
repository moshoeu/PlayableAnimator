using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CostumeAnimator
{
    [CreateAssetMenu(fileName = "StateController", menuName = "PlayableAnimator/StateController", order = 4)]
    public class AssetStateController : ScriptableObject
    {
        [System.Serializable]
        public class Motion
        {
            public AnimationClip clip;
            public float threshold;
            public float speed;
            public AssetBlendTree.BlendTreeType blendTreeType;
            public string stateName;
        }

        public AssetStateLayer[] stateLayers;

        public void AddStates(PlayableAnimator playableAnimator)
        {
            for (int i = 0; i < stateLayers.Length; i++)
            {
                stateLayers[i].AddLayer(playableAnimator, i);
                stateLayers[i].AddStates(playableAnimator, i);
            }
        }
    }
}

