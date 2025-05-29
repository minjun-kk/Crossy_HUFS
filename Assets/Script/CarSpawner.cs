using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSpawnerOnRoad : MonoBehaviour
{
    public GameObject[] carPrefabs; // 자동차 프리팹 배열 (4개 등록 예정)
    public float spawnYOffset = 0.5f; // 도로 위 Y축 오프셋 (자동차가 공중에 떠 있지 않게)
    public float minCarSpacing = 15f; // 같은 도로 위 차량 간 최소 간격

    // 내부 Car 클래스: 자동차 이동, 생존 시간 관리
    public class Car : MonoBehaviour
    {
        private Vector3 direction; // 이동 방향
        private float speed; // 이동 속도
        private float lifetime = 10f; // 최대 생존 시간
        private float timer = 0f; // 생존 시간 카운트
        private int road; // 해당 차가 속한 도로(z 위치)

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
            // 설정된 방향으로 일정 속도로 이동
            transform.Translate(direction * speed * Time.deltaTime, Space.World);
            timer += Time.deltaTime;
        }

        // 생존 시간 초과 여부 확인
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

    // 자동차 오브젝트 관리용 데이터 클래스
    private class CarData
    {
        public Car carComponent; // 자동차 컴포넌트
        public float spawnTime; // 생성 시간
        public GameObject road; // 이 차가 소속된 도로

        public bool IsExpired(float currentTime, float lifetime)
        {
            return currentTime - spawnTime >= lifetime;
        }
    }

    private List<CarData> activeCars = new(); // 현재 존재하는 자동차 목록
    private List<GameObject> roadObjects = new(); // 활성화된 도로 오브젝트 리스트

    // 도로 별로 마지막 스폰 시간 저장
    private Dictionary<GameObject, float> lastSpawnTime = new();
    // 도로 별 스폰 간격 (랜덤)
    private Dictionary<GameObject, float> spawnIntervals = new();
    // 도로별 마지막으로 스폰된 방향(좌/우)
    private Dictionary<float, int> spawnedSidePerRoadZ = new();

    private float roadUpdateInterval = 1f; // 도로 갱신 간격
    private float lastRoadUpdateTime = 0f; // 마지막 도로 갱신 시각
    private float carLifetime = 10f; // 자동차 최대 생존 시간

    void Start()
    {
        UpdateRoadObjects(); // 시작 시 도로 목록 갱신
        UpdateSpawning(); // 즉시 자동차 한 번 생성
    }

    void Update()
    {
        // 일정 간격마다 도로 목록 갱신
        if (Time.time - lastRoadUpdateTime >= roadUpdateInterval)
        {
            UpdateRoadObjects();
            lastRoadUpdateTime = Time.time;
        }

        UpdateCars(); // 만료된 자동차 제거
        UpdateSpawning(); // 자동차 스폰 시도
    }

    // 도로 오브젝트 갱신 (현재 활성화된 도로만 유지)
    void UpdateRoadObjects()
    {
        // 기존 도로 중 비활성화된 도로 제거
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

        // 현재 존재하는 모든 GameObject에서 "road"가 포함된 이름 탐색
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (var obj in allObjects)
        {
            if (obj != null &&
                obj.activeInHierarchy &&
                obj.name.ToLower().Contains("road") &&
                !roadObjects.Contains(obj))
            {
                roadObjects.Add(obj);
                lastSpawnTime[obj] = Time.time - 100f; // 즉시 스폰 가능하도록 초기화
                spawnIntervals[obj] = Random.Range(0.1f, 0.5f); // 초기 스폰 간격 지정
            }
        }
    }

    // 자동차 리스트 갱신 (시간 초과 차량 제거)
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

            // 생존 시간 초과 시 제거
            if (carData.IsExpired(now, carLifetime) || carData.carComponent.IsExpired())
            {
                Destroy(carData.carComponent.gameObject);
                activeCars.RemoveAt(i);
            }
        }
    }

    // 자동차 생성 처리
    void UpdateSpawning()
    {
        int carsSpawnedThisFrame = 0;
        int maxCarsPerFrame = 1; // 프레임당 최대 1대 생성

        foreach (GameObject road in roadObjects)
        {
            if (carsSpawnedThisFrame >= maxCarsPerFrame) break;
            if (road == null || !road.activeInHierarchy) continue;

            if (!lastSpawnTime.ContainsKey(road)) lastSpawnTime[road] = Time.time;
            if (!spawnIntervals.ContainsKey(road)) spawnIntervals[road] = Random.Range(4f, 8f);

            if (Time.time - lastSpawnTime[road] < spawnIntervals[road]) continue;

            float roadY = road.transform.position.y;
            float roadZ = road.transform.position.z;

            // 스폰 방향 랜덤 결정 (왼쪽 → 오른쪽 또는 그 반대)
            int spawnDirection = (Random.value > 0.5f) ? 1 : -1;

            // 이전 방향과 번갈아가며 생성
            if (spawnedSidePerRoadZ.ContainsKey(roadZ) && spawnedSidePerRoadZ[roadZ] == -spawnDirection)
                continue;

            // 스폰 위치 계산 (도로 z위치, x는 왼/오 스폰 위치)
            float spawnX = (spawnDirection == 1) ? -100f : 100f;
            float spawnZ = roadZ;

            Vector3 spawnPos = new Vector3(spawnX, roadY + spawnYOffset, spawnZ);

            // 차량 간 간격 체크 (같은 도로 상에서만)
            bool isTooClose = false;
            foreach (var carData in activeCars)
            {
                if (carData.road == road)
                {
                    float dist = Mathf.Abs(carData.carComponent.transform.position.x - spawnX);
                    if (dist < minCarSpacing)
                    {
                        isTooClose = true;
                        break;
                    }
                }
            }
            if (isTooClose)
                continue; // 가까우면 스폰하지 않음

            // 프리팹 중 랜덤 선택
            int prefabIndex = Random.Range(0, carPrefabs.Length);
            GameObject selectedPrefab = carPrefabs[prefabIndex];

            // 이동 방향 및 회전 각도 설정
            float yRotation;
            Vector3 moveDirection;

            if (spawnDirection == 1) // 오른쪽(x+)
            {
                yRotation = 90f;
                moveDirection = Vector3.right;
            }
            else // 왼쪽(x-)
            {
                yRotation = -90f;
                moveDirection = Vector3.left;
            }

            // 자동차 생성 및 회전 적용
            GameObject newCarObject = Instantiate(selectedPrefab, spawnPos, Quaternion.Euler(0f, yRotation, 0f));

            // Car 컴포넌트 부착 및 설정
            Car newCar = newCarObject.AddComponent<Car>();
            newCar.SetDirection(moveDirection);
            newCar.SetSpeed(20f); // 고정 속도
            newCar.SetRoad((int)roadZ);

            // 자동차 데이터 등록
            CarData carDataNew = new CarData
            {
                carComponent = newCar,
                spawnTime = Time.time,
                road = road
            };

            activeCars.Add(carDataNew);
            carsSpawnedThisFrame++;

            // 스폰 타이밍 및 방향 정보 갱신
            lastSpawnTime[road] = Time.time;
            spawnIntervals[road] = Random.Range(2f, 6f); // 다음 스폰까지 간격
            spawnedSidePerRoadZ[roadZ] = spawnDirection; // 마지막 방향 기록
        }
    }
}
