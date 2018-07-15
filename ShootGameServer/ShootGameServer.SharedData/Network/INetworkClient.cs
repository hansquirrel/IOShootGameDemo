/********************************************************************************

** Copyright(c) 2018 汉家松鼠工作室 All Rights Reserved. 

** auth： cg
** date： 2018/6/29 20:53:53 
** desc： 尚未编写描述 

*********************************************************************************/

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using ShootGameServer.SharedData;

namespace ShootGameServer.Network
{
    /// <summary>
    /// 客户端接口
    /// </summary>
    public interface INetworkClient
    {
        /// <summary>
        /// 消息处理器
        /// </summary>
        event Action<GameMessage> MessageHandler;

        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <param name="ipaddr"></param>
        /// <param name="port"></param>
        void Connect(string connectStr);

        /// <summary>
        /// 发送数据
        /// </summary>
        void Send(GameMessage msg);

        /// <summary>
        /// 是否连接
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// 关闭连接
        /// </summary>
        void Close();
    }
}
