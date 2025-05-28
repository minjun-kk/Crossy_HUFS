`using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // 따라갈 캐릭터
    public Vector3 offset = new Vector3(10, 25, -25); // 대각선 위에서 바라보는 오프셋
    public float smoothSpeed = 0.15f; // 부드러운 이동 속도

    private void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("카메라가 따라갈 대상이 없습니다!");
            return;
        }

        // 목표 위치 계산 (플레이어 위치 + 오프셋)
        Vector3 desiredPosition = target.position + offset;

        // 부드럽게 이동
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // 카메라 각도는 항상 일정 (Crossy Road는 약 45도~60도 정도)
        // X축을 45도로 기울여서 플레이어를 약간 위에서 바라보게 함
        transform.rotation = Quaternion.Euler(45, -15, 0);
    }
}
