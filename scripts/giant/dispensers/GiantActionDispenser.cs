using Godot;
using System;

public partial class GiantActionDispenser : Node3D, IActionDispenser
{
    public virtual IGiantAction BottomAction(Giant giant)
    {
        return null;
    }

    public virtual IGiantAction MidAction(Giant giant)
    {
        return null;
    }

    public virtual IGiantAction TopAction(Giant giant)
    {
        return null;
    }

    public virtual IGiantAction ExternalAction(Giant giant)
    {
        return null;
    }

    public virtual IGiantAction NegateAction(Giant giant)
    {
        return null;
    }

}
