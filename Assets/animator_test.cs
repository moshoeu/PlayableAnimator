using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animator_test : MonoBehaviour
{
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        StartCoroutine(test());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator test()
    {
        while (true)
        {
            animator.CrossFadeInFixedTime("test1", 0.5f);

            yield return new WaitForSeconds(0.2f);

            animator.CrossFadeInFixedTime("test2", 0.5f);

            yield return new WaitForSeconds(0.2f);

            animator.CrossFadeInFixedTime("test3", 0.5f);

            yield return new WaitForSeconds(0.2f);
        }
    }
}
