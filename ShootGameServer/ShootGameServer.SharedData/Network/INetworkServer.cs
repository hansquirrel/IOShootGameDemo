/********************************************************************************

** Copyright(c) 2018 汉家松鼠工作室 All Rights Reserved. 

** auth： cg
** date： 2018/6/29 18:03:25 
** desc： 网络服务器适配器 

*********************************************************************************/

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ShootGameServer.Network
{
    public interface INetworkServer
    {
        /// <summary>
        /// 监听
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        void StartListen(string connectStr);

        /// <summary>
        /// 有新的客户端连入
        /// </summary>
        /// <param name="session"></param>
        event Action<INetworkClientSession> OnNewClient;

        /// <summary>
        /// 关闭服务器
        /// </summary>
        void Shutdown();
    }
}
