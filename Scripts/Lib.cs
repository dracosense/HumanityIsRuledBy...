using Godot;
using System;

public static class Lib
{

    public static readonly string GAME_SAVE_FILE_NAME = "progress_save.save";
    public const float CITY_LEVEL_COST = 1.0f; 
    public const float ADD_C_LEVEL_COST = 0.5f;
    public const float ARMY_DESTROY_RANGE = 120;
    public const float CITY_PRODUCE_LEVEL_ADD = 0.3f;
    public const float C_BASE_SIZE = 40.0f;
    public const float C_L_ADD_SIZE = 20.0f; 
    public const float SHADOW_C_A = 0.5f;
    public const float PRODUCE_MONEY_SPEED = 0.04f;
    public const float PRODUCE_TECH_P_SPEED = 0.04f;
    public const float CITY_A_RANGE = 800;
    public const float CITY_A_ADD_L_RANGE = 40;
    public const float EMPTY_CITY_NUM_C = 0.3f;
    public const float AI_ATTACK_P_CONST = 0.85f;
    public const float TECH_MULT_CONST = 1.5f;
    public const float TECH_DIV_CONST = 2.0f;
    public const float PLAYER_AI_TIMEOUT = 3.0f;
    public const float FAST_CAMERA_CONST = 2.5f;
    public static readonly float[] DIFFICULT_CONST = {0.8f, 1.0f, 1.2f, 1.4f};
    public const int NEUTRAL = -1;
    public const int SHADOW_G = 1; // 
    public const int FRIEND = 3;
    public const int LEVELS_NUM = 9;
    public const int MAX_CITY_LEVEL = 5;
    public const int MONEY_L = 3;
    public const int CURIOSITY_L = 4;
    public const int GOODNESS_L = 5;
    public const int AMBITIONS_L = 6;
    public const int RANDOM_L = 7;
    public const int SHADOW_L = 8;
    public const int CITY_T = 0;
    public const int FACTORY_T = 1;
    public const int LAB_T = 2;
    public const int SHADOW_T = 3;
    public const int MAX_PLAYERS_NUM = 4;
    public const int INF = (int)1e9;
    public const int AI_LEVEL_UP_CONST = 7;
    public const int RAND_CH_CONTROL_CONST = 3;
    public const int DIFFICULTS_NUM = 4;
    public const int NET_PORT = 3927;
    public const int NET_MAX_CLIENTS_NUM = 2;
    public const int LOCAL_ST = -1;
    public const int SERVER_ST = 0;
    public const int CLIENT_ST = 1;
    public const int AI_MAX_HELP_CONST = 6;
    public const int AI_NEAR_ENEMY_HELP_CONST = 2;
    public const int NET_MAPS_NUM = 3;
    public const int CURIOSITY_NET_L = 1;
    public const int RANDOM_NET_L = 2;
    public const int NUMBERS_N = 9;
    public static int PLAYER = 0;
    public static int NET_ENEMY = -2;
    public static readonly int[] LEVEL_CITIES_NUM = {18, 14, 18, 16, 12, 22, 18, 16, 21};
    public static readonly int[] LEVEL_SHADOW_C_NUM = {0, 0, 0, 0, 0, 0, 0, 0, 2};

    public static void GetPFromStr(ref string s, int i, ref int p)
    {
        if (i >= 0 && i < s.Length)
        {
            if (s[i] >= '0' && s[i] <= '9')
            {
                p = s[i] - '0';
            }
        }
    }

}
