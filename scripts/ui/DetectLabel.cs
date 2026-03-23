using Godot;
using System;

public partial class DetectLabel : RichTextLabel
{
    [Export]
    public Giant Giant { get; set; }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        Text = "DETECT ZONE: " + Giant.PlayerDetection.PlayerDetectionZone.ToString();
    }
}
