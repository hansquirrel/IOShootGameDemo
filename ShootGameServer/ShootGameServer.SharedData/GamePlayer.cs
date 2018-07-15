using System;
using System.Xml.Serialization;
using ProtoBuf;

namespace ShootGameServer.SharedData
{
    //游戏玩家
    
    [ProtoContract]
    public class GamePlayer
    {
        //玩家姓名
        
        [ProtoMember(1)]
        public string id;

        [ProtoMember(2)]
        public int hp = ConstSettings.DefaultHp;

        //当前位置
        
        [ProtoMember(3)]
        public float x;
        
        [ProtoMember(4)]
        public float y;

        //当前朝向（和速度）
        
        [ProtoMember(5)]
        public float dx;
        
        [ProtoMember(6)]
        public float dy;

        //颜色
        [ProtoMember(7)]
        public float r;
        
        [ProtoMember(8)]
        public float g;
        
        [ProtoMember(9)]
        public float b;

        /// <summary>
        /// 杀人数量
        /// </summary>
        [ProtoMember(10)]
        public int killed;

        /// <summary>
        /// 死亡数量
        /// </summary>
        [ProtoMember(11)]
        public int dead; 

        public GamePlayer Clone()
        {
            return base.MemberwiseClone() as GamePlayer;
        }
    }
}

