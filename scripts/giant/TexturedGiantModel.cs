using Godot;
using System;

public partial class TexturedGiantModel : Node3D
{
    [Export]
    public Skeleton3D ParentSkeleton { get; set; }

    private Skeleton3D skeleton;

    public override void _Ready()
    {
        base._Ready();

        skeleton = (Skeleton3D)GetNode("Armature/Skeleton3D");

        ParentSkeleton.SkeletonUpdated += UpdatePosition;
    }

    public void UpdatePosition()
    {
        for (int i = 0; i < skeleton.GetBoneCount(); i++)
        {
            Transform3D parentBoneTransform = ParentSkeleton.GetBoneGlobalPose(i);
            skeleton.SetBoneGlobalPose(i, parentBoneTransform);
        }
    }
}
