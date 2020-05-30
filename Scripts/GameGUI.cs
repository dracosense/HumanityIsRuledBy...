using Godot;
using System;
using static Lib;

public class GameGUI : Control
{

    private Root root;
    private Label moneyLabel;
    private Label techPLabel; 
    private TextureRect techPIcon;
    private Color techPColor;

    public void _on_TechP_button_down()
    {
        Input.ActionPress("use_tech_effect");
    }

    public void _on_TechP_button_up()
    {
        Input.ActionRelease("use_tech_effect");
    }

    public override void _Ready()
    {
        root = (Root)GetNode("/root/root");
        moneyLabel = (Label)GetNode("MoneyLabel");
        techPLabel = (Label)GetNode("TechP/HBoxC/TechPLabel");
        techPIcon = (TextureRect)GetNode("TechP/HBoxC/TechPIcon");
        techPColor = techPIcon.Modulate;
    }

    public override void _Process(float delta)
    {
        this.Visible = (root.uiNum == -1);
        if (PLAYER >= 0 && PLAYER < MAX_PLAYERS_NUM)
        {
            moneyLabel.Text = ((int)root.money[PLAYER]).ToString();
            techPLabel.Text = ((int)root.techP[PLAYER]).ToString();
        }
        techPIcon.Modulate = root.useTechEffect?Colors.Black:techPColor;
    }

}
