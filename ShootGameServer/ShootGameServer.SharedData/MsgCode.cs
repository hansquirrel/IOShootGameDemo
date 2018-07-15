using System;
using ProtoBuf;

namespace ShootGameServer.SharedData
{

    //返回值
    public static class GameServerReturnCode
    {
        public const int OK = 0;
        public const int DUPLICATE_USERNAME = 1000;
        public const int USER_NOT_EXIST = 1001;
        public const int CAN_NOT_FIRE_WHILE_NOT_MOVING = 1002;
        public const int PLAYER_POSITION_HIT = 1003; //玩家碰撞
    }

    //指令
    public static class MsgCode
    {
        public const int USER_JOIN = 1;
        public const int USER_STATE_UPDATE = 2;
        public const int USER_QUIT = 3;
        public const int MOVE_AND_FIRE = 4;
        public const int HIT = 5;
        public const int DIE = 6;
        public const int SYNC = 7;
        public const int SYNC_TIME = 8;
        public const int NEW_ROUND = 9; //重新开始游戏

        public const int CLIENT_JOIN = 100;
        public const int CLIENT_MOVE = 101;
        public const int CLIENT_QUIT = 102;
    }
}

