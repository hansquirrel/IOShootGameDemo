/********************************************************************************

** Copyright(c) 2018 汉家松鼠工作室 All Rights Reserved. 

** auth： cg
** date： 2018/6/30 17:01:15 
** desc： 尚未编写描述 

*********************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSFrameWork.Common;
using HSFrameWork.KCP.Client;
using ShootGameServer.Network;
using ShootGameServer.SharedData;

namespace ShootGameServer.HSFramework.Client
{
    public class RoomClientBridge : INetworkClient
    {
        public static uint displayName = 0;
        public static uint localSessionIdCount = 0;

        public RoomClientBridge()
        {
            client = KCPClientFactory.CreateSync(displayName++, this, OnRecvData);
        }

        PlayerLinkClientSync client;

        protected void OnRecvData(PlayerLink client, byte[] data, int offset, int size)
        {
            //数据包=长度（int32）+实际数据
            //至少要有一个int32的length的描述
            if (size < sizeof(Int32))
            {
                throw new Exception("数据错误");
            }

            int length = BitConverter.ToInt32(data, offset);
            int offsetNew = offset + sizeof(Int32);
            int lengthNew = size - sizeof(Int32);
            byte[] d = new byte[lengthNew];
            Array.Copy(data, offsetNew, d, 0, lengthNew);
            var msg = SharedData.DirectProtoBufTools.Deserialize<GameMessage>(d);
            MessageHandler(msg);
        }

        #region 接口实现INetworkClient
        public bool Connected
        {
            get
            {
                return client.Status == PlayerLinkStatus.Connected;
            }
        }

        public event Action<GameMessage> MessageHandler;

        public void Close()
        {
            client.CloseForcefully();
        }

        public void Connect(string connectStr)
        {
            client.Connect(Encoding.UTF8.GetBytes(connectStr), localSessionIdCount++, null,
                (byte[] data, int offset, int length) =>
                {
                    //直接默认同意
                    return true;
                },
                (link, status) => {
                    //do nothing
                });
        }

        public void Send(GameMessage msg)
        {
            if (msg == null) return;

            var data = SharedData.DirectProtoBufTools.Serialize(msg);
            if (data.Length == 0)
                return;

            //数据包=长度（int32）+实际数据
            //background不知道什么意思，先随便设了
            client.Send(BitConverter.GetBytes(data.Length), true);
            client.Send(data, true);
        }
        #endregion
    }
}
