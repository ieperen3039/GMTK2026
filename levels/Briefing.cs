using Godot;
using System;

public partial class Briefing : Control
{
    public Button StartButton => GetNode<Button>("%ButtonStart");
    public Button MainMenuButton => GetNode<Button>("%ButtonMainMenu");
}
