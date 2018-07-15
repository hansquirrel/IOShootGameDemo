using UnityEngine;
using System.Collections;
using ShootGameServer;

public class GameStateGUI : MonoBehaviour
{
    public float fpsMeasuringDelta = 2.0f;

    private float timePassed;
    private int m_FrameCount = 0;
    private float m_FPS = 0.0f;

    public IGameServer server;

    private void Start()
    {
        timePassed = 0.0f;

        guistyle = new GUIStyle();
        guistyle.normal.background = null;    //这是设置背景填充的
        guistyle.normal.textColor = Color.white;
        guistyle.fontSize = 20; 
    }

    private void Update()
    {
        m_FrameCount = m_FrameCount + 1;
        timePassed = timePassed + Time.deltaTime;

        if (timePassed > fpsMeasuringDelta)
        {
            m_FPS = m_FrameCount / timePassed;

            timePassed = 0.0f;
            m_FrameCount = 0;
        }
    }

    GUIStyle guistyle;

    private void OnGUI()
    {
              //当然，这是字体大小

        //居中显示FPS
        //GUI.Label(new Rect((Screen.width/2)-40, 0, 200, 200), "FPS: " + m_FPS + "\nbullets:" + server.GetBulletsCount () + "\nplayers:" + server.GetPlayersCount (), guistyle);
        GUI.Label(new Rect((Screen.width/2)-40, 0, 200, 200), "FPS: " + m_FPS, guistyle);
    }
}