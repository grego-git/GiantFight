using Godot;
using System;

public partial class BreakApartWallTrail : Node3D
{
    [Export]
    public float Increment { get; set; }
    [Export]
    public PackedScene BreakApartWallScene { get; set; }

    private RayCast3D rayCast;
    private BreakApartWall currentWall;

    public override void _Ready()
    {
        base._Ready();

        rayCast = (RayCast3D)GetNode("RayCast3D");
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (currentWall == null || currentWall.Explode)
        {
            rayCast.ForceRaycastUpdate();

            if (rayCast.IsColliding())
            {
                currentWall = (BreakApartWall)BreakApartWallScene.Instantiate();
                currentWall.RiseTimer = 0.25f;

                GetParent().AddChild(currentWall);
                
                currentWall.GlobalPosition = rayCast.GetCollisionPoint() + (Vector3.Down * 160.0f);
                currentWall.Scale = Vector3.One * 20.0f;
                currentWall.SpawnPos = currentWall.GlobalPosition;
                currentWall.LookAt(currentWall.GlobalPosition + GlobalBasis.Z);

                GlobalPosition += GlobalBasis.Z * Increment;
            }
            else
            {
                QueueFree();
                GD.Print("DESTROY TRAIL: " + GlobalPosition.ToString());
            }
        }
    }
}
