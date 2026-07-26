using Godot;
using System;

public partial class LevelComplete : Node
{
    [Signal]
    public delegate void OnNextLevelEventHandler();

    public const float FadeDuration = 1.0f;
    private bool hasFired = false;
    public Score Score;

    private Label scoreNode;
    private Label extraScoreNode;

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
            .TweenMethod(Callable.From<int>(SetVisibleScore), 0, Score.NumLiftedComponents, 2.0f);

        extraScoreNode = GetNode<Label>("%ExtraScoreText");
        extraScoreNode.Modulate = new(Colors.White, 0);

        tween.TweenProperty(extraScoreNode, "modulate:a", 1.0, FadeDuration)
            .SetTrans(Tween.TransitionType.Cubic);
        tween.Parallel()
            .TweenMethod(Callable.From<int>(SetVisibleExtraScore), 0, Score.NumExtras, 2.0f);

        Button continueButton = GetNode<Button>("%ContinueButton");
        continueButton.Modulate = new(Colors.White, 0);
        
        tween.TweenProperty(continueButton, "modulate:a", 1.0, FadeDuration)
            .SetTrans(Tween.TransitionType.Cubic);
        tween.TweenCallback(Callable.From(() => continueButton.Pressed += OnContinue));
    }

    private void SetVisibleScore(int count)
    {
        scoreNode.Text = $"Components lifted: {count} / {Score.TotalComponents}";
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
