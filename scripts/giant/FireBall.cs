using Godot;
using System;

public partial class FireBall : Node3D
{
    public Giant Giant { get; set; }
    public Vector3 Dir { get; set; }
    public float Speed { get; set; }

    private Area3D area3D;
    private GpuParticles3D particles;
    private MeshInstance3D mesh;
    private float killTimer;
    private bool collidedWithBall;

    public override void _Ready()
    {
        base._Ready();

        area3D = (Area3D)GetNode("Area3D");
        area3D.BodyEntered += HitSomething;
        area3D.AreaEntered += HitArea;

        particles = (GpuParticles3D)area3D.GetNode("Particles");
        particles.Emitting = true;
        
        mesh = (MeshInstance3D)area3D.GetNode("MeshInstance3D");
        mesh.Visible = Constants.DEBUG;

        killTimer = 5.0f;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        mesh.Visible = Constants.DEBUG;

        GlobalPosition += Dir * Speed * (float)delta;

        killTimer -= (float)delta;

        if (killTimer <= 0.0)
        {
            QueueFree();
        }
    }

    public void HitSomething(Node3D body)
    {
        if (body.GetType() == typeof(CharacterController)) 
        {
            CharacterController controller = (CharacterController)body;
            controller.EmitSignal("Hit");

            QueueFree();
        }
    }

    public void HitArea(Area3D body)
    {
        if (collidedWithBall)
            return;

        if (body.GetType() == typeof(FireBall) || body.GetParent().GetType() == typeof(FireBall)) 
        {
            killTimer = 0.1f;
            collidedWithBall = true;
            Giant.CharacterData.CameraController.Shake(1.0f, 5.0f);
        }
    }
}
