using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using DG.Tweening;
using ShootGameServer.SharedData;

public class Bullet : MonoBehaviour {

    public bool Active = true;

    public Sprite bombSprite;
    public SpriteRenderer sprite;

    Vector2 direction;

    Action<int> __removeCallback;
    int __id;
    public void Set(float dx,float dy,float x,float y,Action<int> removeCallback,int id){

        transform.localPosition = new Vector3 (x, y, 0);
        __id = id;
        __removeCallback = removeCallback;
        direction = new Vector2 (dx, dy);
        direction.Normalize ();
        direction = direction * ConstSettings.BulletSpeed;

        Active = true;
    }

    public void SetColor(Color c){
        sprite.color = c;
    }
        
	void FixedUpdate () {
        if (!Active)
            return;
        
        var pos = transform.localPosition;
        transform.localPosition = new Vector3 (pos.x + direction.x, pos.y + direction.y);

        //出界了，自我销毁
        if (IsOutOfMap ()) {
            Kill ();
        }
	}

    //自我销毁
    public void Kill(){

        Active = false;
        sprite.color = Color.white;
        sprite.sprite = bombSprite;
        sprite.transform.DOShakeScale (0.5f, 100);
        __removeCallback (__id);
        Invoke ("DoKill", 0.5f);
    }

    void DoKill(){
        Destroy (this.gameObject);
    }

    bool IsOutOfMap(){
        var pos = transform.localPosition;
        return (pos.x < ConstSettings.MinX || pos.x > ConstSettings.MaxX || pos.y < ConstSettings.MinY || pos.y > ConstSettings.MaxY);
    }
}
