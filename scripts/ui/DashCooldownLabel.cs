using Godot;
using System;

public partial class DashCooldownLabel : RichTextLabel
{
    [Export]
    public CharacterData CharacterData { get; set; }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        Text = "DASH: " + CharacterData.DashCooldownMeter.Value.ToString("F2");
    }
}
