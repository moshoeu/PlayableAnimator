using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class Test_Output : MonoBehaviour
{
    public AnimationClip clip01;
    public AnimationClip clip02;
    // Start is called before the first frame update
    void Start()
    {
        PlayableGraph graph = PlayableGraph.Create();
        AnimationClipPlayable clip1 = AnimationClipPlayable.Create(graph, clip01);
        AnimationClipPlayable clip2 = AnimationClipPlayable.Create(graph, clip02);
        AnimationMixerPlayable mixer = AnimationMixerPlayable.Create(graph, 2, true);
        mixer.SetInputWeight(4, 0.5f);
        mixer.SetInputWeight(9, 0.5f);

        AnimationPlayableOutput output = AnimationPlayableOutput.Create(graph, "out", GetComponent<Animator>());
        graph.Connect(clip1, 0, mixer, 4);
        graph.Connect(clip2, 0, mixer, 9);
        Playable playable = Playable.Create(graph, 1);
        graph.Connect(mixer, 0, playable, 0);
        output.SetSourcePlayable(playable);

        graph.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
