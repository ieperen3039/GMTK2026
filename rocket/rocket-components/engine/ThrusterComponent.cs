using Godot;
using System;

public partial class ThrusterComponent : RocketComponent
{
    [Export]
    public float ThrustPower = 100f;
    private float thrustFactor = 0.0f;
    public float altitude { get; private set; }
    private float launchY;

    public bool EnableThrust => thrustFactor > 0.0f;


    public void SetThrustFactor(float fractionOfFull)
    {
        thrustFactor = fractionOfFull;
    }

    public Vector2 GetThrust() => GetThrustAt(thrustFactor);

    public Vector2 GetThrustAt(float fractionOfFull)
    {
        return new(0, ThrustPower * fractionOfFull);
    }
    
    public override void _Ready()
    {
        base._Ready();
        launchY = GlobalPosition.Y;
    }

	public override void _PhysicsProcess(double delta)
    {
        if (EnableThrust)
        {
            Vector2 localUp = -GlobalTransform.Y;
            ApplyCentralForce(localUp * ThrustPower);
        }
    }
}
