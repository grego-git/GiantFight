using Godot;

public partial class FirePillar : Node3D
{
    [Export]
    public Giant Giant { get; set; }

    [Export]
    public MeshInstance3D WarningRing { get; set; }

    private Area3D hitCylinder;
    private GpuParticles3D particles;
    private Meter fireMeter;
    private Meter flickerTimer;
    private Meter killTimer;
    private bool fire;

    public override void _Ready()
    {
        base._Ready();

        fireMeter = new Meter(1.25f);
        flickerTimer = new Meter(0.05f);
        killTimer = new Meter(2.0f);

        hitCylinder = (Area3D)GetNode("Area3D");
        particles = (GpuParticles3D)hitCylinder.GetNode("Particles");

        particles.Emitting = false;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
    
        fireMeter.FillMeter((float)delta);

        if (!fire) 
        {
            if (fireMeter.NormalizedFill() < 0.25f)
            {
                GlobalPosition = new Vector3(Giant.PlayerDetection.PlayerPosition.X, 1.0f, Giant.PlayerDetection.PlayerPosition.Z);

                StandardMaterial3D mat = (StandardMaterial3D)WarningRing.GetSurfaceOverrideMaterial(0);
                mat.AlbedoColor = new Color(0.0f, 0.0f, 1.0f, 0.0f).Lerp(Colors.Blue, (fireMeter.NormalizedFill() / 0.25f) * 0.5f);
            }
            else if (fireMeter.NormalizedFill() < 1.0f)
            {
                flickerTimer.FillMeter((float)delta);

                if (flickerTimer.IsFilled())
                {
                    flickerTimer.Empty();
                    WarningRing.Visible = !WarningRing.Visible;
                }
            }
            else if (fireMeter.IsFilled())
            {
                Giant.CharacterData.CameraController.Shake(1.0f, 5.0f);

                fire = true;
                hitCylinder.Monitorable = true;
                hitCylinder.Monitoring = true;
                particles.Emitting = true;
                WarningRing.Visible = false;
            }
        }
        else
        {
            killTimer.FillMeter((float)delta);

            if (killTimer.NormalizedFill() > 0.5f)
            {
                hitCylinder.Monitorable = false;
                hitCylinder.Monitoring = false;
                particles.Emitting = false;
            }
            if (killTimer.IsFilled())
                QueueFree();
        }
    }
}
