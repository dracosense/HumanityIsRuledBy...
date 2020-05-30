using Godot;
using System;
using System.Collections.Generic;
using static Lib;

public class Root : Node2D
{

    public Node2D objLayer;
    public PackedScene armyPS = (PackedScene)ResourceLoader.Load("res://Scenes/army.tscn");
    public readonly Texture person = (Texture)ResourceLoader.Load("res://Sprites/Icons/person.png");
    public readonly Texture evilPerson = (Texture)ResourceLoader.Load("res://Sprites/Icons/person_evil.png");
    public readonly Texture neutralPerson = (Texture)ResourceLoader.Load("res://Sprites/Icons/person_neutral.png");
    public readonly Texture shadowPerson = (Texture)ResourceLoader.Load("res://Sprites/Icons/person_shadow.png");
    public readonly Texture friendPerson = (Texture)ResourceLoader.Load("res://Sprites/Icons/person_friend.png");
    public Random rand;
    public float[] money;
    [Remote]
    public float[] techP;
    public int uiNum;
    public int playerCitiesNum;
    public int friendCitiesNum;
    public int difficultN;
    [Remote]
    public int activeLevelN;
    public int networkStatus; // network
    public int netEnemyId; // network
    public uint attackTimer = 500;
    public uint lastAttackTime;
    public uint time;
    public uint lastOpenedLevel;
    public bool useTechEffect;

    private List<City> activeCities;
    private int lastNetStatus; // network

    // network
    public void CreateHost()
    {
        NetworkedMultiplayerENet host = new NetworkedMultiplayerENet();
        Error error = host.CreateServer(NET_PORT, NET_MAX_CLIENTS_NUM);
        if (error != Error.Ok)
        {
            GD.Print("Error creating server.");
            return;
        }
        GetTree().NetworkPeer = host;
        lastNetStatus = SERVER_ST;
    }

    public void Connect(string ip)
    {
        NetworkedMultiplayerENet client = new NetworkedMultiplayerENet();
        Error error = client.CreateClient(ip, NET_PORT);
        if (error != Error.Ok)
        {
            GD.Print("Error creationg client.");
            return;
        }
        GetTree().NetworkPeer = client;
        lastNetStatus = CLIENT_ST;
    }

    public void PlayerConnected(int id)
    {
        int x = 0;
        GD.Print("Connected (local network).");
        networkStatus = lastNetStatus;
        netEnemyId = id;
        if (NET_MAPS_NUM > 0 && networkStatus == SERVER_ST)
        {   
            x = rand.Next() % NET_MAPS_NUM;
            activeLevelN = x;
            Rset("activeLevelN", x);
        }
    }

    /*public void PlayerDisconnected(int id)
    {
        activeLevelN = -1;
    }*/

    public bool NetGameExist()
    {
        return (networkStatus != LOCAL_ST && GetTree().NetworkPeer.GetConnectionStatus() == NetworkedMultiplayerPeer.ConnectionStatus.Connected);
    }

    [Remote]
    public void RemoteChangeTechP(int player, int add)
    {
        techP[player] += add;
    } 
    // network 

    public Node2D CreateObj(PackedScene objPS, Vector2 pos)
    {
        Node2D obj = (Node2D)objPS.Instance();
        objLayer.AddChild(obj);
        obj.GlobalPosition = pos; 
        return obj;
    }

    public void ClearACities(bool clearWays = false)
    {
        for (int i = 0; i < activeCities.Count; i++)
        {
            if (activeCities[i].active && clearWays)
            {
                activeCities[i].ClearWay();
            }
            activeCities[i].active = false;
        }
        activeCities.Clear();
    }

    public void SaveGame()
    {
        File file = new File();
        file.Open(GAME_SAVE_FILE_NAME, File.ModeFlags.Write);
        file.Store32(lastOpenedLevel);
        file.Close();
    }

    public void LoadGame()
    {
        File file =  new File();
        if (!file.FileExists(GAME_SAVE_FILE_NAME))
        {
            return;
        }
        file.Open(GAME_SAVE_FILE_NAME, File.ModeFlags.Read);
        try
        {
            lastOpenedLevel = file.Get32();
        }
        catch
        {
            GD.Print("Load game save error.");
        }
        file.Close();
    }

    public City FindTargetCity(Vector2 pos)
    {
        try
        {
            return (City)(((Godot.Collections.Dictionary)(GetWorld2d().DirectSpaceState.IntersectPoint(pos, 1, null, 2)[0]))["collider"]);
        }
        catch
        {
            return null;
        }
    }

    public override void _Ready()
    {
        objLayer = (Node2D)GetNode("/root/Game/ObjLayer");  
        rand = new Random();
        activeCities = new List<City>();
        money = new float[MAX_PLAYERS_NUM];
        techP = new float[MAX_PLAYERS_NUM];
        lastAttackTime = 0;
        time = 0;
        uiNum = 0;
        playerCitiesNum = 0;
        friendCitiesNum = 0;
        lastOpenedLevel = 0;
        activeLevelN = -1;
        difficultN = 1;
        useTechEffect = false;
        LoadGame();
        // network
        networkStatus = LOCAL_ST;
        lastNetStatus = LOCAL_ST;
        GetTree().Connect("network_peer_connected", this, "PlayerConnected");
        //GetTree().Connect("network_peer_disconnected", this, "PlayerDisconnected");
        // network
    }

    public override void _PhysicsProcess(float delta)
    {
        Vector2 mousePos = Vector2.Zero;
        bool b = false;
        bool setWay = false;
        setWay = Input.IsActionPressed("set_way");
        time = OS.GetTicksMsec();
        mousePos = GetGlobalMousePosition(); 
        if (Input.IsActionJustPressed("exit"))
        {
            if (activeLevelN >= 0 && activeLevelN < LEVELS_NUM)
            {
                activeLevelN = -1;
                uiNum = 1;
            }
            else
            {
                switch(uiNum)
                {
                    case 1:
                        uiNum = 0;
                        break;
                    case 2:
                        if (GetTree().NetworkPeer != null && GetTree().NetworkPeer.GetConnectionStatus() != NetworkedMultiplayerPeer.ConnectionStatus.Disconnected)
                        {
                            activeLevelN = -1;
                            if (GetTree().NetworkPeer.GetConnectionStatus() == NetworkedMultiplayerPeer.ConnectionStatus.Disconnected)
                            {
                                Rset("activeLevelN", -1);
                            }
                            ((NetworkedMultiplayerENet)GetTree().NetworkPeer).CloseConnection();
                        }
                        networkStatus = LOCAL_ST;
                        uiNum = 1;
                        break;
                    case 3:
                        uiNum = 1;
                        break;
                    default:
                        GetTree().Quit();
                        break;
                }
            }
        }
        if (activeLevelN >= 0 && activeLevelN < LEVELS_NUM)
        {
            int mouseClick = 0;
            if (Input.IsActionJustPressed("use_tech_effect") && techP[PLAYER] > 0)
            {
                useTechEffect = !useTechEffect;
            }
            if (Input.IsActionJustPressed("mouse_left_click"))
            {
                mouseClick = 1;
            }
            if (Input.IsActionJustPressed("mouse_right_click"))
            {
                mouseClick = 2;
            }
            if (mouseClick == 1 || mouseClick == 2)
            { 
                City target = FindTargetCity(mousePos);
                if (target != null)
                {
                    try
                    {
                        if (!target.Visible)
                        {
                            ClearACities(setWay);
                        }
                        else
                        {
                            switch (mouseClick)
                            {
                                case 1:
                                    if (useTechEffect)
                                    {
                                        if (techP[PLAYER] > 0)
                                        {
                                            if (target.player == PLAYER || target.player == FRIEND)
                                            {
                                                target.SetNetNum(target.num * TECH_MULT_CONST);
                                            }
                                            else
                                            {
                                                switch(target.player)
                                                {
                                                    case NEUTRAL:
                                                        target.SetNetPlayer(PLAYER);
                                                        playerCitiesNum++;
                                                        break;
                                                    default:
                                                        target.SetNetNum(target.num / (TECH_DIV_CONST == 0.0f?1.0f:TECH_DIV_CONST));
                                                        break;
                                                }
                                            }
                                            RemoteChangeTechP(PLAYER, -1);
                                            if (NetGameExist())
                                            {
                                                Rpc("NetChangeTechP", PLAYER, -1);
                                            }
                                        }
                                        useTechEffect = false;
                                        break;
                                    }
                                    if (target.player == PLAYER)
                                    {
                                        if (Input.IsActionPressed("action"))
                                        {
                                            activeCities.Add(target);
                                            target.active = true;
                                        }
                                        else
                                        {
                                            ClearACities();
                                            activeCities.Add(target);
                                            target.active = true;
                                        }
                                        break;
                                    }
                                    goto gm;
                                case 2:
                                    gm:;
                                    if (useTechEffect)
                                    {
                                        useTechEffect = false;
                                        break;
                                    }
                                    b = (Input.IsActionPressed("action") || setWay);
                                    if (lastAttackTime + attackTimer <= time)
                                    {
                                        for (int i = 0; i < activeCities.Count; i++)
                                        {
                                            if (activeCities[i] == target || activeCities[i].active == false) // ?? optimize ?? 
                                            {
                                                continue;
                                            }
                                            if (activeCities[i].Attack(target, target.Position, b) && setWay) // ?? optimize net ??
                                            {
                                                activeCities[i].SetWay(target);
                                            }
                                        }
                                        lastAttackTime = time;
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    catch {}
                }
                else
                {
                    ClearACities(setWay);
                }
            }
        }
        else
        {
            if (Input.IsActionJustPressed("action"))
            {
                difficultN++;
                difficultN %= DIFFICULTS_NUM;
            }
        }
    }

}
