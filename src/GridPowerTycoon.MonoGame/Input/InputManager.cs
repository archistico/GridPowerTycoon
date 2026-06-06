using Microsoft.Xna.Framework.Input;

namespace GridPowerTycoon.MonoGame.Input;

public sealed class InputManager
{
    public MouseState CurrentMouse { get; private set; }
    public MouseState PreviousMouse { get; private set; }
    public KeyboardState CurrentKeyboard { get; private set; }
    public KeyboardState PreviousKeyboard { get; private set; }

    public void Update()
    {
        PreviousMouse = CurrentMouse;
        PreviousKeyboard = CurrentKeyboard;
        CurrentMouse = Mouse.GetState();
        CurrentKeyboard = Keyboard.GetState();
    }

    public bool IsKeyPressed(Keys key)
    {
        return CurrentKeyboard.IsKeyDown(key) && PreviousKeyboard.IsKeyUp(key);
    }

    public bool IsLeftClickPressed()
    {
        return CurrentMouse.LeftButton == ButtonState.Pressed &&
               PreviousMouse.LeftButton == ButtonState.Released;
    }

    public bool IsRightClickPressed()
    {
        return CurrentMouse.RightButton == ButtonState.Pressed &&
               PreviousMouse.RightButton == ButtonState.Released;
    }
}

