using Godot;
using System;

public partial class PauseLabel : RichTextLabel
{
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        Visible = GetTree().Paused;
    }
}
