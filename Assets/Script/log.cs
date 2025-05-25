using UnityEngine;

public class Log : MonoBehaviour
{
    public float speed = 10f;
    private int direction = 1; // 1이면 오른쪽, -1이면 왼쪽

    public Transform character;  // 캐릭터 Transform, LogSpawner에서 할당

    public float followThresholdY = 0.5f;  // 캐릭터와 통나무 Y 위치 차이 임계값
    public float followThresholdXZ = 1f;   // 캐릭터와 통나무 XZ 평면 거리 임계값

    void Update()
    {
        MoveLog();

        if (character != null && IsCharacterOnLog())
        {
            MoveCharacterWithLog();
        }
    }

    void MoveLog()
    {
        transform.Translate(Vector3.right * speed * direction * Time.deltaTime);
    }

    void MoveCharacterWithLog()
    {
        Vector3 moveDelta = Vector3.right * speed * direction * Time.deltaTime;
        character.position += moveDelta;
    }

    // 캐릭터가 통나무 위에 있다고 판단하는 조건 직접 체크
    bool IsCharacterOnLog()
    {
        // Y 차이가 일정 이하이고
        float yDiff = Mathf.Abs(character.position.y - transform.position.y);
        if (yDiff > followThresholdY)
            return false;

        // XZ 평면 거리도 일정 이하이면
        Vector2 logXZ = new Vector2(transform.position.x, transform.position.z);
        Vector2 charXZ = new Vector2(character.position.x, character.position.z);
        float distXZ = Vector2.Distance(logXZ, charXZ);

        if (distXZ > followThresholdXZ)
            return false;

        return true;
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    public void SetDirection(int dir)
    {
        direction = dir;
    }
}
