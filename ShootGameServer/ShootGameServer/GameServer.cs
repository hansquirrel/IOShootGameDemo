/********************************************************************************

** Copyright(c) 2018 汉家松鼠工作室 All Rights Reserved. 

** auth： cg
** date： 2018/6/28 11:33:55 
** desc： 尚未编写描述 

*********************************************************************************/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShootGameServer.Network;
using ShootGameServer.SharedData;

namespace ShootGameServer
{
    public class GameServer
    {
        //剩余时间
        public int TimeLeft = ConstSettings.RoundTime;
        //玩家请求的消息队列
        public ConcurrentQueue<ClientMessageCarrior> PlayerMessageQueue = new ConcurrentQueue<ClientMessageCarrior>();
        //全体玩家连接
        public Dictionary<string, GamePlayerSession> _players = new Dictionary<string, GamePlayerSession>();
        //子弹
        public List<BulletInstance> _bullets = new List<BulletInstance>();
        //移动速度
        const int SPEED_RATE = 3;
        //随机种子
        System.Random _rnd = new System.Random();
        //消息发送队列
        Queue<ServerMessageCarrior> _msgQueue = new Queue<ServerMessageCarrior>();

        #region 业务接口

        //获得子弹数量
        public int GetBulletsCount()
        {
            return _bullets.Count;
        }

        //获得人物数量
        public int GetPlayersCount()
        {
            return _players.Count;
        }

        bool DoCheckOneBullet(BulletInstance b)
        {
            //子弹移动
            b.Update();

            //超出屏幕了直接移除
            bool toBeRemove = b.IsOutOfMap();

            //检查所有玩家和子弹的碰撞
            foreach (var user in _players.Values)
            {
                //这个玩家已经死了
                if (user.user.hp == 0)
                    continue;

                //碰撞检测
                if (b.IsHit(user.user))
                {
                    //分发子弹命中玩家的消息
                    DispatchMsg(new HitMessage()
                    {
                        target = user.user,
                        bullet = b
                    });

                    //移除子弹
                    toBeRemove = true;

                    //扣血
                    lock (user.user)
                    {
                        user.user.hp--;
                    }

                    //这个玩家死了
                    if (user.user.hp <= 0)
                    {
                        string id = b.ownerId; //子弹主人的ID
                        lock (user.user)
                        {
                            user.user.dead++;

                            //重新设置出生位置
                            ResetPositionAndStatus(user.user);
                        }
                        
                        GamePlayer killer = null;
                        if (_players.ContainsKey(id)) //子弹主人还存在
                        {
                            killer = _players[id].user;
                            killer.killed++;
                        }

                        //分发击杀消息
                        DispatchMsg(new DieMessage()
                        {
                            player = user.user,
                            bullet = b,
                            killer = killer
                        });
                    }
                    break;
                }
            }

            return toBeRemove;
        }

        //子弹碰撞检测(线程池版本)
        void CheckAllBulletsTaskPool()
        {
            ConcurrentQueue<int> removeIndexs = new ConcurrentQueue<int>();

            List<Task> tasks = new List<Task>();

            int TaskBulletCount = 30;
            //每100个子弹一个Task
            if(_bullets.Count > TaskBulletCount)
            {
                for(int i = 0; i < _bullets.Count; i = i + TaskBulletCount)
                {
                    int start = i;
                    int end = Math.Min(i + TaskBulletCount, _bullets.Count);

                    var task = Task.Run(() => {
                        for(int j = start; j < end; ++j)
                        {
                            var remove = DoCheckOneBullet(_bullets[j]);
                            if (remove)
                            {
                                removeIndexs.Enqueue(j);
                            }
                        }
                    });
                    tasks.Add(task);
                }
            }
            
            Task.WaitAll(tasks.ToArray());
            var list = removeIndexs.ToList();
            list.Sort();
            
            for (int i= list.Count-1;i>=0;--i)
            {
                int index = list[i];
                _bullets.RemoveAt(index);
            }
        }
        //子弹碰撞检测
        void CheckAllBullets()
        {
            //子弹碰撞检测
            for (int i = _bullets.Count - 1; i >= 0; i--)
            {
                var b = _bullets[i];
                var remove = DoCheckOneBullet(b);
                if (remove)
                {
                    _bullets.RemoveAt(i);
                }
            }
        }

        List<string> __disConnectedPlayers = new List<string>();
        void FixedUpdate()
        {
            //子弹碰撞检测
            CheckAllBullets();
            //CheckAllBulletsTaskPool();

            __disConnectedPlayers.Clear();
            //分发所有消息
            while (_msgQueue.Count > 0)
            {
                var msg = _msgQueue.Dequeue();
                foreach(var player in _players)
                {
                    if (!string.IsNullOrEmpty(msg.sendto) && msg.sendto != player.Key) continue; //只发给需要发的人
                    if (!string.IsNullOrEmpty(msg.except) && msg.except == player.Key) continue; //剔除掉需要剔除的人

                    var client = player.Value.client;
                    
                    if (client.Connected)
                    {
                        try
                        {
                            client.Send(msg.message);
                        }
                        catch
                        {
                            //连接断了
                            __disConnectedPlayers.Add(player.Key);
                        }
                    }
                }
            }

            //踢掉3秒以上无心跳的客户端
            foreach (var user in _players.Values)
            {
                if (DateTime.Now - user.lastActiveTime > TimeSpan.FromSeconds(ConstSettings.DropIfNotAliveTime))
                {
                    __disConnectedPlayers.Add(user.id);
                }
            }

            //踢出断开连接的角色
            foreach (var player in __disConnectedPlayers)
            {
                PlayerDropped(player);
            }
        }

        //踢出玩家
        public void PlayerDropped(string id)
        {
            if (_players.ContainsKey(id)) {
                _players[id].Close(); //关闭连接
                _players.Remove(id);
            }
        }

        //寻找一个没有人的地点
        void ResetPositionAndStatus(GamePlayer user)
        {
            //寻找一个没有人的地点
            while (true)
            {
                user.x = (float)(_rnd.NextDouble() * ConstSettings.MaxX * 2 - ConstSettings.MaxX);//随机生成位置x
                user.y = (float)(_rnd.NextDouble() * ConstSettings.MaxY * 2 - ConstSettings.MaxY);//随机生成位置y
                if (!IsHitAnyPlayer(user.id, user.x, user.y))
                {
                    break;
                }
            }

            //回复血
            user.hp = ConstSettings.DefaultHp;
        }

        //新玩家
        public int NewPlayer(string id, GamePlayerSession session)
        {
            lock (this)
            {
                if (_players.ContainsKey(id))
                    return GameServerReturnCode.DUPLICATE_USERNAME;

                //新玩家
                var user = new GamePlayer();
                user.id = id;

                //寻找新位置，回复HP
                ResetPositionAndStatus(user);
                
                //生成随机颜色
                user.r = (float)_rnd.NextDouble();
                user.g = (float)_rnd.NextDouble();
                user.b = (float)_rnd.NextDouble();

                //创建session设置
                session.user = user;

                //给其他玩家分发玩家加入消息
                DispatchMsg(new GamePlayerMessage(MsgCode.USER_JOIN, user), "", id);
                
                _players.Add(id, session);

                //给自己全量同步
                DispatchMsg(GenerateSyncMessage(), id);

                return GameServerReturnCode.OK;
            }
        }

        //生成全量同步的信息
        SyncMessage GenerateSyncMessage()
        {
            SyncMessage msg = new SyncMessage();
            msg.players = new List<GamePlayer>();
            foreach (var p in _players.Values)
            {
                msg.players.Add(p.user.Clone());
            }
            msg.bullets = new List<BulletInstance>();
            foreach (var b in _bullets)
            {
                msg.bullets.Add(b.Clone());
            }
            msg.timeleft = TimeLeft;
            return msg;
        }

        //移动指令
        public int MoveAndFire(string id, float dx, float dy)
        {
            if (!_players.ContainsKey(id))
            {
                return GameServerReturnCode.USER_NOT_EXIST;
            }

            var session = _players[id];
            var user = session.user;
            user.dx = dx;
            user.dy = dy;

            float targetX = user.x + dx * SPEED_RATE;
            float targetY = user.y + dy * SPEED_RATE;
            AdjustPosition(ref targetX, ref targetY); //调整位置不超出最大范围限制

            //检测是否有撞到任何一个其他玩家
            if (IsHitAnyPlayer(user.id, targetX, targetY))
            {
                return GameServerReturnCode.PLAYER_POSITION_HIT;
            }

            //赋值，分发
            user.x = targetX;
            user.y = targetY;

            BulletInstance bullet = null;

            //只有移动的时候可以开火
            if(dx !=0 && dy != 0)
            {
                //开火，生成子弹
                bullet = new BulletInstance();
                bullet.ownerId = user.id;

                //向量标准化
                var m = Math.Sqrt(user.dx * user.dx + user.dy * user.dy);

                bullet.dx = (float)(user.dx / m * ConstSettings.BulletSpeed);
                bullet.dy = (float)(user.dy / m * ConstSettings.BulletSpeed);

                bullet.x = user.x;
                bullet.y = user.y;
                _bullets.Add(bullet);
            }

            //分发移动消息
            DispatchMsg(new MoveAndFireMessage()
            {
                player = user,
                bullet = bullet,
            });

            return GameServerReturnCode.OK;
        }

        //是否撞到任何一个玩家
        bool IsHitAnyPlayer(string userId, float x, float y)
        {
            //检测是否有撞到任何一个其他玩家
            foreach (var u in _players)
            {
                if (u.Value.user == null || u.Value.user.id == userId)
                    continue;
                var user = u.Value.user;
                var dist = Math.Sqrt((x - user.x) * (x - user.x) + (y - user.y) * (y - user.y));
                //碰撞检测距离
                if (dist < ConstSettings.HitDetectRange)
                {
                    return true;
                }
            }
            return false;
        }

        //退出
        public int Quit(string id)
        {
            if (!_players.ContainsKey(id))
            {
                return GameServerReturnCode.USER_NOT_EXIST;
            }

            var session = _players[id];
            session.Close();
            _players.Remove(id);
            DispatchMsg(new GamePlayerMessage(MsgCode.USER_QUIT, session.user));
            return GameServerReturnCode.OK;
        }

        //判断位置
        void AdjustPosition(ref float x, ref float y)
        {
            if (x < ConstSettings.MinX)
                x = ConstSettings.MinX;
            else if (x > ConstSettings.MaxX)
                x = ConstSettings.MaxX;
            if (y < ConstSettings.MinY)
                y = ConstSettings.MinY;
            else if (y > ConstSettings.MaxY)
                y = ConstSettings.MaxY;
        }

        //分发消息
        void DispatchMsg(GameMessage msg,string sendto = "",string except = "")
        {
            ServerMessageCarrior c = new ServerMessageCarrior();
            c.sendto = sendto;
            c.except = except;
            c.message = msg;
            lock (_msgQueue)
            {
                _msgQueue.Enqueue(c);
            }
        }

        #endregion

        //开始新的一局游戏
        void NewRound()
        {
            _msgQueue.Clear(); //清空消息队列
            _bullets.Clear(); //清空所有子弹

            //重置所有角色
            foreach(var p in _players)
            {
                var user = p.Value.user;
                user.killed = 0;
                user.dead = 0;

                ResetPositionAndStatus(user);

                //发送新局
                DispatchMsg(new GameMessage() { stateCode = MsgCode.NEW_ROUND });

                //重新同步
                DispatchMsg(GenerateSyncMessage());
            }

            TimeLeft = ConstSettings.RoundTime;
        }

        //游戏一帧的数据
        void FrameLogic()
        {
            lock (this)
            {
                //处理所有客户端请求
                while(PlayerMessageQueue.Count > 0)
                {
                    ClientMessageCarrior clientMsg;
                    PlayerMessageQueue.TryDequeue(out clientMsg);
                    PlayerMessage.Execute(clientMsg, this);
                }

                //处理游戏逻辑
                FixedUpdate();
            }
        }

        //主循环函数
        void LoopFun()
        {
            int FRAME_TIME = 20;
            int timeStep = FRAME_TIME;

            int lastTimeStamp = System.Environment.TickCount;
            int delta = 0;
            int frame = 0; //数据帧
            while (true)
            {
                try
                {
                    frame++;
                    int before = System.Environment.TickCount;
                    FrameLogic();
                    int after = System.Environment.TickCount;
                    delta = after - before;

                    //修正计算时间
                    timeStep = FRAME_TIME - delta;
                    if (timeStep < 0)
                        timeStep = 0;
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                Thread.Sleep(timeStep); //先直接粗暴的这样模拟时间
                TimeLeft -= (delta + timeStep);

                //500毫秒发送一次时间同步信息
                if(frame * FRAME_TIME % 500 == 0)
                {
                    DispatchMsg(new TimeMessage() { timeleft = TimeLeft });
                }

                //2秒统计一次
                if (frame * FRAME_TIME % 2000 == 0) 
                {
                    Console.WriteLine(string.Format("players:{0},bullets:{1},PlayerMessageQueue:{2},sendQueue:{3},每帧计算时间:{4}",
                        GetPlayersCount(),
                        GetBulletsCount(),
                        PlayerMessageQueue.Count,
                        _msgQueue.Count, 
                        delta));
                }

                //时间到了
                if(TimeLeft <= 0)
                {
                    NewRound();
                }
            }
        }

        public void Start()
        {
            var listener = NetworkFactory.CreateNetworkServer<TcpNetworkServer>();
            listener.OnNewClient += (client) =>
            {
                GamePlayerSession session = new GamePlayerSession(client, this);
                //立刻读取请求
                //var msg = client.ReadSync();
                //if (msg != null && msg is ClientJoinMessage)
                //{
                //    var id = ((ClientJoinMessage)msg).id;
                //    this.NewPlayer(id, client);
                //}
                //else
                //{
                //    client.Close(); //非法传入，直接关闭
                //}
            };
            listener.StartListen(ConstSettings.DefaultServerEndPoint);

            //接收所有的客户端请求
            LoopFun();
        }
    }
}
