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
    [ProtoContract]
    public class TimeMessage : GameMessage
    {
        public TimeMessage()
        {
            stateCode = MsgCode.SYNC_TIME;
        }

        [ProtoMember(1)]
        public int timeleft;
    }
}
