using Godot;
using System;
using System.Collections.Generic;

public partial class Rocket : RigidBody2D
{
    private List<ThrusterComponent> thrusters;
    private bool IsEmpty = true;

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
    }

    public IReadOnlyList<ThrusterComponent> GetThrusters() => thrusters;
}
