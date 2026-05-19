using Godot;
using System;

public partial class FireLimb : GpuParticles3D
{
    [Export]
    public float MinScale { get; set; }
    [Export]
    public float Heat { get; set; }

    public bool HeatUp { get; set; }

    private ParticleProcessMaterial processMat;
    private GiantHitBox hitBox;

    public override void _Ready()
    {
        base._Ready();

        Utils.ReScaleParticles((ParticleProcessMaterial)ProcessMaterial, MinScale, ((Node3D)(GetParent().GetParent())).Scale.X);

        hitBox = (GiantHitBox)GetNode("HitBox");
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (Heat == 1.0f)
        {
            hitBox.Monitorable = true;
            hitBox.Monitoring = true;
            Emitting = true;
        }
        else if (Heat == 0.0f)
        {
            hitBox.Monitorable = false;
            hitBox.Monitoring = false;
            Emitting = false;
        }
    
        Heat += (float)delta * (HeatUp ? 0.5f : -0.5f);
        Heat = Mathf.Clamp(Heat, 0.0f, 1.0f);

        HeatUp = false;
    }
}
