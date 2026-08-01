using Godot;
using System;

public partial class KeelSection : RocketComponent
{
    private const int Stiffness = 75;
    private const int SpringLength = 4;

    [Export]
    private KeelSection ConnectedTo;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Vector2 jointPosition = GetNode<Node2D>("Joint").Position;

        if (ConnectedTo != null)
        {
            AddChild(new PinJoint2D()
            {
                Position = jointPosition,
                NodeA = this.GetPath(),
                NodeB = ConnectedTo.GetPath()
            });

            AddChild(new DampedSpringJoint2D()
            {
                Position = jointPosition + new Vector2(64, 0),
                NodeA = this.GetPath(),
                NodeB = ConnectedTo.GetPath(),
                Length = 2 * SpringLength,
                RestLength = SpringLength,
                Stiffness = Stiffness
            });

            AddChild(new DampedSpringJoint2D()
            {
                Position = jointPosition - new Vector2(64, 0),
                NodeA = this.GetPath(),
                NodeB = ConnectedTo.GetPath(),
                Length = 2 * SpringLength,
                RestLength = SpringLength,
                Stiffness = Stiffness
            });
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
