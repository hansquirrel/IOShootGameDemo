using System;


namespace ShootGameServer.SharedData
{
    public class ConstSettings
    {
        //地图的最大移动范围
        public const float MaxX = 1000;
        public const float MaxY = 1000;
        public const float MinX = -1000;
        public const float MinY = -1000;

        //子弹速度
        public const int BulletSpeed = 10;

        //碰撞检测半径
        public const int HitDetectRange = 45;

        //默认玩家名字
        public const string DefaultName = "Player";

        //模拟延迟等级
        public const int DelaySimulate = 0;

        //移动速度因子
        public const float MoveSpeed = 1.5f;

        //一局游戏的时间(单位：毫秒)
        public const int RoundTime = 60 * 1000;

        //默认服务器和端口
        //public const string DefaultServerEndPoint = "39.108.192.210:9003";
        public const string DefaultServerEndPoint = "127.0.0.1:9003";

        //是否关闭调试
        public const bool DisableConsolePanelInRuntime = false;

        //每0.02秒提交一次数据
        public const float DataSubmitTime = 0.02f;

        //最大信息条数
        public const int InfoMaxLines = 6;

        //默认血量
        public const int DefaultHp = 100;

        //踢掉不存活的客户端时间，单位：秒
        public const int DropIfNotAliveTime = 3;
    }
}

