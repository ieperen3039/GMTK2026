using Godot;

interface IMouseTool
{
    void OnClick(Vector2 mousePosition);
    void OnRelease(Vector2 mousePosition);
    void OnCancel();
}