using Godot;
using System;

public partial class ResetLabel : RichTextLabel
{
    [Export]
    public World World { get; set; }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        Visible = World.StageEnd && !GetTree().Paused;
    }
}
