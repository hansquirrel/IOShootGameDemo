using System;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.IO;
using ShootGameServer.SharedData;
using ShootGameServer.Network;

namespace ShootGameServer
{
    public class DemoTcpClient : IGameServer
    {
        Action<GameMessage> _msgHander;

        INetworkClient client;

        void Connect(string endpoint){
            client = NetworkFactory.CreateNetworkClient<TcpNetworkClient> ();
            //client = NetworkFactory.CreateNetworkClient<RoomClientBridge> ();
            client.MessageHandler += (msg) => {
                RecieveMessage (msg);
            };
			client.Connect (endpoint);
        }

        void RecieveMessage(GameMessage msg){
            try{
                if (_msgHander != null) {
                    _msgHander (msg);
                }
            }catch(Exception e){
                Debug.LogError (e.ToString ());
            }
        }

        void Send(GameMessage msg){
            if(client.Connected){
                client.Send (msg);
            }
        }


        //新玩家, msgHandler待集成的时候适配
        public int NewPlayer(string id, Action<GameMessage> msgHandler){
            _msgHander = msgHandler;
            Connect (ShootGameManager.ServerEndpoint);
            Send (new ClientJoinMessage() {
                id = id
            });
            return 0;
        }

        //移动
        public int OnMoveTowards (string id, float dx, float dy){
            Send (new ClientMoveMessage(){
                dx = dx,
                dy = dy
            });  
            return 0;
        }

        //退出
        public int Quit(string id){
            Send (new GameMessage (){ 
                stateCode = MsgCode.CLIENT_QUIT
            });
            client.Close();
            return 0;
        }

        //开枪
//        public int Fire(string id){
//            Send ("2");
//            return 0;
//        }

        //获得子弹数量
        public int GetBulletsCount (){
            return 0;
        }

        //获得人物数量
        public int GetPlayersCount(){
            return 0;
        }
    }
}

