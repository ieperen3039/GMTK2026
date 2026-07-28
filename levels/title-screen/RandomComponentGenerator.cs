using Godot;
using System;

public partial class RandomComponentGenerator : Node2D
{
    private const float TimeBetweenAdd = 1f;
    private float timeUntilNewAdd = 0;
    private int componentsAdded = 0;

    private Vector2 SpawnPosition;
    private Tuple<PackedScene, int>[] componentScenes;
    private int totalWeight = 0;
    private Random rng = new();
    CountdownTimer timer;

    // Called when the node enters the scene tree for the first time.

    public override void _Ready()
    {
        SpawnPosition = GetNode<Marker2D>("SpawnPosition").Position;
        componentScenes = [
            new(ResourceLoader.Load<PackedScene>("uid://3ypjldxcxkvw"), 10), // tank
            new(ResourceLoader.Load<PackedScene>("uid://d3sd7kyiugv60"), 5), // cone
            new(ResourceLoader.Load<PackedScene>("uid://c4x5k3q1n002b"), 4), // thruster
            new(ResourceLoader.Load<PackedScene>("uid://b5v30djg1rxq8"), 5), // mini thruster
            new(ResourceLoader.Load<PackedScene>("uid://73ile0xgnbys"), 2), // traffic cone
            new(ResourceLoader.Load<PackedScene>("uid://s7v7dbr5g7n4"), 1), // bowling ball
        ];

        foreach (var (_, weigth) in componentScenes)
        {
            totalWeight += weigth;
        }

        
        timer = GetNode<CountdownTimer>("%CountdownTimer");
        timer.Quiet = true;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        timeUntilNewAdd -= (float)delta;
        if (timeUntilNewAdd < 0)
        {
            timeUntilNewAdd += TimeBetweenAdd;
            componentsAdded++;
            timer.Value = componentsAdded;
            // add component

            int selection = rng.Next() % totalWeight;
            foreach (var (scene, weigth) in componentScenes)
            {
                selection -= weigth;
                if (selection < 0)
                {
                    RigidBody2D component = scene.Instantiate<RigidBody2D>();
                    component.GlobalPosition = SpawnPosition;
                    Util.Toss(component, rng, 100, 20);
                    AddChild(component);
                    break;
                }
            }
        }
    }
}
