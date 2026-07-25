using Godot;
using System;

public partial class TitleScreen : Control
{
    public Button StartButton => GetNode<Button>("%Start");
    public Button CreditsButton => GetNode<Button>("%Credits");
    public Button LevelSelectionButton => GetNode<Button>("%LevelSelection");
    public Button ExitButton => GetNode<Button>("%Exit");

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }
}
