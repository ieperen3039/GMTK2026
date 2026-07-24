using Godot;
using System;

public partial class RocketEngine : RocketComponent
{
    [Export]
    public float ThrustPower = 100f;
    private bool enableThrust = false;


	public override void _PhysicsProcess(double delta)
    {
        if (enableThrust)
        {
            Vector2 localUp = -GlobalTransform.Y;
            ApplyCentralForce(localUp * ThrustPower);
        }
    }

    public void StartEngine()
    {
        GD.Print("Liftoff!!!");
        enableThrust = true;
    }

    public void StopEngine()
    {
        enableThrust = false;
    }
}
