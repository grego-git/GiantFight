using Godot;
using System;

public partial class BreakApartWallTrailSpawner : Node3D
{
    [Export]
    public bool Spawn { get; set; }

    [Export]
    public PackedScene TrailScene;

    private Giant giant;

    public override void _Ready()
    {
        base._Ready();

        giant = (Giant)GetNode("../");
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (Spawn)
        {
            var trailSpawned = (Node3D)TrailScene.Instantiate();
            
            giant.GetParent().AddChild(trailSpawned);
            trailSpawned.GlobalPosition = GlobalPosition;
            trailSpawned.LookAt(trailSpawned.GlobalPosition - giant.GlobalBasis.Z);

            giant.CharacterData.CameraController.Shake(1.0f, 5.0f);

            Spawn = false;
        }
    }
}
