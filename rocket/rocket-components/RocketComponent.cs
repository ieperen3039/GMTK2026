using Godot;
using System;

public partial class RocketComponent : CharacterBody2D
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        InputPickable = true;
        CollisionLayer = 0b_0001;
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
    }
    
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    private void OnMouseEntered()
    {
        GD.Print("Mouse entered");
    }

    private void OnMouseExited()
    {
        GD.Print("Mouse exited");
    }

}
