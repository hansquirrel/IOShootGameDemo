using System;
using ProtoBuf;
using ShootGameServer.SharedData;

namespace ShootGameServer
{
    public interface IGameServer
    {
        //新玩家, msgHandler待集成的时候适配
        int NewPlayer(string id, Action<GameMessage> msgHandler);

        //移动
        int OnMoveTowards (string id, float dx, float dy);

        //退出
        int Quit(string id);

        //开枪
        //int Fire(string id);

        //获得子弹数量
        int GetBulletsCount ();

        //获得人物数量
        int GetPlayersCount();
    }     
}

