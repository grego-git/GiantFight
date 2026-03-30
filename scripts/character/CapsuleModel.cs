using Godot;
using System;

public partial class CapsuleModel : MeshInstance3D
{
    [Export]
    public CharacterData CharacterData { get; set; }
    [Export]
    public AnimationPlayer AnimPlayer { get; set; }

    private MeshInstance3D visor;

    public override void _Ready()
    {
        base._Ready();

        visor = (MeshInstance3D)GetNode("MeshInstance3D");
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
    
        if (!CharacterData.StunMeter.IsEmpty())
        {
            AnimPlayer.Play("stun");
            ((StandardMaterial3D)GetSurfaceOverrideMaterial(0)).AlbedoColor = Colors.White.Lerp(Colors.Blue, CharacterData.StunMeter.NormalizedFill());
            ((StandardMaterial3D)visor.GetSurfaceOverrideMaterial(0)).AlbedoColor = Colors.White.Lerp(Colors.Blue, CharacterData.StunMeter.NormalizedFill());
        }
        else
        {
            AnimPlayer.Play("idle");
            ((StandardMaterial3D)GetSurfaceOverrideMaterial(0)).AlbedoColor = Colors.White;
            ((StandardMaterial3D)visor.GetSurfaceOverrideMaterial(0)).AlbedoColor = Colors.White;
        }
    }
}
