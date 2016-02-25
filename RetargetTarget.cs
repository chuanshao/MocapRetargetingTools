using UnityEngine;
using System.Collections;
using NatNetStreaming;

[RequireComponent(typeof(Animator))]
public class RetargetTarget : MonoBehaviour {

    public RetargetSource src;
    public Transform skeletonRoot;
    Animator animator;
    HumanPoseHandler poseHandler;
    
    void Start () {
        animator = GetComponent<Animator>();
        if (skeletonRoot == null) skeletonRoot = transform;
        poseHandler = new HumanPoseHandler(animator.avatar, skeletonRoot);
    }
	
	void Update () {
    }

    void LateUpdate() {
        if (src == null || animator == null) return;
        poseHandler.SetHumanPose(ref src.currentPose);
    }

    void OnAnimatorIK(int layerIndex) {

    }
}
