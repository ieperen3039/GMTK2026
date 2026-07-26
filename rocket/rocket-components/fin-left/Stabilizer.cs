using Godot;
using System;

public partial class Stabilizer : RocketComponent
{
    public const float DragFactor = 0.01f;

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
    {
        ApplyCentralForce(-LinearVelocity * DragFactor);
    
        base._PhysicsProcess(delta);
    }
}
