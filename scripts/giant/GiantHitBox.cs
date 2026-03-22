using Godot;
using System;

public partial class GiantHitBox : Area3D
{
    [Export]
    public MeshInstance3D Mesh { get; set; }

    public override void _Ready()
    {
        base._Ready();

        BodyEntered += HitSomething;
        
        if (Mesh != null)
            Mesh.Visible = Constants.DEBUG;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (Mesh != null)
            Mesh.Visible = Constants.DEBUG;
    }

    public void HitSomething(Node3D body)
    {
        CharacterController controller = (CharacterController)body;
        controller.EmitSignal("Hit");
    }
}
