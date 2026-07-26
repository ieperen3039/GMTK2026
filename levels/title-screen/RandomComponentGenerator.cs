using Godot;
using System;

public partial class RandomComponentGenerator : Node2D
{
    private const float TimeBetweenAdd = 1f;
    private float timeUntilNewAdd = -3;
    private int componentsAdded = 1;

    private Vector2 SpawnPosition;
    private PackedScene[] componentScene;
    private Random rng = new();

    // Called when the node enters the scene tree for the first time.

    public override void _Ready()
    {
        SpawnPosition = GetNode<Marker2D>("SpawnPosition").Position;
        componentScene = [
            ResourceLoader.Load<PackedScene>("uid://3ypjldxcxkvw"), // tank
            ResourceLoader.Load<PackedScene>("uid://3ypjldxcxkvw"), // tank
            ResourceLoader.Load<PackedScene>("uid://d3sd7kyiugv60"), // cone
        ];
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        timeUntilNewAdd -= (float) delta;
        if (timeUntilNewAdd < 0)
        {
            timeUntilNewAdd += TimeBetweenAdd;
            componentsAdded++;
            // add component

            int idx = rng.Next() % componentScene.Length;
            RocketComponent component = componentScene[idx].Instantiate<RocketComponent>();
            component.GlobalPosition = SpawnPosition;
            AddChild(component);
        }
    }
}
