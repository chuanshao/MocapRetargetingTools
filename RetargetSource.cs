using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class RetargetSource : MonoBehaviour {

    public Transform skeletonRoot;
    [HideInInspector]
    public HumanPose currentPose;
    Animator animator;
    HumanPoseHandler poseHandler;


	void Start () {
        animator = GetComponent<Animator>();
        if (skeletonRoot == null) skeletonRoot = transform;
        poseHandler = new HumanPoseHandler(animator.avatar, skeletonRoot);
	}
	
	// Update is called once per frame
	void Update () {
        poseHandler.GetHumanPose(ref currentPose);
    }
}
