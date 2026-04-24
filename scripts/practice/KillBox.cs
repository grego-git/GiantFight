using Godot;
using System;

public partial class KillBox : Area3D
{
    [Export]
    public bool Persistent { get; set; }
    
    private MeshInstance3D mesh;
    private Meter attackMeter;
    private Meter activeMeter;

    public override void _Ready()
    {
        base._Ready();

        BodyEntered += HitSomething;
        
        mesh = (MeshInstance3D)GetNode("MeshInstance3D");

        attackMeter = new Meter(2.25f);
        activeMeter = new Meter(0.5f, fill: true);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (!Persistent) 
        {
            if (!attackMeter.IsFilled())
            {
                attackMeter.FillMeter((float)delta);
                ((StandardMaterial3D)mesh.GetSurfaceOverrideMaterial(0)).AlbedoColor = new Color(1.0f, 0.0f, 0.0f, (Mathf.Clamp(attackMeter.Value, 0.5f, 1.5f) - 0.5f) * 0.5f);
                Monitorable = false;
                Monitoring = false;
            }
            else
            {
                activeMeter.FillMeter(-(float)delta);
                ((StandardMaterial3D)mesh.GetSurfaceOverrideMaterial(0)).AlbedoColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
                Monitorable = true;
                Monitoring = true;

                if (activeMeter.IsEmpty())
                {
                    activeMeter.FillToMax();
                    attackMeter.Empty();
                }
            }
        }
    }

    public void HitSomething(Node3D body)
    {
        CharacterController controller = (CharacterController)body;
        controller.EmitSignal("Hit");
    }
}
