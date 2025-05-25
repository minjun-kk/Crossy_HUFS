using UnityEngine;

public class Car : MonoBehaviour
{
    public float speed = 5f;
    public float lifetime = 10f;
    private float timer = 0f;
    private int direction = 1;

    private GameObject road;  // 차량이 생성된 도로 정보 저장용

    public void SetDirection(int dir)
    {
        direction = dir;
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    void Update()
    {
        transform.Translate(Vector3.right * direction * speed * Time.deltaTime);
        timer += Time.deltaTime;
        // lifetime 경과해도 직접 파괴하지 않음
    }

    public bool IsExpired()
    {
        return timer >= lifetime;
    }

    public void SetRoad(GameObject roadObj)
    {
        road = roadObj;
    }

    public GameObject GetRoad()
    {
        return road;
    }

}