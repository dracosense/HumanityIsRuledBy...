using Godot;
using System;
using static Lib;

public class MouseKeyControl : Control
{

    public void _on_mouse_entered()
    {
        Input.ActionPress(this.Name);
    }

    public void _on_mouse_exited()
    {
        Input.ActionRelease(this.Name);
    }
    
}
