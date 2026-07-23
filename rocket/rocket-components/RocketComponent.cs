using Godot;
using System;

public partial class RocketComponent : RigidBody2D
{

    public const float InitialVelocity = 10.0f;
    public const float InitialRotation = 100.0f;

    public const float SnapSpeed = 20f;
    public const float SnapDampening = 20f;

    public const float AngularSnapSpeed = 5f;
    public const float AngularSnapDampening = 100f;
    private bool isDragging = false;
    private Vector2 localGrabOffset = new();

    // Called when the node enters the scene tree for the first time.

    public override void _Ready()
    {
        InputPickable = true;
        CollisionLayer = Game.COLLISION_LAYER_ROCKET_COMPONENTS;
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
        MaxContactsReported = 1;
        ContactMonitor = true;

        Random rng = new();
        ApplyImpulse(RandomUnitVector(rng) * InitialVelocity);

        bool clockwise = (rng.Next() & 0x01) == 0;
        ApplyTorqueImpulse(clockwise ? InitialRotation : -InitialRotation);
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
        }
        else
        {
            // beetje helpen
            // mod rotation to (-180, +180) degrees
            float rotationOffset = Mathf.PosMod(Rotation + Mathf.Pi, 2 * Mathf.Pi) - Mathf.Pi;
            float targetAngularVelocity = rotationOffset * AngularSnapSpeed;
            float angularVelocityDifference = targetAngularVelocity - AngularVelocity;
            ApplyTorque(-1 * angularVelocityDifference * AngularSnapDampening);
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
