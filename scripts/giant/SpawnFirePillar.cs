using Godot;
using System;

public partial class SpawnFirePillar : Node3D
{
    [Export]
    public PackedScene FirePillarScene { get; set; }
    [Export]
    public bool Spawn { get; set; }

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
            var spawnedPillar = (FirePillar)FirePillarScene.Instantiate();
            giant.GetParent().AddChild(spawnedPillar);
            
            spawnedPillar.Giant = giant;
            spawnedPillar.GlobalPosition = new Vector3(giant.GlobalPosition.X, 1.0f, giant.GlobalPosition.Z);

            Spawn = false;
        }
    }
}
