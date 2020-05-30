using Godot;
using System;
using static Lib;

public class GameCamera : Camera2D
{

    public static readonly Vector2 minPos = new Vector2(-1600.0f,  -900.0f);
    public  static readonly Vector2 maxPos = new Vector2(1600.0f, 900.0f);
    public  static readonly Vector2 nKeysPos = new Vector2(1600.0f, 900.0f);
    public static readonly Vector2 speed = 1200.0f * new Vector2(1.0f, 1.0f);

    public override void _Ready()
    {
        
    }

    public override void _PhysicsProcess(float delta)
    {
        this.Position += (Input.IsActionPressed("c_fast_move")?FAST_CAMERA_CONST:1.0f) * delta * new Vector2(speed.x * (Input.GetActionStrength("camera_right") - Input.GetActionStrength("camera_left")),
         speed.y * (Input.GetActionStrength("camera_down") - Input.GetActionStrength("camera_up")));
        this.Position = new Vector2(Mathf.Max(Mathf.Min(this.Position.x, maxPos.x), minPos.x), Mathf.Max(Mathf.Min(this.Position.y, maxPos.y), minPos.y)); 
        for (int i = 0; i < NUMBERS_N; i++)
        {
            if (Input.IsActionJustPressed((i + 1).ToString()))
            {
                this.Position = new Vector2(((i % 3) - 1) * nKeysPos.x, ((i / 3) - 1) * nKeysPos.y);
            }
        }   
    }

}
