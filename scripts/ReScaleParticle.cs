using Godot;
using System;

public partial class ReScaleParticle : GpuParticles3D
{
    [Export]
    public Node3D ReScaleNode;
    
    [Export]
    public float MinScale { get; set; }

    public override void _Ready()
    {
        base._Ready();

        Utils.ReScaleParticles((ParticleProcessMaterial)ProcessMaterial, MinScale, ReScaleNode.Scale.X);
    }
}
