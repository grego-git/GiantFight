using Godot;
using System;

public partial class BreakApartWall : Node3D
{
    [Export]
    public bool Explode { get; set; }
    public Vector3 SpawnPos { get; set; }
    public float RiseTimer { get; set; }

    private float timer;

    private RandomNumberGenerator rng;
    private GiantHitBox hitBox;
    private BreakApartWallPiece[] pieces;

    public override void _Ready()
    {
        base._Ready();
    
        rng = new RandomNumberGenerator();
        hitBox = (GiantHitBox)GetNode("HitBox");
        pieces = new BreakApartWallPiece[24];

        for (int i = 1; i <= 24; i++)
        {
            pieces[i - 1] = (BreakApartWallPiece)GetNode("Cube" + i.ToString());
        }

        timer = RiseTimer;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
    
        if (timer > 0.0f)
        {
            timer -= (float)delta;

            if (timer <= 0.0f)
            {
                Explode = true;
                GlobalPosition = new Vector3(SpawnPos.X, 0.0f, SpawnPos.Z);
                hitBox.Monitorable = false;
                hitBox.Monitoring = false;
            }
            else 
                GlobalPosition = new Vector3(SpawnPos.X, 0.0f, SpawnPos.Z).Lerp(new Vector3(SpawnPos.X, -160.0f, SpawnPos.Z), timer);
        }
        else
        {
            Explode = false;
        }

        if (Explode)
        {
            foreach (var piece in pieces)
            {
                Vector3 horizontalForce = new Vector3(rng.RandfRange(-1.0f, 1.0f), 0.0f, rng.RandfRange(-1.0f, 1.0f)).Normalized() * rng.RandfRange(7000.0f, 9000.0f);
                Vector3 verticalForce = Vector3.Up * rng.RandfRange(10000.0f, 20000.0f);
                piece.Explode(horizontalForce + verticalForce, (float)delta);
            }
        }
        else
        {
            bool allDead = true;

            foreach (var piece in pieces)
            {
                if (!piece.Dead)
                    allDead = false;    
            }

            if (allDead)
                QueueFree();
        }
    }
}
