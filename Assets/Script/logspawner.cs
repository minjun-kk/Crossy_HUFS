using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogSpawnerOnRiver : MonoBehaviour
{
    public GameObject[] logPrefabs;        // 여러 종류의 통나무 프리팹 배열
    public float spawnInterval = 3f;
    public float spawnYOffset = 0.5f;      // 통나무가 물 위에 떠 있도록

    void Start()
    {
        StartCoroutine(SpawnLogsRoutine());
    }

    IEnumerator SpawnLogsRoutine()
    {
        while (true)
        {
            // 씬에서 이름에 "river"가 포함된 모든 오브젝트 찾기
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            List<GameObject> riverObjects = new List<GameObject>();

            foreach (var obj in allObjects)
            {
                if (obj.name.ToLower().Contains("river"))
                {
                    riverObjects.Add(obj);
                }
            }

            if (riverObjects.Count > 0 && logPrefabs.Length > 0)
            {
                // 랜덤한 river 오브젝트 선택
                GameObject randomRiver = riverObjects[Random.Range(0, riverObjects.Count)];

                // 랜덤한 통나무 프리팹 선택
                GameObject randomLogPrefab = logPrefabs[Random.Range(0, logPrefabs.Length)];

                // 방향 랜덤 결정
                bool moveRight = Random.value > 0.5f;

                // 방향에 따라 X 위치 설정
                float spawnX = moveRight ? -100f : 100f;

                Vector3 spawnPos = new Vector3(spawnX, randomRiver.transform.position.y + spawnYOffset, randomRiver.transform.position.z);

                // 방향에 따른 회전 설정
                Quaternion spawnRotation = moveRight ? Quaternion.Euler(0f, 180f, 0f) : Quaternion.identity;

                GameObject log = Instantiate(randomLogPrefab, spawnPos, spawnRotation);

                // 이동 방향 및 속도 설정
                Log logScript = log.GetComponent<Log>();
                logScript.SetDirection(moveRight ? -1 : -1); // 방향은 1 또는 -1

                float randomSpeed = Random.Range(10f, 20f);   // 통나무는 느릴 수 있음
                logScript.SetSpeed(randomSpeed);
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
