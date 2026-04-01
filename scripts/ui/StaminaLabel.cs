using Godot;
using System;

public partial class StaminaLabel : RichTextLabel
{
    [Export]
    public CharacterData CharacterData { get; set; }

    public override void _Process(double delta)
    {
        base._Process(delta);

        Text = "STAMINA: " + CharacterData.StaminaMeter.Value.ToString("F2");
    }
}
