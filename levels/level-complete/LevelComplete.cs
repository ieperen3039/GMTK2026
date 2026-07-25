using Godot;
using System;

public partial class LevelComplete : Node
{
    [Signal]
    public delegate void OnNextLevelEventHandler();

    public const float FadeDuration = 1.0f;
    private bool hasFired = false;
    [Export]
    public int NumLiftedComponents;
    [Export]
    public int NumExtras;
    [Export]
    public int TotalComponents;

    Label scoreNode;
    Label extraScoreNode;


    public override void _Ready()
    {
        Tween tween = GetTree().CreateTween();

        Control textNode = GetNode<Control>("%Title");
        textNode.Modulate = new(Colors.White, 0);

        tween.TweenProperty(textNode, "modulate:a", 1.0, FadeDuration)
            .SetTrans(Tween.TransitionType.Cubic);

        scoreNode = GetNode<Label>("%ScoreText");
        scoreNode.Modulate = new(Colors.White, 0);

        tween.TweenProperty(scoreNode, "modulate:a", 1.0, FadeDuration)
            .SetTrans(Tween.TransitionType.Cubic);
        tween.Parallel()
            .TweenMethod(Callable.From<int>(SetVisibleScore), 0, NumLiftedComponents, 2.0f);

        extraScoreNode = GetNode<Label>("%ExtraScoreText");
        extraScoreNode.Modulate = new(Colors.White, 0);

        tween.TweenProperty(extraScoreNode, "modulate:a", 1.0, FadeDuration)
            .SetTrans(Tween.TransitionType.Cubic);
        tween.Parallel()
            .TweenMethod(Callable.From<int>(SetVisibleExtraScore), 0, NumExtras, 2.0f);

        Button continueButton = GetNode<Button>("%ContinueButton");
        continueButton.Modulate = new(Colors.White, 0);
        
        tween.TweenProperty(continueButton, "modulate:a", 1.0, FadeDuration)
            .SetTrans(Tween.TransitionType.Cubic);
        tween.TweenCallback(Callable.From(() => continueButton.Pressed += OnContinue));
    }

    private void SetVisibleScore(int count)
    {
        scoreNode.Text = $"Components lifted: {count} / {TotalComponents}";
    }

    private void SetVisibleExtraScore(int count)
    {
        extraScoreNode.Text = $"Extra objects: {count}";
    }

    private void OnContinue()
    {
        GD.Print("ContinueButton::OnMouseEvent");

        if (hasFired) return;
        hasFired = true;

        EmitSignal(SignalName.OnNextLevel);
    }
}
