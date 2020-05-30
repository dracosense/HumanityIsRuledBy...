using Godot;
using System;
using static Lib;

public class LevelButton : Control
{

    private Root root;
    private Button button;
    private int num;

    public void _on_button_down()
    {
        root.activeLevelN = num + NET_MAPS_NUM;
    }

    public override void _Ready()
    {
        root = (Root)GetNode("/root/root");
        button = (Button)GetNode("Button");
        num = -1;
        if (this.Name[0] >= '0' && this.Name[0] <= '9')
        {
            num = this.Name[0] - '0';
        }
    }

    public override void _Process(float delta)
    {
        if(num <= root.lastOpenedLevel)
        {
            button.Disabled = false;
            Modulate = Colors.White;
        }
        else
        {
            button.Disabled = true;
            Modulate = Colors.Black;
        }
    }

}
