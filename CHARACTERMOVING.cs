using System.Collections;
using UnityEngine;

public class PlayerHopper : MonoBehaviour
{
    public float moveDistance = 1f;
    public float jumpHeight = 0.5f;
    public float jumpDuration = 0.25f;

    private bool isJumping = false;
    private Coroutine jumpCoroutine = null;

    void Update()
    {
        if (isJumping) return;

        if (Input.GetKeyDown(KeyCode.W))
            StartJump(Vector3.forward);
        else if (Input.GetKeyDown(KeyCode.S))
            StartJump(Vector3.back);
        else if (Input.GetKeyDown(KeyCode.A))
            StartJump(Vector3.left);
        else if (Input.GetKeyDown(KeyCode.D))
            StartJump(Vector3.right);
    }

    void StartJump(Vector3 direction)
    {
        if (jumpCoroutine != null)
        {
            StopCoroutine(jumpCoroutine);
        }

        jumpCoroutine = StartCoroutine(JumpMove(direction));
    }

    IEnumerator JumpMove(Vector3 direction)
    {
        isJumping = true;

        // 회전 (그냥 시각적인 방향만 바꿔주는 것)
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = targetRotation;

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + direction * moveDistance;

        float timer = 0f;

        while (timer < jumpDuration)
        {
            float t = timer / jumpDuration;
            float height = Mathf.Sin(Mathf.PI * t) * jumpHeight;

            // 항상 월드 기준 방향으로 이동
            Vector3 position = Vector3.Lerp(startPos, endPos, t) + Vector3.up * height;
            transform.position = position;

            timer += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
        isJumping = false;
    }
}
