using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class Test_Output : MonoBehaviour
{
    public AnimationClip clip;
    // Start is called before the first frame update
    void Start()
    {
        PlayableGraph graph = PlayableGraph.Create();
        AnimationClipPlayable clip1 = AnimationClipPlayable.Create(graph, clip);
        AnimationMixerPlayable mixer = AnimationMixerPlayable.Create(graph, 1, true);
        mixer.SetInputWeight(0, 1f);
        AnimationPlayableOutput output = AnimationPlayableOutput.Create(graph, "out", GetComponent<Animator>());
        graph.Connect(clip1, 0, mixer, 0);
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
