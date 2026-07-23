using Godot;
using System;

public partial class DuctTape : Node2D
{
    public enum StatusValue
    {
        Empty,
        HalfConnected,
        FullConnected
    }

    public const float MousePullFactor = 0.2f;
    public const float SnapSpeed = 400f;
    public const float SnapDampening = 1f;

    public RocketComponent ComponentA { get; private set; } = null;
    private Vector2 anchorA = new();
    public RocketComponent ComponentB { get; private set; } = null;

    private Vector2 anchorB = new();

    private Line2D graphic;
    private float length;

    public override void _Ready()
    {
        GD.Print("Tape Ready");
        graphic = GetNode<Line2D>("Graphics");
    }

    public StatusValue Status => ComponentA == null ? StatusValue.Empty : (ComponentB == null ? StatusValue.HalfConnected : StatusValue.FullConnected);

    public void Attach(RocketComponent component, Vector2 localAttachmentPosition)
    {
        switch (Status)
        {
            case StatusValue.Empty:
                GD.Print("Tape attach A");
                ComponentA = component;
                anchorA = localAttachmentPosition;
                Vector2 linePoint = ToLocal(ComponentA.ToGlobal(localAttachmentPosition));
                graphic.AddPoint(linePoint);
                graphic.AddPoint(linePoint);
                return;

            case StatusValue.HalfConnected:
                if (component == ComponentA)
                {
                    GD.Print("Tape detach A");
                    ComponentA = null;
                    return;
                }

                GD.Print("Tape attach B");
                ComponentB = component;
                anchorB = localAttachmentPosition;

                Vector2 globalAnchorA = ComponentA.ToGlobal(anchorA);
                Vector2 globalAnchorB = ComponentB.ToGlobal(anchorB);
                length = globalAnchorA.DistanceTo(globalAnchorB);
                return;

            case StatusValue.FullConnected:
                throw new Exception("Already attached to two components");
        }
    }

    public void Update(double delta)
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
        Vector2 globalAnchorA = ComponentA.ToGlobal(anchorA);
        graphic.SetPointPosition(0, ToLocal(globalAnchorA));
        graphic.SetPointPosition(1, ToLocal(mousePosition));

        Vector2 direction = mousePosition - ComponentA.GlobalPosition;
        Vector2 targetVelocity = direction * RocketComponent.SnapSpeed * MousePullFactor;
        Vector2 velocityDifference = targetVelocity - ComponentA.LinearVelocity;

        ComponentA.ApplyForce(velocityDifference * RocketComponent.SnapDampening * MousePullFactor);
    }

    private void UpdateConnected(double delta)
    {
        Vector2 globalAnchorA = ComponentA.ToGlobal(anchorA);
        Vector2 globalAnchorB = ComponentB.ToGlobal(anchorB);

        graphic.SetPointPosition(0, ToLocal(globalAnchorA));
        graphic.SetPointPosition(1, ToLocal(globalAnchorB));

        // relative to A
        Vector2 gapBToA = globalAnchorA - globalAnchorB;
        Vector2 targetMovement = (gapBToA.Normalized() * length) - gapBToA;
        Vector2 targetVelocity = targetMovement * SnapSpeed;
        Vector2 velocityDifference = targetVelocity - (ComponentA.LinearVelocity - ComponentB.LinearVelocity);
        Vector2 force = velocityDifference * SnapDampening;

        ComponentA.ApplyForce(force, globalAnchorA - ComponentA.GlobalPosition);
        ComponentB.ApplyForce(-force, globalAnchorB - ComponentB.GlobalPosition);
    }
}
