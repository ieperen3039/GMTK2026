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

    public Tuple<Vector2, Vector2> GetThrust(Transform2D transform) => GetThrustAt(transform, thrustFactor);

    // returns (Global thrust vector, offset from parent in global space)
    public Tuple<Vector2, Vector2> GetThrustAt(Transform2D transform, float fractionOfFull)
    {
        return new(
            GetLocalThrustAt(fractionOfFull).Rotated(GlobalRotation), 
            transform.BasisXform(Position)
        );
    }


    public Vector2 GetLocalThrust() => GetLocalThrustAt(thrustFactor);

    // note: towards negative Y
    public Vector2 GetLocalThrustAt(float fractionOfFull) => new(0, -1 * ThrustPower * fractionOfFull);
}
