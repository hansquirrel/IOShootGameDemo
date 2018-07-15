/********************************************************************************

** Copyright(c) 2018 汉家松鼠工作室 All Rights Reserved. 

** auth： cg
** date： 2018/6/30 16:04:20 
** desc： 尚未编写描述 

*********************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSFrameWork.Common;
using HSFrameWork.RoomService;
using ShootGameServer.Network;
using ShootGameServer.SharedData;

namespace ShootGameServer.HSFramework.Room
{
    public class RoomServerBridge : RoomLogic , INetworkServer
    {
        GameServer server;
        uint currentClientId = 0;

        Dictionary<PlayerLink, RoomSessionBridge> sessionMap = new Dictionary<PlayerLink, RoomSessionBridge>();

        #region 接口实现RoomLogic
        public Task<byte[]> MainTask { get; set; }

        public void Init(byte[] setupData, out byte[] clientSetupData)
        {
            clientSetupData = null;
        }

        public bool OnProcessQuickKnock(byte[] data, int offset, int size, out object validationState)
        {
            validationState = null;
            return true;
        }

        public void OnProcessKnock(object callBackState, object validationState, byte[] data, int offset, int size, HandShakeCheckCallBack callback)
        {
            //新的连接session
            //state先保存一个RoomServer的引用，不知道是否后面会用
            callback(callBackState, true, currentClientId++, this, OnDataReceived, OnEvent, null);
        }

        public Task<byte[]> RunAsync()
        {
            MainTask = RunAsyncRoomServer();
            return MainTask;
        }

        async Task<byte[]> RunAsyncRoomServer()
        {
            await Task.Run(() => {
                server = new GameServer();
                server.Start();
            });
            return null;
        }
        #endregion

        #region 接口实现INetworkServer

        public event Action<INetworkClientSession> OnNewClient;

        public void StartListen(string connectStr)
        {
            //do nothing.
            //已经被HSFramework托管
        }

        public void Shutdown()
        {
            //do nothing.
            //已经被HSFramework托管
        }
        #endregion


        //收到了新的数据
        private void OnDataReceived(PlayerLink link, byte[] data, int offset, int size)
        {
            //如果没有连接，那么不接收数据
            if (!sessionMap.ContainsKey(link)) return;
            var session = sessionMap[link];

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

            session.OnMessage(msg);
        }

        //客户端状态发生变化
        private void OnEvent(PlayerLink link, PlayerLinkStatus status)
        {
            switch (status)
            {
                case PlayerLinkStatus.Connected:
                    //创建session
                    var session = new RoomSessionBridge(link, this);
                    sessionMap.Add(link, session);
                    OnNewClient(session);
                    break;
                default: 
                    break;
            }
        }

        public void RemovePlayerLink(PlayerLink link)
        {
            if (sessionMap.ContainsKey(link))
            {
                sessionMap.Remove(link);
            }
        }
    }
}
