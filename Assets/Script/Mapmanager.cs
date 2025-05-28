using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("타일 프리팹")]
    public GameObject[] rowPrefabs; // 0: 잔디, 1: 도로, 2: 강

    [Header("장애물 프리팹")]
    public GameObject[] obstaclePrefabs; // 돌1, 돌2, 나무1, 나무2 등

    [Header("맵 설정")]
    public float rowLength = 10f; // z축 한 칸의 길이
    public float obstacleChance = 0.3f; // 한 칸에 장애물 생성 확률 (0~1)

    [Header("플레이어/버퍼")]
    public Transform player;
    public int bufferTiles = 10; // 플레이어 앞에 미리 생성할 타일 개수

    private Queue<GameObject> spawnedRows = new Queue<GameObject>();
    private Dictionary<GameObject, List<GameObject>> rowObstacles = new Dictionary<GameObject, List<GameObject>>(); // 각 타일에 속한 장애물 추적
    private int nextRowZ = 0; // 다음에 생성할 타일의 Z 위치

    // === 추가: 특정 구간 잔디+장애물 없음 ===
    bool IsAlwaysGrassNoObstacle(int rowIndex)
    {
        return (rowIndex >= 20 && rowIndex <= 27) ||
               (rowIndex >= 40 && rowIndex <= 47) ||
               (rowIndex >= 80 && rowIndex <= 87) ||
               (rowIndex >= 100 && rowIndex <= 117);
    }

    void Start()
    {
        // 초기 40칸 생성
        for (int i = 0; i < 40; i++)
        {
            if (i < 10)
                SpawnRow(i, 0); // 처음 10칸은 잔디
            else if (IsAlwaysGrassNoObstacle(i)) // 추가: 특정 구간은 잔디
                SpawnRow(i, 0);
            else
                SpawnRandomRowLimited(i, 0, 1); // 10~39칸: 잔디/도로 랜덤
        }
        nextRowZ = 40;
    }

    void Update()
    {
        int playerZ = Mathf.FloorToInt(player.position.z / rowLength);

        // 플레이어가 bufferTiles 전쯤 오면 맵 생성
        while (playerZ + bufferTiles > nextRowZ)
        {
            // 추가: 특정 구간은 잔디
            if (IsAlwaysGrassNoObstacle(nextRowZ))
                SpawnRow(nextRowZ, 0);
            else if (nextRowZ < 60)
                SpawnRandomRowLimited(nextRowZ, 0, 1); // 잔디/도로
            else if (nextRowZ < 80)
                SpawnRow(nextRowZ, 2); // 강
            else
                SpawnRandomRowLimited(nextRowZ, 0, 1); // 잔디/도로

            nextRowZ++;
        }

        // 지나간 타일 삭제 (플레이어 뒤 5칸 지나면 제거)
        while (spawnedRows.Count > 0)
        {
            GameObject oldestRow = spawnedRows.Peek();
            float oldestZ = oldestRow.transform.position.z;

            if (oldestZ < player.position.z - 5 * rowLength)
            {
                // 해당 타일에 속한 모든 장애물 제거
                if (rowObstacles.ContainsKey(oldestRow))
                {
                    foreach (GameObject obstacle in rowObstacles[oldestRow])
                    {
                        if (obstacle != null)
                            Destroy(obstacle);
                    }
                    rowObstacles.Remove(oldestRow);
                }

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

    // 첫 8칸만 장애물 생성 안함 (특정 구간 조건 제거)
    if (z >= 8)
    {
        SpawnObstacles(row, type, z);
    }
}


    void SpawnRandomRowLimited(int z, int type1, int type2)
    {
        float rand = Random.value;
        int type = (rand < 0.5f) ? type1 : type2;
        SpawnRow(z, type);
    }

    // 장애물 생성 (특정 구간 제외)
    void SpawnObstacles(GameObject row, int rowType, int rowIndex)
{
    if (rowType != 0) return; // 잔디 타일만 장애물 생성

    float z = rowIndex * rowLength;
    int minX = -120;
    int maxX = 120;
    int step = 10;
    int obstacleCount = (maxX - minX) / step + 1;

    if (!rowObstacles.ContainsKey(row))
    {
        rowObstacles[row] = new List<GameObject>();
    }

    HashSet<float> usedXPositions = new HashSet<float>();

    for (int i = 0; i < obstacleCount; i++)
    {
        float x = minX + i * step;

        // 특정 구간(20~27, 40~47, 80~87, 100~117)에서 x=-70 ~ -30인 경우 스킵
        if (IsAlwaysGrassNoObstacle(rowIndex) && x >= -70 && x <= -30)
            continue;

        if (Random.value < obstacleChance && !usedXPositions.Contains(x))
        {
            usedXPositions.Add(x);
            Vector3 pos = new Vector3(x, 0.5f, z);
            int obstacleIndex = Random.Range(0, obstaclePrefabs.Length);
            GameObject obstacle = Instantiate(obstaclePrefabs[obstacleIndex], pos, Quaternion.identity);
            rowObstacles[row].Add(obstacle);
        }
    }
}

}
