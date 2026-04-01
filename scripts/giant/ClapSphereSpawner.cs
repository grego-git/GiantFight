using Godot;
using System;

public partial class ClapSphereSpawner : Node3D
{
    [Export]
    public bool Spawn { get; set; }
    [Export]
    public AudioStreamPlayer3D Sound { get; set; }

    [Export]
    public PackedScene ClapSphere;
    [Export]
    public PackedScene ClapParticles;

    private Giant giant;

    public override void _Ready()
    {
        base._Ready();

        giant = (Giant)GetNode("../");
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        
        GlobalPosition = giant.Fists[0].GlobalPosition.Lerp(giant.Fists[1].GlobalPosition, 0.5f);
        Sound.GlobalPosition = GlobalPosition;

        if (Spawn)
        {
            var spawnedSphere = (Node3D)ClapSphere.Instantiate();
            
            giant.AddChild(spawnedSphere);
            spawnedSphere.GlobalPosition = GlobalPosition;
            
            var spawnedParticles = (KillOneShotParticle)ClapParticles.Instantiate();

            giant.GetParent().AddChild(spawnedParticles);
            spawnedParticles.GlobalPosition = GlobalPosition;
            spawnedParticles.Start();
            spawnedParticles.LookAt(giant.Fists[0].GlobalPosition);

            giant.CharacterData.CameraController.Shake(1.0f, 5.0f);

            Spawn = false;
        }
    }
}
