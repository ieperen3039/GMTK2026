using Godot;

public partial class Grabbable : RigidBody2D
{
    public const float SnapSpeed = 20f;
    public const float SnapDampening = 30f;
    public const float MaxSpeedWhendragging = 1_000;

    protected bool isDragging = false;
    private Vector2 localGrabOffset = new();
    private PhysicsMaterial originalMaterial;
    private float pullFactor = 1;

    public const float ClangVelocityFactor = 0.2f;
    public const float ClangVolumeFactor = 1 / 1000f;
    public const float ClangVelocityDelta = 5f;
    private float clangVolumeDropOff;
    private Vector2 lastMeasuredVelocityForClang;
    private AudioStreamPlayer2D sfxPlayer;
    private float adjustedPreviousSfxVolume;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        InputPickable = true;
        CollisionLayer |= Game.CollisionLayerGrabbable;
        MaxContactsReported = 1;
        ContactMonitor = true;
        originalMaterial = PhysicsMaterialOverride;
        

        sfxPlayer = GetNodeOrNull<AudioStreamPlayer2D>("ClangSfx");
        if (sfxPlayer != null)
        {
            clangVolumeDropOff = sfxPlayer.PitchScale / (float)sfxPlayer.Stream.GetLength();
        }
    }

    // Called every physics update. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
    {
        if (isDragging && !Freeze)
        {
            Vector2 targetPosition = GetGlobalMousePosition();
            Vector2 direction = targetPosition - ToGlobal(localGrabOffset);
            Vector2 targetVelocity = direction * SnapSpeed;
            Vector2 velocityDifference = targetVelocity - LinearVelocity;
            Vector2 globalOffset = GlobalTransform.BasisXform(localGrabOffset);
            // for small masses, reduce the force to avoid slingshotting
            ApplyForce(velocityDifference * SnapDampening * pullFactor * Mathf.Min(0.5f, Mass), globalOffset);
            LinearVelocity = LinearVelocity.LimitLength(MaxSpeedWhendragging);
        }
    }

    public override void _Process(double delta)
    {
        if (sfxPlayer != null)
        {
            UpdateSound(delta);
        }
    }

    private void UpdateSound(double delta)
    {
        adjustedPreviousSfxVolume -= (float)delta * clangVolumeDropOff;
        float currSpeed = LinearVelocity.Length();
        float prevSpeed = lastMeasuredVelocityForClang.Length();

        bool didStop = currSpeed < prevSpeed * ClangVelocityFactor;
        bool isSignificant = prevSpeed > ClangVelocityDelta;
        if (didStop && isSignificant)
        {
            float volume = (prevSpeed - currSpeed - ClangVelocityDelta) * ClangVolumeFactor;
            if (volume > adjustedPreviousSfxVolume)
            {
                adjustedPreviousSfxVolume = Mathf.Clamp(volume, 0, 1);
                sfxPlayer.VolumeLinear = adjustedPreviousSfxVolume;
                sfxPlayer.Play();
            }
        }
        lastMeasuredVelocityForClang = LinearVelocity;
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
