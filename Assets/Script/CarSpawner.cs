using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSpawnerOnRoad : MonoBehaviour
{
    public GameObject carPrefab;
    public float spawnInterval = 3f;
    public float spawnYOffset = 0.5f;  // 자동차가 땅에서 조금 떠 있게

    void Start()
    {
        StartCoroutine(SpawnCarsRoutine());
    }

    IEnumerator SpawnCarsRoutine()
    {
        while (true)
        {
            // 씬에 존재하는 모든 오브젝트 중 이름에 "road"가 포함된 것 찾기
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            List<GameObject> roadObjects = new List<GameObject>();

            foreach (var obj in allObjects)
            {
                if (obj.name.ToLower().Contains("road"))
                {
                    roadObjects.Add(obj);
                }
            }

            if (roadObjects.Count > 0)
            {
                // road 중에 랜덤으로 선택
                GameObject randomRoad = roadObjects[Random.Range(0, roadObjects.Count)];

                // 방향 랜덤 결정 (true면 오른쪽, false면 왼쪽)
                bool moveRight = Random.value > 0.5f;

                // 방향에 따른 생성 X 위치 지정 (왼쪽 끝 또는 오른쪽 끝)
                float spawnX = moveRight ? -100f : 100f;

                Vector3 spawnPos = new Vector3(spawnX, randomRoad.transform.position.y + spawnYOffset, randomRoad.transform.position.z);

                // 방향에 따른 회전 설정
                Quaternion spawnRotation = moveRight ? Quaternion.Euler(0f, 180f, 0f) : Quaternion.identity;

                GameObject car = Instantiate(carPrefab, spawnPos, spawnRotation);

                // 생성된 자동차에 방향 및 속도 설정
                Car carScript = car.GetComponent<Car>();
                carScript.SetDirection(moveRight ? -1 : -1);

                float randomSpeed = Random.Range(10f, 30f);
                carScript.SetSpeed(randomSpeed);
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
