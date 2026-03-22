using Godot;
using System;

public partial class DetectRing : ShapeCast3D
{
    private MeshInstance3D mesh;

    public override void _Ready()
    {
        base._Ready();

        mesh = (MeshInstance3D)GetNode("MeshInstance3D");
        mesh.Visible = Constants.DEBUG;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        mesh.Visible = Constants.DEBUG;
    }
}
