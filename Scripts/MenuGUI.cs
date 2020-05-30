using Godot;
using System;

public class MenuGUI : Control
{

    private Root root;
    private Control startScreen;
    private Control levelsScreen;
    private Control networkScreen;
    private Control winScreen;

    public override void _Ready()
    {
        root = (Root)GetNode("/root/root");
        startScreen = (Control)GetNode("StartScreen");
        levelsScreen = (Control)GetNode("LevelsScreen");
        networkScreen = (Control)GetNode("NetworkScreen");
        winScreen = (Control)GetNode("WinScreen");
    }

    public override void _Process(float delta)
    {
        this.Visible = (root.uiNum != -1); 
        startScreen.Visible = (root.uiNum == 0);
        levelsScreen.Visible = (root.uiNum == 1);
        networkScreen.Visible = (root.uiNum == 2);
        winScreen.Visible = (root.uiNum == 3);
    }

}
