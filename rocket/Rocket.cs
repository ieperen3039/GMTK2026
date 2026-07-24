using Godot;
using System;
using System.Collections.Generic;

public partial class Rocket : RigidBody2D
{
    [Signal]
    public delegate void AltitudeChangedEventHandler(float Altitude);

    private List<ThrusterComponent> thrusters = new();
    private bool IsEmpty = true;

    public override void _Process(double delta)
    {
        EmitSignal(SignalName.AltitudeChanged, GlobalPosition.Y);
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

            if (child is ThrusterComponent thruster)
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

    public IReadOnlyList<ThrusterComponent> GetThrusters() => thrusters;
}
