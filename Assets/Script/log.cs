using UnityEngine;

// 통나무 오브젝트의 이동을 제어하는 스크립트
public class Log : MonoBehaviour
{
    // 이동 방향 (1: 오른쪽, -1: 왼쪽)
    private int direction = 1;

    // 이동 속도
    private float speed = 15f;

    // 이동 방향 설정 (외부에서 호출)
    public void SetDirection(int dir)
    {
        direction = dir;
    }

    // 이동 속도 설정 (외부에서 호출)
    public void SetSpeed(float spd)
    {
        speed = spd;
    }

    void Update()
    {
        // 월드 좌표 기준으로 일정 속도로 이동
        transform.Translate(Vector3.right * direction * speed * Time.deltaTime, Space.World);
    }
}
