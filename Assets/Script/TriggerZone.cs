using UnityEngine;

public class TriggerZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            // 부모(boo)의 Controller 스크립트에서 물에 빠짐 처리 호출
            Controller controller = GetComponentInParent<Controller>();
            if (controller != null)
            {
                controller.FallIntoWater();
            }
        }
    }
}
