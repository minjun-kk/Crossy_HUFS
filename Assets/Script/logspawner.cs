using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogSpawnerOnRiver : MonoBehaviour
{
    public GameObject[] logPrefabs;
    public float spawnYOffset = 0.5f;

    // 강 오브젝트 관리
    private List<GameObject> riverObjects = new List<GameObject>();
    private HashSet<GameObject> registeredRivers = new HashSet<GameObject>();

    // 통나무 관리
    private List<GameObject> activeLogs = new List<GameObject>();

    // 강별 다음 스폰 시간 및 방향 저장
    private Dictionary<GameObject, float> nextSpawnTime = new Dictionary<GameObject, float>();
    private Dictionary<GameObject, bool> riverDirections = new Dictionary<GameObject, bool>();

    // 통나무가 씬에서 사라졌을 때 삭제 임계값(X좌표)
    public float destroyXLimit = 120f;

    void Start()
    {
        UpdateRiverObjects();
    }

    void Update()
    {
        UpdateRiverObjects();
        UpdateSpawning();
        CleanupLogs();
    }

    // 씬 내 "River" 태그가 붙은 강 오브젝트 갱신
    void UpdateRiverObjects()
    {
        GameObject[] rivers = GameObject.FindGameObjectsWithTag("River");
        foreach (var river in rivers)
        {
            if (!registeredRivers.Contains(river))
            {
                riverObjects.Add(river);
                registeredRivers.Add(river);

                bool direction = Random.value > 0.5f;
                riverDirections[river] = direction;

                nextSpawnTime[river] = Time.time + Random.Range(1f, 5f);
            }
        }
    }

    void UpdateSpawning()
    {
        // null 참조 통나무 정리
        activeLogs.RemoveAll(log => log == null);

        foreach (GameObject river in riverObjects)
        {
            if (!nextSpawnTime.ContainsKey(river)) continue;
            if (Time.time < nextSpawnTime[river]) continue;

            // 강의 Z축 근처에 이미 통나무가 존재하는지 체크
            bool hasLogOnThisRiver = false;
            foreach (GameObject existingLog in activeLogs)
            {
                if (existingLog == null) continue;

                float zDist = Mathf.Abs(existingLog.transform.position.z - river.transform.position.z);
                if (zDist < 0.5f)
                {
                    hasLogOnThisRiver = true;
                    break;
                }
            }
            if (hasLogOnThisRiver) continue;

            bool moveRight = riverDirections.ContainsKey(river) ? riverDirections[river] : true;

            float spawnX = moveRight ? -100f : 100f;
            float spawnY = river.transform.position.y + spawnYOffset;
            float spawnZ = river.transform.position.z;

            Vector3 spawnPos = new Vector3(spawnX, spawnY, spawnZ);
            Quaternion rotation = moveRight ? Quaternion.identity : Quaternion.Euler(0f, 180f, 0f);

            GameObject randomLogPrefab = logPrefabs[Random.Range(0, logPrefabs.Length)];
            GameObject newLog = Instantiate(randomLogPrefab, spawnPos, rotation);
            activeLogs.Add(newLog);

            Log logScript = newLog.GetComponent<Log>();
            if (logScript != null)
            {
                int dir = moveRight ? 1 : 1;
                logScript.SetDirection(dir);
                logScript.SetSpeed(Random.Range(10f, 20f));
            }
            else
            {
                Debug.LogWarning("Log prefab에 Log 스크립트가 연결되지 않았습니다.");
            }

            nextSpawnTime[river] = Time.time + Random.Range(2f, 6f);
        }
    }

    // 씬 밖으로 나간 통나무 제거 및 리스트 정리
    void CleanupLogs()
    {
        // 뒤에서부터 제거하는게 안전
        for (int i = activeLogs.Count - 1; i >= 0; i--)
        {
            GameObject log = activeLogs[i];
            if (log == null)
            {
                activeLogs.RemoveAt(i);
                continue;
            }

            float posX = log.transform.position.x;
            if (Mathf.Abs(posX) > destroyXLimit)
            {
                Destroy(log);
                activeLogs.RemoveAt(i);
            }
        }
    }
}
