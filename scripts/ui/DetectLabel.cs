using Godot;
using System;

public partial class DetectLabel : RichTextLabel
{
    [Export]
    public Giant Giant { get; set; }

    public override void _Process(double delta)
    {
        base._Process(delta);

        Visible = Constants.DEBUG;

        Text = "DETECT ZONE: " + Giant.PlayerDetection.PlayerDetectionZone.ToString();
    }
}
