using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Rocket : Node
{
    [Signal]
    public delegate void AltitudeChangedEventHandler(float Altitude);
    private const float PlayerControlTorque = 100.0f;

    private List<ThrustSource> thrusters = new();
    public ControlComponent ControlComponent { get; private set; }
    private bool IsEmpty = true;

    private Vector2 unweightedCenterOfMass = Vector2.Zero;

    public override void _PhysicsProcess(double delta)
    {
        float rightSteer = Input.GetAxis("move_left", "move_right");
        ControlComponent.ApplyTorque(PlayerControlTorque * rightSteer);

        DynamicThrustReduction.BalanceThrusters(ControlComponent, thrusters, rightSteer);

        // negative Y is up
        EmitSignal(SignalName.AltitudeChanged, -ControlComponent.GlobalPosition.Y);
    }

    public void AddComponent(RocketComponent component)
    {
        thrusters.AddRange(component.ThrustSources);

        if (component is ControlComponent control)
        {
            if (ControlComponent != null) throw new Exception("Double control component");

            ControlComponent = control;
        }
    }

    public IReadOnlyList<ThrustSource> GetThrusters() => thrusters;
}
