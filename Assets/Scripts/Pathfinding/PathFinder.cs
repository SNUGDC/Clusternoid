﻿using System.Collections;
using UnityEngine;

public class PathFinder : Singleton<PathFinder>
{
    public Texture2D map;
    public Transform target;
    Color[] resultPixels;
    bool calculated;

    IEnumerator Start()
    {
        if (map == null) yield break;
//        map.Resize(map.width * 4, map.height * 4);
        var center = new Vector2(map.width / 2f, map.height / 2f);
        resultPixels = new Color[map.width * map.height];
        while (target == null) yield return null;
        while (true)
        {
            var targetPosition =
                new Vector2Int(Mathf.FloorToInt(target.position.x + center.x),
                    Mathf.RoundToInt(target.position.y + center.y));
            yield return
                StartCoroutine(DistanceMapCalculator.CalculateFlowMap(map, null, targetPosition, resultPixels));
            calculated = true;
        }
    }

    public static Vector2 GetAcceleration(Vector2 position)
    {
        if (!instance.calculated) return Vector2.zero;
        var center = new Vector2(instance.map.width / 2f, instance.map.height / 2f);
        var localPos = instance.transform.InverseTransformPoint(position);
        var x = Mathf.RoundToInt(localPos.x + center.x);
        var y = Mathf.RoundToInt(localPos.y + center.y);
        var color = instance.resultPixels[x + y * instance.map.width];
        return -ColorToAccel(color).normalized;
    }

    static Vector2 ColorToAccel(Color sample)
        => new Vector2(sample.b - sample.g, 1 - 2 * sample.r);
}