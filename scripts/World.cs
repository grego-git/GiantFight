using Godot;
using System;

public partial class World : Node3D
{
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        
        Engine.TimeScale += delta;
        Engine.TimeScale = Mathf.Clamp(Engine.TimeScale, 0.0f, 1.0f);
    }    

    public void SlowDown(float scale = 0.25f)
    {
        Engine.TimeScale = scale;
    }
}