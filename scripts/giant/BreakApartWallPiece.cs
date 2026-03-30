using Godot;
using System;

public partial class BreakApartWallPiece : MeshInstance3D
{
    [Export]
    public bool Enabled { get; set; }
    [Export]
    public float Mass { get; set; }

    public bool Dead { get; set; }

    private float lifeSpan;
    private Vector3 velocity;

    public override void _Ready()
    {
        base._Ready();

        lifeSpan = 1.0f;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
    
        if (!Enabled)
            return;

        lifeSpan -= (float)delta;

        if (lifeSpan <= 0.0f)
            Dead = true;
        else 
        {
            ((StandardMaterial3D)GetSurfaceOverrideMaterial(0)).AlbedoColor = new Color(1.0f, 1.0f, 1.0f, 0.0f).Lerp(Colors.White, lifeSpan / 2.0f);
            velocity += Vector3.Down * 9.81f * Mass;
            GlobalPosition += velocity * (float)delta;
        }
    }

    public void Explode(Vector3 explosionForce, float delta)
    {
        velocity += explosionForce * delta;
        Enabled = true;
    }
}
