﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Clusternoid;
using System;

public class Pistol : Weapon
{
    public override void Fire()
    {
        //TODO: 부하 감소를 위해 IEnumerator으로 구현하기
        //TODO: 무기 종류&탄약 종류에 따라 다양한 총알 발사하기
        var player = IsPlayer();
        if (player && isEmittingSound)
        {
            firingPosition.gameObject.GetComponent<SoundPlayer>().Play(SoundType.Weapon_Single_Fire);
        }
        else if (!player)
        {
            firingPosition.gameObject.GetComponent<SoundPlayer>().Play(SoundType.Enemy_Turret_Fire);
        }

        var spreadAngle = Clusternoid.Math.NextGaussian(0, spread, -45, 45);

        var bullet = player ? BulletPool.Get("bullet") : BulletPool.Get("longbullet");
        bullet.Transform.position = firingPosition.position;
        bullet.Transform.rotation = firingPosition.rotation;
        bullet.Transform.Rotate(new Vector3(0, 0, spreadAngle));
        bullet.Initialize(gameObject.tag, bulletSpeed, damage);
    }
}