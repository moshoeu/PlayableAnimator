using UnityEngine;
using UnityEngine.Playables;

namespace PlayableAnimator
{
    public class PlayableAmimatorDriver : PlayableBehaviour
    {
        private PlayableGraph m_Graph;
        private Playable m_Playable;
        private PlayableStateController m_StateController;

        public PlayableAmimatorDriver(PlayableGraph graph, PlayableStateController ctrl)
        {
            m_Graph = graph;
            m_StateController = ctrl;
        }

        public override void OnPlayableCreate(Playable playable)
        {
            m_Playable = playable;
            m_StateController.SetPlayableOutput(0, 0, playable);
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            
        }

        /// <summary>
        /// This function is called during the PrepareFrame phase of the PlayableGraph.
        /// PrepareFrame should be used to do topological modifications, change connection weights, time changes , etc.
        /// </summary>
        /// <param name="playable"></param>
        /// <param name="info"></param>
        public override void PrepareFrame(Playable playable, FrameData info)
        {
            m_StateController.Update(info.deltaTime);
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            
        }
    }
}
