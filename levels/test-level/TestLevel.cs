using Godot;
using System;
using System.Collections.Generic;

public partial class TestLevel : Node2D
{
    private Rocket rocket;
    private Node2D centerOfMass;
    private List<RocketComponent> rocketComponents = new();
    private Node ductTapeInstancesNode;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Node rocketComponentsNode = GetNode<Node>("RocketComponents");
        ductTapeInstancesNode = GetNode<Node>("DuctTapeInstances");
        PackedScene rocketScene = ResourceLoader.Load<PackedScene>("uid://dmdekhk5ugqao");
        PackedScene ductTapeScene = ResourceLoader.Load<PackedScene>("uid://dxtpf7xkx1g4k");

        rocket = rocketScene.Instantiate<Rocket>();

        foreach (Node child in rocketComponentsNode.GetChildren())
        {
            if (child is RocketComponent part)
            {
                rocketComponents.Add(part);
                rocket.AddComponent(part);
                part.LinearDamp = 100;
            }
        }

        // tape everything
        foreach (RocketComponent part in rocketComponents)
        {
            foreach (RocketComponent part2 in rocketComponents)
            {
                if (part == part2) continue;
                if (part.GlobalPosition.DistanceTo(part2.GlobalPosition) > 46) continue;

                DuctTape tape = ductTapeScene.Instantiate<DuctTape>();
                ductTapeInstancesNode.AddChild(tape);

                tape.Attach(part, Vector2.Zero);
                tape.Attach(part2, Vector2.Zero);
            }
        }

        GetNode<Camera2D>("Camera2D").Reparent(rocket.ControlComponent, false);
        centerOfMass = GetNode<Node2D>("ComIndicator");
        centerOfMass.Position = rocket.ToGlobal(rocket.CenterOfMass);
        centerOfMass.Visible = true;

        AddChild(rocket);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
    {
        foreach (Node child in ductTapeInstancesNode.GetChildren())
        {
            if (child is ProtoDuctTape proto)
            {
                DuctTape tape = proto.Realize();
                ductTapeInstancesNode.AddChild(tape);
                proto.QueueFree();
            }
        }

        foreach (RocketComponent part in rocketComponents)
        {
            part.LinearDamp = 0;
        }

        centerOfMass.Position = rocket.ToGlobal(rocket.CenterOfMass);
    }

}
