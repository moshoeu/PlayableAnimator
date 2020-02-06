using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace PlayableAnimator
{
    [RequireComponent(typeof(Animator))]
    public class PlayableAnimator : MonoBehaviour
    {
        private PlayableGraph m_Graph;
        private Animator m_Animator;
        private PlayableStateController m_StateController;
        private PlayableBehaviour m_PlayableBehaviour;

        private bool m_IsInitialized = false;

        #region Life Method
        private void Start()
        {
            Initialize();
        }

        private void OnEnable()
        {
            
        }

        private void OnDisable()
        {
            
        }

        private void OnDestroy()
        {
            
        }

        private void Update()
        {

        }
        #endregion

        private void Initialize()
        {
            if (m_IsInitialized) return;
            m_Animator = GetComponent<Animator>();
            m_Graph = PlayableGraph.Create();
            m_StateController = new PlayableStateController(m_Graph);

            m_IsInitialized = true;
        }
    }
}

