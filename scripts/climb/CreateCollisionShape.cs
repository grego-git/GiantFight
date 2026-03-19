using Godot;
using System;
using System.Collections.Generic;

public partial class CreateCollisionShape : Node3D
{
    [Export]
    public CharacterData CharacterData { get; set; }
    [Export]
    public Giant Giant { get; set; }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (Giant == null)
            return;

        foreach (var climbEntity in Giant.ClimbAnimatedEntities)
        {
            climbEntity.ResetCollidableFaces();
        }

        var bonesDetected = CharacterData.Controller.LimbChecker.CheckForLimbs();

        foreach (var climbEntity in Giant.ClimbAnimatedEntities)
        {
            climbEntity.GetCollidableFaces(bonesDetected);
        }
    }
}
