using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CostumeAnimator;

public class Test_Animator : MonoBehaviour
{
    private PlayableAnimator animator;
    float hSliderValue = 0;
    bool isAsk = false;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<PlayableAnimator>();
    }

    // Update is called once per frame
    void Update()
    {
        
        animator.StateController.Params.SetFloat("dir", hSliderValue);
    }

    private void OnGUI()
    {

        if (GUI.Button(new Rect(20, 40, 80, 20), "待机动作"))
        {
            animator.CrossfadeInFixedTime("Drib_Stand_Cautious", 0.2f);
        }

        if (GUI.Button(new Rect(20, 80, 80, 20), "无球移动"))
        {
            animator.CrossfadeInFixedTime("Move", 0.2f);
        }

        if (GUI.Button(new Rect(20, 120, 80, 20), "要球"))
        {
            isAsk = !isAsk;
            animator.CrossfadeInFixedTime("Askedball_Stand", 0.2f, 1);
            animator.StateController.SetLayerWeight(1, isAsk ? 1f :0f);
        }

        hSliderValue = GUI.HorizontalSlider(new Rect(25, 25, 100, 30), hSliderValue, -180f, 180f);
    }
}
