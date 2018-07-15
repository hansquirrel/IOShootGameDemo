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
    /// 角色死亡消息
    /// </summary>
    [ProtoContract]
    public class DieMessage : GameMessage
    {
        public DieMessage()
        {
            stateCode = MsgCode.DIE;
        }
        /// <summary>
        /// 被杀的人，同时同步它的新的出生点
        /// </summary>
        [ProtoMember(1)]
        public GamePlayer player; 

        /// <summary>
        /// 杀人的人，同时同步它的杀死人的数量
        /// 有可能为NULL，杀死的时候这人已经退了
        /// </summary>
        [ProtoMember(2)]
        public GamePlayer killer;

        /// <summary>
        /// 杀死这个人的这颗子弹
        /// </summary>
        [ProtoMember(3)]
        public BulletInstance bullet; 
    }
}
