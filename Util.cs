using System;
using Godot;

class Util
{
    // mod rotation to (-180, +180) degrees
    public static float RotationRelativeToUp(float angle)
    {
        return Mathf.PosMod(angle + Mathf.Pi, 2 * Mathf.Pi) - Mathf.Pi;
    }

    public static void Toss(RigidBody2D target, Random rng, float maxVelocity = 50.0f, float maxRotation = 10.0f)
    {
        target.AngularVelocity = maxRotation * rng.NextSingle();
        target.LinearVelocity = RandomUnitVector(rng) * maxVelocity;
    }

    private static Vector2 RandomUnitVector(Random rng)
    {
        return new Vector2(1, 0)
            .Rotated(2 * Mathf.Pi * rng.NextSingle());
    }
}