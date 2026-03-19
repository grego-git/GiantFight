using Godot;
using System;
using System.Collections.Generic;

public partial class Giant : Node3D
{
    [Export]
    public ClimbableAnimatedEntity[] ClimbAnimatedEntities { get; set; }

    public override void _Ready()
    {
        base._Ready();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
    }
}
