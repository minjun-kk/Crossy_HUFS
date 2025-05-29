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
    private Dictionary<GameObject, bool> riverDirections = new Dictionary<GameObject, bool>();
    public float destroyXLimit = 120f;

    void Start() => UpdateRiverObjects();

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

                // 강 인덱스 기반 방향 설정
                bool direction = (riverObjects.Count - 1) % 2 == 0;
                riverDirections[river] = direction;

                nextSpawnTime[river] = Time.time + Random.Range(1f, 5f);
            }
        }
    }

    void UpdateSpawning()
    {
        activeLogs.RemoveAll(log => log == null);

        foreach (GameObject river in riverObjects)
        {
            if (!nextSpawnTime.ContainsKey(river) || Time.time < nextSpawnTime[river])
                continue;

            bool moveRight = riverDirections[river];
            float spawnX = moveRight ? 100f : -100f;
            Vector3 spawnPos = new Vector3(spawnX, river.transform.position.y + spawnYOffset, river.transform.position.z);

            // 통나무 생성 및 설정
            GameObject newLog = Instantiate(
                logPrefabs[Random.Range(0, logPrefabs.Length)],
                spawnPos,
                moveRight ? Quaternion.identity : Quaternion.Euler(0f, 180f, 0f)
            );

            Log logScript = newLog.GetComponent<Log>();
            if (logScript != null)
            {
                logScript.SetDirection(moveRight ? -1 : 1);
                logScript.SetSpeed(15f);
            }

            activeLogs.Add(newLog);
            nextSpawnTime[river] = Time.time + Random.Range(1.5f, 4f);
        }
    }

    void CleanupLogs()
    {
        for (int i = activeLogs.Count - 1; i >= 0; i--)
        {
            GameObject log = activeLogs[i];
            if (log == null || Mathf.Abs(log.transform.position.x) > destroyXLimit)
            {
                if (log != null) Destroy(log);
                activeLogs.RemoveAt(i);
            }
        }
    }
}
