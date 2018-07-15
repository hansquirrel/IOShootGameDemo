/********************************************************************************

** Copyright(c) 2018 汉家松鼠工作室 All Rights Reserved. 

** auth： cg
** date： 2018/6/29 18:06:31 
** desc： TCP适配器的实现 

*********************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ShootGameServer.SharedData;

using System.Threading;

namespace ShootGameServer.Network
{
    //TCP的适配客户端实现
    public class TcpNetworkClientSession : INetworkClientSession
    {
        TcpClient client;
        BinaryWriter writer;
        BinaryReader reader;

        public event Action<GameMessage> MessageHandler;

        Thread _recieveThread = null;
        public TcpNetworkClientSession(TcpClient c)
        {
            client = c;
            writer = new BinaryWriter(c.GetStream());
            reader = new BinaryReader(c.GetStream());

            _recieveThread = new Thread((o) =>
            {
                try
                {
                    while (true)
                    {
                        int length = reader.ReadInt32();
                        var data = reader.ReadBytes(length);
                        var msg = DirectProtoBufTools.Deserialize<GameMessage>(data);
                        MessageHandler(msg);
                    }
                }catch
                {

                }
                
            });
            _recieveThread.Start();
        }

        public bool Connected
        {
            get
            {
                return client.Connected;
            }
        }

        public void Close()
        {
            _recieveThread.Abort();
            client.Close();
        }

        public void Send(GameMessage msg)
        {
            if (msg == null) return;

            var data = DirectProtoBufTools.Serialize(msg);
            if (data.Length == 0)
                return;

            writer.Write(data.Length);
            writer.Write(data);
        }

        public string GetRemoteConnectStr()
        {
            return client.Client.RemoteEndPoint.ToString();
        }
    }
}
