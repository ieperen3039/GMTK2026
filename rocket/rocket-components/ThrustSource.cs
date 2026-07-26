using Godot;
using System;

[Tool]
public partial class ThrustSource : Node2D
{
    [Export]
    public float ThrustPower {get; private set; }
    [Export]
    public bool IsPassive {get; private set; }

    public float ThrustFactor = 0.0f;

    public bool EnableThrust => ThrustFactor > 0.0f;

    public void SetActivationThrustFactor() => ThrustFactor = (IsPassive ? 0 : 1);

    public Vector2 GetThrust() => GetThrustAt(ThrustFactor);

    // returns (Global thrust vector, offset from parent in global space)
    public Vector2 GetThrustAt(float fractionOfFull)
    {
        return GetLocalThrustAt(fractionOfFull).Rotated(GlobalRotation);
    }


    public Vector2 GetLocalThrust() => GetLocalThrustAt(ThrustFactor);

    public virtual Vector2 GetLocalThrustAt(float fractionOfFull) => Vector2.Up * ThrustPower * fractionOfFull;

    public override void _Draw()
    {
        base._Draw();
        if (Engine.IsEditorHint())
        {
            DrawLine(Vector2.Zero, GetLocalThrustAt(1.0f) * -0.1f, new Color(1, 0, 0, 0.5f), 2.0f);
            DrawLine(Vector2.Zero, new Vector2(5, 5), new Color(1, 0, 0, 0.5f), 2.0f);
            DrawLine(Vector2.Zero, new Vector2(-5, 5), new Color(1, 0, 0, 0.5f), 2.0f);
        }
    }
}
