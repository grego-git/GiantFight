using Godot;
using System;

public partial class StateLabel : RichTextLabel
{
    [Export]
    public CharacterData CharacterData { get; set; }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        Text = "STATE: " + CharacterData.GetState();
    }
}
