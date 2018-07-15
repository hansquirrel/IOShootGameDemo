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
    public class TcpNetworkServer : INetworkServer
    {
        TcpListener __listener;
        Thread __listenThread;

        public event Action<INetworkClientSession> OnNewClient;

        public void Shutdown()
        {
            __listener.Stop();
            __listenThread.Abort();
        }

        public void StartListen(string connectStr)
        {
            var ipaddr = connectStr.Split(':')[0];
            int port = int.Parse(connectStr.Split(':')[1]);
            IPAddress ip = IPAddress.Parse(ipaddr);

            __listener = new TcpListener(ip, port);
            __listener.Start();

            //核心线程
            __listenThread = new Thread(()=> {

                Console.WriteLine("waiting for client connect..");
                try
                {
                    while (true)
                    {
                        //接收所有的客户端请求
                        var client = __listener.AcceptTcpClient();
                        TcpNetworkClientSession session = new TcpNetworkClientSession(client);
                        Console.WriteLine("new client connected:" + session.GetRemoteConnectStr());
                        OnNewClient(session);
                    }
                }catch(Exception e)
                {
                    //一般是断开连接
                    Console.WriteLine(e.ToString());
                }
            });
            __listenThread.Start();
        }
    }
}
