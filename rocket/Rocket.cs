using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Rocket : Node
{
    [Signal]
    public delegate void AltitudeChangedEventHandler(float Altitude);

    private const int MaxRocketComponents = 100;
    private const float PlayerControlTorque = 1000.0f;

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

    public void AddAllNearbyRecursively(ControlComponent core)
    {
        HashSet<RocketComponent> nodesToCheck = [core];
        HashSet<RocketComponent> nodesSeen = [core];
        AddComponent(core);

        int iterationsUntilBreak = MaxRocketComponents;
        while (nodesToCheck.Count > 0 && iterationsUntilBreak-- > 0)
        {
            RocketComponent nodeToCheck = nodesToCheck.First();
            nodesToCheck.Remove(nodeToCheck);

            // find all components connected to nodeToCheck.
            // add all of them to a new Rocket
            foreach (RigidBody2D near in nodeToCheck.GetNearbyBodies())
            {
                if (near is not RocketComponent component) continue;
                if (nodesSeen.Contains(component)) continue;

                AddComponent(component);
                nodesToCheck.Add(component);
                nodesSeen.Add(component);
            }
        }
    }

    public IReadOnlyList<ThrustSource> GetThrusters() => thrusters;
}
