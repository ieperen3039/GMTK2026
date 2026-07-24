using Godot;
using System;

public partial class ThrustSource : Marker2D
{
    [Export]
    public float ThrustPower = 100f;
    private float thrustFactor = 0.0f;

    public bool EnableThrust => thrustFactor > 0.0f;

    public void SetThrustFactor(float fractionOfFull)
    {
        thrustFactor = fractionOfFull;
    }

    public Vector2 GetThrust() => GetThrustAt(thrustFactor);

    // Thrust applied relative from this.Position
    public Vector2 GetThrustAt(float fractionOfFull) => GetLocalThrustAt(fractionOfFull).Rotated(GlobalRotation);

    public Vector2 GetLocalThrust() => GetLocalThrustAt(thrustFactor);

    public Vector2 GetLocalThrustAt(float fractionOfFull) => new(0, ThrustPower * fractionOfFull);
}
