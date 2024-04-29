using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawnpoints : MonoBehaviour
{
    public static Spawnpoints Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public Transform[] spawnPoints;

    public Vector3 getRandomPoint()
    {
        return spawnPoints[Random.Range(0, spawnPoints.Length)].position;
    }
}
