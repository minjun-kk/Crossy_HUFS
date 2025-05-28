using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSpawnerOnRoad : MonoBehaviour
{
    public GameObject carPrefab;
    public float spawnYOffset = 0.5f;

    // 내부 Car 클래스
    public class Car : MonoBehaviour
    {
        private Vector3 direction;
        private float speed;
        private float lifetime = 10f;
        private float timer = 0f;
        private int road;

        public void SetDirection(Vector3 dir)
        {
            direction = dir;
        }

        public void SetSpeed(float spd)
        {
            speed = spd;
        }

        void Update()
        {
            transform.Translate(direction * speed * Time.deltaTime);
            timer += Time.deltaTime;
        }

        public bool IsExpired()
        {
            return timer > lifetime;
        }

        public void SetRoad(int r)
        {
            road = r;
        }

        public int GetRoad()
        {
            return road;
        }
    }

    // CarData는 Car 오브젝트 관리용 데이터 클래스
    private class CarData
    {
        public Car carComponent;
        public float spawnTime;
        public GameObject road;

        public bool IsExpired(float currentTime, float lifetime)
        {
            return currentTime - spawnTime >= lifetime;
        }
    }

    private List<CarData> activeCars = new List<CarData>();
    private List<GameObject> roadObjects = new List<GameObject>();

    private Dictionary<GameObject, float> lastSpawnTime = new Dictionary<GameObject, float>();
    private Dictionary<GameObject, float> spawnIntervals = new Dictionary<GameObject, float>();
    private Dictionary<float, int> spawnedSidePerRoadZ = new Dictionary<float, int>();

    private float roadUpdateInterval = 1f;
    private float lastRoadUpdateTime = 0f;
    private float carLifetime = 10f;

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

        UpdateCars();
        UpdateSpawning();
    }

    void UpdateRoadObjects()
    {
        for (int i = roadObjects.Count - 1; i >= 0; i--)
        {
            GameObject road = roadObjects[i];
            if (road == null || !road.activeInHierarchy)
            {
                roadObjects.RemoveAt(i);
                lastSpawnTime.Remove(road);
                spawnIntervals.Remove(road);
            }
        }

        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (var obj in allObjects)
        {
            if (obj != null &&
                obj.activeInHierarchy &&
                obj.name.ToLower().Contains("road") &&
                !roadObjects.Contains(obj))
            {
                roadObjects.Add(obj);
                lastSpawnTime[obj] = Time.time - 100f;
                spawnIntervals[obj] = Random.Range(0.1f, 0.5f);
            }
        }
    }

    void UpdateCars()
    {
        float now = Time.time;

        for (int i = activeCars.Count - 1; i >= 0; i--)
        {
            CarData carData = activeCars[i];

            if (carData.carComponent == null)
            {
                activeCars.RemoveAt(i);
                continue;
            }

            if (carData.IsExpired(now, carLifetime) || carData.carComponent.IsExpired())
            {
                Destroy(carData.carComponent.gameObject);
                activeCars.RemoveAt(i);
            }
        }
    }

    void UpdateSpawning()
    {
        int carsSpawnedThisFrame = 0;
        int maxCarsPerFrame = 1;

        foreach (GameObject road in roadObjects)
        {
            if (carsSpawnedThisFrame >= maxCarsPerFrame) break;
            if (road == null || !road.activeInHierarchy) continue;

            if (!lastSpawnTime.ContainsKey(road)) lastSpawnTime[road] = Time.time;
            if (!spawnIntervals.ContainsKey(road)) spawnIntervals[road] = Random.Range(4f, 8f);

            if (Time.time - lastSpawnTime[road] < spawnIntervals[road]) continue;

            float roadY = road.transform.position.y;
            float roadZ = road.transform.position.z;

            int spawnDirection = (Random.value > 0.5f) ? 1 : -1;

            if (spawnedSidePerRoadZ.ContainsKey(roadZ) && spawnedSidePerRoadZ[roadZ] == -spawnDirection)
                continue;

            float spawnX = (spawnDirection == 1) ? -100f : 100f;
            float spawnZ = roadZ + (spawnDirection == -1 ? 2f : -2f);

            Vector3 spawnPos = new Vector3(spawnX, roadY + spawnYOffset, spawnZ);
            GameObject newCarObject = Instantiate(carPrefab, spawnPos, Quaternion.identity);

            Car newCar = newCarObject.AddComponent<Car>();

            Vector3 moveDirection;

            if (spawnDirection == 1)
            {
                newCarObject.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                moveDirection = Vector3.right * spawnDirection * -1;
            }
            else
            {
                moveDirection = Vector3.right * spawnDirection;
            }

            newCar.SetDirection(moveDirection);
            newCar.SetSpeed(20f);
            newCar.SetRoad((int)roadZ);

            CarData carData = new CarData
            {
                carComponent = newCar,
                spawnTime = Time.time,
                road = road
            };

            activeCars.Add(carData);
            carsSpawnedThisFrame++;

            lastSpawnTime[road] = Time.time;
            spawnIntervals[road] = Random.Range(2f, 6f);
            spawnedSidePerRoadZ[roadZ] = spawnDirection;
        }
    }
}
