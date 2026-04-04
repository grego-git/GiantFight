using Godot;
using System;

public partial class World : Node3D
{
    public bool StageEnd { get; set; }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        
        Engine.TimeScale += delta;
        Engine.TimeScale = Mathf.Clamp(Engine.TimeScale, 0.0f, 1.0f);

        if (StageEnd && !GetTree().Paused)
        {
            if (Input.IsActionJustPressed("restart"))
            {
                GetTree().ReloadCurrentScene();
                return;
            }
        }
    }    

    public void SlowDown(float scale = 0.25f)
    {
        Engine.TimeScale = scale;
    }
}