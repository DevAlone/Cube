[System.Serializable]
public struct LimitedFloatValue
{
    public static implicit operator float(LimitedFloatValue limitedValue)
    {
        limitedValue.Clamp();
        return limitedValue._value;
    }
    public static LimitedFloatValue operator +(LimitedFloatValue left, float right)
    {
        var res = left;
        res._value += right;
        res.Clamp();

        return res;
    }
    public static LimitedFloatValue operator -(LimitedFloatValue left, float right)
    {
        var res = left;
        res._value -= right;
        res.Clamp();

        return res;
    }

    public float minimumValue;
    public float maximumValue;

    public LimitedFloatValue(float minValue, float maxValue, float value)
    {
        minimumValue = minValue;
        maximumValue = maxValue;
        _value = value;
    }

    private float _value;

    private void Clamp()
    {
        if (_value < minimumValue)
        {
            _value = minimumValue;
        }
        else if (_value > maximumValue)
        {
            _value = maximumValue;
        }
    }
}
