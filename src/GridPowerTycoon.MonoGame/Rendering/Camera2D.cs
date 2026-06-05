using Microsoft.Xna.Framework;

namespace GridPowerTycoon.MonoGame.Rendering;

public sealed class Camera2D
{
    public Vector2 Position { get; private set; } = Vector2.Zero;
    public float Zoom { get; private set; } = 1f;
    public float MinZoom { get; set; } = 0.4f;
    public float MaxZoom { get; set; } = 3f;

    public Matrix GetTransformMatrix()
    {
        return Matrix.CreateTranslation(new Vector3(-Position, 0f)) *
               Matrix.CreateScale(Zoom, Zoom, 1f);
    }

    public void Move(Vector2 delta)
    {
        Position += delta;
    }

    public void SetZoom(float zoom)
    {
        Zoom = MathHelper.Clamp(zoom, MinZoom, MaxZoom);
    }

    public void ZoomAtScreenPoint(float factor, Vector2 screenPoint)
    {
        var before = ScreenToWorld(screenPoint);
        SetZoom(Zoom * factor);
        var after = ScreenToWorld(screenPoint);
        Position += before - after;
    }

    public Vector2 ScreenToWorld(Vector2 screenPosition)
    {
        return Vector2.Transform(screenPosition, Matrix.Invert(GetTransformMatrix()));
    }
}
