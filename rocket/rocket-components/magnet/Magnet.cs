using Godot;
using System;

public partial class Magnet : Area2D
{
    public const float PullStrength = 500f;

    public Magnet connectedTo = null;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Monitoring = true;
        AreaEntered += OnShapeEnter;
        AreaExited += OnShapeExit;
        CollisionLayer = Game.CollisionLayerMagnet;
        CollisionMask = Game.CollisionLayerMagnet;
    }

    private void OnShapeEnter(Area2D body)
    {
        if (body is Magnet other)
        {
            GD.Print($"Magnet {Name} connect to {body.Name}");
            connectedTo = other;
        }
    }

    private void OnShapeExit(Area2D body)
    {
        if (body is Magnet && connectedTo != null)
        {
            GD.Print($"Magnet {Name} disconnect from {body.Name}");
            connectedTo = null;
        }
    }
    
    public Vector2 GetForce()
    {
        if (connectedTo == null) return Vector2.Zero;

        return GlobalPosition.DirectionTo(connectedTo.GlobalPosition) * PullStrength;
    }
}
