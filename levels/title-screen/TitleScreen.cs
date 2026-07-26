using Godot;
using System;

public partial class TitleScreen : Control
{
    [Signal]
    public delegate void OnLevelSelectEventHandler(int levelIndex);


    private Control mainMenu;
    private Control levelSelectionMenu;
    private Control creditsMenu;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        mainMenu = GetNode<Control>("MainMenu");
        levelSelectionMenu = GetNode<Control>("LevelSelection");
        creditsMenu = GetNode<Control>("Credits");

        Button StartButton = mainMenu.GetNode<Button>("%Start");
        StartButton.Pressed += () => EmitSignal(SignalName.OnLevelSelect, 0);

        mainMenu.GetNode<Button>("%Exit").Pressed += () => GetTree().Quit();
        mainMenu.GetNode<Button>("%Credits").Pressed += SetCredits;
        mainMenu.GetNode<Button>("%LevelSelection").Pressed += SetLevelSelection;

        levelSelectionMenu.GetNode<Button>("%Back").Pressed += SetMainMenu;
        creditsMenu.GetNode<Button>("%Back").Pressed += SetMainMenu;

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
    }

    private void SetCredits()
    {
        mainMenu.Visible = false;
        levelSelectionMenu.Visible = false;
        creditsMenu.Visible = true;
    }

    private void SetMainMenu()
    {
        mainMenu.Visible = true;
        levelSelectionMenu.Visible = false;
        creditsMenu.Visible = false;

    }
}
