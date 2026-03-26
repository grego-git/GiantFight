using Godot;
using System;

public partial class GiantHPLabel : RichTextLabel
{
    [Export]
    public Giant Giant { get; set; }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        Text = "GIANT HP: " + Giant.CurrentHP;
    }
}
