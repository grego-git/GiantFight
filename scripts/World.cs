using Godot;
using System;

public partial class World : Node3D
{
    [Export]
    public bool TutorialScene { get; set; }
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
        else if (!TutorialScene && GetTree().Paused && Input.IsActionJustPressed("exit_to_menu"))
        {
            GetTree().Paused = false;
            GetTree().ChangeSceneToFile("res://prototype_scenes/practice_scene.tscn");
            return;
        }
    }    

    public void SlowDown(float scale = 0.25f)
    {
        Engine.TimeScale = scale;
    }
}