using Godot;
using System;

public partial class PracticeHPLabelManager : Control
{
    [Export]
    public PracticeHPLabel[] Labels { get; set; }

    public void HitPointHit(PracticeHPLabel hitLabel)
    {
        foreach (var label in Labels)
        {
            if (label == hitLabel)
                label.Visible = true;
            else
                label.Visible = false;
        }
    }
}
