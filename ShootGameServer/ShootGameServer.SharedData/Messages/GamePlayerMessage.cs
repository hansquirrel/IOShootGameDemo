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
    public class GamePlayerMessage : GameMessage
    {

        [ProtoMember(1)]
        public GamePlayer player;

        public GamePlayerMessage()
        {
        }

        public GamePlayerMessage(int code, GamePlayer u)
        {
            stateCode = code;
            player = u;
        }
    }
}
