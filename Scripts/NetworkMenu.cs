using Godot;
using System;

public class NetworkMenu : VBoxContainer
{

    private Root root;
    private LineEdit connectIP;
    private Button connectButton;
    public Button createServerButton;

    public void _on_CreateServerButton_down()
    {
        connectButton.Disabled = true;
        createServerButton.Disabled = true;
        root.CreateHost();
    }

    public void _on_ConnectButton_down()
    {
        connectButton.Disabled = true;
        createServerButton.Disabled = true;
        root.Connect(connectIP.Text);
    }

    public override void _Ready()
    {
        root = (Root)GetNode("/root/root");
        connectIP = (LineEdit)GetNode("Connect/IP");
        connectButton = (Button)GetNode("Connect/ConnectButton");
        createServerButton = (Button)GetNode("CreateServerButton");

    }

    public override void _Process(float delta)
    {
        if (root.uiNum != 2)
        {
            connectButton.Disabled = false;
            createServerButton.Disabled = false;
        }
    }

}
