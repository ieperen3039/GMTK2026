using Godot;
using System;

public partial class LevelComplete : Node2D
{
    [Signal]
    public delegate void OnNextLevelEventHandler();

    public const float FadeDuration = 1.0f;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Node2D textNode = GetNode<Node2D>("Text");
        textNode.Modulate = new(Colors.White, 0);

        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(textNode, "modulate:a", 1.0, FadeDuration)
            .SetTrans(Tween.TransitionType.Cubic);

        Area2D clickArea = GetNode("ContinueButton").GetNode<Area2D>("Area2D");
        clickArea.InputPickable = true;
        clickArea.CollisionLayer = 0b_1000;
        clickArea.InputEvent += OnMouseEvent;
        // TODO add delayed fade-in for the "continue" button, which calls OnNextLevelEventHandler
    }

    private void OnMouseEvent(Node viewport, InputEvent mouse_event, long shapeIdx)
    {
        if (mouse_event.IsActionPressed("Click"))
        {
            EmitSignal(SignalName.OnNextLevel);
        }
    }

}
