/**
 *Created on 2020.2
 *Author:ZhangYuhao
 *Title: 动画控制器组件
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace CostumeAnimator
{
    [RequireComponent(typeof(Animator))]
    public class PlayableAnimator : MonoBehaviour
    {
        private PlayableGraph m_Graph;
        private Animator m_Animator;
        private Playable m_OutputPlayable;
        private PlayableStateController m_StateController;
        public PlayableStateController StateController {get { return m_StateController; } }

        [SerializeField]
        private AssetStateController _assetStateController;

        private bool m_IsInitialized = false;

        #region Life Method
        private void Start()
        {
            Initialize();
            if (_assetStateController != null)
            {
                _assetStateController.AddStates(this);
            }
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

        public void AddState(AnimationClip clip, string stateName = null, string groupName = null, int layer = 0)
        {
            AnimationClipPlayable clipPlayable = AnimationClipPlayable.Create(m_Graph, clip);
            if (!clip.isLooping || clip.wrapMode == WrapMode.Once)
            {
                clipPlayable.SetDuration(clip.length);
            }
            m_StateController.AddState(stateName, clipPlayable, clip, groupName, layer);
        }

        public void AddBlendTree(PlayableStateController.BlendTreeConfig[] configs, string paramName, string stateName = null, string groupName = null, int layer = 0)
        {
            if (configs == null) return;
            for (int i = 0; i < configs.Length; i++)
            {
                AnimationClipPlayable clipPlayable = AnimationClipPlayable.Create(m_Graph, configs[i].clip);
                configs[i].playable = clipPlayable;
            }
            Playable playable = Playable.Create(m_Graph, 1);
            m_StateController.AddBlendTree(stateName, playable, configs, paramName);
        }
        //public AnimationClip[] clips;
        //private void testLoad()
        //{
        //    //for (int i = 0; i < clips.Length; i++)
        //    //{
        //    //    AddState(clips[i], i.ToString());
        //    //}
        //    AddBlendTree();
        //    StartCoroutine(testPlay());
        //}
        //IEnumerator testPlay()
        //{
        //    //while (true)
        //    //{
        //    //    yield return new WaitForSeconds(0.2f);
        //    //    CrossfadeInFixedTime("0", 0.2f);
        //    //    yield return new WaitForSeconds(0.2f);
        //    //    CrossfadeInFixedTime("1", 0.2f);
        //    //    yield return new WaitForSeconds(0.2f);
        //    //    CrossfadeInFixedTime("2", 0.2f);
        //    //}
        //    yield return new WaitForSeconds(0.2f);
        //    Play("tree");
        //}
    }
}

