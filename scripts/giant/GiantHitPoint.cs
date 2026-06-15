using Godot;
using System;

public partial class GiantHitPoint : StaticBody3D
{
    [Signal]
    public delegate void HitPointDeadEventHandler();

    [Export]
    public int HP { get; set; }
    [Export]
    public string Bone { get; set; }
    [Export]
    public Skeleton3D Skeleton { get; set; }

    public bool Enabled { get; set; }

    private bool setOffset;
    private MeshInstance3D mesh;
    private ShaderMaterial mat;
    private Transform3D offsetToBone;

    private Meter hitMeter;
    private Meter flashMeter;

    private bool dead;
    private float turnColorOff;

    public override void _Ready()
    {
        base._Ready();

        Skeleton.SkeletonUpdated += UpdatePosition;
        Enabled = true;
        
        mesh = (MeshInstance3D)GetNode("MeshInstance3D");
        mat = (ShaderMaterial)mesh.GetSurfaceOverrideMaterial(0);
        
        hitMeter = new Meter(0.25f);
        flashMeter = new Meter(0.02f);

        turnColorOff = 1.0f;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (HP == 0)
        {
            turnColorOff -= (float)delta * 2.0f;
            turnColorOff = Mathf.Clamp(turnColorOff, 0.0f, 1.0f);

            mat.SetShaderParameter("albedo", Colors.Red.Lerp(Colors.Black, 1.0f - turnColorOff));
        }
        else 
            mat.SetShaderParameter("albedo", Colors.Red);

        if (!hitMeter.IsEmpty())
        {
            hitMeter.FillMeter(-(float)delta);
            flashMeter.FillMeter(-(float)delta);

            if (flashMeter.IsEmpty())
            {
                flashMeter.FillToMax();
                mesh.Visible = !mesh.Visible;
            }
        }
        else
        {
            mesh.Visible = true;
        }
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

    public void Hit(int damage)
    {
        if (dead)
            return;

        if (!hitMeter.IsEmpty())
            return;
        
        HP -= damage;
        HP = Mathf.Max(HP, 0);

        if (HP == 0)
        {
            dead = true;
            EmitSignal("HitPointDead");
        }

        hitMeter.FillToMax();
        flashMeter.FillToMax();
    }
}
