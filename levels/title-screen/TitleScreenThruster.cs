using Godot;
using System;
using System.Collections.Generic;

public partial class TitleScreenThruster : RigidBody2D
{
    [Export]
    public float ThrustPower { get; private set; }
    [Export]
    public double TimeUntilBlastoff = 10f;
    [Export]
    public double ThrustTime = 5f;
    public bool IsBlastingOff = false;

    private List<CpuParticles2D> particles = [];

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Node2D particlesNode = GetNode<Node2D>("ExhaustParticles");
        foreach (Node node in particlesNode.GetChildren())
        {
            if (node is CpuParticles2D particle)
            {
                particles.Add(particle);
                particle.Emitting = false;
                particle.Visible = true;
            }
        }
    }

    // Called every physics update. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
    {
        TimeUntilBlastoff -= delta;

        if (TimeUntilBlastoff < -ThrustTime)
        {
            if (IsBlastingOff)
            {
                IsBlastingOff = false;
                foreach (var particle in particles)
                {
                    particle.Emitting = false;
                }
            }
        }
        else if (TimeUntilBlastoff < 0)
        {
            if (!IsBlastingOff)
            {
                IsBlastingOff = true;

                GD.Print($"particle.Emitting = true");
                foreach (var particle in particles)
                {
                    particle.Emitting = true;
                }
            }
        }

        if (IsBlastingOff)
        {
            float undulation = (float) Mathf.Sin(TimeUntilBlastoff * 10) * 0.2f;

            Vector2 force = GlobalTransform.BasisXform(Vector2.Up * ThrustPower).Rotated(undulation);
            ApplyCentralForce(force);

            foreach (var particle in particles)
            {
                particle.Rotation = undulation;
            }
        }
    }
}
