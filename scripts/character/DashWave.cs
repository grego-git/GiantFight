using Godot;
using System;

public partial class DashWave : MeshInstance3D
{
    [Export]
    public float TimerDuration { get; set; }
    [Export]
    public float ExpandSpeed { get; set; }
    [Export]
    public float InitialScale { get; set; }
    [Export]
    public float TargetScale { get; set; }
    
    private StandardMaterial3D material;
    private Meter timer;

    public override void _Ready()
    {
        base._Ready();

        material = (StandardMaterial3D)GetSurfaceOverrideMaterial(0);
        timer = new Meter(TimerDuration, fill: true);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        timer.FillMeter(-(float)delta * ExpandSpeed);

        Scale = (Vector3.One * InitialScale).Lerp(Vector3.One * TargetScale, timer.OneMinusNormalizedFill());
        material.AlbedoColor = new Color(1.0f, 1.0f, 1.0f, Mathf.Pow(timer.NormalizedFill(), 2.0f));

        if (timer.IsEmpty())
            QueueFree();
    }
}
