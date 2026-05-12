using Godot;
using System;

public partial class FireBallSpawner : Node3D
{
    [Export]
    public PackedScene FireBallScene { get; set; }
    [Export]
    public Giant Giant { get; set; }
    [Export]
    public bool Spawn { get; set; }
    [Export]
    public bool LeftSpawner { get; set; }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
    
        if (Spawn)
        {
            Vector3 dir;

            if (LeftSpawner)
                dir = (Giant.RightArmIKTarget.GlobalPosition - Giant.LeftArmIKTarget.GlobalPosition).Normalized();
            else
                dir = (Giant.LeftArmIKTarget.GlobalPosition - Giant.RightArmIKTarget.GlobalPosition).Normalized();

            var spawnedFireBall = (FireBall)FireBallScene.Instantiate();            
            Giant.GetParent().AddChild(spawnedFireBall);

            spawnedFireBall.Giant = Giant;
            spawnedFireBall.Dir = dir;
            spawnedFireBall.Speed = 200.0f;
            spawnedFireBall.GlobalPosition = GlobalPosition;

            Spawn = false;
        }
    }
}
