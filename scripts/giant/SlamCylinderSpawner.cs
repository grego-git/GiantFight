using Godot;
using System;

public partial class SlamCylinderSpawner : Node3D
{
    [Export]
    public bool Spawn { get; set; }

    [Export]
    public PackedScene SlamCylinder;
    [Export]
    public PackedScene SlamParticles;

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
            var spawnedCylinder = (Node3D)SlamCylinder.Instantiate();
            
            giant.AddChild(spawnedCylinder);
            spawnedCylinder.GlobalPosition = GlobalPosition;
            
            var spawnedParticles = (KillOneShotParticle)SlamParticles.Instantiate();

            giant.GetParent().AddChild(spawnedParticles);
            spawnedParticles.GlobalPosition = GlobalPosition;
            spawnedParticles.Start();

            giant.CharacterData.CameraController.Shake(1.0f, 5.0f);

            Spawn = false;
        }
    }
}
