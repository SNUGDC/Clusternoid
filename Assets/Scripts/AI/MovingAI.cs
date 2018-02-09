﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AI))]
public class MovingAI : MonoBehaviour
{
    [HideInInspector]
    public Animator ani;
    [HideInInspector]
    public Weapon wb;
    [HideInInspector]
    public AI ai;
    [HideInInspector]
    public Rigidbody2D rb;
    [HideInInspector]
    public RaycastHit2D hit;

    public float alertDistance = 8f;
    public float attackDistance = 5f;

    [HideInInspector]
    public bool targetInRange;
    [HideInInspector]
    public bool attack;
    [HideInInspector]
    public bool superArmor;
    [HideInInspector]
    public Vector2 path;

    void Start()
    {
        ani = GetComponentInChildren<Animator>();
        wb = GetComponent<Weapon>();
        ai = GetComponent<AI>();
        rb = GetComponent<Rigidbody2D>();
        targetInRange = false;
        attack = false;
        superArmor = false;
        path = Vector2.zero;
    }

    void Update()
    {
        CheckAlert();
        CheckObstacle();
    }

    void CheckAlert()
    {
        var playerInAlertRange = Physics2D.OverlapCircle(transform.position, alertDistance, 1 << LayerMask.NameToLayer("Player"));
        if (playerInAlertRange != null)
        {
            targetInRange = true;
        }
        else
        {
            targetInRange = false;
        }
        ani.SetBool("targetInRange", targetInRange);
    }

    void CheckObstacle()
    {
        ai.FindNearestCharacter();
        var marginVector = (ai.nearestCharacter.transform.position - transform.position).normalized;

        // "Trigger" 레이어만 빼고 모두 충돌하는 linecast를 하고, 처음 충돌하는 것을 hit에 담는다.
        hit = Physics2D.Linecast(transform.position + marginVector,
            ai.nearestCharacter.transform.position,
            ~(1 << LayerMask.NameToLayer("Trigger")));


        if (!hit.collider.CompareTag("Player"))
        {
            ani.SetBool("obstacle", true);
        }
        else
        {
            ani.SetBool("obstacle", false);
        }
    }
}