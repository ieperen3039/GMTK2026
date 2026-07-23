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

    public Func<Vector2, RocketComponent> ComponentQuery;

    private const float Pull = 10;
    public const float MousePullFactor = 0.1f;

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

    public void Attach(RocketComponent component, Vector2 relativeAttachmentPosition)
    {
        switch (Status)
        {
            case StatusValue.Empty:
                GD.Print("Tape attach A");
                ComponentA = component;
                anchorA = relativeAttachmentPosition;
                Vector2 linePoint = ToLocal(ComponentA.ToGlobal(relativeAttachmentPosition));
                graphic.AddPoint(linePoint);
                graphic.AddPoint(linePoint + Vector2.Up);
                return;

            case StatusValue.HalfConnected:
                if (component == ComponentA)
                {
                    ComponentA = null;
                    return;
                }

                GD.Print("Tape attach B");
                ComponentB = component;
                anchorB = relativeAttachmentPosition;
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

        ComponentA.ApplyForce(velocityDifference * RocketComponent.SnapDampening);
    }

    private void UpdateConnected(double delta)
    {
        Vector2 globalAnchorA = ComponentA.ToGlobal(anchorA);
        Vector2 globalAnchorB = ComponentB.ToGlobal(anchorB);

        graphic.SetPointPosition(0, ToLocal(globalAnchorA));
        graphic.SetPointPosition(1, ToLocal(globalAnchorB));

        Vector2 vecAToB = globalAnchorB - globalAnchorA;
        Vector2 force = vecAToB.Normalized() * Pull;

        ComponentA.ApplyForce(force, globalAnchorA - ComponentA.GlobalPosition);
        ComponentB.ApplyForce(-force, globalAnchorB - ComponentB.GlobalPosition);
    }
}
