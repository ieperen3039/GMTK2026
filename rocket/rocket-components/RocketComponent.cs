using Godot;
using System;

public partial class RocketComponent : RigidBody2D
{
    [Signal]
    public delegate void OnClickEventHandler(RocketComponent who, InputEventMouseButton mouseEvent);

    public const float InitialVelocity = 10.0f;
    public const float InitialRotation = 100.0f;

    public const float SnapSpeed = 20f;
    public const float SnapDampening = 20f;

    public const float AngularSnapSpeed = 50f;
    public const float AngularSnapDampening = 50f;
    private bool isDragging = false;

    // Called when the node enters the scene tree for the first time.

    public override void _Ready()
    {
        InputPickable = true;
        CollisionLayer = Game.COLLISION_LAYER_ROCKET_COMPONENTS;
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
        InputEvent += OnInputEvent;
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
            Vector2 direction = targetPosition - GlobalPosition;
            Vector2 targetVelocity = direction * SnapSpeed;
            Vector2 velocityDifference = targetVelocity - LinearVelocity;
            ApplyForce(velocityDifference * SnapDampening);

            // mod rotation to (-180, +180) degrees
            float rotationOffset = Mathf.PosMod(Rotation + Mathf.Pi, 2 * Mathf.Pi) - Mathf.Pi;
            float targetAngularVelocity = rotationOffset * AngularSnapSpeed;
            float angularVelocityDifference = targetAngularVelocity - AngularVelocity;
            ApplyTorque(-1 * angularVelocityDifference * AngularSnapDampening);
        }
    }

    private void OnInputEvent(Node viewport, InputEvent inputEvent, long shapeIdx)
    {
        if (inputEvent is InputEventMouseButton mouseEvent)
        {
            EmitSignal(SignalName.OnClick, this, mouseEvent);
        }
    }

    public void OnRelease()
    {
        isDragging = false;
        GravityScale = 1.0f;
    }

    public void OnGrab(Vector2 localGrabPosition)
    {
        isDragging = true;
        ContactMonitor = true;
        GravityScale = 0.0f;
    }

    private void OnMouseEntered()
    {
        // light up?
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
