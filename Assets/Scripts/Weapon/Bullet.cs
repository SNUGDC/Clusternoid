﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBullet
{
    bool Active { get; set; }
    BulletTransform Transform { get; }
    void Initialize(string tag, float bulletSpeed, int damage);
    void OnTriggerEnter2D(Collider2D collision);
    LayerMask LayerMask { get; }
}

public class Bullet : IBullet
{
    public string firedFrom; // 누가 쐈는가?
    public int damage = 3;

    public bool Active { get; set; }
    public BulletTransform Transform { get; }

    public Bullet()
    {
        Transform = new BulletTransform();
        LayerMask = LayerMask.GetMask("Destroyable", "Wall", "Default");
    }

    //캐릭터가 쐈으면 캐릭터는 통과, 적에는 히트 판정&대미지.
    //적이 쐈으면 캐릭터는 히트 판정, 적에는 ???(통과? 히트 판정? 대미지까지?)
    // 공통으로 지형지물은 부딛힘(나중에 지형무시 총알이 나올 떄 다시 생각해보자)
    // 언젠가는 enum으로 넘어가야 한다.


    public void Initialize(string tag, float bulletSpeed, int d)
    {
        // 아직은 아무것도 안하지만
        // firedFrom이나 대미지 등을 어딘가에서 불러와서?
        // 결정해준다.
        Transform.velocity = (Transform.up * bulletSpeed);
        firedFrom = tag;
        damage = d;
        Active = true;
    }

    // OnTriggerEnter2D는 Collider2D other가 트리거가 될 때 호출됩니다(2D 물리학에만 해당).
    public void OnTriggerEnter2D(Collider2D collision)
    {
        var otherTag = collision.gameObject.tag;
        // 일단 플레이어만 쏜다고 가정하고 만들어보자. 
        if ((collision.gameObject.CompareTag(firedFrom) || collision.gameObject.CompareTag("bullet"))
            || collision.gameObject.CompareTag("trigger")
            || collision.gameObject.CompareTag("healthPack"))
        {
            // do nothing just pass
            return;
        }
        else
        {
            var attack = new Attack(firedFrom.GetHashCode(), damage, Transform.up, 0, 0);
            var hl = collision.GetComponent<HitListener>();
            hl?.TriggerListener(attack);
            //switch (tag)
            //{
            //    case "Player":
            //        PlayerController.groupCenter.GetComponent<PlayerController>().SendMessage("RemoveCharacter", collision.gameObject);
            //        break;
            //    case "enemy":
            //        Debug.Log("HIT!");
            //        Destroy(collision.gameObject); //TODO: 이것도 관리자를 통하게 해야 함.(GameManager 등)
            //        if (firedFrom.Equals("Player"))
            //        {
            //           PlayerController.groupCenter.SendMessage("AddCharacter");
            //        }
            //        break;
            //    default: break;
            //}
            //and destroy itself
            //with animation hopefully..
            //여기에 충돌 애니메이션을 넣으시오
            Active = false;
        }
    }

    public LayerMask LayerMask { get; }
}