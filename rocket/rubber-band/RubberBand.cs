using Godot;
using System;

public partial class RubberBand : Node2D
{
    public enum StatusValue
    {
        Empty,
        HalfConnected,
        FullConnected
    }

    private PackedScene connectorScene;
    private RigidBody2D ComponentA = null;
    private RigidBody2D ComponentB = null;
    private Line2D graphic;
    private const float ForceFactor = 25f;
    private const float MaxForce = 10_000f;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        connectorScene = ResourceLoader.Load<PackedScene>("uid://cwcg3yk8l3hi8");
        graphic = GetNode<Line2D>("Graphics");
        graphic.TopLevel = false;
    }
    
    public StatusValue Status => ComponentA == null ? StatusValue.Empty : (ComponentB == null ? StatusValue.HalfConnected : StatusValue.FullConnected);

    public void Place(Vector2 where)
    {
        switch (Status)
        {
            case StatusValue.Empty:
                ComponentA = connectorScene.Instantiate<RigidBody2D>();
                ComponentA.GlobalPosition = where;
                AddChild(ComponentA);
                graphic.AddPoint(where);
                graphic.AddPoint(where);
                ComponentA.Freeze = true;
                return;

            case StatusValue.HalfConnected:
                ComponentB = connectorScene.Instantiate<RigidBody2D>();
                ComponentB.GlobalPosition = where;
                AddChild(ComponentB);

                ComponentA.Freeze = false;
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
        graphic.SetPointPosition(0, ToLocal(ComponentA.GlobalPosition));
        graphic.SetPointPosition(1, ToLocal(mousePosition));
    }

    private void UpdateConnected(double delta)
    {
        graphic.SetPointPosition(0, ToLocal(ComponentA.GlobalPosition));
        graphic.SetPointPosition(1, ToLocal(ComponentB.GlobalPosition));

        // relative to A
        Vector2 gapAToB = ComponentB.GlobalPosition - ComponentA.GlobalPosition;
        Vector2 force = (gapAToB * ForceFactor).LimitLength(MaxForce);

        ComponentA.ApplyCentralForce(force);
        ComponentB.ApplyCentralForce(-force);
    }
}
