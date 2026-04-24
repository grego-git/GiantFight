using Godot;
using System;

public partial class FadeIn : ColorRect
{
    float fade;

    public override void _Ready()
    {
        base._Ready();

        fade = 1.0f;
        
        Color = Colors.Black;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        fade -= (float)delta;
        fade = Mathf.Clamp(fade, 0.0f, 1.0f);

        Color = new Color(0.0f, 0.0f, 0.0f, fade);
    }
}
