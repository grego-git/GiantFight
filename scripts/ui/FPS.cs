using Godot;
using System;

public partial class FPS : RichTextLabel
{
    public override void _Process(double delta)
    {
        base._Process(delta);

        Visible = Constants.DEBUG;

        float fps = 1.0f / (float)(delta / Engine.TimeScale);
    
        Text = "FPS: " + fps.ToString("F2");

        if (Mathf.RoundToInt(fps) < 60)
            GD.Print($"FPS DROP BELOW 60 {fps.ToString("F2")}");
    }

}
