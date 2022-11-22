using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationPlayer : MonoBehaviour
{
    public Animator animator;

    public AnimationLoader al;

    public int fakeFrame = 200;

    private Dictionary<HumanBodyBones, AnimationLoader.rotNPos> defaultModelMovement = new Dictionary<HumanBodyBones, AnimationLoader.rotNPos>();

    // Start is called before the first frame update
    void Start()
    {
        foreach (string bone in Enum.GetNames(typeof(HumanBodyBones)))
        {
            HumanBodyBones boneEnum = (HumanBodyBones) Enum.Parse(typeof(HumanBodyBones), bone);
            
            Transform boneTransfrom = animator.GetBoneTransform(boneEnum);

            AnimationLoader.rotNPos rp = new AnimationLoader.rotNPos();

            rp.vec = boneTransfrom.position;
            rp.rot = boneTransfrom.rotation;

            defaultModelMovement.Add(boneEnum, rp);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        Dictionary<HumanBodyBones, AnimationLoader.rotNPos> bonePos = al.getBoneMovementAtFrame("Gimme×Gimme", (uint) fakeFrame);

        //Debug.Log(bonePos.Keys.Count);

        foreach (KeyValuePair<HumanBodyBones, AnimationLoader.rotNPos> bone in bonePos)
        {
            Transform boneTransfrom = animator.GetBoneTransform(bone.Key);

            if (!defaultModelMovement.ContainsKey(bone.Key))
                continue;
            
            AnimationLoader.rotNPos baseRP = defaultModelMovement[bone.Key];

            //boneTransfrom.position = bone.Value.vec + baseRP.vec;

            boneTransfrom.localRotation = bone.Value.rot * baseRP.rot;
            //boneTransfrom.localRotation = Quaternion.Euler(bone.Value.rot.eulerAngles + baseRP.rot.eulerAngles);

            //Debug.Log(bone.Value);
        }
    }
}
