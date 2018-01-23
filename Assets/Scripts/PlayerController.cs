﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Math = Clusternoid.Math;

public class PlayerController : MonoBehaviour
{
    public List<Character> characters; // 플레이어가 조종하는 복제인간들이 들어있는 리스트
    public static PlayerController groupCenter; // 바로 이거.
    public GameObject characterModel; // 복제할 붕어빵
    public float maxDistance; // 인싸와 아싸를 결정하는 붕어빵 사이의 기본 거리

    public Character leader; // 중력의 중심점이 될 캐릭터;
    [NonSerialized] public Vector2 input;

    Plane xyPlane;
    int floorMask; // A layer mask so that a ray can be cast just at gameobjects on the floor layer.
    float camRayLength = 1000f; // The length of the ray from the camera into the scene.
    Transform target;
    HashSet<Tuple<Character, Character>> charPairs;


    // Use this for initialization
    void Awake()
    {
        charPairs = new HashSet<Tuple<Character, Character>>();
        var targetGO = new GameObject("PathFinder Target");
        target = targetGO.transform;
        target.SetParent(transform);
        groupCenter = this;
        xyPlane = new Plane(Vector3.forward, Vector3.zero);
        // Create a layer mask for the floor layer.
        floorMask = LayerMask.GetMask("Floor");

        // Set up references.
        // anim = GetComponent<Animator>();
        GetComponent<Rigidbody2D>();
        leader = AddCharacter();
        StartCoroutine(nameof(DoInsiderCheck));
    }

    void Start()
    {
        PathFinder.instance.target = target;
    }

    Vector2 CenterOfGravity()
    {
        if (characters.Count == 0)
        {
            return new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);
        }
        var insiderCharacters = characters.Where(character => character.GetComponent<Character>().isInsider)
            .ToList();
        return new Vector2(
            insiderCharacters.Select(character => character.transform.position.x).Average(),
            insiderCharacters.Select(character => character.transform.position.y).Average()
        );
    }

    // Update is called once per frame
    void Update()
    {
        // Position `groupCenter` at the average position of the insider characters.
        var centerOfGravity = CenterOfGravity();
        transform.position = (centerOfGravity * 2 + (Vector2) leader.transform.position) / 3;
        target.position = leader.transform.position;

        if (Input.GetKeyDown(KeyCode.E))
            AddCharacter();
        else if (Input.GetKeyDown(KeyCode.Q))
            RemoveLastCharacter();

        if (Input.GetButtonDown("Fire1"))
            Attack();
    }

    void FixedUpdate()
    {
        // Turn the player to face the mouse cursor.
        Turning();
        input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (characters.Count == 0) return;
        InsiderCheck();
        AddRepulsions();
    }

    void Attack()
    {
        foreach (var item in characters)
        {
            //각 item의 characterManager의 Attack()을 호출하면
            //Attack()은 각 캐릭터마다 가지고 있는 무기로 공격을 한다
            //sendmessage()가 더 나으려나()
            item.SendMessage("Attack");
        }
    }

    void Turning()
    {
        // Create a ray from the mouse cursor on screen in the direction of the camera.
        var camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        float distance;
        if (xyPlane.Raycast(camRay, out distance))
        {
            transform.rotation = Math.RotationAngle(transform.position, camRay.GetPoint(distance));
        }
    }

    Character AddCharacter()
    {
        // Place the character slightly next to groupCenter.
        var position = Math.RandomOffsetPosition(transform.position, 0.1f);
        return AddCharacter(position);
        //TryMovingCharacters();
        //instantiate(투명하게)
        //add to characters
        //anim
    }

    void AddRepulsions()
    {
        foreach (var pair in charPairs)
        {
            var t1 = pair.Item1.transform.position;
            var t2 = pair.Item2.transform.position;
            if (!pair.Item1.IsRepulsing(pair.Item2)) continue;
            var dist = Vector2.Distance(t1, t2);
            var repulsion = (1 - dist / pair.Item1.repulsionCollisionRadius) * pair.Item1.repulsionIntensity;
            pair.Item1.repulsion += (Vector2) (t1 - t2).normalized * repulsion;
            pair.Item2.repulsion += (Vector2) (t2 - t1).normalized * repulsion;
        }
    }

    IEnumerable<Tuple<Character, Character>> GetPairs()
        => characters.Select((character, index) => new {character, index})
            .SelectMany(x => characters.Skip(x.index + 1),
                (x, y) => new Tuple<Character, Character>(x.character, y));

    public Character AddCharacter(Vector3 position)
    {
        var newCharacter = Instantiate(characterModel, position, transform.rotation).GetComponent<Character>();
        characters.Add(newCharacter);
        charPairs.Clear();
        charPairs.UnionWith(GetPairs());
        return newCharacter;
    }

    void ResetCenterOfGravityCharacter()
    {
        if (input.magnitude > 0.5f)
            leader = characters.Where(c => c.isInsider && IsInRange(c, leader))
                .OrderByDescending(character => Vector2.Dot(character.transform.position, input))
                .First();
        else
        {
            leader = characters.Where(c => c.isInsider).OrderBy(character =>
                    Vector2.Distance(character.transform.position, CenterOfGravity()))
                .First();
        }
    }


    public void RemoveCharacter(Character character)
    {
        if (characters.Count > 1 && leader.Equals(character))
        {
            characters.Remove(character);
            ResetCenterOfGravityCharacter();
        }
        else
        {
            characters.Remove(character);
        }
        character.SendMessage("KillCharacter");
        charPairs.RemoveWhere(p => p.Item1 == character || p.Item2 == character);
    }

    void RemoveLastCharacter()
    {
        if (characters.Any())
        {
            RemoveCharacter(characters.Last());
        }
    }

    /// <summary>
    /// insider인지 체크하는 함수
    /// </summary>
    /// 0. 모두 (isInsider =  false)
    /// 0.1. isCenterOfGravity == true인 item부터 시작한다. item.isInsider = true;
    /// 1. item과 insiderDistance 이내인 친구들을 모두 선택(콜라이더 이용)
    /// 2. 그 친구들에 대해 모두 isInsider = true;
    /// 3. 재귀적으로 그 친구들에게 InsiderCheck() 수행
    /// 4. 더 이상 방문할 친구들이 없으면 끝
    /// 코루틴으로 빼도록 하자.
    IEnumerator DoInsiderCheck()
    {
        while (characters.Count > 0)
        {
            ResetCenterOfGravityCharacter();
            yield return new WaitForFixedUpdate();
        }
        StopCoroutine(nameof(DoInsiderCheck));
    }

    void InsiderCheck()
    {
        foreach (var item in characters)
        {
            item.isInsider = false;
        }
        var first = leader;
        InsiderCheckRecursive(first, characters);
    }

    void InsiderCheckRecursive(Character vertex, List<Character> list)
    {
        vertex.isInsider = true;
        foreach (var item in list)
        {
            if (!item.isInsider
                && Vector3.Distance(vertex.transform.position, item.transform.position) < maxDistance)
            {
                InsiderCheckRecursive(item, list);
            }
        }
    }

    public Character FindNearestCharacter(Vector3 from)
    {
        Character nearest = null;
        var distance = Vector3.Distance(transform.position, from);
        foreach (var ch in characters)
        {
            var curr = Vector3.Distance(ch.transform.position, from);
            if (curr > distance) continue;
            nearest = ch;
            distance = curr;
        }
        return nearest;
    }

    public float FindNearestDistance(Vector3 from)
        => characters.Min(ch => Vector3.Distance(ch.transform.position, from));

    bool IsInRange(Character one, Character other)
        => Vector2.Distance(one.transform.position, other.transform.position) < maxDistance;
}