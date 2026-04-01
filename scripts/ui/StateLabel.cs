using Godot;
using System;

public partial class StateLabel : RichTextLabel
{
    [Export]
    public CharacterData CharacterData { get; set; }

    public override void _Process(double delta)
    {
        base._Process(delta);

        Visible = Constants.DEBUG;

        Text = "STATE: " + CharacterData.GetState();
    }
}
