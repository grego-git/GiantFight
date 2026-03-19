using Godot;
using System;

public partial class StaminaLabel : RichTextLabel
{
    [Export]
    public CharacterData CharacterData { get; set; }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        Text = "STAMINA: " + CharacterData.StaminaMeter.Value.ToString("F2");
    }
}
