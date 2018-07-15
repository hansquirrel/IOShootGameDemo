/********************************************************************************

** Copyright(c) 2018 汉家松鼠工作室 All Rights Reserved. 

** auth： cg
** date： 2018/6/29 17:29:09 
** desc： 尚未编写描述 

*********************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace ShootGameServer.SharedData
{
    [ProtoContract]
    public class BulletInstance
    {

        static int __index;
        public BulletInstance()
        {
            id = __index++;
        }

        [ProtoMember(1)]
        public int id;
        [ProtoMember(2)]
        public string ownerId;
        [ProtoMember(3)]
        public float dx;
        [ProtoMember(4)]
        public float dy;
        [ProtoMember(5)]
        public float x;
        [ProtoMember(6)]
        public float y;

        public BulletInstance Clone()
        {
            return base.MemberwiseClone() as BulletInstance;
        }

        public bool IsOutOfMap()
        {
            return (x < ConstSettings.MinX || x > ConstSettings.MaxX || y < ConstSettings.MinY || y > ConstSettings.MaxY);
        }

        public bool IsHit(GamePlayer user)
        {

            //子弹不能命中自己
            if (ownerId == user.id)
                return false;

            //求两点的距离
            var dist = Math.Sqrt((x - user.x) * (x - user.x) + (y - user.y) * (y - user.y));

            //碰撞检测距离
            return dist < ConstSettings.HitDetectRange;
        }

        public void Update()
        {
            x += dx;
            y += dy;
        }
    }
}
