using System;
using NUnit.Framework;

using ShootGameServer;

using System.Collections.Generic;
using ShootGameServer.SharedData;

namespace ShootGameTest
{
    [TestFixture]
    public class ProtobufTest
    {
        [TestCase]
        public void OnTest(){
            GamePlayer p = new GamePlayer ();
            p.r = 400f;
            p.dx = 0.001f;

            byte[] data = DirectProtoBufTools.Serialize (p);
            Assert.True (data.Length > 0);

            var sp = DirectProtoBufTools.Deserialize<GamePlayer> (data);
            Assert.True (sp.r == p.r);
            Assert.True (sp.dx == p.dx);
        }
    }
}

