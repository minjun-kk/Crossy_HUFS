public class LogSpawnerOnRiver : MonoBehaviour
{
    // 생성할 통나무 프리팹들
    public GameObject[] logPrefabs;

    // 생성 위치를 강 표면보다 얼마나 위로 띄울지
    public float spawnYOffset = 0.5f;

    // 현재 등록된 강 오브젝트들
    private List<GameObject> riverObjects = new List<GameObject>();

    // 이미 등록된 강인지 확인하는 해시셋
    private HashSet<GameObject> registeredRivers = new HashSet<GameObject>();

    // 현재 활성화된 통나무 리스트
    private List<GameObject> activeLogs = new List<GameObject>();

    // 강마다 다음 통나무 생성 시간 저장
    private Dictionary<GameObject, float> nextSpawnTime = new Dictionary<GameObject, float>();

    // 강마다 통나무가 움직일 방향 (true = 오른쪽 → 왼쪽)
    private Dictionary<GameObject, bool> riverDirections = new Dictionary<GameObject, bool>();

    // 통나무가 화면을 벗어났다고 판단할 x좌표 경계
    public float destroyXLimit = 120f;

    // 게임 시작 시 한 번 강 오브젝트 갱신
    void Start() => UpdateRiverObjects();

    // 매 프레임마다 강 갱신, 통나무 생성, 통나무 정리
    void Update()
    {
        UpdateRiverObjects();  // 새로운 강 탐색
        UpdateSpawning();      // 통나무 생성
        CleanupLogs();         // 오래된 통나무 제거
    }

    void UpdateRiverObjects()
    {
        GameObject[] rivers = GameObject.FindGameObjectsWithTag("Water");

        foreach (var river in rivers)
        {
            if (!registeredRivers.Contains(river))
            {
                riverObjects.Add(river);                             // 리스트에 추가
                registeredRivers.Add(river);                         // 중복 등록 방지
                riverDirections[river] = (riverObjects.Count - 1) % 2 == 0; // 방향 결정
                nextSpawnTime[river] = Time.time + Random.Range(1f, 5f);   // 생성 시간 초기화
            }
        }
    }

    void UpdateSpawning()
    {
        activeLogs.RemoveAll(log => log == null);  // 삭제된 통나무 정리

        foreach (GameObject river in riverObjects)
        {
            if (!nextSpawnTime.ContainsKey(river) || Time.time < nextSpawnTime[river])
                continue;

            bool moveRight = riverDirections[river];
            float spawnX = moveRight ? 100f : -100f;

            Vector3 spawnPos = new Vector3(
                spawnX,
                river.transform.position.y + spawnYOffset,
                river.transform.position.z
            );

            GameObject newLog = Instantiate(
                logPrefabs[Random.Range(0, logPrefabs.Length)],
                spawnPos,
                moveRight ? Quaternion.identity : Quaternion.Euler(0f, 180f, 0f)
            );

            Log logScript = newLog.GetComponent<Log>();
            if (logScript != null)
            {
                logScript.SetDirection(moveRight ? -1 : 1); // 이동 방향 설정
                logScript.SetSpeed(15f);                    // 속도 설정
            }

            activeLogs.Add(newLog);
            nextSpawnTime[river] = Time.time + Random.Range(1.5f, 4f); // 다음 생성 예약
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
