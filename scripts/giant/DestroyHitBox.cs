using Godot;
using System;

public partial class DestroyHitBox : Node3D
{
    [Export]
    public float KillTimer { get; set; }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        KillTimer -= (float)delta;
        KillTimer = Mathf.Max(KillTimer, 0.0f);

        if (KillTimer == 0.0f)
            QueueFree();
    }
}
