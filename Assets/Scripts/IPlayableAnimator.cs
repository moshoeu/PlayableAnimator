using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace PlayableAnimator
{
    public interface IPlayableAnimatorNode
    {
        void SetPlayableOutput(int outputPort, int desInputPort, Playable inputNode);
    }
}

