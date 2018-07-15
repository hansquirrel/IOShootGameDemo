/********************************************************************************

** Copyright(c) 2018 汉家松鼠工作室 All Rights Reserved. 

** auth： cg
** date： 2018/6/29 16:45:59 
** desc： 网络连接适配器工厂 

*********************************************************************************/

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace ShootGameServer.Network
{
    //适配器工厂
    public static class NetworkFactory
    {
        static public INetworkServer CreateNetworkServer<T>() where T : INetworkServer, new()
        {
            return new T();
        }

        static public INetworkClient CreateNetworkClient<T>() where T : INetworkClient, new()
        {
            return new T();
        }
    }
}
