using Godot;
using System;

public partial class HitPoint : StaticBody3D
{
    [Export]
    public int HP { get; set; }

    private MeshInstance3D mesh;
    private StandardMaterial3D mat;

    private Meter hitMeter;
    private Meter flashMeter;

    private float turnColorOff;

    public override void _Ready()
    {
        base._Ready();
        
        mesh = (MeshInstance3D)GetNode("MeshInstance3D");
        mat = (StandardMaterial3D)mesh.GetSurfaceOverrideMaterial(0);
        
        hitMeter = new Meter(0.25f);
        flashMeter = new Meter(0.02f);

        turnColorOff = 1.0f;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (HP == 0)
        {
            turnColorOff -= (float)delta * 2.0f;
            turnColorOff = Mathf.Clamp(turnColorOff, 0.0f, 1.0f);

            mat.AlbedoColor = Colors.Red.Lerp(Colors.Black, 1.0f - turnColorOff);

            if (turnColorOff == 0.0f)
            {
                GetTree().ChangeSceneToFile("res://prototype_scenes/climb_test_scene.tscn");
                return;
            }
        }
        else 
            mat.AlbedoColor = Colors.Red;

        if (!hitMeter.IsEmpty())
        {
            hitMeter.FillMeter(-(float)delta);
            flashMeter.FillMeter(-(float)delta);

            if (flashMeter.IsEmpty())
            {
                flashMeter.FillToMax();
                mesh.Visible = !mesh.Visible;
            }
        }
        else
        {
            mesh.Visible = true;
        }
    }

    public void Hit(int damage)
    {
        if (!hitMeter.IsEmpty())
            return;
        
        HP -= damage;
        HP = Mathf.Max(HP, 0);

        hitMeter.FillToMax();
        flashMeter.FillToMax();
    }
}
