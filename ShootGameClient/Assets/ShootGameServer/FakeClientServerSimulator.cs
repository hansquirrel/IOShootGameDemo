using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

using System.Threading;
using ShootGameServer.SharedData;

namespace ShootGameServer
{         
	//玩家连接
	public class FakePlayerSession{

		//该玩家的数据
		public GamePlayer user;

		//数据调用回调
		public Action<GameMessage> msgHandler;
	}

    public class FakeClientServerSimulator : IGameServer
    {
        public void Start(){
            Thread t = new Thread (GameServerThread);
            t.Start ();
        }

        public void GameServerThread(){
            while (true) {
                try{
                    Thread.Sleep (20);
                    FixedUpdate ();
                }catch(Exception e){
                    Console.WriteLine (e.ToString ());
                }
            }
        }

        //获得子弹数量
        public int GetBulletsCount (){
            return _bullets.Count;
        }

        //获得人物数量
        public int GetPlayersCount(){
            return _players.Count;
        }


        void FixedUpdate()
        {
            //子弹碰撞检测
            for (int i = _bullets.Count - 1; i >= 0; i--) {
                var b = _bullets [i];

                //子弹移动
                b.Update ();

                //超出屏幕了直接移除
                bool toBeRemove = b.IsOutOfMap();

                //检查所有玩家
                foreach (var user in _players.Values) {
                    //碰撞检测
                    if (b.IsHit (user.user)) {

                        //分发子弹命中玩家的消息
                        DispatchMsg (new HitMessage(){
                            target = user.user.Clone (),
                            bullet = b.Clone()
                        });

                        //移除子弹
                        toBeRemove = true;

                        //扣血
                        user.user.hp --;
                        break;
                    }
                }

                if (toBeRemove) {
                    _bullets.RemoveAt (i);
                }
            }

            //死亡检测
            List<string> diePlayers = new List<string>();
            foreach (var user in _players.Values) {
                if (user.user.hp <= 0) {
                    diePlayers.Add (user.user.id);
                    DispatchMsg (new GamePlayerMessage (MsgCode.DIE, user.user.Clone()));
                }
            }

            //踢出死亡的角色
            foreach (var player in diePlayers) {
                _players.Remove (player);
            }


            //模拟一个随机延迟
            __delaySimulateIndex++;
            if (__delaySimulateIndex >= ConstSettings.DelaySimulate) {
                __delaySimulateIndex = 0;
                //dispatch all msg
                while (_msgQueue.Count > 0) {
                    var msg = _msgQueue.Dequeue ();
                    if (stateMsgDispatcher != null) {
                        stateMsgDispatcher (msg);
                    }
                }
            }
        }

        int __delaySimulateIndex = 0;

        //移动速度
        const int SPEED_RATE = 3;

        //user session
        Dictionary<string, FakePlayerSession> _players = new Dictionary<string, FakePlayerSession>();

        //all bullets
        List<BulletInstance> _bullets = new List<BulletInstance>();

        //随机种子
        System.Random _rnd = new System.Random();

        //状态消息事件
        event Action<GameMessage> stateMsgDispatcher;

        //消息发送队列
        Queue<GameMessage> _msgQueue = new Queue<GameMessage>();

        //新玩家
        public int NewPlayer(string id, Action<GameMessage> msgHandler){

            //检查是否有重复名字
            if(_players.ContainsKey (id)){
                return GameServerReturnCode.DUPLICATE_USERNAME;
            }

            //创建新的Session
            var user = new GamePlayer ();
            user.id = id;

            //寻找一个没有人的地点
            while(true){
                user.x = (float)(_rnd.NextDouble () * ConstSettings.MaxX * 2 - ConstSettings.MaxX);//随机生成位置x
                user.y = (float)(_rnd.NextDouble () * ConstSettings.MaxY * 2 - ConstSettings.MaxY);//随机生成位置y
                if (!IsHitAnyPlayer (user.id, user.x, user.y)) {
                    break;
                }
            }

            user.dx = 0;
            user.dy = 0;

            //生成随机颜色
            user.r = (float)_rnd.NextDouble ();
            user.g = (float)_rnd.NextDouble ();
            user.b = (float)_rnd.NextDouble ();

            var s = new FakePlayerSession ();
            s.user = user;
            s.msgHandler = msgHandler;
            stateMsgDispatcher += s.msgHandler;
            _players.Add (id, s);
           
            //分发消息
            DispatchMsg (new GamePlayerMessage(MsgCode.USER_JOIN, user.Clone()));

            return GameServerReturnCode.OK;
        }

        //移动指令
        public int OnMoveTowards(string id, float dx, float dy){
            if (!_players.ContainsKey (id)) {
                return GameServerReturnCode.USER_NOT_EXIST;
            }

            var session = _players [id];
            var user = session.user;
            user.dx = dx;
            user.dy = dy;

            float targetX = user.x + dx * SPEED_RATE;
            float targetY = user.y + dy * SPEED_RATE;
            AdjustPosition (ref targetX, ref targetY); //调整位置不超出最大范围限制

            //检测是否有撞到任何一个其他玩家
            if (IsHitAnyPlayer (user.id, targetX, targetY)) {
                return GameServerReturnCode.PLAYER_POSITION_HIT;
            }

            //赋值，分发
            user.x = targetX;
            user.y = targetY;

            //分发移动消息
            DispatchMsg(new GamePlayerMessage(MsgCode.USER_STATE_UPDATE, user.Clone()));

            return GameServerReturnCode.OK;
        }

        //是否撞到任何一个玩家
        bool IsHitAnyPlayer(string userId, float x, float y){
            //检测是否有撞到任何一个其他玩家
            foreach (var u in _players) {
                if (u.Value.user.id == userId)
                    continue;
                var user = u.Value.user;
                var dist = Math.Sqrt ((x - user.x) * (x - user.x) + (y - user.y) * (y - user.y));
                //碰撞检测距离
                if (dist < ConstSettings.HitDetectRange) {
                    return true;
                }
            }
            return false;
        }

        //退出
        public int Quit(string id){
            if (!_players.ContainsKey (id)) {
                return GameServerReturnCode.USER_NOT_EXIST;
            }

            var session = _players [id];
            stateMsgDispatcher -= session.msgHandler;
            _players.Remove (id);
            DispatchMsg (new GamePlayerMessage(MsgCode.USER_QUIT, session.user.Clone()));
            return GameServerReturnCode.OK;
        }

        //开火
//        public int Fire(string id){
//            if (!_players.ContainsKey (id)) {
//                return GameServerReturnCode.USER_NOT_EXIST;
//            }
//
//            var session = _players [id];
//            var user = session.user;
//
//            if (user.dx == 0 && user.dy == 0) {
//                return GameServerReturnCode.CAN_NOT_FIRE_WHILE_NOT_MOVING;
//            }
//
//            var bullet = new BulletInstance ();
//            bullet.ownerId = user.id;
//
//            //向量标准化
//            var m = Math.Sqrt(user.dx*user.dx + user.dy*user.dy);
//
//            bullet.dx = (float)(user.dx / m * ConstSettings.BulletSpeed);
//            bullet.dy = (float)(user.dy / m * ConstSettings.BulletSpeed);
//
//            bullet.x = user.x;
//            bullet.y = user.y;
//            _bullets.Add (bullet);
//            DispatchMsg (new FireMessage(){
//                bullet = bullet.Clone(),
//                player = user.Clone()
//            });
//            return GameServerReturnCode.OK;
//        }

        //判断位置
        void AdjustPosition(ref float x,ref float y){
            if (x < ConstSettings.MinX)
                x = ConstSettings.MinX;
            if (x > ConstSettings.MaxX)
                x = ConstSettings.MaxX;
            if (y < ConstSettings.MinY)
                y = ConstSettings.MinY;
            if (y > ConstSettings.MaxY)
                y = ConstSettings.MaxY;
        }

        //分发消息
        void DispatchMsg(GameMessage msg){
            _msgQueue.Enqueue (msg);
        }
    }

}

