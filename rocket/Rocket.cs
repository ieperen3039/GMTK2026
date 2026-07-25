using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public partial class Rocket : RigidBody2D
{
    [Signal]
    public delegate void AltitudeChangedEventHandler(float Altitude);

    private const int MaxRocketComponents = 100;
    private const float PlayerControlTorque = 1000.0f;
    // in pixels/s
    private const float MaxVelocityDelta = 1000.0f;
    // in pixels
    private const float MaxDistance = 100;
    private const float MaxDistanceSquared = MaxDistance * MaxDistance;
    private const float MaxVelocityDeltaSquared = MaxVelocityDelta * MaxVelocityDelta;

    public ControlComponent ControlComponent { get; private set; }

    private List<ThrustSource> thrusters = new();
    private List<RocketComponent> components = new();
    private bool IsEmpty = true;

    public override void _Ready()
    {
        // this rigid body will be a ephemeral representation of the rocket
        Freeze = true;
        ContactMonitor = false;
        CenterOfMassMode = CenterOfMassModeEnum.Custom;
        RecomputeMassDistribution();
    }
    
    public override void _PhysicsProcess(double delta)
    {
        // copy basic physics properties from control component
        GlobalTransform = ControlComponent.GlobalTransform;
        LinearVelocity = ControlComponent.LinearVelocity;
        AngularVelocity = ControlComponent.AngularVelocity;

        RemoveFallenComponents();
        RecomputeMassDistribution();

        float rightSteer = Input.GetAxis("move_left", "move_right");
        ControlComponent.ApplyTorque(PlayerControlTorque * rightSteer);

        DynamicThrustReduction.BalanceThrusters(this, thrusters, rightSteer);

        // negative Y is up
        EmitSignal(SignalName.AltitudeChanged, -ControlComponent.GlobalPosition.Y);
    }

    private void RemoveFallenComponents()
    {
        Vector2 averageVelocity = Vector2.Zero;
        foreach (RocketComponent component in components)
        {
            averageVelocity += component.LinearVelocity;
        }
        averageVelocity /= components.Count;

        HashSet<RocketComponent> toRemove = new();
        foreach (RocketComponent component in components)
        {
            // avoid ditching the control component
            if (component == ControlComponent) continue;

            float distanceSq = component.GlobalPosition.DistanceSquaredTo(GlobalPosition);
            float velocityDeltaSq = component.LinearVelocity.DistanceSquaredTo(averageVelocity);
            if (distanceSq > MaxDistanceSquared || velocityDeltaSq > MaxVelocityDeltaSquared)
            {
                GD.Print($"Dropping {component.Name} from Rocket");
                toRemove.Add(component);
                foreach(ThrustSource thruster in component.ThrustSources)
                {
                    thrusters.Remove(thruster);
                }
            }
        }

        components.RemoveAll(toRemove.Contains);
    }

    private void RecomputeMassDistribution()
    {
        // assume orientation of ControlComponent
        GlobalTransform = ControlComponent.GlobalTransform;

        Vector2 newCenterOfMass = Vector2.Zero;
        float newMass = 0;
        foreach (RocketComponent component in components)
        {
            Vector2 localCenterOfMass = ToLocal(component.ToGlobal(component.CenterOfMass));
            newCenterOfMass += localCenterOfMass * component.Mass;
            newMass += component.Mass;
        }
        newCenterOfMass /= newMass;

        CenterOfMass = newCenterOfMass;
        Mass = newMass;
        
        float newInertia = 0f;
        foreach (RocketComponent component in components)
        {
            Vector2 localCenterOfMass = ToLocal(component.ToGlobal(component.CenterOfMass));
            float distSq = localCenterOfMass.DistanceSquaredTo(CenterOfMass);
            newInertia += component.Inertia + component.Mass * distSq;
        }
        Inertia = newInertia;

        GD.Print($"Relative CenterOfMass = {CenterOfMass}");
    }

    public void AddComponent(RocketComponent component)
    {
        GD.Print($"Add {component.Name} to Rocket");
        components.Add(component);

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
