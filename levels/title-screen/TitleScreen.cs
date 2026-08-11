using Godot;
using System;

public partial class TitleScreen : Control
{
    [Signal]
    public delegate void OnLevelSelectEventHandler(int levelIndex);


    private Control mainMenu;
    private Control levelSelectionMenu;
    private Control creditsMenu;
    private Control tipsMenu;

    // Called when the node enters the scene tree for the first time.

    public override void _Ready()
    {
        mainMenu = GetNode<Control>("MainMenu");
        levelSelectionMenu = GetNode<Control>("LevelSelection");
        creditsMenu = GetNode<Control>("Credits");
        tipsMenu = GetNode<Control>("Tips");

        // NOTE: Start actually shows tips
        mainMenu.GetNode<Button>("%ButtonStart").Pressed += SetTips;
        // tips.start starts the game
        Button StartButton = tipsMenu.GetNode<Button>("%ButtonStart");
        StartButton.Pressed += () => EmitSignal(SignalName.OnLevelSelect, 0);

        mainMenu.GetNode<Button>("%ButtonExit").Pressed += () => GetTree().Quit();
        mainMenu.GetNode<Button>("%ButtonLevelSelection").Pressed += SetLevelSelection;
        mainMenu.GetNode<Button>("%ButtonCredits").Pressed += SetCredits;

        levelSelectionMenu.GetNode<Button>("%ButtonBack").Pressed += SetMainMenu;
        creditsMenu.GetNode<Button>("%ButtonBack").Pressed += SetMainMenu;
        tipsMenu.GetNode<Button>("%ButtonBack").Pressed += SetMainMenu;

        Container container = levelSelectionMenu.GetNode<Container>("%LevelButtons");

        int levelIdx = 0;
        foreach (Node node in container.GetChildren())
        {
            if (node is Button button)
            {
                int levelIndexForLambda = levelIdx++;
                GD.Print($"Assigning level index {levelIdx} to button {button.Name}");
                button.Pressed += () => EmitSignal(SignalName.OnLevelSelect, levelIndexForLambda);
            }
        }

        SetMainMenu();
    }

    private void SetLevelSelection()
    {
        mainMenu.Visible = false;
        levelSelectionMenu.Visible = true;
        creditsMenu.Visible = false;
        tipsMenu.Visible = false;
    }

    private void SetCredits()
    {
        mainMenu.Visible = false;
        levelSelectionMenu.Visible = false;
        creditsMenu.Visible = true;
        tipsMenu.Visible = false;
    }

    private void SetTips()
    {
        mainMenu.Visible = false;
        levelSelectionMenu.Visible = false;
        creditsMenu.Visible = false;
        tipsMenu.Visible = true;
    }

    private void SetMainMenu()
    {
        mainMenu.Visible = true;
        levelSelectionMenu.Visible = false;
        creditsMenu.Visible = false;
        tipsMenu.Visible = false;
    }
}
