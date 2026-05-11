public interface IActionDispenser
{
    public IGiantAction BottomAction(Giant giant);
    public IGiantAction MidAction(Giant giant);
    public IGiantAction TopAction(Giant giant);
    public IGiantAction ExternalAction(Giant giant);
    public IGiantAction NegateAction(Giant giant);
    public IGiantAction DesperationAction(Giant giant);
    public IGiantAction AttackBodyAction(Giant giant, string animation, bool useLeftHand);
}