using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public GameObject[] rowPrefabs; // 0: 잔디, 1: 도로, 2: 강
    public float rowLength = 10f;
    public Transform player;

    private Queue<GameObject> spawnedRows = new Queue<GameObject>();
    private int lastRowZ = 0;

    void Start()
    {
        // 30칸까지 한 번에 미리 생성
        for (int i = 0; i < 40; i++)
        {
            if (i < 8)
                SpawnRow(i, 0); // 0번(잔디)로 고정
            else
                SpawnRandomRow(i);
        }
        lastRowZ = 30;
    }

    void SpawnRow(int z, int type)
    {
        GameObject row = Instantiate(
            rowPrefabs[type],
            new Vector3(0, 0, z * rowLength),
            Quaternion.identity,
            this.transform
        );
        spawnedRows.Enqueue(row);
    }

    void SpawnRandomRow(int z)
    {
        float rand = Random.value;
        int type;
        if (rand < 0.6f)
            type = 0; // 잔디
        else if (rand < 0.85f)
            type = 1; // 도로
        else
            type = 2; // 강

        GameObject row = Instantiate(
            rowPrefabs[type],
            new Vector3(0, 0, z * rowLength),
            Quaternion.identity,
            this.transform
        );
        spawnedRows.Enqueue(row);
    }
}
