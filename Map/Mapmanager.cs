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

    void Start()
    {
        // 초기 40칸 생성
        for (int i = 0; i < 40; i++)
        {
            if (i < 5)
                SpawnRow(i, 0); // 초기 5칸은 잔디
            else
                SpawnRandomRowLimited(i, 0, 1); // 5~39칸: 잔디/도로 랜덤
        }
        nextRowZ = 40;
    }

    void Update()
    {
        int playerZ = Mathf.FloorToInt(player.position.z / rowLength);

        // 플레이어가 10칸 전쯤 오면 맵 생성
        while (playerZ + bufferTiles > nextRowZ)
        {
            if (nextRowZ < 40)
                SpawnRandomRowLimited(nextRowZ, 0, 1); // 잔디/도로
            else if (nextRowZ < 60)
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

        // 첫 두 줄(z=0,1)에는 장애물 생성 안함
        if (z >= 2)
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

    // 장애물 생성 (x: -120 ~ 120, 10단위)
    void SpawnObstacles(GameObject row, int rowType, int rowIndex)
    {
        if (rowType != 0) return; // 잔디(0)에서만 장애물

        float z = rowIndex * rowLength;
        int minX = -120;
        int maxX = 120;
        int step = 10;
        int obstacleCount = (maxX - minX) / step + 1;

        // 이 타일에 속한 장애물 리스트 초기화
        if (!rowObstacles.ContainsKey(row))
        {
            rowObstacles[row] = new List<GameObject>();
        }

        for (int i = 0; i < obstacleCount; i++)
        {
            if (Random.value < obstacleChance)
            {
                float x = minX + i * step; // -120, -110, ..., 0, ..., 110, 120
                Vector3 pos = new Vector3(x, 0.5f, z);
                int obstacleIndex = Random.Range(0, obstaclePrefabs.Length);
                
                // 장애물을 타일의 자식이 아닌 독립 오브젝트로 생성
                GameObject obstacle = Instantiate(obstaclePrefabs[obstacleIndex], pos, Quaternion.identity);
                
                // 장애물을 이 타일에 속한 것으로 추적
                rowObstacles[row].Add(obstacle);
            }
        }
    }
}
