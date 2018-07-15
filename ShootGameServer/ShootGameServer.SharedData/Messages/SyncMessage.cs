/********************************************************************************

** Copyright(c) 2018 汉家松鼠工作室 All Rights Reserved. 

** auth： cg
** date： 2018/6/29 18:19:46 
** desc： 尚未编写描述 

*********************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace ShootGameServer.SharedData
{
    /// <summary>
    /// 全量同步消息
    /// </summary>

    [ProtoContract]
    public class SyncMessage : GameMessage
    {
        public SyncMessage()
        {
            stateCode = MsgCode.SYNC;
        }

        [ProtoMember(1)]
        public List<GamePlayer> players;


        [ProtoMember(2)]
        public List<BulletInstance> bullets;

        [ProtoMember(3)]
        public int timeleft;
    }
}
