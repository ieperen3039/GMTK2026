using Godot;
using System;

public partial class CrewMember : Grabbable
{
    private const string AnimationNameWalk = "Walk";
    private const string AnimationNameFall = "Fall";
    private const string AnimationNameStand = "Stand";
    private const float WalkForceFactor = 50f;
    private const float TargetWalkSpeed = 5f;
    private const float FallVelocity = 10f;

    public Node2D WalkTarget;
    private float WalkForce;
    private bool WalkLeft = true;

    private AnimatedSprite2D animation;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        WalkTarget = this;

        base._Ready();
        animation = GetNode<AnimatedSprite2D>("Animation");
        animation.Play(AnimationNameStand);
        WalkForce = PhysicsMaterialOverride.Friction * WalkForceFactor;

        LockRotation = true;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        base._Process(delta);
        bool isAtTarget = Mathf.Abs(GlobalPosition.X - WalkTarget.GlobalPosition.X) < 0.1f;

        switch (animation.Animation)
        {
            case AnimationNameStand:
                if (Sleeping == true && !isAtTarget) animation.Play(AnimationNameWalk);
                if (LinearVelocity.Y > 1f) animation.Play(AnimationNameFall);
                break;
            case AnimationNameWalk:
                if (isAtTarget) animation.Play(AnimationNameStand);
                else if (LinearVelocity.Length() > FallVelocity) animation.Play(AnimationNameFall);
                break;
            case AnimationNameFall:
                if (Sleeping == true) animation.Play(AnimationNameWalk);
                if (LinearVelocity.Y <= 0) animation.Play(AnimationNameStand);
                break;
            default:
                GD.Print($"Unhandled animation '{animation.Animation}'");
                animation.Play(AnimationNameWalk);
                break;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        WalkLeft = GlobalPosition.X < WalkTarget.GlobalPosition.X;

        if (WalkLeft) animation.Scale = new(-1, 1);
        else animation.Scale = new(1, 1);

        if (!isDragging && animation.Animation == AnimationNameWalk)
        {
            Sleeping = false;
            float fractionOfTargetSpeed = Mathf.Abs(LinearVelocity.X / TargetWalkSpeed);
            float totalWalkForce = WalkForce * (1.1f - Mathf.Clamp(fractionOfTargetSpeed, 0, 1));

            if (WalkLeft)
            {
                // also pull up a little for the sake of figting friction
                ApplyCentralForce(new Vector2(totalWalkForce, -Mass * 400));
            }
            else
            {
                // also pull up a little for the sake of figting friction
                ApplyCentralForce(new Vector2(-totalWalkForce, -Mass * 400));
            }
        }
    }

}
