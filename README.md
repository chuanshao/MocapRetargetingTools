# MocapRetargetingTools

Some scripts for capturing & retargeting motion streamed from xsens and optitrack/natnet systems to Mecanim Avatars in Unity3D.

## How to use

### XSens
If you want to retarget animations from XSens, use [their plugin](https://www.assetstore.unity3d.com/en/#!/content/11338) to drive
the MVNPuppet avatar. Attach RetargetSource to the MVNPuppet and RetargetTarget to your own avatar. Configure the src property to
refer to the MVNPuppet's RetargetSource script. Configure the skeletonRoot properties of both script to point to the respective
roots on the skeletons.

### NatNet / OptiTrack
NaturalPoint does not provide a nice way of consuming the NatNet live stream of their skeleton data, so it is included in this
project instead. You can attach the  NatNetManager script to an empty GameObject.
Then, configure rigidbodies and skeleton(s) in it. To retarget streamed Skeleton
motion to your avatar, attach the RetargetTarget script to it. Reference it in the skeleton section in the NatNetManager.
You don't need to set the src property of RetargetTarget, NatNet manager will take care of that at runtime.

### Requirements
The retargeting scripts makes use of Unity's HumanPoseHandler API (supportet since ~v5.3). For it to work, the characters must
be properly configured Mecanim agents. Messy topology will result in bad results.

## Credits for the NatNet streaming scripts
[Original from johny3212](https://forums.naturalpoint.com/viewtopic.php?f=59&t=10454&start=30#p57378)  
[Adapted by Matt Oskamp](https://github.com/MattOskamp/UnityOptitrack)  
[Rewritten & extended by Jan Kolkmeier](https://github.com/jankolkmeier/MocapRetargetingTools)  