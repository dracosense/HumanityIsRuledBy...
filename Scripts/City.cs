using Godot;
using System;
using System.Collections.Generic;
using static Lib;

public class City : StaticBody2D
{

    public float size;
    public float speed = 0.7f;
    public float dSpeed = 2; // destroy speed
    public float attackTimeout = 1.0f;
    [Remote]
    public float num = 0;
    public int level = 0;
    public int type = 0;
    [Remote]
    public int player = NEUTRAL;
    public uint aiHelp;
    public bool active;
    [Remote]
    public bool grow;

    protected Root root;
    protected AnimatedSprite sprite;
    protected TextureProgress bar;
    protected TextureRect icon;
    protected Label label;
    protected Label attackLabel;
    protected Label shieldLabel;
    protected Button levelUpButton;
    protected Control sIcons; // skills icons
    protected Light2D light;
    protected Area2D aRangeObj;
    protected Sprite wayTargetTip;
    protected City wayTarget;
    protected List<City> nearestCities;
    protected Color cityColor;
    protected float attack = 2;
    protected float shield = 3;
    protected float aRange;
    protected float timeFromLastAttack;
    protected float timeFromLastAIAttack;
    protected int upgradesNum;
    protected int firstPlayer;

    public float GetAttack()
    {
        return attack;
    } 

    public float GetShield()
    {
        return shield;
    }

    [Remote]
    public bool LevelUp()
    {
        float c = CITY_LEVEL_COST * ((int)(ADD_C_LEVEL_COST * level) + 1); // cost
        if (type == CITY_T && level < MAX_CITY_LEVEL && player >= 0 && player < MAX_PLAYERS_NUM && root.money[player] >= c)
        {
            level++;
            upgradesNum++;
            size += C_L_ADD_SIZE;
            speed += CITY_PRODUCE_LEVEL_ADD;
            aRange += CITY_A_ADD_L_RANGE;
            aRangeObj.Scale = aRange * new Vector2(1.0f, 1.0f);
            root.money[player] -= c;
            return true;
        }
        return false;
    }

    [Remote]
    public void AUpgrade()
    {
        if (upgradesNum > 0)
        {
            attack++;
            upgradesNum--;
        }
    }

    [Remote]
    public void SUpgrade()
    {
        if (upgradesNum > 0)
        {
            shield++;
            upgradesNum--;
        }
    }

    public void SyncParams() // fix
    {
        if (root.networkStatus == SERVER_ST)
        {
            RsetId(root.netEnemyId, "player", player);
            RsetId(root.netEnemyId, "num", num);
            RsetId(root.netEnemyId, "grow", grow);
        }
    }

    [Remote]
    public void SetWayRemoted(Vector2 pos)
    {
        wayTarget = root.FindTargetCity(pos);
    }

    public void SetWay(City target)
    {
        if (root.NetGameExist())
        {
            RpcId(root.netEnemyId, "SetWayRemoted", target.Position);
        }
        wayTarget = target;
    }

    [Remote]
    public void ClearWay()
    {
        if (root.NetGameExist())
        {
            RsetId(root.netEnemyId, "SetWayRemoted", null);
        }
        wayTarget = null;
    }

    public void _on_LevelUp_button_down()
    {
        if (player == PLAYER)
        {
            if (root.NetGameExist())
            {
                RpcId(root.netEnemyId, "LevelUp");
            }
            LevelUp();
        }
    }

    public void _on_Attack_button_down()
    {
        if (player == PLAYER)
        {
            if (root.NetGameExist())
            {
                RpcId(root.netEnemyId, "AUpgrade");
            }
            AUpgrade();
        }
    }

    public void _on_Shield_button_down()
    {
        if (player == PLAYER)
        {
            if (root.NetGameExist())
            {
                RpcId(root.netEnemyId, "SUpgrade");
            }
            SUpgrade();
        }
    }

    public void _on_ARange_body_entered(Node2D body)
    {
        try
        {
            City city = (City)body;
            nearestCities.Add(city); // ?? optimize ?? 
        }
        catch {}
    }

    [Remote]
    public void SetNetPlayer(int p)
    {
        player = p;
        if (root.NetGameExist())
        {
            Rset("player", p);
        }
        if (p != NEUTRAL)
        {
            if (type != FACTORY_T && type != LAB_T)
            {
                grow = true;
                if (root.NetGameExist())
                {
                    Rset("grow", true);
                }
            }
        }
        else
        {
            grow = false;
            if (root.NetGameExist())
            {
                Rset("grow", false);
            }
        }
    }

    [Remote]
    public void SetNetNum(float n)
    {
        num = n;
        if (root.NetGameExist())
        {
            Rset("num", n);
        }
    }
    
    [Remote]
    public bool Attack(City target, Vector2 pos, bool all = false, bool otherNetPlayer = false, bool sendedByAI = false)
    {
        if (root.NetGameExist())
        {
            if (!otherNetPlayer)
            {
                RpcId(root.netEnemyId, "Attack", target, target.Position, all, true, sendedByAI);
            }
            else
            {
                target = root.FindTargetCity(pos);
            }
        }
        if (target != wayTarget)
        {
            ClearWay();
        }
        if (target == null || ((int)(all ? (int)num : (int)num) / 2) <= 0 || (target.Position - this.Position).Length() > aRange ||
        ((player == PLAYER || player == NET_ENEMY) && target.player == FRIEND) ||
         (timeFromLastAttack < attackTimeout && (sendedByAI || target == wayTarget)))
        {
            return false;
        }
        try
        {
            Army obj = (Army)root.CreateObj(root.armyPS, this.Position + ARMY_DESTROY_RANGE * (target.Position - this.Position).Normalized());
            obj.target = target;
            obj.player = player;
            obj.num = attack * (all ? (int)num : (((int)num) / 2));
            obj.attack = attack;
            num -= (all ? (int)num : ((int)num) / 2);
            timeFromLastAttack = 0;
            if (sendedByAI)
            {
                timeFromLastAIAttack = 0;
            }
            return true;
        }
        catch
        {
            GD.Print("Create army error.");
            return false;
        }
    }

    public void AttackAI()
    {
        City target = null;
        City city= null;
        float targetK = 0;
        float cityK = 0;
        bool b = false;
        bool nearEnemy = false;
        aiHelp = INF;
        targetK = INF;
        for (int i = 0; i < nearestCities.Count; i++) // ?? optimize ??
        {
            city = nearestCities[i];
            if (city == this || (city.Position - this.Position).Length() > aRange)
            {
                continue;
            }
            if (city.attack == 0.0f)
            {
                cityK = 0;
            }
            else
            {
                cityK = city.num * city.shield / city.attack;
            }
            if (player == PLAYER)
            {
                b = (city.player != PLAYER && city.player != FRIEND && city.player != NEUTRAL);
            }
            else
            {
                if (player == NET_ENEMY)
                {
                    b = (city.player != NET_ENEMY && city.player != FRIEND && city.player != NEUTRAL);
                }
                else
                {
                    if (player == FRIEND)
                    {
                        b = (city.player != FRIEND && city.player != PLAYER &&
                         city.player != NET_ENEMY && city.player != NEUTRAL);
                    }
                    else
                    {
                        b = (city.player == PLAYER || city.player == NET_ENEMY || city.player == FRIEND);
                    }
                }
            }
            if (b)
            {
                if (city.num * city.shield < AI_ATTACK_P_CONST * num * attack) // ??
                {
                    target = city;
                    aiHelp = 0;
                    break;
                }
                nearEnemy = (player != PLAYER && player != NET_ENEMY) || nearEnemy;
            }
            if ((b && (city.type == FACTORY_T || city.type == LAB_T || num > size)) || city.player == NEUTRAL) // ?? types ??
            {
                if (target == null || target.player == player || (((city.type == FACTORY_T || city.type == LAB_T)?1:0) + ((target.type == CITY_T)?1:0) + ((cityK < targetK)?1:0) >= 2))
                {
                    target = city;
                    targetK = cityK;
                    aiHelp = 0;
                }
            }
            if (targetK == INF && city.player == player && city.aiHelp < aiHelp &&
            city.aiHelp < AI_MAX_HELP_CONST && player != PLAYER && player != NET_ENEMY)
            {
                target = city;
                targetK = INF;
                aiHelp = city.aiHelp + 1;
            }
        }
        if ((type == FACTORY_T || type == LAB_T) && num <  EMPTY_CITY_NUM_C * size)
        {
            aiHelp = 0;
            target = null;
        }
        if ((type == FACTORY_T || type == LAB_T) && num < 2 * EMPTY_CITY_NUM_C * size)
        {
            target = null;
        }
        if (nearEnemy && aiHelp > 0)
        {
            aiHelp = AI_NEAR_ENEMY_HELP_CONST;
            target = null;
        }
        if (target != null && ((player != PLAYER && PLAYER != NET_ENEMY) || timeFromLastAIAttack > PLAYER_AI_TIMEOUT))
        {
            Attack(target, target.Position, false, false, true);
        }
    }

    public override void _Ready()
    {
        string name = this.Name;
        root = (Root)GetNode("/root/root");
        sprite = (AnimatedSprite)GetNode("ASprite");
        bar = (TextureProgress)GetNode("Bar");
        icon = (TextureRect)GetNode("Icon");
        label = (Label)GetNode("Label");
        attackLabel = (Label)GetNode("SIcons/AttackLabel");
        shieldLabel = (Label)GetNode("SIcons/ShieldLabel");
        levelUpButton = (Button)GetNode("LevelUpButton");
        sIcons = (Control)GetNode("SIcons");
        light = (Light2D)GetNode("Light");
        aRangeObj = (Area2D)GetNode("ARange");
        wayTargetTip = (Sprite)GetNode("WayTargetTip");
        nearestCities = new List<City>();
        cityColor = Colors.White;
        wayTarget = null;
        active = false;
        grow = true;
        aiHelp = INF;
        GetPFromStr(ref name, 0, ref player);
        GetPFromStr(ref name, 1, ref level);
        GetPFromStr(ref name, 2, ref type);
        firstPlayer = player;
        if (player == PLAYER)
        {
            root.playerCitiesNum++;
        }
        if (player == FRIEND)
        {
            root.playerCitiesNum++;
            root.friendCitiesNum++;
        }
        upgradesNum = level;
        if (player == SHADOW_G)
        {
            level = MAX_CITY_LEVEL;
            type = SHADOW_T;
            upgradesNum = 0;
            speed /= 2;
            attack = 1;
            shield = 10;
        }
        size = C_BASE_SIZE + level * C_L_ADD_SIZE;
        aRange = CITY_A_RANGE + level * CITY_A_ADD_L_RANGE;
        aRangeObj.Scale = aRange * new Vector2(1.0f, 1.0f);
        switch (type)
        {
            case CITY_T:
                sprite.Animation = level.ToString();
                cityColor = Colors.LightBlue;
                break;
            case FACTORY_T:
                sprite.Animation = "factory";
                cityColor = Colors.White;
                level += 3;
                shield += 3;
                size = 1.5f * C_BASE_SIZE;
                grow = false;
                break;
            case LAB_T:
                sprite.Animation = "lab";
                cityColor = Colors.Purple;
                level += 3;
                shield += 3;
                size = 1.5f * C_BASE_SIZE;
                grow = false;
                break;
            case SHADOW_T:
                sprite.Animation = "shadow"; 
                cityColor = Colors.LightBlue;
                break;
            default:
                break;
        }
        if (player == NEUTRAL)
        {
            num = EMPTY_CITY_NUM_C * size;
            grow = false;
        }
        sprite.Modulate = cityColor;
    }

    public override void _Process(float delta)
    {
        Color c = new Color();
        /*if (root.networkStatus != LOCAL_ST) // ??
        {
            if (player == PLAYER)
            {
                this.SetNetworkMaster(GetTree().GetNetworkUniqueId());
            }
            if (player == NET_ENEMY)
            {
                this.SetNetworkMaster(root.netEnemyId);
            }
            
        }*/
        if (player != PLAYER)
        {
            active = false;
            ClearWay();
        }
        timeFromLastAttack += delta;
        timeFromLastAIAttack += delta;
        if (type == CITY_T)
        {
            sprite.Animation = level.ToString();
        }
        light.Enabled = ((root.activeLevelN == CURIOSITY_L || root.activeLevelN == CURIOSITY_NET_L) && (player == PLAYER));
        if (root.activeLevelN >= 0 && root.activeLevelN < LEVELS_NUM)
        {
            this.Visible = (firstPlayer != SHADOW_G || (root.playerCitiesNum >= LEVEL_CITIES_NUM[root.activeLevelN] - LEVEL_SHADOW_C_NUM[root.activeLevelN]));
        }
        if (active)
        {
            sprite.Modulate = Colors.Black;
        }
        else
        {
            sprite.Modulate = cityColor;
        }
        aRangeObj.Visible = active;
        if (type == SHADOW_T)
        {
            c = sprite.Modulate;
            c.a = SHADOW_C_A;
            sprite.Modulate = c;
        }
        if (player == PLAYER)
        {
            icon.Texture = root.person;
            icon.Modulate = Colors.Gray;
            c = Colors.Green;
            c.a = bar.Modulate.a;
            bar.Modulate = c;
        }
        else
        {
            switch  (player)
            {
                case NEUTRAL:
                    icon.Texture = root.neutralPerson;
                    icon.Modulate = Colors.Gray;
                    c = Colors.White;
                    c.a = bar.Modulate.a;
                    bar.Modulate = c;
                    break;
                case SHADOW_G:
                    icon.Texture = root.shadowPerson;
                    icon.Modulate = Colors.White;
                    c = Colors.LightSkyBlue;
                    c.a = bar.Modulate.a;
                    bar.Modulate = c; 
                    break;
                case FRIEND:
                    icon.Texture = root.friendPerson;
                    icon.Modulate = Colors.White;
                    c = Colors.Yellow;
                    c.a = bar.Modulate.a;
                    bar.Modulate = c;
                    break;
                default:
                    icon.Texture = root.evilPerson;
                    icon.Modulate = Colors.Gray;
                    c = Colors.Red;
                    c.a = bar.Modulate.a;
                    bar.Modulate = c;
                    break;
            }
        }
        if (num <= size)
        {
            if (grow)
            {
                num = Mathf.Min(num + ((player == PLAYER || player == NET_ENEMY/* || player == FRIEND*/)?1.0f:DIFFICULT_CONST[root.difficultN]) * delta * speed, size);
                label.SelfModulate = Colors.Green;
            }
            else
            {
                label.SelfModulate = Colors.LightSkyBlue;
            }
        }
        else
        {
            num = Mathf.Max(num - delta * dSpeed, size);
            label.SelfModulate = Colors.Red;
        }
        bar.Value = bar.MaxValue * ((size == 0) ? 1 : num / size);
        label.Text = ((int)num).ToString();
        attackLabel.Text = attack.ToString();
        shieldLabel.Text = shield.ToString();
        levelUpButton.Text = (level + 1).ToString();
        if (upgradesNum > 0)
        {
            sIcons.Modulate = Colors.Green;
        }
        else
        {
            sIcons.Modulate = Colors.White;
        }
        if (player >= 0 && player < MAX_PLAYERS_NUM)
        {
            switch (type)
            {
                case FACTORY_T:
                    root.money[player] += delta * PRODUCE_MONEY_SPEED;
                    break;
                case LAB_T:
                    root.techP[player] += delta * PRODUCE_TECH_P_SPEED;
                    break;
                default:
                    break;
            }
        }
        if (wayTarget != null)
        {
            wayTargetTip.Visible = true;
            wayTargetTip.GlobalRotation = (wayTarget.Position - this.Position).Angle() + Mathf.Pi / 2.0f;
            Attack(wayTarget, wayTarget.Position, true);
        }
        else
        {
            wayTargetTip.Visible = false;
            wayTargetTip.GlobalRotation = 0.0f;
        }
        if (((player != PLAYER && player != NET_ENEMY) || root.activeLevelN == AMBITIONS_L) &&
         player != NEUTRAL && (root.networkStatus == LOCAL_ST || root.networkStatus == SERVER_ST))
        {
            if (root.rand.Next() % AI_LEVEL_UP_CONST == 0)
            {
                LevelUp();
            }
            if (upgradesNum > 0)
            {
                if (root.rand.Next() % 2 == 0)
                {
                    AUpgrade();
                }
                else
                {
                    SUpgrade();
                }
            }
            AttackAI();
        }
    } 

}
