using Godot;
using System;

public partial class GiantHitBox : Area3D
{
    [Export]
    public string Bone { get; set; }
    [Export]
    public Skeleton3D Skeleton { get; set; }
    
    private bool setOffset;
    private MeshInstance3D mesh;
    private Transform3D offsetToBone;

    public override void _Ready()
    {
        base._Ready();

        BodyEntered += HitSomething;

        if (Skeleton != null && !string.IsNullOrEmpty(Bone))
        {
            Skeleton.SkeletonUpdated += UpdatePosition;
        }
        
        mesh = (MeshInstance3D)GetNode("MeshInstance3D");
        mesh.Visible = Constants.DEBUG;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        mesh.Visible = true;
    }

    public void HitSomething(Node3D body)
    {
        CharacterController controller = (CharacterController)body;
        controller.EmitSignal("Hit");
    }
    
    public void UpdatePosition()
    {
        if (!Skeleton.IsInsideTree())
            return;

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
