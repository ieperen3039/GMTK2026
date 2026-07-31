using Godot;
using Godot.Collections;
using System;
using System.Linq;

[Tool]
public partial class ProtoDuctTape : Line2D
{
    private float length;

    public override void _Ready()
    {
        TopLevel = false;
        AddPoint(Vector2.Zero);
        AddPoint(Vector2.Zero);
        while (Points.Count() > 2)
        {
            RemovePoint(2);
        }
    }

    public Vector2 GlobalAnchorA() => ToGlobal(Points[0]);
    public Vector2 GlobalAnchorB() => ToGlobal(Points[1]);

    public DuctTape Realize()
    {
        PackedScene ductTapeScene = ResourceLoader.Load<PackedScene>("uid://dxtpf7xkx1g4k");
        DuctTape tape = ductTapeScene.Instantiate<DuctTape>();
        tape._Ready();

        // A
        PhysicsPointQueryParameters2D query = new()
        {
            Position = GlobalAnchorA(),
        };

        Array<Dictionary> hits = GetWorld2D().DirectSpaceState.IntersectPoint(query, 1);
        if (hits.Count > 0)
        {
            GodotObject collider = (GodotObject)hits[0]["collider"];
            if (collider is RocketComponent body)
            {
                tape.Attach(body, body.ToLocal(GlobalAnchorA()));
            }
        }

        // B
        query = new()
        {
            Position = GlobalAnchorB(),
        };

        hits = GetWorld2D().DirectSpaceState.IntersectPoint(query, 1);
        if (hits.Count > 0)
        {
            GodotObject collider = (GodotObject)hits[0]["collider"];
            if (collider is RocketComponent body)
            {
                tape.Attach(body, body.ToLocal(GlobalAnchorB()));
            }
        }

        return tape;
    }
}
