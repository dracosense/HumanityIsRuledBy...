using Godot;
using System;
using static Lib;

public class Army : KinematicBody2D
{

    public City target;
    public float speed = 240.0f;
    public int player = NEUTRAL; 
    public float num = 0;
    public float attack = 1;

    private Root root;
    private AnimatedSprite sprite;
    private Label label;
    private float destroyRange = ARMY_DESTROY_RANGE;
    private int levelN;

    public void Damage()
    {
        if (attack == 0.0f)
        {
            QueueFree();
        }
        if (target.player == player)
        {
            target.num += num / attack;
        }
        else
        {
            target.num -= num / ((target.GetShield() == 0.0f)?1.0f:target.GetShield());
            if (target.num < 0.0f)
            {
                if (target.player == PLAYER)
                {
                    root.playerCitiesNum--;
                }
                if (target.player == FRIEND)
                {
                    root.playerCitiesNum--;
                    root.friendCitiesNum--;
                }
                target.num = Mathf.Abs(target.num);
                target.num *= ((target.GetShield() == 0.0f)?1.0f:target.GetShield());
                target.num /= attack;
                if ((levelN != RANDOM_L && levelN != RANDOM_NET_L) || root.rand.Next() % RAND_CH_CONTROL_CONST != 0)
                {
                    target.player = player;
                }
                if (target.player == PLAYER)
                {
                    root.playerCitiesNum++;
                }
                if (target.player == FRIEND)
                {
                    root.playerCitiesNum++;
                    root.friendCitiesNum++;
                }
                if (target.player != NEUTRAL && target.type != FACTORY_T && target.type != LAB_T)
                {
                    target.grow = true;
                }
            }
        }
    }

    public override void _Ready()
    {
        root = (Root)GetNode("/root/root");
        sprite = (AnimatedSprite)GetNode("ASprite");
        label = (Label)GetNode("Label");
        levelN = root.activeLevelN;
        if (levelN < 0 || levelN >= LEVELS_NUM)
        {
            QueueFree();
            return;
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        Vector2 toTarget = Vector2.Zero;
        if (levelN != root.activeLevelN)
        {
            QueueFree();
            return;
        }
        this.Visible =  (player != SHADOW_G || root.playerCitiesNum >= LEVEL_CITIES_NUM[levelN] - LEVEL_SHADOW_C_NUM[levelN]);
        if (player == PLAYER)
        {
            sprite.Animation = "person";
        }
        else
        {
            switch (player)
            {
                case NEUTRAL:
                    sprite.Animation = "neutral_person";
                    break;
                case SHADOW_G:
                    sprite.Animation = "shadow_person";
                    break;
                case FRIEND:
                    sprite.Animation = "friend_person";
                    break;
                default:
                    sprite.Animation = "evil_person";
                    break;
            }
        }
        label.Text = num.ToString();
        if (target != null)
        {
            toTarget = target.GlobalPosition - this.GlobalPosition;
            if (toTarget.Length() < delta * speed + destroyRange)
            {
                Damage();
                QueueFree();
            }
            else
            {
                if (toTarget != Vector2.Zero)
                {
                    MoveAndSlide((speed / toTarget.Length()) * toTarget);
                }
            }
        }
    }

}
