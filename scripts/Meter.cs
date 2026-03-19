using Godot;

public class Meter
{
    public float Value { get; private set; }
    public float MaxValue { get; private set; }
    public float MinValue { get; private set; }

    public Meter (float max, float min = 0.0f, bool fill = false)
    {
        MaxValue = max;
        MinValue = min;
        
        if (fill)
            Value = MaxValue;
        else
            Value = MinValue;
    }

    public void FillMeter(float fillAmount)
    {
        Value += fillAmount;
        Value = Mathf.Clamp(Value, MinValue, MaxValue);
    }

    public void Empty()
    {
        Value = MinValue;
    }

    public void FillToMax()
    {
        Value = MaxValue;
    }
    
    public void FillToMiddle()
    {
        Value = MaxValue * 0.5f;
    }

    public float NormalizedFill()
    {
        return (Value - MinValue) / (MaxValue - MinValue);
    }

    public float OneMinusNormalizedFill()
    {
        return 1.0f - NormalizedFill();
    }

    public bool IsEmpty()
    {
        return Value <= MinValue;
    }

    public bool IsFilled()
    {
        return Value >= MaxValue;
    }
}