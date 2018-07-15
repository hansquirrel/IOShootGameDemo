using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using ShootGameServer;
using UnityEngine.SceneManagement;
using ShootGameServer.SharedData;

public class ShootGameManager : MonoBehaviour {

    static public string MyId = ConstSettings.DefaultName;
    static public string ServerEndpoint = ConstSettings.DefaultServerEndPoint;

    public Gamer zhujue;
    public Gamer gamerPrefab;
    public Bullet bulletPrefab;

    public Transform gamerParent;

    public Joystick joyStick;

    public Button fireButton;
    public GameStateGUI fpsGUI;

    public Text playerText;
    public Text infoText;
    public Text timeText;

    public IGameServer server;

    Dictionary<string, Gamer> _gamers = new Dictionary<string, Gamer>();
    Dictionary<string, NPCSimulator> _npcClients = new Dictionary<string, NPCSimulator>();
    Dictionary<int, Bullet> _bullets = new Dictionary<int, Bullet>();

    [HideInInspector]
    public int TimeLeft;

    void Awake(){
        //server = new FakeClientServerSimulator ();
        //((FakeClientServerSimulator)server).Start ();

        server = new DemoTcpClient ();
    }

	// Use this for initialization
	void Start () {

        //是否关闭调试面板
        if (ConstSettings.DisableConsolePanelInRuntime) {
            GameObject.Find ("debugpanel").SetActive (Application.isEditor);
        }

        zhujue.client = new RoomClientSimulator (server, MyId);
        zhujue.client.Connect ();

        fpsGUI.server = server;

        //for test
        //SpawnClientTest(10);
	}
		
    public void SpawnClientTest(int count){
        for (int i = 0; i < count; ++i) {
			var testId = "NPCPlayer:" + System.Guid.NewGuid ().ToString ().Substring (0, 8);
            var newclient = new NPCSimulator (new DemoTcpClient(), testId, this);
            newclient.Connect ();

            _npcClients.Add (testId, newclient);
        }
    }

    public void KillClientTest(int count){

        List<string> killList = new List<string> ();

        foreach (var kv in _gamers) {
            if (kv.Value != zhujue) {
                var find = kv.Key;
                killList.Add (find);;
                if (killList.Count >= count)
                    break;
            }
        }

        foreach (var kill in killList) {
            if (_npcClients.ContainsKey (kill)) {
                var client = _npcClients [kill];
                if (client != null) {
                    client.Quit ();
                    _npcClients.Remove (kill);
                }
            }
        }
    }

    void AddPlayer(GamePlayer user){

        //不重复加s
        if (_gamers.ContainsKey (user.id))
            return;

        //自己
        if (user.id == MyId) {
            zhujue.SetName (user.id);
            zhujue.SetHp (user.hp);
            zhujue.SetPosition (user.x, user.y);
            zhujue.SetColor (GetColor(user));
            _gamers.Add (user.id, zhujue);
        } else {
            //其他人加入
            var newGamerObj = Instantiate (gamerPrefab.gameObject);
            var newGamer = newGamerObj.GetComponent<Gamer> ();
            newGamer.transform.SetParent (gamerParent, false);
            newGamer.SetName (user.id);
            newGamer.SetHp (user.hp);
            newGamer.SetPosition (user.x, user.y);
            newGamer.SetRotation (user.dx, user.dy);
            newGamer.SetColor (GetColor(user));
            _gamers.Add (user.id, newGamer);
        }
    }

    Dictionary<string,Color> __colorMap = new Dictionary<string, Color>();
    Color GetColor(GamePlayer p){
        if (!__colorMap.ContainsKey (p.id)) {
            var color = new Color (p.r, p.g, p.b);
            __colorMap.Add (p.id, color);
        }

        return __colorMap [p.id];
    }

    void AddBullet(BulletInstance b){
        if (_bullets.ContainsKey (b.id))
            return;

        var newBullet = Instantiate (bulletPrefab.gameObject);
        var bullet = newBullet.GetComponent<Bullet> ();
        bullet.Set (b.dx, b.dy, b.x, b.y, RemoveBullet, b.id);
        if (_gamers.ContainsKey (b.ownerId)) {
            var owner = _gamers [b.ownerId];
            bullet.SetColor (owner.GetColor());
        } else {
            bullet.SetColor (Color.white);
        }
        bullet.transform.SetParent (gamerParent);
        _bullets.Add (b.id, bullet); 
    }

    //收到服务器的消息
    void OnRecieveMsg(GameMessage basemsg){
        if (basemsg == null)
            return;


        //TODO
        int code =basemsg.stateCode;

        if (code == MsgCode.USER_JOIN) {
            var msg = (GamePlayerMessage)basemsg;
            var user = msg.player;
            Debug.Log ("on user join, id=" + user.id);
            AddInfo (string.Format ("{0} joined game", user.id));
            AddPlayer (user);
            RefreshPlayerText ();
        } else if (code == MsgCode.USER_STATE_UPDATE) {
            var msg = (GamePlayerMessage)basemsg;
            var user = msg.player;
            if (user == null)
                return;
            
            var curId = user.id;
            if (_gamers.ContainsKey (curId)) {
                var gamer = _gamers [curId];
                gamer.SetPosition (user.x, user.y);
                gamer.SetRotation (user.dx, user.dy);
            }

        } else if (code == MsgCode.USER_QUIT) {
            var msg = (GamePlayerMessage)basemsg;
            var user = msg.player;

            var curId = user.id;
            Debug.Log ("on player quit, id=" + user.id);
            AddInfo (string.Format ("{0} quit game", user.id));
            if (_gamers.ContainsKey (curId)) {
                var gamer = _gamers [curId];
                Destroy (gamer.gameObject);
                _gamers.Remove (curId);
                RefreshPlayerText ();
            }
        } else if (code == MsgCode.MOVE_AND_FIRE) {
            var msg = (MoveAndFireMessage)basemsg;
            var user = msg.player;
            var b = msg.bullet;

            var curId = user.id;
            if (_gamers.ContainsKey (curId)) {
                //角色移动
                var gamer = _gamers [curId];
                gamer.SetPosition (user.x, user.y);
                gamer.SetRotation (user.dx, user.dy);

                //生成子弹
                if (b != null && !_bullets.ContainsKey (b.id)) {
                    var newBullet = Instantiate (bulletPrefab.gameObject);
                    var bullet = newBullet.GetComponent<Bullet> ();
                    bullet.Set (user.dx, user.dy, user.x, user.y, RemoveBullet, b.id);
                    bullet.SetColor (GetColor (user));
                    bullet.transform.SetParent (gamerParent);
                    _bullets.Add (b.id, bullet);
                }
            }
        } else if (code == MsgCode.HIT) {
            var msg = (HitMessage)basemsg;
            var target = msg.target;
            var bullet = msg.bullet;

            if (_gamers.ContainsKey (target.id)) {
                var gamer = _gamers [target.id];
                gamer.OnHit ();
                gamer.SetHp (target.hp);
                var bulletId = bullet.id;
                if (_bullets.ContainsKey (bulletId)) {
                    var b = _bullets [bulletId];
                    b.Kill ();
                }
            }
        } else if (code == MsgCode.DIE) {
            var msg = (DieMessage)basemsg;
            var user = msg.player;

            SyncKillAndDead (msg.player);
            SyncKillAndDead (msg.killer);

            AddInfo (string.Format ("{0} has been killed by {1}", user.id, msg.bullet.ownerId));

            if (_gamers.ContainsKey (user.id)) {
                
                var gamer = _gamers [user.id];

                //同步位置，重新出生
                gamer.SetPosition (user.x, user.y);
                gamer.SetRotation (user.dx, user.dy);
                gamer.SetHp (user.hp);
            }

            RefreshPlayerText ();

        } else if (code == MsgCode.SYNC) {

            Debug.Log ("on sync command");
            //全场同步
            var msg = (SyncMessage)basemsg;
            if (msg.players != null) {
                foreach (var p in msg.players) {
                    AddPlayer (p);
                }
            }
            if (msg.bullets != null) {
                foreach (var b in msg.bullets) {
                    AddBullet (b);
                }
            }
            TimeLeft = msg.timeleft;
            RefreshPlayerText ();
            UpdateTimeText ();
        } else if (code == MsgCode.SYNC_TIME) {
            var msg = (TimeMessage)basemsg;
            TimeLeft = msg.timeleft;
            UpdateTimeText ();
        } else if (code == MsgCode.NEW_ROUND) {

            AddInfo ("new round start..");
            //清空所有信息
            foreach (var p in _gamers) {
                if (p.Value != zhujue) {
                    Destroy (p.Value.gameObject);
                }
            }
            _gamers.Clear ();
            foreach (var b in _bullets) {
                Destroy (b.Value.gameObject);
            }
            _bullets.Clear ();

            zhujue.killed = 0;
            zhujue.dead = 0;
        }
    }

    void RemoveBullet(int id){
        if (_bullets.ContainsKey (id)) {
            _bullets.Remove (id);
        }
    }

    void SyncKillAndDead(GamePlayer player){
        if (_gamers.ContainsKey (player.id)) {
            var p = _gamers [player.id];
            p.killed = player.killed;
            p.dead = player.dead;
        }
    }

    void FixedUpdate(){
        //处理消息队列
        lock (zhujue.client.msgQueue) {
            while (zhujue.client.msgQueue.Count > 0) {
                var msg = zhujue.client.msgQueue.Dequeue ();
                OnRecieveMsg (msg);
            }
        }

        Vector2 input = joyStick.JoystickInput;
        float m = input.magnitude;
        if (m > 1) {
            input.Normalize ();
        }

        //本地客户端朝向joystick的方向
        zhujue.SetRotation (input.x, input.y);
        //移动指令
        zhujue.client.Move (input.x * ConstSettings.MoveSpeed, input.y * ConstSettings.MoveSpeed);
        //每一帧自动开枪
        //zhujue.client.Fire ();
    }

    public void Quit(){
        zhujue.client.Quit ();
        Application.Quit ();
    }


    void OnApplicationQuit(){
        zhujue.client.Quit ();
    }

    void RefreshPlayerText(){
        string txt = "";
        foreach (var p in _gamers) {
            txt += string.Format("{0} KILL:<color=yellow>{1}</color> DEAD:<color=red>{2}</color>" , p.Key, p.Value.killed, p.Value.dead) + "\n";
        }

        playerText.text = txt;
    }


    public void AddInfo(string info){
        infoText.text = info + "\n" + infoText.text;
    }

    void UpdateTimeText(){
        timeText.text = "timeleft:" + TimeLeft / 1000;
    }
}
