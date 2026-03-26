using Godot;
using System;

public partial class CapsuleModel : MeshInstance3D
{
    [Export]
    public CharacterData CharacterData { get; set; }
    [Export]
    public AnimationPlayer AnimPlayer { get; set; }

    public override void _Ready()
    {
        base._Ready();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
    
        if (!CharacterData.StunMeter.IsEmpty())
            AnimPlayer.Play("stun");
        else
            AnimPlayer.Play("idle");
    }
}
