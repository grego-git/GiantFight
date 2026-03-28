using Godot;
using System;

public partial class StompCylinderSpawner : Node3D
{
    [Export]
    public bool Spawn { get; set; }

    [Export]
    public PackedScene StompCylinder;
    [Export]
    public PackedScene StompParticles;

    private Giant giant;

    public override void _Ready()
    {
        base._Ready();

        giant = (Giant)GetNode("../");
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        
        GlobalPosition = giant.LeftLegIKTarget.GlobalPosition + (Vector3.Down  * giant.StompPadding);

        if (Spawn)
        {
            var spawnedCylinder = (Node3D)StompCylinder.Instantiate();
            
            giant.AddChild(spawnedCylinder);
            spawnedCylinder.GlobalPosition = GlobalPosition;
            
            var spawnedParticles = (KillOneShotParticle)StompParticles.Instantiate();

            giant.GetParent().AddChild(spawnedParticles);
            spawnedParticles.GlobalPosition = GlobalPosition;
            spawnedParticles.Start();

            Spawn = false;
        }
    }
}
