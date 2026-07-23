using Godot;

interface IMouseTool
{
    void OnClick(Vector2 mousePosition);
    void OnRocketPartEvent(RocketComponent component, InputEventMouseButton mouseEvent);
    void OnRelease();
    void OnCancel();
}