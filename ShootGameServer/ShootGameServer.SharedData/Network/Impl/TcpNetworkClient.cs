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
    //TCP适配服务器实现
    public class TcpNetworkClient : INetworkClient
    {
        Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        BinaryReader reader;
        BinaryWriter writer;

        public event Action<GameMessage> MessageHandler;

        Thread _recieveThread = null;

        public void Connect(string connectStr)
        {
            var ipaddr = connectStr.Split(':')[0];
            int port = int.Parse(connectStr.Split(':')[1]);
            IPAddress ip = IPAddress.Parse(ipaddr);

            clientSocket.Connect(new IPEndPoint(ip, port));

            reader = new BinaryReader(new NetworkStream(clientSocket));
            writer = new BinaryWriter(new NetworkStream(clientSocket));

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

        public void Send(GameMessage msg)
        {
            if (msg == null) return;

            var data = DirectProtoBufTools.Serialize(msg);
            if (data.Length == 0)
                return;

            writer.Write(data.Length);
            writer.Write(data);
        }

        public void Close()
        {
            _recieveThread.Abort();
            clientSocket.Close();
        }

        public bool Connected
        {
            get
            {
                return clientSocket.Connected;
            }
        }
    }
}
