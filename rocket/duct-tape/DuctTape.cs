using Godot;
using System;

public partial class DuctTape : Node2D
{
    public enum StatusValue
    {
        Empty,
        HalfConnected, // ComponentA is held by us
        FullConnected
    }

    public const float MousePullFactor = 0.05f;
    public const float SnapSpeed = 1000f;
    public const float SnapDampening = 1f;
    private const float MaxForce = 100_000f;

    public RocketComponent ComponentA { get; private set; } = null;
    private Vector2 anchorA = new();
    public RocketComponent ComponentB { get; private set; } = null;
    private Vector2 anchorB = new();

    private Line2D graphic;
    private float length;


    public override void _Ready()
    {
        graphic = GetNode<Line2D>("Graphics");
        graphic.TopLevel = false;
    }

    public StatusValue Status => ComponentA == null ? StatusValue.Empty : (ComponentB == null ? StatusValue.HalfConnected : StatusValue.FullConnected);

    public void Attach(RocketComponent component, Vector2 localAttachmentPosition)
    {
        switch (Status)
        {
            case StatusValue.Empty:
                GD.Print("Tape attach A");
                // set position to make it easier later when converting to rocket
                Position = component.Position;
                ComponentA = component;
                anchorA = localAttachmentPosition;
                ComponentA.OnGrab(localAttachmentPosition, MousePullFactor);
                Vector2 linePoint = ToLocal(GlobalAnchorA());
                graphic.AddPoint(linePoint);
                graphic.AddPoint(linePoint);
                return;

            case StatusValue.HalfConnected:
                ComponentA.OnRelease();
                if (component == ComponentA)
                {
                    GD.Print("Tape detach A");
                    ComponentA = null;
                    return;
                }

                GD.Print("Tape attach B");
                ComponentB = component;
                anchorB = localAttachmentPosition;

                length = GlobalAnchorA().DistanceTo(GlobalAnchorB());
                return;

            case StatusValue.FullConnected:
                throw new Exception("Already attached to two components");
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Status == StatusValue.FullConnected)
        {
            UpdateConnected(delta);
        }
        else if (Status == StatusValue.HalfConnected)
        {
            UpdateHalfConnected();
        }
    }

    private void UpdateHalfConnected()
    {
        Vector2 mousePosition = GetGlobalMousePosition();

        // update graphical part
        Vector2 globalAnchorA = GlobalAnchorA();
        graphic.SetPointPosition(0, ToLocal(globalAnchorA));
        graphic.SetPointPosition(1, ToLocal(mousePosition));

        // component A pull is handled by RocketComponent
    }

    private void UpdateConnected(double delta)
    {
        Vector2 globalAnchorA = GlobalAnchorA();
        Vector2 globalAnchorB = GlobalAnchorB();

        graphic.SetPointPosition(0, ToLocal(globalAnchorA));
        graphic.SetPointPosition(1, ToLocal(globalAnchorB));

        // relative to A
        Vector2 gapAToB = globalAnchorB - globalAnchorA;
        float modifiedLength = Mathf.Clamp(gapAToB.Length() - length, 1f, length * 2f);
        Vector2 targetMovement = gapAToB.Normalized() * modifiedLength;
        Vector2 targetVelocity = targetMovement * SnapSpeed;
        Vector2 velocityDifference = targetVelocity - (ComponentA.LinearVelocity - ComponentB.LinearVelocity);
        Vector2 force = (velocityDifference * SnapDampening).LimitLength(MaxForce);

        ComponentA.ApplyForce(force, globalAnchorA - ComponentA.GlobalPosition);
        ComponentB.ApplyForce(-force, globalAnchorB - ComponentB.GlobalPosition);
        
        if (gapAToB.Length() > length * 2) Snap();
    }

    public void Snap()
    {
        if (Status == StatusValue.HalfConnected)
        {
            ComponentA.OnRelease();
        }

        ComponentA = null;
        ComponentB = null;
        graphic.ClearPoints();
    }


    public Vector2 GlobalAnchorA() => ComponentA.ToGlobal(anchorA);
    public Vector2 GlobalAnchorB() => ComponentB.ToGlobal(anchorB);

    public void ReparentGraphics(Node2D newParent)
    {
        // this also takes care of the global-to-local conversions of the current points
        graphic.Reparent(newParent);
        graphic = null;
    }
}
