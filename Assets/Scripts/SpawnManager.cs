using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager instance;

    [SerializeField] Transform[] spawnpointsT1, spawnpointsT2;

    private void Awake() {
        instance = this; 
    }

    public Transform GetSpawnpoint(int team) {
        if (team == 0) return spawnpointsT1[Random.Range(0, spawnpointsT1.Length)]; 
        else return spawnpointsT2[Random.Range(0, spawnpointsT2.Length)]; 
    }
}
