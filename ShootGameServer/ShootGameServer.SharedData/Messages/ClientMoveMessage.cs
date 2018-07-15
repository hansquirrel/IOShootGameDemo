/********************************************************************************

** Copyright(c) 2018 汉家松鼠工作室 All Rights Reserved. 

** auth： cg
** date： 2018/6/29 20:38:24 
** desc： 尚未编写描述 

*********************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace ShootGameServer.SharedData
{
    [ProtoContract]
    public class ClientMoveMessage : GameMessage
    {
        public ClientMoveMessage()
        {
            stateCode = MsgCode.CLIENT_MOVE;
        }

        [ProtoMember(1)]
        public float dx;

        [ProtoMember(2)]
        public float dy;
    }
}
