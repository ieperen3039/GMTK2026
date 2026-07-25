using Godot;
using System;

public partial class Magnet : Node2D
{
    public const float PullStrength = 100f;

    public Magnet connectedTo = null;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Area2D pullAreaNode = GetNode<Area2D>("PullArea");
        pullAreaNode.Monitorable = true;
        pullAreaNode.BodyEntered += OnShapeEnter;
        pullAreaNode.BodyExited += OnShapeExit;
        pullAreaNode.CollisionLayer = Game.CollisionLayerMagnet;
        pullAreaNode.CollisionMask = Game.CollisionLayerMagnet;
    }

    private void OnShapeEnter(Node2D body)
    {
        if (body is Magnet other && connectedTo != null)
        {
            connectedTo = other;
        }
    }

    private void OnShapeExit(Node2D body)
    {
        if (body is Magnet)
        {
            connectedTo = null;
        }
    }
    
    public Vector2 GetForce()
    {
        if (connectedTo == null) return Vector2.Zero;

        return GlobalPosition.DirectionTo(connectedTo.GlobalPosition) * PullStrength;
    }
}
