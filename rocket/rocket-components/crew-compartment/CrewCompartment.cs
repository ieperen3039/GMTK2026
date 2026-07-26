using Godot;
using System;
using System.Collections.Generic;

public partial class CrewCompartment : ControlComponent
{
    private Sprite2D fullSprite;
    private Sprite2D emptySprite;
    private bool isFilled = false;
    public int NumCrewInside { get; private set; } = 0;

    public override void _Ready()
    {
        base._Ready();

        fullSprite = GetNode<Sprite2D>("Full");
        emptySprite = GetNode<Sprite2D>("Empty");
        Area2D collectionHitbox = GetNode<Area2D>("CrewCollectionHitbox");
        collectionHitbox.BodyEntered += OnBodyEnter;

        SetFilled(false);
    }

    private void OnBodyEnter(Node2D body)
    {

        if (body is CrewMember crew)
        {
            // eat
            SetFilled(true);
            NumCrewInside++;
            Mass += crew.Mass;
            crew.OnRelease();
            crew.Visible = false;
            crew.ProcessMode = ProcessModeEnum.Disabled;
            crew.Reparent(this, false);
            crew.Position = Vector2.Zero;
        }
    }


    public void SetFilled(bool setFilled)
    {
        isFilled = setFilled;
        fullSprite.Visible = isFilled;
        emptySprite.Visible = !isFilled;
    }
}
