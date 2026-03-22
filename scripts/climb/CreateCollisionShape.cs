using Godot;
using System;
using System.Collections.Generic;

public partial class CreateCollisionShape : Node3D
{
    [Export]
    public CharacterData CharacterData { get; set; }
    [Export]
    public Giant Giant { get; set; }

    public override void _Ready()
    {
        base._Ready();

        Giant.Skeleton.SkeletonUpdated += Update;
    }

    public void Update()
    {
        if (Giant == null || !Giant.IsInsideTree())
            return;
        
        if (CharacterData == null || !CharacterData.IsInsideTree())
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

        foreach (var climbEntity in Giant.ClimbAnimatedEntities)
        {
            climbEntity.CreateShape();
            climbEntity.Update();
        }
    }
}
