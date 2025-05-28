using UnityEngine;

public class Log : MonoBehaviour
{
    public Transform character;

    private int direction = 1;  // 1: 오른쪽, -1: 왼쪽 (월드 기준)
    private float speed = 15f;

    public void SetDirection(int dir)
    {
        direction = dir;
    }

    public void SetSpeed(float spd)
    {
        speed = spd;
    }

    void Update()
    {
        // 월드 좌표계 기준으로 이동
        transform.Translate(Vector3.right * direction * speed * Time.deltaTime, Space.World);
    }
}
