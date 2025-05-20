using UnityEngine;

public class Car : MonoBehaviour
{
    public float speed = 5f;
    public float lifetime = 10f;
    private float timer = 0f;
    private int direction = 1;

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
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
