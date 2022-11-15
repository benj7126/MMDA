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
            /*
            HumanBodyBones boneEnum = (HumanBodyBones) Enum.Parse(typeof(HumanBodyBones), bone);
            
            Transform boneTransfrom = animator.GetBoneTransform(boneEnum);
            */
        }
    }

    // Update is called once per frame
    void Update()
    {
        Dictionary<HumanBodyBones, AnimationLoader.rotNPos> bonePos = al.getBoneMovementAtFrame("Gimme×Gimme", (uint) fakeFrame);

        Debug.Log(bonePos.Keys.Count);

        foreach (KeyValuePair<HumanBodyBones, AnimationLoader.rotNPos> bone in bonePos)
        {
            Transform boneTransfrom = animator.GetBoneTransform(bone.Key);

            boneTransfrom.localPosition = bone.Value.vec;
            boneTransfrom.localRotation = bone.Value.rot;

            //Debug.Log(bone.Value);
        }
    }
}
