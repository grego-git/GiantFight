using Godot;
using System;

public partial class ClapSphereSpawner : Node3D
{
    [Export]
    public bool Spawn { get; set; }

    [Export]
    public PackedScene ClapSphere;

    private Giant giant;

    public override void _Ready()
    {
        base._Ready();

        giant = (Giant)GetNode("../");
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        
        GlobalPosition = giant.LeftArmIKTarget.GlobalPosition.Lerp(giant.RightArmIKTarget.GlobalPosition, 0.5f);

        if (Spawn)
        {
            var spawnedSphere = (Node3D)ClapSphere.Instantiate();
            
            giant.AddChild(spawnedSphere);
            spawnedSphere.GlobalPosition = GlobalPosition;

            Spawn = false;
        }
    }
}
