using Godot;
using System;
using System.Collections.Generic;

public partial class Rocket : RigidBody2D
{
    [Signal]
    public delegate void AltitudeChangedEventHandler(float Altitude);
    private const float PlayerControlTorque = 1.0f;

    private List<ThrustSource> thrusters = new();
    private bool IsEmpty = true;

    public override void _PhysicsProcess(double delta)
    {
        float rightSteer = Input.GetAxis("move_left", "move_right");

        ApplyTorque(PlayerControlTorque * rightSteer);
        DynamicThrustReduction.BalanceThrusters(this, rightSteer);

        foreach (ThrustSource thruster in thrusters)
        {
            var (thrust, position) = thruster.GetThrust(Transform);
            GD.Print($"Thrust to {thrust} at {position}");
            ApplyForce(thrust, position);
        }

        // negative Y is up
        EmitSignal(SignalName.AltitudeChanged, -GlobalPosition.Y);
    }

    public void AddComponent(RocketComponent component)
    {
        // TODO center of mass
        if (IsEmpty)
        {
            Mass = component.Mass;
        }
        else
        {
            Mass += component.Mass;
        }

        foreach (Node child in component.GetChildren())
        {
            if (child.GetParent() == this) throw new Exception($"compoment {component.Name} aready added to {Name}");

            child.Reparent(this);

            if (child is ThrustSource thruster)
            {
                thrusters.Add(thruster);
            }
        }
        component.QueueFree();
    }

    public void AddDuctTape(DuctTape tape)
    {
        // updating the tape is no longer necessary
        tape.ReparentGraphics(this);
        tape.QueueFree();
    }

    public IReadOnlyList<ThrustSource> GetThrusters() => thrusters;
}
