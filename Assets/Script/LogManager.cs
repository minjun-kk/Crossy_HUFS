using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogSpawnerOnRiver : MonoBehaviour
{
    public GameObject[] logPrefabs;
    public float spawnYOffset = 0.5f;

    private List<GameObject> riverObjects = new List<GameObject>();
    private HashSet<GameObject> registeredRivers = new HashSet<GameObject>();

    private List<GameObject> activeLogs = new List<GameObject>();

    private Dictionary<GameObject, float> nextSpawnTime = new Dictionary<GameObject, float>();
    private Dictionary<GameObject, int> riverDirections = new Dictionary<GameObject, int>();

    public float destroyXLimit = 120f;

    void Start()
    {
        UpdateRiverObjects();
        SetInitialDirections(); // ★ 원하는 방향 지정
    }

    void Update()
    {
        UpdateRiverObjects();
        UpdateSpawning();
        CleanupLogs();
    }

    void UpdateRiverObjects()
    {
        GameObject[] rivers = GameObject.FindGameObjectsWithTag("Water");
        foreach (var river in rivers)
        {
            if (!registeredRivers.Contains(river))
            {
                riverObjects.Add(river);
                registeredRivers.Add(river);

                // 임시로 1로 넣어두고, 아래 SetInitialDirections에서 덮어씀
                riverDirections[river] = 1;

                nextSpawnTime[river] = Time.time + Random.Range(1f, 6f);
            }
        }
    }

    // ★ 각 강의 초기 방향을 원하는 대로 지정
    void SetInitialDirections()
    {
        for (int i = 0; i < riverObjects.Count; i++)
        {
            GameObject river = riverObjects[i];
            int direction;
            if (i % 2 == 0)
                direction = 1;    // 1번, 3번, 5번...: 오른쪽(왼쪽에서 오른쪽)
            else
                direction = -1;   // 2번, 4번...: 왼쪽(오른쪽에서 왼쪽)
            riverDirections[river] = direction;
        }
    }

    void UpdateSpawning()
    {
        activeLogs.RemoveAll(log => log == null);

        foreach (GameObject river in riverObjects)
        {
            if (!nextSpawnTime.ContainsKey(river)) continue;
            if (Time.time < nextSpawnTime[river]) continue;

            // 이미 이 river(z값)에 통나무가 떠 있으면 생성하지 않음
            bool hasLogOnThisRiver = false;
            foreach (GameObject log in activeLogs)
            {
                if (log == null) continue;
                if (Mathf.Abs(log.transform.position.z - river.transform.position.z) < 0.5f)
                {
                    hasLogOnThisRiver = true;
                    break;
                }
            }
            if (hasLogOnThisRiver) continue;

            int moveDir = riverDirections.ContainsKey(river) ? riverDirections[river] : 1;

            float spawnX = (moveDir == 1) ? -100f : 100f;
            float spawnY = river.transform.position.y + spawnYOffset;
            float spawnZ = river.transform.position.z;

            Vector3 spawnPos = new Vector3(spawnX, spawnY, spawnZ);
            Quaternion rotation = (moveDir == 1) ? Quaternion.identity : Quaternion.Euler(0f, 180f, 0f);

            GameObject randomLogPrefab = logPrefabs[Random.Range(0, logPrefabs.Length)];
            GameObject newLog = Instantiate(randomLogPrefab, spawnPos, rotation);
            activeLogs.Add(newLog);

            Log logScript = newLog.GetComponent<Log>();
            if (logScript != null)
            {
                logScript.SetDirection(moveDir);
                logScript.SetSpeed(15f);

                GameObject player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    logScript.character = player.transform;
                }
            }
            else
            {
                Debug.LogWarning("Log prefab에 Log 스크립트가 연결되지 않았습니다.");
            }

            // 다음엔 반대 방향으로!
            riverDirections[river] = -moveDir;

            nextSpawnTime[river] = Time.time + Random.Range(2f, 6f);
        }
    }

    void CleanupLogs()
    {
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
