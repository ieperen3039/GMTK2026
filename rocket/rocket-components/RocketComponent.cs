using Godot;
using System;
using System.Collections.Generic;

public partial class RocketComponent : Grabbable
{
    public const float InitialVelocity = 50.0f;
    public const float InitialRotation = 10.0f;

    public const float AnglePull = 5f;
    private List<ThrustSource> thrustSources = [];
    private List<Magnet> magnets = [];
    public IReadOnlyList<ThrustSource> ThrustSources => thrustSources;

    // Called when the node enters the scene tree for the first time.

    public override void _Ready()
    {
        base._Ready();

        CollisionLayer = Game.CollisionLayerPrimary | Game.CollisionLayerGrabbable;
        AngularDamp = 1.0f;
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;

        Random rng = new();
        AngularVelocity = InitialRotation * rng.NextSingle();
        LinearVelocity = RandomUnitVector(rng) * InitialVelocity;

        foreach (Node child in GetChildren())
        {
            if (child is ThrustSource thruster)
            {
                thrustSources.Add(thruster);
            }
            else if (child is Magnet magnet)
            {
                magnets.Add(magnet);
            }
        }
    }

    // Called every physics update. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (!isDragging)
        {
            // note that thruster should be off at the start of the game
            foreach (ThrustSource thruster in thrustSources)
            {
                if (!thruster.EnableThrust) continue;

                Vector2 globalThrustVector = thruster.GetThrust();
                Vector2 globalOffset = thruster.GlobalPosition - GlobalPosition;
                ApplyForce(globalThrustVector, globalOffset);
            }
        }
            
        foreach (Magnet magnet in magnets)
        {
            Vector2 globalThrustVector = magnet.GetForce();
            Vector2 globalOffset = magnet.GlobalPosition - GlobalPosition;
            ApplyForce(globalThrustVector, globalOffset);
        }
    }

    private void OnMouseEntered()
    {
    }

    private void OnMouseExited()
    {
    }

    private static Vector2 RandomUnitVector(Random rng)
    {
        return new Vector2(1, 0)
            .Rotated(2 * Mathf.Pi * rng.NextSingle());
    }
}
