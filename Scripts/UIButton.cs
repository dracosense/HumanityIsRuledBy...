using Godot;
using System;

public class UIButton : Button
{

    private Root root;
    private int num;

    public void _on_button_down()
    {
        root.uiNum = num;
    }

    public override void _Ready()
    {
        root = (Root)GetNode("/root/root");
        num = -1;
        if (this.Name[0] >= '0' && this.Name[0] <= '9')
        {
            num = this.Name[0] - '0';
        }
    }
}
