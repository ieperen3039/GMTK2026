using Godot;
using System;

public partial class RocketComponent : RigidBody2D
{
    public const float InitialVelocity = 1.0f;
    public const float InitialRotation = 10.0f;


    // handles both the target speed and the dampening of the speed as objects are moving to the cursor
    // (0, inf]
    public const float SnapSpeed = 2.0f;
    // target speed will be no higher than this number of pixels per second
    // (0, inf]
    // public const float SnapSpeedLimit = 10.0f;
    // handles the rate at which the object velocity becomes the target velocity
    // (0, 1]
    public const float SnapAcceleration = 0.8f;
    // gravity of objects not dragged in pixels/sec
    public const float GravityAcceleration = 0.5f;
    private bool isDragging = false;

    // Called when the node enters the scene tree for the first time.

    public override void _Ready()
    {
        InputPickable = true;
        CollisionLayer = Game.COLLISION_LAYER_ROCKET_COMPONENTS;
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
        InputEvent += OnInputEvent;

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
            // move towards the cursor
            Vector2 targetPosition = GetGlobalMousePosition();
            Vector2 direction = GlobalPosition - targetPosition;
            Vector2 targetVelocity = direction * SnapSpeed;
            GD.Print(targetVelocity);
            ApplyForce(targetVelocity);
        }
    }

    private void OnInputEvent(Node viewport, InputEvent inputEvent, long shapeIdx)
    {
        GD.Print(Name + "::OnInputEvent");
        if (inputEvent is InputEventMouseButton mouseEvent)
        {
            GD.Print("inputEvent is InputEventMouseButton");
            if (mouseEvent.ButtonIndex == MouseButton.Left)
            {
                if (mouseEvent.Pressed)
                {
                    isDragging = true;
                    GravityScale = 0.0f;
                }
                else
                {
                    isDragging = false;
                    GravityScale = 1.0f;
                }
            }
        }
    }

    private void OnMouseEntered()
    {
        GD.Print(Name + "::OnMouseEntered");
    }

    private void OnMouseExited()
    {
        GD.Print(Name + "::OnMouseExited");
    }

    private static Vector2 RandomUnitVector(Random rng)
    {
        return new Vector2(1, 0)
            .Rotated(2 * Mathf.Pi * rng.NextSingle());
    }
}
