using Godot;
using System;

public partial class FireLimb : GpuParticles3D
{
    [Export]
    public MeshInstance3D Mesh { get; set; }
    [Export]
    public float MinScale { get; set; }
    [Export]
    public float Heat { get; set; }

    public bool HeatUp { get; set; }

    private ParticleProcessMaterial processMat;
    private GiantHitBox hitBox;
    private StandardMaterial3D material;

    public override void _Ready()
    {
        base._Ready();

        Utils.ReScaleParticles((ParticleProcessMaterial)ProcessMaterial, MinScale, ((Node3D)(GetParent().GetParent())).Scale.X);

        hitBox = (GiantHitBox)GetNode("HitBox");
        material = (StandardMaterial3D)Mesh.Mesh.SurfaceGetMaterial(0);
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
    
        Heat += (float)delta * (HeatUp ? 0.4f : -0.5f);
        Heat = Mathf.Clamp(Heat, 0.0f, 1.0f);

        material.AlbedoColor = new Color(1.0f, 1.0f, 1.0f, material.AlbedoColor.A).Lerp(new Color(1.0f, 0.0f, 0.0f, material.AlbedoColor.A), Heat);

        HeatUp = false;
    }
}
