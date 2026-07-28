using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class RocketComponent : Grabbable
{
    // in pixels;
    private const float RocketCheckMargin = 2;

    public const float AnglePull = 5f;
    private List<ThrustSource> thrustSources = [];
    private List<Magnet> magnets = [];
    private List<Tuple<Shape2D, Node2D>> collisionBoxes = [];
    public bool PartOfRocket = false;

    public IReadOnlyList<ThrustSource> ThrustSources => thrustSources;

    // Called when the node enters the scene tree for the first time.

    public override void _Ready()
    {
        base._Ready();

        CollisionLayer = Game.CollisionLayerPrimary | Game.CollisionLayerGrabbable;
        AngularDamp = 1.0f;
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;

        foreach (Node child in GetChildren())
        {
            if (child is ThrustSource thruster)
            {
                thrustSources.Add(thruster);
            }
            else if (child is Magnet magnet)
            {
                magnets.Add(magnet);
            }
            else if (child is CollisionShape2D collider)
            {
                collisionBoxes.Add(new(collider.Shape, collider));
            }
            else if (child is CollisionPolygon2D concaveCollider)
            {
                foreach (Vector2[] piece in Geometry2D.DecomposePolygonInConvex(concaveCollider.Polygon))
                {
                    collisionBoxes.Add(new (
                        new ConvexPolygonShape2D() { Points = piece }, 
                        concaveCollider
                    ));
                }
            }
        }
    }

    // Called every physics update. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (Freeze) return;

        if (!isDragging)
        {
            // note that thruster should be off at the start of the game
            foreach (ThrustSource thruster in thrustSources)
            {
                if (!thruster.EnableThrust) continue;

                Vector2 globalThrustVector = thruster.GetThrust();
                Vector2 globalOffset = thruster.GlobalPosition - GlobalPosition;
                ApplyForce(globalThrustVector, globalOffset);
            }
        }

        foreach (Magnet magnet in magnets)
        {
            Vector2 globalThrustVector = magnet.GetForce();
            Vector2 globalOffset = magnet.GlobalPosition - GlobalPosition;
            ApplyForce(globalThrustVector, globalOffset);
        }
    }

    public List<RigidBody2D> GetNearbyBodies()
    {
        List<RigidBody2D> results = new();

        foreach (var (shape, owner) in collisionBoxes)
        {
            PhysicsShapeQueryParameters2D query = new()
            {
                Shape = shape,
                Transform = owner.GlobalTransform,
                Margin = RocketCheckMargin,
                CollideWithBodies = true,
                CollideWithAreas = false,
                Exclude = [ GetRid() ]
            };
            Array<Dictionary> hits = GetWorld2D().DirectSpaceState.IntersectShape(query, 8);
            foreach (Dictionary hit in hits)
            {
                GodotObject collider = (GodotObject)hit["collider"];
                if (collider is RigidBody2D body) results.Add(body);
            }
        }

        return results;
    }

    private void OnMouseEntered()
    {
    }

    private void OnMouseExited()
    {
    }
}
