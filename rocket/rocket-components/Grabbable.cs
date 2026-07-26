using Godot;

public partial class Grabbable : RigidBody2D
{
    public const float SnapSpeed = 20f;
    public const float SnapDampening = 20f;

    protected bool isDragging = false;
    private Vector2 localGrabOffset = new();
    private PhysicsMaterial originalMaterial;
    private float pullFactor = 1;

    // Called when the node enters the scene tree for the first time.

    public override void _Ready()
    {
        InputPickable = true;
        CollisionLayer |= Game.CollisionLayerGrabbable;
        MaxContactsReported = 1;
        ContactMonitor = true;
        originalMaterial = PhysicsMaterialOverride;
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
            // for small masses, reduce the force to avoid slingshotting
            ApplyForce(velocityDifference * SnapDampening * pullFactor * Mathf.Min(0.5f, Mass), globalOffset);
        }
    }

    public void OnRelease()
    {
        isDragging = false;
        PhysicsMaterialOverride = originalMaterial;
    }

    public void OnGrab(Vector2 localGrabOffset, float pullFactor = 1.0f)
    {
        this.localGrabOffset = localGrabOffset;
        this.pullFactor = pullFactor;
        isDragging = true;

        PhysicsMaterialOverride = new PhysicsMaterial()
        {
            Friction = 0.1f,
            Bounce = 0.1f
        };
    }
}
