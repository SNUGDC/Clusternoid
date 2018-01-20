﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AI))]
public class Turret : MonoBehaviour {

    Animator ani;
    Weapon wb;
    AI ai;
    LineRenderer line;
    Rigidbody2D rb;
    int rotation;

    public int Rotation
    {
        get
        {
            return rotation;
        }

    }

    private void Start()
    {
        ani = GetComponent<Animator>();
        wb = GetComponent<Weapon>();
        ai = GetComponent<AI>();
        rb = GetComponent<Rigidbody2D>();

        line = wb.firingPosition.GetComponent<LineRenderer>();
        line.positionCount = 2;
        line.startWidth = 0.1f;
        line.endWidth = 0.1f;
        line.enabled = false;
    }

    private void FixedUpdate()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            wb.firingPosition.position,
            transform.up,
            ai.alertDistance
            );
        if (hit.collider.CompareTag("Player"))
        {
            ani.SetBool("targetFound", true);
            ani.SetBool("fire", true);
            rb.freezeRotation = true;
            line.enabled = true;
            
            line.SetPosition(0, wb.firingPosition.position);
            line.SetPosition(1, hit.point);
        }
        else
        {
            ani.SetBool("targetFound", false);
            ani.SetBool("fire", false);
            rb.freezeRotation = false;
            line.enabled = false;
        }

    }

    // Update is called once per frame
    void Update () {
        //GetComponent<Weapon>().SendMessage("TryToFire");

	}

    public void ChooseRotation()
    {
        rotation = 2 * UnityEngine.Random.Range(0, 2) - 1;// -1 or 1
    }
}
