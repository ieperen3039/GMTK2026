using Godot;
using System;
using System.Collections.Generic;

public partial class RocketComponent : RigidBody2D
{

    public const float InitialVelocity = 50.0f;
    public const float InitialRotation = 10.0f;

    public const float SnapSpeed = 20f;
    public const float SnapDampening = 20f;

    public const float AnglePull = 5f;
    private bool isDragging = false;
    private Vector2 localGrabOffset = new();

    private List<ThrustSource> thrustSources = [];
    public IReadOnlyList<ThrustSource> ThrustSources => thrustSources;

    // Called when the node enters the scene tree for the first time.

    public override void _Ready()
    {
        InputPickable = true;
        CollisionLayer = Game.COLLISION_LAYER_ROCKET_COMPONENTS;
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
        MaxContactsReported = 1;
        ContactMonitor = true;
        AngularDamp = 1.0f;

        Random rng = new();
        AngularVelocity = InitialRotation * rng.NextSingle();
        LinearVelocity = RandomUnitVector(rng) * InitialVelocity;

        foreach (Node child in GetChildren())
        {
            if (child is ThrustSource thruster)
            {
                thrustSources.Add(thruster);
            }
        }
    }

    // Called every physics update. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
    {
        if (isDragging)
        {
            Vector2 targetPosition = GetGlobalMousePosition();
            Vector2 direction = targetPosition - ToGlobal(localGrabOffset);
            Vector2 targetVelocity = direction * SnapSpeed;
            Vector2 velocityDifference = targetVelocity - LinearVelocity;
            Vector2 globalOffset = GlobalTransform.BasisXform(localGrabOffset);
            ApplyForce(velocityDifference * SnapDampening, globalOffset);

             // beetje helpen
            float targetForce = -1 * Util.RotationRelativeToUp(Rotation) * AnglePull;
            ApplyTorque(Mathf.Clamp(targetForce - 1, 0, AnglePull));
        }
        else
        {
            // note that thruster should be off at the start of the game
            foreach (ThrustSource thruster in thrustSources)
            {
                Vector2 globalThrustVector = thruster.GetThrust();
                Vector2 globalOffset = thruster.GlobalPosition - GlobalPosition;
                ApplyForce(globalThrustVector, globalOffset);
            }
        }
    }

    public void OnRelease()
    {
        isDragging = false;
    }

    public void OnGrab(Vector2 localGrabOffset)
    {
        this.localGrabOffset = localGrabOffset;
        isDragging = true;
        ContactMonitor = true;
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
