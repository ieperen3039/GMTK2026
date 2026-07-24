using Godot;
using System;

public partial class LevelComplete : Node
{
    [Signal]
    public delegate void OnNextLevelEventHandler();

    public const float FadeDuration = 1.0f;
    private bool hasFired = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Node2D textNode = GetNode<Node2D>("Text");
        textNode.Modulate = new(Colors.White, 0);

        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(textNode, "modulate:a", 1.0, FadeDuration)
            .SetTrans(Tween.TransitionType.Cubic);

        Button continueButton = GetNode<Button>("ContinueButton");
        continueButton.Modulate = new(Colors.White, 0);
        
        tween.TweenProperty(continueButton, "modulate:a", 1.0, FadeDuration)
            .SetTrans(Tween.TransitionType.Cubic);
        tween.TweenCallback(Callable.From(() => continueButton.Pressed += OnContinue));
    }

    private void OnContinue()
    {
        GD.Print("ContinueButton::OnMouseEvent");

        if (hasFired) return;
        hasFired = true;

        EmitSignal(SignalName.OnNextLevel);
    }
}
