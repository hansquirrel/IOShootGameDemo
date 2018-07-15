/********************************************************************************

** Copyright(c) 2018 汉家松鼠工作室 All Rights Reserved. 

** auth： cg
** date： 2018/6/30 16:29:52 
** desc： 尚未编写描述 

*********************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSFrameWork.Common;
using ShootGameServer.Network;
using ShootGameServer.SharedData;

namespace ShootGameServer.HSFramework.Room
{
    public class RoomSessionBridge : INetworkClientSession
    {
        public RoomSessionBridge(PlayerLink l, RoomServerBridge s)
        {
            link = l;
            server = s;
        }

        RoomServerBridge server;
        PlayerLink link;

        #region 接口实现
        public bool Connected
        {
            get
            {
                return link.Status == PlayerLinkStatus.Connected;
            }
        }

        public event Action<GameMessage> MessageHandler;

        public void Close()
        {
            server.RemovePlayerLink(link);
            link.CloseForcefully();//用哪个？
        }

        public string GetRemoteConnectStr()
        {
            return link.PK;
        }

        public void Send(GameMessage msg)
        {
            if (msg == null) return;

            var data = SharedData.DirectProtoBufTools.Serialize(msg);
            if (data.Length == 0)
                return;
            
            //数据包=长度（int32）+实际数据
            //background不知道什么意思，先随便设了
            link.Send(BitConverter.GetBytes(data.Length), true);
            link.Send(data, true);
        }
        #endregion

        public void OnMessage(GameMessage msg)
        {
            MessageHandler(msg);
        }
    }
}
