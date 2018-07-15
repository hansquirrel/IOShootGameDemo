/********************************************************************************

** Copyright(c) 2018 汉家松鼠工作室 All Rights Reserved. 

** auth： cg
** date： 2018/6/29 17:59:51 
** desc： 客户端连接适配器 

*********************************************************************************/

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ShootGameServer.SharedData;

namespace ShootGameServer.Network
{
    //客户端在服务端这边的session
    public interface INetworkClientSession
    {
        /// <summary>
        /// 消息处理器
        /// </summary>
        event Action<GameMessage> MessageHandler;

        /// <summary>
        /// 发送数据
        /// </summary>
        void Send(GameMessage msg);

        /// <summary>
        /// 关闭连接
        /// </summary>
        void Close();

        /// <summary>
        /// 是否连接
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// 获得客户端IP端口
        /// </summary>
        /// <returns></returns>
        string GetRemoteConnectStr();
    }
}
