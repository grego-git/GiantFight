using Godot;
using System;

public partial class FatigueNeedle : HSlider
{
    [Export]
    public CharacterData CharacterData { get; set; }

    public override void _Ready()
    {
        base._Ready();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        Visible = CharacterData.IsFatigued && CharacterData.GetState() != "GRABBED";
        GlobalPosition = CharacterData.CameraController.Camera3D.UnprojectPosition(CharacterData.Controller.GlobalPosition);
        GlobalPosition -= Size / 2.0f;

        float zeroToOne = CharacterData.FatigueMeter.NormalizedFill();
        float negativeOneToOne = (zeroToOne - 0.5f) * 2.0f;
        Value = zeroToOne * 100.0f;

        Modulate = CharacterData.CanRelieveFatigue ? Colors.Green.Lerp(Colors.White, Mathf.Abs(negativeOneToOne)) : Colors.Gray;
    }

}
