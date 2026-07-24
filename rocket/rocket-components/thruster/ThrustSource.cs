using Godot;
using System;

public partial class ThrustSource : Marker2D
{
    [Export]
    public float ThrustPower;
    private float thrustFactor = 0.0f;

    public bool EnableThrust => thrustFactor > 0.0f;

    public void SetThrustFactor(float fractionOfFull)
    {
        thrustFactor = fractionOfFull;
    }

    public Vector2 GetThrust() => GetThrustAt(thrustFactor);

    // returns (Global thrust vector, offset from parent in global space)
    public Vector2 GetThrustAt(float fractionOfFull)
    {
        return GetLocalThrustAt(fractionOfFull).Rotated(GlobalRotation);
    }


    public Vector2 GetLocalThrust() => GetLocalThrustAt(thrustFactor);

    // note: towards negative Y
    public Vector2 GetLocalThrustAt(float fractionOfFull) => Vector2.Up * ThrustPower * fractionOfFull;
}
