using Godot;
using System;

public partial class PracticeHPLabel : RichTextLabel
{
    [Export]
    public HitPoint HitPoint { get; set; }

    public PracticeHPLabelManager Manager { get; set; }

    public bool Hit = false;

    public override void _Ready()
    {
        base._Ready();
    
        Manager = (PracticeHPLabelManager)GetNode("../");

        HitPoint.HitPointHit += HitPointHit;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        Text = "PRACTICE ROOM HP: " + HitPoint.HP;
    }

    public void HitPointHit()
    {
        Manager.HitPointHit(this);
    }
}
