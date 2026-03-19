using Godot;
using System;

public partial class KillOneShotParticle : GpuParticles3D
{
    [Export]
    public GpuParticles3D[] Particles { get; set; }

    private bool started;
    private float timer;

    public override void _Ready()
    {
        base._Ready();
        timer = 1.0f;
    }


    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (started && !Emitting)
        {
            timer -= (float)delta;

            if (timer <= 0.0f)
                QueueFree();
        }
    }


    public void Start()
    {
        Restart();
        started = true;
        Emitting = true;
        
        if (Particles == null)
            return;

        foreach (var particle in Particles)
            particle.Emitting = true;
    }
}
