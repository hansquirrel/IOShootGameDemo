/********************************************************************************

** Copyright(c) 2018 汉家松鼠工作室 All Rights Reserved. 

** auth： cg
** date： 2018/6/29 18:26:28 
** desc： 尚未编写描述 

*********************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace ShootGameServer.SharedData
{
    public class ServerMessageCarrior
    {
        public string sendto;//只发给某个玩家
        public string except;//不包含某个玩家

        public GameMessage message;
    }

    public class ClientMessageCarrior
    {
        public string id; //发送者ID

        public GameMessage message;
    }
}
