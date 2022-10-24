using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationPlayer : MonoBehaviour
{
    public Animator animator;

    public AnimationLoader al;

    public int fakeFrame = 200;

    // Start is called before the first frame update
    void Start()
    {
        foreach (string bone in Enum.GetNames(typeof(HumanBodyBones)))
        {
            HumanBodyBones boneEnum = (HumanBodyBones) Enum.Parse(typeof(HumanBodyBones), bone);

            Transform boneTransfrom = animator.GetBoneTransform(boneEnum);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Dictionary<string, AnimationLoader.rotNPos> bonePos = al.getBoneMovementAtFrame("HandClap", (uint) fakeFrame);
        
        foreach (KeyValuePair<string, AnimationLoader.rotNPos> bone in bonePos)
        {
            if (Enum.TryParse<HumanBodyBones>(bone.Key, out var boneEnum))
            {
                Transform boneTransfrom = animator.GetBoneTransform(boneEnum);
                
                Debug.Log($"One {bone.Key} got through");
            }
        }
    }
}
