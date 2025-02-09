﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Laser : Weapon
{
    [Range(0, 10)] public float radius = 1;
    public RaycastHit2D[] hits;
    private bool _isActive;
    private LineRenderer _line;
    public float duration = 1.5f;

    void Start()
    {
        if (firingPosition == null)
        {
            firingPosition = transform.Find("Firing Position");
        }
        _line = GetComponent<LineRenderer>();
        _line.positionCount = 2;
        _line.startWidth = radius;
        _line.endWidth = radius;
        _line.enabled = false;
        
    }

    public override void Fire()
    {
        _isActive = true;

        //firingPosition.gameObject.GetComponent<SoundPlayer>().Play();
        Invoke(nameof(StopFiring), duration);
    }

    private void StopFiring()
    {
        _isActive = false;
    }

    public void Update()
    {
        if (_isActive)
        {
            var check = Physics2D.Raycast(firingPosition.transform.position,
                transform.up,
                30,
                LayerMask.GetMask("Wall", "Default"));
            float distance = 30;
            if (check.collider != null)
            {
                distance = check.distance;
            }

            _line.SetPosition(0, firingPosition.position);
            _line.SetPosition(1, firingPosition.position  + firingPosition.up * distance);
            _line.enabled = true;
            hits = Physics2D.CircleCastAll(
                firingPosition.transform.position + firingPosition.up * radius,
                radius,
                transform.up,
                distance,
                LayerMask.GetMask("Destroyable", "Wall", "Default"));
            var hitCount = hits.Length;

            for (var i = 0; i < hitCount; i++)
            {
                var hit = hits[i];
                var attack = new Attack(tag.GetHashCode(), damage, firingPosition.rotation.eulerAngles, 0, 0);
                var hl = hit.collider.GetComponent<HitListener>();
                hl?.TriggerListener(attack);
            }
            
        }
        else
        {
            _line.enabled = false;
        }
    }

}
