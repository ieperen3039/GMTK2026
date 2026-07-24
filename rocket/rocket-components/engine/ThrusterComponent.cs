using Godot;
using System;

public partial class ThrusterComponent : RocketComponent
{
    [Export]
    private float Thrust = 10;
    private float powerLevel = 1.0f;


    public void SetPowerLevel(float fractionOfFull)
    {
        powerLevel = fractionOfFull;
    }

    public Vector2 GetThrust() => GetThrustAt(powerLevel);

    public Vector2 GetThrustAt(float fractionOfFull)
    {
        return new(0, Thrust * fractionOfFull);
    }
}
