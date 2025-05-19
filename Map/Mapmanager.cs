using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public GameObject[] rowPrefabs; // 0: 잔디, 1: 도로, 2: 강
    public float rowLength = 10f;
    public Transform player;
    public int bufferTiles = 10; // 플레이어 앞에 미리 생성할 타일 개수

    private Queue<GameObject> spawnedRows = new Queue<GameObject>();
    private int nextRowZ = 0; // 다음에 생성할 타일의 Z 위치

    void Start()
    {
        // 초기 40칸 생성
        for (int i = 0; i < 40; i++)
        {
            if (i < 5)
                SpawnRow(i, 0); // 처음 5칸은 잔디
            else
                SpawnRandomRowLimited(i, 0, 1); // 5~39칸은 잔디/도로 랜덤
        }
        nextRowZ = 40; // 다음 생성할 Z 위치
    }

    void Update()
    {
        int playerZ = Mathf.FloorToInt(player.position.z / rowLength);

        // 플레이어가 10칸 전쯤 오면 맵 생성
        while (playerZ + bufferTiles > nextRowZ)
        {
            if (nextRowZ < 40)
                SpawnRandomRowLimited(nextRowZ, 0, 1); // 잔디/도로 랜덤
            else if (nextRowZ < 60)
                SpawnRow(nextRowZ, 2); // 40~59칸은 강 고정
            else
                SpawnRandomRowLimited(nextRowZ, 0, 1); // 60칸 이후 잔디/도로 랜덤

            nextRowZ++;
        }

        // 지나간 타일 삭제 (플레이어 뒤 5칸 지나면 제거)
        while (spawnedRows.Count > 0)
        {
            GameObject oldestRow = spawnedRows.Peek();
            float oldestZ = oldestRow.transform.position.z;

            if (oldestZ < player.position.z - 5 * rowLength)
            {
                Destroy(oldestRow);
                spawnedRows.Dequeue();
            }
            else
            {
                break;
            }
        }
    }

    void SpawnRow(int z, int type)
    {
        GameObject row = Instantiate(
            rowPrefabs[type],
            new Vector3(0, 0, z * rowLength),
            Quaternion.identity,
            transform
        );
        spawnedRows.Enqueue(row);
    }

    void SpawnRandomRowLimited(int z, int type1, int type2)
    {
        float rand = Random.value;
        int type;
        if (rand < 0.5f)
            type = type1;
        else
            type = type2;

        SpawnRow(z, type);
    }
}
