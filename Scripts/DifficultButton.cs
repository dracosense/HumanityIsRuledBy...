using Godot;
using System;

public class DifficultButton : Button
{

    private Root root;

    public void _on_button_down()
    {
        Input.ActionPress("action");
    }

    public void _on_button_up()
    {
        Input.ActionRelease("action");
    }

    public override  void _Ready()
    {
        root = (Root)GetNode("/root/root");
    }

    public override void _Process(float delta)
    {
        switch(root.difficultN)
        {
            case 0:
                this.Text = "Easy";
                break;
            case 1:
                this.Text = "Normal";
                break;
            case 2:
                this.Text = "Hard"; 
                break;
            case 3:
                this.Text = "Impossible";
                break;
            default:
                this.Text = "";
                break;
        }
    }

}
