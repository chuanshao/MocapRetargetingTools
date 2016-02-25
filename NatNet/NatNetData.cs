/**
 * Original from johny3212 (https://forums.naturalpoint.com/viewtopic.php?f=59&t=10454&start=30#p57378)
 * Adapted by Matt Oskamp (https://github.com/MattOskamp/UnityOptitrack)
 * Rewritten & extended by Jan Kolkmeier (https://github.com/jankolkmeier/MocapRetargetingTools)
 */
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace NatNetStreaming {

    public class NatNetData {

        // Not needed anymore, but might be useful in the future.
        public static Dictionary<string, string> MecanimMapping = new Dictionary<string, string>() {
            { "Hips", "Hips" },
            { "Spine", "Spine" },
            { "Spine1", "Chest" },
            { "Neck", "Neck" },
            { "Head", "Head" },
            { "LeftShoulder", "LeftShoulder" },
            { "LeftArm", "LeftUpperArm" },
            { "LeftForeArm", "LeftLowerArm" },
            { "LeftHand", "LeftHand" },
            { "RightShoulder", "RightShoulder" },
            { "RightArm", "RightUpperArm" },
            { "RightForeArm", "RightLowerArm" },
            { "RightHand", "RightHand" },
            { "LeftUpLeg", "LeftUpperLeg" },
            { "LeftLeg", "LeftLowerLeg" },
            { "LeftFoot", "LeftFoot" },
            { "RightUpLeg", "RightUpperLeg" },
            { "RightLeg", "RightLowerLeg" },
            { "RightFoot", "RightFoot" },
            { "LeftToeBase", "LeftToes" },
            { "RightToeBase", "RightToes" },
            { "LeftHandThumb1", "Left Thumb Proximal" },
            { "LeftHandThumb2", "Left Thumb Intermediate" },
            { "LeftHandThumb3", "Left Thumb Distal" },
            { "LeftHandIndex1", "Left Index Proximal" },
            { "LeftHandIndex2", "Left Index Intermediate" },
            { "LeftHandIndex3", "Left Index Distal" },
            { "LeftHandMiddle1", "Left Middle Proximal" },
            { "LeftHandMiddle2", "Left Middle Intermediate" },
            { "LeftHandMiddle3", "Left Middle Distal" },
            { "LeftHandRing1", "Left Ring Proximal" },
            { "LeftHandRing2", "Left Ring Intermediate" },
            { "LeftHandRing3", "Left Ring Distal" },
            { "LeftHandPinky1", "Left Little Proximal" },
            { "LeftHandPinky2", "Left Little Intermediate" },
            { "LeftHandPinky3", "Left Little Distal" },
            { "RightHandThumb1", "Right Thumb Proximal" },
            { "RightHandThumb2", "Right Thumb Intermediate" },
            { "RightHandThumb3", "Right Thumb Distal" },
            { "RightHandIndex1", "Right Index Proximal" },
            { "RightHandIndex2", "Right Index Intermediate" },
            { "RightHandIndex3", "Right Index Distal" },
            { "RightHandMiddle1", "Right Middle Proximal" },
            { "RightHandMiddle2", "Right Middle Intermediate" },
            { "RightHandMiddle3", "Right Middle Distal" },
            { "RightHandRing1", "Right Ring Proximal" },
            { "RightHandRing2", "Right Ring Intermediate" },
            { "RightHandRing3", "Right Ring Distal" },
            { "RightHandPinky1", "Right Little Proximal" },
            { "RightHandPinky2", "Right Little Intermediate" },
            { "RightHandPinky3", "Right Little Distal" }
        };

        // marker(set)s?
        public Dictionary<int, NatNetRigidBody> rigidBodies;
        public Dictionary<int, NatNetSkeleton> skeletons;

        public NatNetData() {
            rigidBodies = new Dictionary<int, NatNetRigidBody>();
            skeletons = new Dictionary<int, NatNetSkeleton>();
        }
    }

    public class NatNetMarker {
        public int ID = -1;
        public Vector3 pos;
    }

    public class NatNetRigidBody {

        public string name = "";
        public int ID = -1;
        public int parentID = -1;
        public Vector3 offset;
        public Vector3 position;
        public Quaternion rotation;
        public bool tracked;
        public Transform bone = null;

        public NatNetRigidBody(int id) {
            ID = id;
        }
    }

    public class NatNetSkeleton {
        public string name = "";
        public int ID = -1;
        public Dictionary<int, NatNetRigidBody> bones;
        public bool receivedDescription = false;
        public bool initialized = false;
        public Transform root = null;
        public Animator animator = null;
        public RetargetSource retargetSource = null;

        public NatNetSkeleton(int id) {
            ID = id;
            bones = new Dictionary<int, NatNetRigidBody>();
        }
    }
}