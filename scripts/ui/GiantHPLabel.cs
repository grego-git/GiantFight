using Godot;
using System;

public partial class GiantHPLabel : RichTextLabel
{
    [Export]
    public Giant Giant { get; set; }

    public override void _Process(double delta)
    {
        base._Process(delta);

        Text = "GIANT HP: " + Giant.CurrentHP;
    }
}
