using GridPowerTycoon.MonoGame.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GridPowerTycoon.MonoGame.Input;

public sealed class CameraInputController
{
    private readonly Camera2D _camera;
    private readonly InputManager _input;
    private readonly Func<Point, bool>? _isMouseOverUi;

    public CameraInputController(Camera2D camera, InputManager input, Func<Point, bool>? isMouseOverUi = null)
    {
        _camera = camera;
        _input = input;
        _isMouseOverUi = isMouseOverUi;
    }

    public void Update(GameTime gameTime)
    {
        HandleMouseZoom();
        HandleKeyboardZoom();
        HandleMousePan();
        HandleKeyboardPan(gameTime);
    }

    private void HandleMouseZoom()
    {
        var scrollDelta = _input.CurrentMouse.ScrollWheelValue - _input.PreviousMouse.ScrollWheelValue;
        if (scrollDelta == 0)
            return;

        var mousePoint = new Point(_input.CurrentMouse.X, _input.CurrentMouse.Y);
        if (_isMouseOverUi?.Invoke(mousePoint) == true)
            return;

        var mousePosition = mousePoint.ToVector2();
        _camera.ZoomAtScreenPoint(scrollDelta > 0 ? 1.1f : 1f / 1.1f, mousePosition);
    }

    private void HandleKeyboardZoom()
    {
        var zoomIn = _input.IsKeyPressed(Keys.OemPlus) || _input.IsKeyPressed(Keys.Add);
        var zoomOut = _input.IsKeyPressed(Keys.OemMinus) || _input.IsKeyPressed(Keys.Subtract);
        var screenCenter = new Vector2(640, 360);

        if (zoomIn)
            _camera.ZoomAtScreenPoint(1.1f, screenCenter);

        if (zoomOut)
            _camera.ZoomAtScreenPoint(1f / 1.1f, screenCenter);
    }

    private void HandleMousePan()
    {
        if (_input.CurrentMouse.MiddleButton != ButtonState.Pressed ||
            _input.PreviousMouse.MiddleButton != ButtonState.Pressed)
        {
            return;
        }

        var mousePoint = new Point(_input.CurrentMouse.X, _input.CurrentMouse.Y);
        if (_isMouseOverUi?.Invoke(mousePoint) == true)
            return;

        var current = new Vector2(_input.CurrentMouse.X, _input.CurrentMouse.Y);
        var previous = new Vector2(_input.PreviousMouse.X, _input.PreviousMouse.Y);
        var screenDelta = current - previous;
        _camera.Move(-screenDelta / _camera.Zoom);
    }

    private void HandleKeyboardPan(GameTime gameTime)
    {
        var keyboard = _input.CurrentKeyboard;
        var direction = Vector2.Zero;

        if (keyboard.IsKeyDown(Keys.W)) direction.Y -= 1;
        if (keyboard.IsKeyDown(Keys.S)) direction.Y += 1;
        if (keyboard.IsKeyDown(Keys.A)) direction.X -= 1;
        if (keyboard.IsKeyDown(Keys.D)) direction.X += 1;

        if (direction == Vector2.Zero)
            return;

        direction.Normalize();
        const float panSpeed = 600f;
        var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _camera.Move(direction * panSpeed * deltaSeconds / _camera.Zoom);
    }
}
