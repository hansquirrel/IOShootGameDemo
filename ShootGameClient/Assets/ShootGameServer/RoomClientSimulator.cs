using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ShootGameServer.SharedData;

namespace ShootGameServer
{
    //这个中间的实现要替换成网络函数
    public class RoomClientSimulator
    {
        public virtual bool DropMsg(){
            return false;
        }

        IGameServer _server;
        string _id;

        public RoomClientSimulator(IGameServer s, string id){
            _server = s ;
            _id = id;
        }

        public Queue<GameMessage> msgQueue = new Queue<GameMessage>();
        void OnRecieveMsg(GameMessage basemsg){
            if (DropMsg ())
                return;
            
            lock (msgQueue) {
                msgQueue.Enqueue (basemsg);
            }
        }

        public bool Connect(){
            return (_server.NewPlayer (_id, OnRecieveMsg) == GameServerReturnCode.OK);
        }

        public int Move(float dx, float dy){
            return _server.OnMoveTowards (_id,dx,dy);
        }

        public int Quit(){
            return _server.Quit (_id);
        }

//        public int Fire(){
//            return _server.Fire (_id);
//        }
    }

    //模拟电脑的客户端
    public class NPCSimulator : RoomClientSimulator
    {
        public override bool DropMsg ()
        {
            return true;
        }

        public static double GetRandom(double a, double b)
        {
            double k = 0;
            k = rnd.NextDouble();
            double tmp = 0;
            if (b > a)
            {
                tmp = a;
                a = b;
                b = tmp;
            }
            return b + (a - b) * k;
        }

        public static int GetRandomInt(int a, int b)
        {
            int k =  (int)GetRandom(a, b + 1);
            if (k >= b && b >= a)
                k = b;
            return k;
        }

        MonoBehaviour _parent;
        static System.Random rnd = new System.Random();

        public NPCSimulator (IGameServer s, string id, MonoBehaviour m) : base (s, id){
            _parent = m;
            _parent.StartCoroutine (DORandomAI());
        }

        int waitround = 0;
        float dx = 0;
        float dy = 0;
		float ddx = 0;
		float ddy = 0;
        const int MAXSPEED = 3;

        IEnumerator DORandomAI(){
            yield return new WaitForSeconds (0.02f);

            //每0.5-2秒钟改变一下自己的方向
            if (waitround == 0) {
                waitround = (int)(GetRandom (10, 40));
                Vector2 speed = new Vector2 ((float)GetRandom (-MAXSPEED, MAXSPEED), (float)GetRandom (-MAXSPEED, MAXSPEED));
                if (speed.magnitude > 1) {
                    speed.Normalize ();
                }

				//保持一个旋转角度
				ddx = (float)(GetRandom (-0.1, 0.1));
				ddy = (float)(GetRandom (-0.1, 0.1));

                dx = speed.x;
                dy = speed.y;
            }

            waitround--;
			dx += ddx;
			dy += ddy;

            Move (dx, dy);
            //Fire ();

            _parent.StartCoroutine (DORandomAI());
        }

    }
}

