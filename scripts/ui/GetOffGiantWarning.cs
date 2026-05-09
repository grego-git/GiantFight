using Godot;
using System;

public partial class GetOffGiantWarning : RichTextLabel
{
    [Export]
    public Giant Giant { get; set; }

    private Meter blinkMeter;
    private Meter blinkDuration;

    public override void _Ready()
    {
        base._Ready();
    
        blinkMeter = new Meter(0.1f);
        blinkDuration = new Meter(4.0f);

        foreach (var hitPoint in Giant.HitPoints)
        {
            hitPoint.HitPointDead += StartBlinking;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
    
        if (blinkDuration.IsEmpty() || Giant.Dead)
        {
            Visible = false;
        }
        else
        {
            blinkDuration.FillMeter(-(float)delta);
            blinkMeter.FillMeter((float)delta);

            if (blinkMeter.IsFilled())
            {
                Visible = !Visible;
                blinkMeter.Empty();
            }
        }
    }

    public void StartBlinking()
    {
        if (!blinkDuration.IsEmpty())
            return;

        blinkDuration.FillToMax();
        blinkMeter.Empty();
        
        Visible = true;
    }
}
