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
        private Playable m_OutputPlayable;

        private bool m_IsInitialized = false;

        #region Life Method
        private void Start()
        {
            Initialize();
            testLoad();
        }

        private void OnEnable()
        {
            
        }

        private void OnDisable()
        {
            
        }

        private void OnDestroy()
        {
            m_Graph.Destroy();
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
            m_Graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            m_StateController = new PlayableStateController(m_Graph);

            var template = new PlayableAmimatorDriver();
            template.Initialize(m_Graph, m_StateController);
            m_OutputPlayable = ScriptPlayable<PlayableAmimatorDriver>.Create(m_Graph, template, 1);

            AnimationPlayableUtilities.Play(m_Animator, m_OutputPlayable, m_Graph);

            m_IsInitialized = true;
        }

        public void Play()
        {
            m_OutputPlayable.Play();
        }

        public void Pause()
        {
            m_OutputPlayable.Pause();
        }

        public void Play(string stateName, int layer = 0)
        {
            m_StateController.EnableState(stateName, layer);
        }

        public void PlayInFixedTime(string stateName, float fixedTime, int layer = 0)
        {
            m_StateController.EnableState(stateName, fixedTime, layer);
        }

        public void Crossfade(string stateName, float normalnizeTime, int layer = 0)
        {
            m_StateController.Crossfade(stateName, normalnizeTime, true, layer);
        }

        public void CrossfadeInFixedTime(string stateName, float fixedTime, int layer = 0)
        {
            m_StateController.Crossfade(stateName, fixedTime, false, layer);
        }

        // test
        private void AddState(AnimationClip clip, string stateName = null, string groupName = null, int layer = 0)
        {
            AnimationClipPlayable clipPlayable = AnimationClipPlayable.Create(m_Graph, clip);
            if (!clip.isLooping || clip.wrapMode == WrapMode.Once)
            {
                clipPlayable.SetDuration(clip.length);
            }
            m_StateController.AddState(stateName, false, clipPlayable);
        }
        public AnimationClip[] clips;
        private void testLoad()
        {
            for (int i = 0; i < clips.Length; i++)
            {
                AddState(clips[i], i.ToString());
            }
            StartCoroutine(testPlay());
        }
        IEnumerator testPlay()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.1f);
                CrossfadeInFixedTime("0", 0.2f);
                yield return new WaitForSeconds(0.2f);
                CrossfadeInFixedTime("1", 0.2f);
                yield return new WaitForSeconds(0.2f);
                CrossfadeInFixedTime("2", 0.2f);
            }
        }
    }
}

