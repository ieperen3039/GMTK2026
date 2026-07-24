using Godot;

class Util
{
    // mod rotation to (-180, +180) degrees
    public static float RotationRelativeToUp(float angle)
    {
        return Mathf.PosMod(angle + Mathf.Pi, 2 * Mathf.Pi) - Mathf.Pi;
    }
}