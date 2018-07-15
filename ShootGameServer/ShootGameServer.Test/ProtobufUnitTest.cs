using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShootGameServer.SharedData;

namespace ShootGameServer.Test
{
    [TestClass]
    public class ProtobufUnitTest
    {
        [TestMethod]
        public void TestSerializeAndDeserialize()
        {
            GamePlayer p = new GamePlayer();
            p.r = 400f;
            p.dx = 0.001f;

            byte[] data = DirectProtoBufTools.Serialize(p);
            Assert.IsTrue(data.Length > 0);

            var sp = DirectProtoBufTools.Deserialize<GamePlayer>(data);
            Assert.IsTrue(sp.r == p.r);
            Assert.IsTrue(sp.dx == p.dx);
        }

        [TestMethod]
        public void TestProtobufList()
        {
            SyncMessage msg = GenerateSyncMessage();
            GameMessage baseMsg = msg;

            byte[] data = DirectProtoBufTools.Serialize(baseMsg);
            Assert.IsTrue(data.Length > 0);

            var r = DirectProtoBufTools.Deserialize<SyncMessage>(data);

            Assert.IsTrue(r.stateCode == msg.stateCode);
            Assert.IsTrue(r.players.Count == 1);
            Assert.IsTrue(r.players[0].id == "cg");
            Assert.IsTrue(r.bullets.Count == 2);
            Assert.IsTrue(r.bullets[1].id == 12);
        }

        [TestMethod]
        public void TestDynamicCast()
        {
            SyncMessage msg = GenerateSyncMessage();
            byte[] data = DirectProtoBufTools.Serialize(msg);
            Assert.IsTrue(data.Length > 0);
            var baseR = DirectProtoBufTools.Deserialize<GameMessage>(data);
            SyncMessage r = (SyncMessage)baseR;

            Assert.IsTrue(r.stateCode == msg.stateCode);
            Assert.IsTrue(r.players.Count == 1);
            Assert.IsTrue(r.players[0].id == "cg");
            Assert.IsTrue(r.bullets.Count == 2);
            Assert.IsTrue(r.bullets[1].id == 12);
        }

        SyncMessage GenerateSyncMessage()
        {
            SyncMessage msg = new SyncMessage();
            msg.stateCode = 100;
            msg.players = new List<GamePlayer>();
            msg.players.Add(new GamePlayer()
            {
                id = "cg",
            });
            msg.bullets = new List<BulletInstance>();
            msg.bullets.Add(new BulletInstance()
            {
                id = 11,
            });
            msg.bullets.Add(new BulletInstance()
            {
                id = 12,
            });
            return msg;
        }
    }
}
