using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSpawnerOnRoad : MonoBehaviour
{
    public GameObject carPrefab;
    public float spawnYOffset = 0.5f;

    private List<GameObject> activeCars = new List<GameObject>();
    private List<GameObject> roadObjects = new List<GameObject>();

    private Dictionary<GameObject, float> lastSpawnTime = new Dictionary<GameObject, float>();
    private Dictionary<GameObject, float> spawnIntervals = new Dictionary<GameObject, float>();

    private Dictionary<float, int> spawnedSidePerRoadZ = new Dictionary<float, int>();
    // z좌표 -> 최근 소환된 x방향 (1: 오른쪽에서 왼쪽, -1: 왼쪽에서 오른쪽)

    private float roadUpdateInterval = 1f;
    private float lastRoadUpdateTime = 0f;

    void Start()
    {
        UpdateRoadObjects();
        UpdateSpawning();
    }

    void Update()
    {
        if (Time.time - lastRoadUpdateTime >= roadUpdateInterval)
        {
            UpdateRoadObjects();
            lastRoadUpdateTime = Time.time;
        }

        UpdateSpawning();
    }

    void UpdateRoadObjects()
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (var obj in allObjects)
        {
            if (obj != null &&
                obj.name.ToLower().Contains("road") &&
                !roadObjects.Contains(obj))
            {
                roadObjects.Add(obj);
                lastSpawnTime[obj] = Time.time - 100f;
                spawnIntervals[obj] = Random.Range(0.1f, 0.5f);
            }
        }
    }

    void UpdateSpawning()
    {
        activeCars.RemoveAll(car => car == null);

        for (int i = activeCars.Count - 1; i >= 0; i--)
        {
            Car carScript = activeCars[i].GetComponent<Car>();
            if (carScript != null && carScript.IsExpired())
            {
                Destroy(activeCars[i]);
                activeCars.RemoveAt(i);
            }
        }

        int carsSpawnedThisFrame = 0;
        int maxCarsPerFrame = 1;

        foreach (GameObject road in roadObjects)
        {
            if (carsSpawnedThisFrame >= maxCarsPerFrame) break;
            if (!lastSpawnTime.ContainsKey(road)) lastSpawnTime[road] = Time.time;
            if (!spawnIntervals.ContainsKey(road)) spawnIntervals[road] = Random.Range(4f, 8f);

            if (Time.time - lastSpawnTime[road] < spawnIntervals[road]) continue;

            float roadY = road.transform.position.y;
            float roadZ = road.transform.position.z;

            int spawnDirection = (Random.value > 0.5f) ? 1 : -1;

            // 이 도로의 z좌표에서 반대 방향으로 이미 소환된 적이 있다면 skip
            if (spawnedSidePerRoadZ.ContainsKey(roadZ) && spawnedSidePerRoadZ[roadZ] == -spawnDirection)
                continue;

            float spawnX = (spawnDirection == 1) ? -100f : 100f;
            float spawnZ = roadZ + -5f;

            // 왼쪽에서 오른쪽으로 이동하는 차량이면 z값 조정
            if (spawnDirection == -1)
            {
                spawnZ += 10f;
            }

            Vector3 spawnPos = new Vector3(
                spawnX,
                roadY + spawnYOffset,
                spawnZ
            );

            GameObject newCar = Instantiate(carPrefab, spawnPos, Quaternion.identity);

            bool isRotated = false;
            // 오른쪽에서 왼쪽으로 이동하는 차량은 회전
            if (spawnDirection == 1)
            {
                newCar.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                isRotated = true;
            }

            activeCars.Add(newCar);
            carsSpawnedThisFrame++;

            Car carScriptNew = newCar.GetComponent<Car>();
            if (carScriptNew != null)
            {
                if (isRotated)
                    carScriptNew.SetDirection(-spawnDirection);
                else
                    carScriptNew.SetDirection(spawnDirection);

                carScriptNew.SetSpeed(20f);
            }
            else
            {
                Debug.LogWarning("Car prefab에 Car 스크립트가 연결되지 않았습니다.");
            }

            lastSpawnTime[road] = Time.time;
            spawnIntervals[road] = Random.Range(2f, 6f);
            spawnedSidePerRoadZ[roadZ] = spawnDirection;
        }
    }
}
