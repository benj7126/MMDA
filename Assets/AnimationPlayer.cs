using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationPlayer : MonoBehaviour
{
    public Animator animator;
    private Avatar avatar;

    public AnimationLoader al;

    public float fakeFrame = 0;

    // Start is called before the first frame update
    void Start()
    {
        avatar = animator.avatar;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
