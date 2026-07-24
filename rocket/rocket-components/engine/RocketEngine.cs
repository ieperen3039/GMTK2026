using Godot;
using System;

public partial class RocketEngine : RocketComponent
{
    [Signal]
    public delegate void AltitudeChangedEventHandler(float Altitude);
    [Export]
    public float ThrustPower = 100f;
    private bool EnableThrust = false;
    private float Altitude;
    private float _LaunchY;

    public override void _Ready()
    {
        base._Ready();
        _LaunchY = GlobalPosition.Y;
    }


	public override void _PhysicsProcess(double delta)
    {
        if (EnableThrust)
        {
            Vector2 localUp = -GlobalTransform.Y;
            ApplyCentralForce(localUp * ThrustPower);

            Altitude = _LaunchY - GlobalPosition.Y;
            EmitSignal(SignalName.AltitudeChanged, Altitude);
        }
    }

    public void StartEngine()
    {
        GD.Print("Liftoff!!!");
        EnableThrust = true;
    }

    public void StopEngine()
    {
        EnableThrust = false;
    }
}
