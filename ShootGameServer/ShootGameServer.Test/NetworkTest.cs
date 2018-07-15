using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShootGameServer.Network;
using System.Net;
using ShootGameServer.SharedData;
using System.Threading;
using ShootGameServer.HSFramework.Room;
using ShootGameServer.HSFramework.Client;

namespace ShootGameServer.Test
{
    /// <summary>
    /// NetworkTest 的摘要说明
    /// </summary>
    [TestClass]
    public class NetworkTest
    {
        [TestMethod]
        public void TcpNetworkIntergrateTest()
        {
            NetworkIntergrateTest<TcpNetworkServer, TcpNetworkClient>("127.0.0.1:9003");
        }

        [TestMethod]
        public void HSFrameworkKCPIntergrateTest()
        {
            NetworkIntergrateTest<RoomServerBridge, RoomClientBridge>("endpoint case");
        }

        #region 模拟阻塞输入

        INetworkClientSession _blockWaitClientSession;
        /// <summary>
        /// 阻塞等待新客户端连入
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        INetworkClientSession BlockWaitNewClient(INetworkServer server)
        {
            Semaphore sema1 = new Semaphore(0, 1);
            _blockWaitClientSession = null;

            server.OnNewClient += (session) =>
            {
                _blockWaitClientSession = session;
                sema1.Release();
            };
            sema1.WaitOne();
            return _blockWaitClientSession;
        }

        GameMessage _msg;
        GameMessage BlockRead(INetworkClientSession session)
        {
            Semaphore sema1 = new Semaphore(0, 1);
            _msg = null;
            session.MessageHandler += (msg) => 
            {
                _msg = msg;
                sema1.Release();
            };
            sema1.WaitOne();
            return _msg;
        }
        GameMessage BlockRead(INetworkClient client)
        {
            Semaphore sema1 = new Semaphore(0, 1);
            _msg = null;
            client.MessageHandler += (msg) =>
            {
                _msg = msg;
                sema1.Release();
            };
            sema1.WaitOne();
            return _msg;
        }

        #endregion

        /// <summary>
        /// 网络集成测试
        /// </summary>
        public void NetworkIntergrateTest<T,V>(string connectStr) 
            where T : INetworkServer, new() 
            where V : INetworkClient, new()
        {
            //开启服务器
            INetworkServer server = NetworkFactory.CreateNetworkServer<T>();
            server.StartListen(connectStr);

            //测试用客户端进行连接
            INetworkClient client = null;

            ThreadPool.QueueUserWorkItem((o) => {
                Thread.Sleep(50); //假设50毫秒服务器可以开起来
                client = NetworkFactory.CreateNetworkClient<V>();
                client.Connect(connectStr);
            });

            //服务器获得新的session
            INetworkClientSession session = BlockWaitNewClient(server);
            Assert.IsNotNull(session);
            Assert.IsNotNull(client);
            Assert.IsTrue(client.Connected);

            //测试客户端往服务器发消息
            var testMsg = new GameMessage() { stateCode = 9999 };
            client.Send(testMsg);
            var msg = BlockRead(session);
            Assert.AreEqual(msg.stateCode, 9999);

            //测试服务器往客户端发消息
            session.Send(new SyncMessage() { stateCode = 9993 , timeleft = 3333 });
            msg = BlockRead(client);
            Assert.IsTrue(msg is SyncMessage);
            Assert.AreEqual(msg.stateCode, 9993);
            Assert.AreEqual((msg as SyncMessage).timeleft, 3333);

            //关闭连接
            client.Close();
            Assert.IsFalse(client.Connected);
            session.Close();
            Assert.IsFalse(session.Connected);

            //关闭服务器
            server.Shutdown();
        }
    }
}
