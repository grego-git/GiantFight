using Godot;
using System;

public partial class PracticeHPLabel : RichTextLabel
{
    [Export]
    public HitPoint HitPoint { get; set; }

    public override void _Process(double delta)
    {
        base._Process(delta);

        Text = "PRACTICE ROOM HP: " + HitPoint.HP;
    }
}
