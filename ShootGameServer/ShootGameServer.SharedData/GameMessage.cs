using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using ProtoBuf;

namespace ShootGameServer.SharedData
{
    [ProtoContract]
    [ProtoInclude(4, typeof(GamePlayerMessage))]
    [ProtoInclude(5, typeof(HitMessage))]
    [ProtoInclude(6, typeof(MoveAndFireMessage))]
    [ProtoInclude(7, typeof(SyncMessage))]
    [ProtoInclude(8, typeof(TimeMessage))]
    [ProtoInclude(9, typeof(DieMessage))]
    
    [ProtoInclude(10, typeof(ClientJoinMessage))]
    [ProtoInclude(11, typeof(ClientMoveMessage))]
    public class GameMessage
    {
        [ProtoMember(1)]
        public int stateCode;

        [ProtoMember(2)]
        public string content;//可以用于携带消息
    }
}

