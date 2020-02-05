using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace PlayableAnimator
{
    public class PlayableAmimatorDriver : PlayableBehaviour
    {
        private PlayableGraph m_Graph;
        private Playable m_Playable;
        private PlayableStateController m_StateController;

        public PlayableAmimatorDriver(PlayableGraph graph)
        {
            m_Graph = graph;
            m_StateController = new PlayableStateController(graph);
        }

        public override void OnPlayableCreate(Playable playable)
        {
            m_Playable = playable;
        }

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            //m_StateController.
        }
    }
}
