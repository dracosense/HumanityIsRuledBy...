using Godot;
using System;
using static Lib;

public class Game : Node2D
{

    private Root root;
    private Camera2D camera;
    private Camera2D menuCamera;
    private Node2D activeLevel;
    private Node2D levelLayer;
    private PackedScene[] levels;
    private int activeLevelN;

    public bool SetLevel(uint n)
    {
        switch(root.networkStatus)
        {
            case LOCAL_ST:
                PLAYER = 0;
                NET_ENEMY = -2;
                break;
            case SERVER_ST:
                PLAYER = 0;
                NET_ENEMY = 2; // 
                break;
            case CLIENT_ST:
                PLAYER = 2; // 
                NET_ENEMY = 0;
                break;
            default:
                break;
        }
        if (n >= LEVELS_NUM)
        {
            return false;
        }
        Node2D obj;
        RemoveLevel();
        try
        {
            obj = (Node2D)levels[n].Instance();
            levelLayer.AddChild(obj);
            activeLevel = obj;
            activeLevelN = (int)n;
            return true;
        }
        catch 
        {
            GD.Print("Create level error.");
        }
        return false;
    }

    public void RemoveLevel()
    {
        if (activeLevel == null)
        {
            return;
        }
        try
        {
            for (int i = 0; i < MAX_PLAYERS_NUM; i++)
            {
                root.money[i] = 0;
                root.techP[i] = 0;
            }
            activeLevel.QueueFree();
            activeLevel = null;
            //levelLayer.RemoveChild(activeLevel);
        }
        catch
        {
            GD.Print("Remove level error.");
        }
    }

    public override void _Ready()
    {
        root = (Root)GetNode("/root/root");
        camera = (Camera2D)GetNode("Camera");
        menuCamera = (Camera2D)GetNode("MenuCamera");
        levelLayer = (Node2D)GetNode("LevelLayer");
        levels = new PackedScene[LEVELS_NUM];
        for (int i = 0; i < LEVELS_NUM; i++)
        {
            try
            {
                levels[i] = (PackedScene)ResourceLoader.Load("res://Scenes/level" + i.ToString() + ".tscn");
            }
            catch
            {
                GD.Print("Levels load error.");
            }
        }
    }

    public override void _Process(float delta)
    {
        if (activeLevel != null && root.activeLevelN >= 0 && root.activeLevelN < LEVELS_NUM)
        {
            if (root.playerCitiesNum >= LEVEL_CITIES_NUM[activeLevelN])
            {
                root.networkStatus = LOCAL_ST;
                root.lastOpenedLevel = (uint)Mathf.Max(activeLevelN - NET_MAPS_NUM + 1, (int)root.lastOpenedLevel);
                root.playerCitiesNum = 0;
                root.friendCitiesNum = 0;
                RemoveLevel();
                root.activeLevelN = -1;
                activeLevelN = -1;
                root.uiNum = 3;
                root.SaveGame();
            }
            else
            {
                if (root.playerCitiesNum <= 0 || (root.friendCitiesNum <= 0 && root.activeLevelN == GOODNESS_L))
                {
                    root.activeLevelN = -1;
                    root.uiNum = 1;
                }
            }
        }
        if (activeLevelN != root.activeLevelN)
        {
            if (root.activeLevelN < 0)
            {
                RemoveLevel();
                activeLevelN = root.activeLevelN;
            }
            else
            {
                root.playerCitiesNum = 0;
                if (SetLevel((uint)root.activeLevelN))
                {
                    root.uiNum = -1;
                }
            }
        }
        if (activeLevel == null)
        {
            camera.Position = Vector2.Zero;
            camera.Current = false;
            menuCamera.Current = true;
        }
        else
        {
            camera.Current = true;
            menuCamera.Current = false;
        }
    }

}
