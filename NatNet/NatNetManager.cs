/**
 * Original from johny3212 (https://forums.naturalpoint.com/viewtopic.php?f=59&t=10454&start=30#p57378)
 * Adapted by Matt Oskamp (https://github.com/MattOskamp/UnityOptitrack)
 * Rewritten & extended by Jan Kolkmeier (https://github.com/jankolkmeier/MocapRetargetingTools)
 */
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using NatNetStreaming;

[System.Serializable]
public class RigidBodyObject {
    public GameObject gameObject;
    public bool applyRotation;
    public bool applyTranslation;
}

[System.Serializable]
public class SkeletonObject {
    public RetargetTarget target;
    // TODO: Choose skeleton by name / id;
}

public class NatNetManager : MonoBehaviour {

    public string clientName;
    public string localAdapter = "";
    public float fps;
    public RigidBodyObject[] rigidBodyObjects;
    public SkeletonObject[] skeletonObjects;
    public Dictionary<string, int> rbMap;
    public RuntimeAnimatorController defaultAnimatorController;
    public Avatar defaultAvatar;

    private bool _deinitValue = false;

    ~NatNetManager() {
        NatNetSocketClient.Close();
    }

    void OnApplicationQuit() {
        NatNetSocketClient.Close();
    }

    void Start() {
        Debug.Log("Starting NatNet client as " + clientName);
        rbMap = new Dictionary<string, int>();
        for (int i = 0; i < rigidBodyObjects.Length; i++) {
            rbMap[rigidBodyObjects[i].gameObject.name] = i;
        }

        NatNetSocketClient.localAdapter = localAdapter;
        NatNetSocketClient.Start();
    }



    // Update is called once per frame
    void Update() {

        NatNetSocketClient.Update();

        if (NatNetSocketClient.IsInit()) {
            NatNetData streamData = NatNetSocketClient.GetStreamData();

            foreach (KeyValuePair<int, NatNetRigidBody> rb in streamData.rigidBodies) {
                if (rbMap.ContainsKey(rb.Value.name)) {
                    int id = rbMap[rb.Value.name];
                    if (rigidBodyObjects[id].gameObject != null) {
                        if (rigidBodyObjects[id].applyTranslation) {
                            rigidBodyObjects[id].gameObject.transform.position = transform.TransformPoint(rb.Value.position);
                        }

                        if (rigidBodyObjects[id].applyRotation) {
                            rigidBodyObjects[id].gameObject.transform.rotation = transform.rotation * rb.Value.rotation;
                        }
                    }
                }
            }

            foreach (KeyValuePair<int, NatNetSkeleton> sk in streamData.skeletons) {
                if (sk.Value.receivedDescription && !sk.Value.initialized) {
                    InitializeSkeleton(sk.Value);
                } 

                if (sk.Value.initialized) {

                    foreach (KeyValuePair<int, NatNetRigidBody> boneKV in sk.Value.bones) {
                        boneKV.Value.bone.position = transform.TransformPoint(boneKV.Value.position);
                        boneKV.Value.bone.rotation = transform.rotation * boneKV.Value.rotation;
                    }

                    //sk.Value.poseHandler.GetHumanPose(ref sk.Value.currentPose);
                }
            }
        }

        if (_deinitValue) {
            _deinitValue = false;
            NatNetSocketClient.Close();
        }
    }

    void InitializeSkeleton(NatNetSkeleton sk) {
        Debug.Log("Initializing Skeleton " + sk.name);
        GameObject parent = new GameObject("_"+sk.name);
        parent.transform.parent = transform;
        parent.transform.localPosition = Vector3.zero;
        parent.transform.localRotation = Quaternion.identity;
        GameObject root = new GameObject("Root");
        root.transform.parent = parent.transform;
        root.transform.localPosition = Vector3.zero;
        root.transform.localRotation = Quaternion.identity;

        //List<HumanBone> humanBones = new List<HumanBone>();
        //List<SkeletonBone> skeletonBones = new List<SkeletonBone>();
        //HumanBone rootBone = new HumanBone();
        //rootBone.boneName = root.name;
        //humanBones.Add(rootBone);
        //SkeletonBone rootSkeletonBone = new SkeletonBone();
        //rootSkeletonBone.name = root.name;
        //skeletonBones.Add(rootSkeletonBone);

        foreach (KeyValuePair<int, NatNetRigidBody> boneKV in sk.bones) {
            string name = boneKV.Value.name;
            GameObject bone = new GameObject(name);
            bone.transform.parent = root.transform;
            boneKV.Value.bone = bone.transform;

            /*
            if (NatNetData.MecanimMapping.ContainsKey(name)) {
                HumanBone humanBone = new HumanBone();
                humanBone.boneName = name;
                humanBone.humanName = NatNetData.MecanimMapping[name.Substring(sk.name.Length + 1)];
                humanBone.limit.useDefaultValues = true;
                //Debug.Log(humanBone.boneName + " => " + humanBone.humanName);
                humanBones.Add(humanBone);
            }
            */
        }

        foreach (KeyValuePair<int, NatNetRigidBody> boneKV in sk.bones) {
            if (boneKV.Value.parentID > 0) {
                boneKV.Value.bone.parent = sk.bones[boneKV.Value.parentID].bone;
            } else {
                boneKV.Value.bone.parent = root.transform;
            }
            boneKV.Value.bone.transform.localPosition = boneKV.Value.offset;
            boneKV.Value.bone.transform.localRotation = Quaternion.identity;

            /*
            SkeletonBone skeletonBone = new SkeletonBone();
            skeletonBone.name = boneKV.Value.bone.name;
            skeletonBone.scale = new Vector3(1, 1, 1);
            skeletonBone.position = boneKV.Value.bone.transform.localPosition;
            skeletonBone.rotation = boneKV.Value.bone.transform.localRotation;
            skeletonBones.Add(skeletonBone);
            */
        }


        root.AddComponent<DebugSkeleton>();
        sk.animator = root.AddComponent<Animator>();
        /*
        HumanDescription humanDescription = new HumanDescription();
        humanDescription.skeleton = skeletonBones.ToArray();
        humanDescription.human = humanBones.ToArray();
        humanDescription.upperArmTwist = 0.5f;
        humanDescription.lowerArmTwist = 0.5f;
        humanDescription.upperLegTwist = 0.5f;
        humanDescription.lowerLegTwist = 0.5f;
        humanDescription.armStretch = 0.05f;
        humanDescription.legStretch = 0.05f;
        humanDescription.feetSpacing = 0.0f;
        
        sk.animator.avatar = AvatarBuilder.BuildHumanAvatar(root, humanDescription);
        sk.animator.avatar.name = "NatNetAvatar";
        sk.animator.runtimeAnimatorController = Instantiate(defaultAnimatorController);
        */
        sk.animator.avatar = defaultAvatar;
        sk.animator.runtimeAnimatorController = defaultAnimatorController;
        sk.animator.Rebind();
        sk.root = root.transform;
        sk.retargetSource = root.AddComponent<RetargetSource>();
        sk.retargetSource.skeletonRoot = root.transform;
        sk.initialized = true;


        for (int i = 0; i < skeletonObjects.Length; i++) {
            skeletonObjects[i].target.src = sk.retargetSource;
        }
        //Debug.Log("Fail? " + (sk.animator.GetBoneTransform(HumanBodyBones.Hips)==null).ToString());

    }

}