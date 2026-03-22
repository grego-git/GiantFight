using Godot;
using System;

public partial class GiantLimb : Area3D
{
    [Export]
    public string Bone { get; set; }
    [Export]
    public Skeleton3D Skeleton { get; set; }

    public bool Enabled { get; set; }

    private bool setOffset;
    private MeshInstance3D mesh;
    private Transform3D offsetToBone;

    public override void _Ready()
    {
        base._Ready();

        Skeleton.SkeletonUpdated += UpdatePosition;

        Enabled = true;

        mesh = (MeshInstance3D)GetNode("MeshInstance3D");
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
    }

    public void UpdatePosition()
    {
        GD.Print("UPDATING LIMBS");

        if (!setOffset)
        {
            Transform3D boneTransform = Skeleton.GlobalTransform * Skeleton.GetBoneGlobalPose(Skeleton.FindBone(Bone));
            offsetToBone = boneTransform.AffineInverse() * GlobalTransform;
            setOffset = true;
        }
        else 
        {
            Transform3D boneTransform = Skeleton.GlobalTransform * Skeleton.GetBoneGlobalPose(Skeleton.FindBone(Bone));
            GlobalTransform = boneTransform * offsetToBone;
        }
    }
}
