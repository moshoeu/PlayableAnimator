using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using AnimationPlayer;

[RequireComponent(typeof(Animator))]
public class TestPlayable : MonoBehaviour
{
    class ClipHandle : IResourceHandle
    {
        private AnimationClip clip;

        public T GetResource<T>() where T : Object
        {
            return clip as T;
        }

        public void ReleaseResource()
        {
            
        }

        public ClipHandle(AnimationClip clip)
        {
            this.clip = clip;
        }

    }

    public AnimationClip clip;

    private APStateBase clipState;

    // Start is called before the first frame update
    void Start()
    {
        PlayableGraph playableGraph = PlayableGraph.Create();
        playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        var playable = Playable.Create(playableGraph);
        Debug.Log(playable.IsValid());

        //clipState = new APClipState(EAPStateID.None);
        //clipState.OnCreate(playableGraph, new ClipHandle(clip));
        //clipState.OnEnter();

        //Playable output = clipState.Output;

        //AnimationPlayableOutput playableOutput = AnimationPlayableOutput.Create(playableGraph, "out", GetComponent<Animator>());
        //playableOutput.SetSourcePlayable(output);

        playableGraph.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnGUI()
    {
        if ( GUI.Button(new Rect(100, 100, 200, 100), "动画速度降为0.5"))
        {
            clipState.SetSpeed(0.5f);
        }

        if (GUI.Button(new Rect(100, 250, 200, 100), "节点速度降为0.5"))
        {
            clipState.Output.SetSpeed(0.5f);
        }

        if (GUI.Button(new Rect(100, 400, 200, 100), "节点时间设置为0"))
        {
            clipState.Output.SetTime(0f);
        }

    }
}
