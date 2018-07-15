/********************************************************************************

** Copyright(c) 2018 汉家松鼠工作室 All Rights Reserved. 

** auth： cg
** date： 2018/6/28 11:39:28 
** desc： 尚未编写描述 

*********************************************************************************/

using System;
using ShootGameServer.Network;
using ShootGameServer.SharedData;

namespace ShootGameServer
{
    public class GamePlayerSession
    {
        public GamePlayerSession(INetworkClientSession c, GameServer s)
        {
            server = s;
            client = c;
            client.MessageHandler += OnMessage;
        }

        public DateTime lastActiveTime = DateTime.Now;

        public string id = "";
        public GamePlayer user;

        public GameServer server;
        public INetworkClientSession client;

        
        //关闭跟此玩家的链接
        public void Close()
        {
            client.Close();
        }

        void OnMessage(GameMessage msg)
        {
            if (msg == null) //掉线了
            {
                server.PlayerDropped(id);
                return;
            }

            //第一个包必须是join包
            if (string.IsNullOrEmpty(id))
            {
                if (!(msg is ClientJoinMessage))
                    throw new Exception("unexpected join package!");

                var joinMsg = (ClientJoinMessage)(msg);

                id = joinMsg.id;
                server.NewPlayer(id, this);
            }
            else //其他包，直接处理
            {
                lastActiveTime = DateTime.Now;
                //进入消息队列
                server.PlayerMessageQueue.Enqueue(new ClientMessageCarrior()
                {
                    id = id,
                    message = msg
                }); 
            }
        }
    }
}
