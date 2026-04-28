public interface IActionDispenser
{
    public IGiantAction BottomAction(Giant giant);
    public IGiantAction MidAction(Giant giant);
    public IGiantAction TopAction(Giant giant);
    public IGiantAction ExternalAction(Giant giant);
    public IGiantAction NegateAction(Giant giant);
}