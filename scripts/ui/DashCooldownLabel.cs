using Godot;
using System;

public partial class DashCooldownLabel : RichTextLabel
{
    [Export]
    public CharacterData CharacterData { get; set; }

    public override void _Process(double delta)
    {
        base._Process(delta);

        Text = "DASH: " + CharacterData.DashCooldownMeter.Value.ToString("F2");
    }
}
