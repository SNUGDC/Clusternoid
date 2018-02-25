﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class WeaponAdder : MonoBehaviour {


    int random;

	// Use this for initialization
	void OnEnable() {
        random = Random.Range(0, 3);
        switch (random)
        {
            case 0:
                gameObject.AddComponent<Pistol>();
                GetComponent<Pistol>().preShoot = Clusternoid.Math.NextGaussian(80, 1, 50, 100);
                break;

            case 1:
                gameObject.AddComponent<Shotgun>();
                GetComponent<Shotgun>().preShoot = Clusternoid.Math.NextGaussian(3, 1, 1.5f, 4.5f);
                GetComponent<Shotgun>().angle = 15;
                break;

            case 2:
                gameObject.AddComponent<Burst>();
                GetComponent<Burst>().preShoot = Clusternoid.Math.NextGaussian(3, 1, 1.5f, 4.5f);
                GetComponent<Burst>().burst = 3;
                GetComponent<Burst>().delay = 0.3f;
                break;

            default:
                gameObject.AddComponent<Pistol>();
                GetComponent<Pistol>().preShoot = 100f;
                Debug.LogError("WeaponAdder Random value error, random: " + random);
                break;
        }
        var weapon = GetComponent<Weapon>();
        weapon.firingPosition = transform.Find("Firing Position");
        if (weapon.firingPosition == null)
        {
            Debug.LogError("Firing Position not found in this object.");
        }
        weapon.damage = 1;
        weapon.bulletSpeed = 30;
        weapon.spread = 0.5f;
        
    }

    private void OnDisable()
    {
        Destroy(GetComponent<Weapon>());
    }
}