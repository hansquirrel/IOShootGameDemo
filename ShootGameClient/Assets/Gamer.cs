using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ShootGameServer;
using DG.Tweening;

public class Gamer : MonoBehaviour {

    [HideInInspector]
    public int killed;

    [HideInInspector]
    public int dead;

    public RoomClientSimulator client;

    public Text nameText;
    public SpriteRenderer sprite;

    string __id;
    int __hp;

    public void SetName(string id){
        __id = id;
        UpdateText ();
    }

    public void SetHp(int hp){
        __hp = hp;
        UpdateText ();
    }

    void UpdateText(){
		nameText.text = __hp.ToString () + "\n" + __id;
    }

    public void SetPosition(float x,float y){
        this.transform.localPosition = new Vector3 (x, y, 0);
    }

    public void SetRotation(float dx,float dy){
        sprite.transform.localRotation = Quaternion.FromToRotation (Vector3.up, new Vector3 (dx, dy, 0));
    }

    public void SetColor(Color c){
        sprite.color = c;
        nameText.color = c;
    }

    public Color GetColor(){
        return sprite.color;
    }

    public void OnHit(){
        Debug.Log ("on hit! id =" + __id);
    }
}
