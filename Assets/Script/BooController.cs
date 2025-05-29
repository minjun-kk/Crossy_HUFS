using System.Collections;
using UnityEngine;

// 캐릭터의 이동, 점프, 통나무 탑승, 트리거 감지 등 전체 제어를 담당
public class Controller : MonoBehaviour
{
    public float moveDistance = 2f;         // 한 번 이동 시 이동 거리
    public float jumpHeight = 1f;           // 점프 최고 높이
    public float jumpDuration = 0.1f;       // 점프 소요 시간(0.3 > 0.1)

    [Header("스쿼시 앤 스트레치 효과")]
    public Transform spriteTransform;                           // 스케일을 변경할 스프라이트 트랜스폼
    public Vector3 squashScale = new Vector3(1.2f, 0.8f, 1f);   // 압축 시 스케일
    public float squashDuration = 0.05f;                        // 압축/복원 소요 시간(0.1 > 0.05)

    public LayerMask obstacleLayer;                             // 충돌 체크용 레이어
    public Animator animator;                                   // Animator 컴포넌트

    private bool isJumping = false;                             // 추가적인 이동이나 점프 방지
    private Vector3 originalScale;
    private Vector3 currentMoveDir;
    private Quaternion currentTargetRot;
    private bool isSquashed = false;
    private Coroutine currentSquashCoroutine;

    private bool isGameOver = false;                        // 게임오버 상태 플래그
    [HideInInspector] public Transform currentLog = null;   // 현재 탑승 중인 통나무 Transform
    [HideInInspector] public Vector3 lastLogPosition;       // 통나무의 이전 위치
    
    // 이동 방향 벡터
    private readonly Vector3 FORWARD = new Vector3(0, 0, 1);
    private readonly Vector3 BACKWARD = new Vector3(0, 0, -1);
    private readonly Vector3 LEFT = new Vector3(-1, 0, 0);
    private readonly Vector3 RIGHT = new Vector3(1, 0, 0);

    void Start()
    {
        if (spriteTransform == null)                 // spriteTransform이 비어 있으면 자기 자신으로 설정
            spriteTransform = transform;
        originalScale = spriteTransform.localScale;   // 기존 스케일 저장
        SetupChildTriggerZones();                     // 트리거 콜라이더 자동 연결
    }

    void Update()
    {
        if (isJumping || isGameOver) return;             // 점프 중이거나 게임오버 시 입력 무시

        HandleLogMovement();                             // 통나무 위에 있으면 통나무 이동량만큼 캐릭터도 이동

        // 방향키 입력 시 이동 방향 및 회전값 설정, 스쿼시 효과 적용
        if (Input.GetKeyDown(KeyCode.W))
        {
            currentMoveDir = FORWARD;
            currentTargetRot = Quaternion.identity;
            ApplySquashEffect(true);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            currentMoveDir = BACKWARD;
            currentTargetRot = Quaternion.Euler(0, 180, 0);
            ApplySquashEffect(true);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            currentMoveDir = LEFT;
            currentTargetRot = Quaternion.Euler(0, -90, 0);
            ApplySquashEffect(true);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            currentMoveDir = RIGHT;
            currentTargetRot = Quaternion.Euler(0, 90, 0);
            ApplySquashEffect(true);
        }

        if (Input.GetKeyUp(KeyCode.W) && isSquashed)
            ApplySquashEffect(false, () => TryMove(currentMoveDir, currentTargetRot));
        else if (Input.GetKeyUp(KeyCode.S) && isSquashed)
            ApplySquashEffect(false, () => TryMove(currentMoveDir, currentTargetRot));
        else if (Input.GetKeyUp(KeyCode.A) && isSquashed)
            ApplySquashEffect(false, () => TryMove(currentMoveDir, currentTargetRot));
        else if (Input.GetKeyUp(KeyCode.D) && isSquashed)
            ApplySquashEffect(false, () => TryMove(currentMoveDir, currentTargetRot));
    }

    // 자식 트리거 콜라이더를 자동으로 찾아서 TriggerForwarder로 연결
    void SetupChildTriggerZones()
    {
        Collider[] childColliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in childColliders)
        {
            // 자기 자신이 아니고 트리거인 콜라이더만
            if (col.isTrigger && col.gameObject != this.gameObject)
            {
                // TriggerForwarder 컴포넌트가 없으면 추가
                TriggerForwarder forwarder = col.gameObject.GetComponent<TriggerForwarder>();
                if (forwarder == null)
                {
                    forwarder = col.gameObject.AddComponent<TriggerForwarder>();
                }
                // 부모 Controller 연결
                forwarder.parentController = this;
            }
        }
    }

    // 통나무 위에 있을 때, 통나무 이동량만큼 캐릭터도 이동
    void HandleLogMovement()
    {
        if (currentLog != null && !isJumping)
        {
            Vector3 logMovement = currentLog.position - lastLogPosition;
            transform.position += logMovement;
            lastLogPosition = currentLog.position;
        }
    }

    // 스쿼시 효과 적용(압축/복원) 및 완료 후 콜백 실행
    void ApplySquashEffect(bool squash, System.Action onComplete = null)
    {
        if (currentSquashCoroutine != null)
            StopCoroutine(currentSquashCoroutine);

        currentSquashCoroutine = StartCoroutine(SquashEffect(squash, onComplete));
    }

    IEnumerator SquashEffect(bool squash, System.Action onComplete = null)
    {
        Vector3 targetScale = squash ? squashScale : originalScale;
        Vector3 startScale = spriteTransform.localScale;

        float timer = 0f;

        while (timer < squashDuration)
        {
            timer += Time.deltaTime;
            float t = timer / squashDuration;
            spriteTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        spriteTransform.localScale = targetScale;
        isSquashed = squash;
        currentSquashCoroutine = null;

        if (onComplete != null)
            onComplete();
    }

    // 이동 시도(장애물 체크, 통나무 탑승 해제)
    void TryMove(Vector3 moveDir, Quaternion targetRot)
    {
        // 통나무에서 내리기
        if (currentLog != null)
        {
            currentLog = null;
        }

        // 장애물 있으면 회전만 하고 이동하지 않음
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, moveDir, moveDistance, obstacleLayer))
        {
            return;
        }

        StartCoroutine(RotateThenMove(moveDir, targetRot));
    }

    // 장애물에 막혔을 때 회전만 수행
    IEnumerator RotateOnly(Quaternion targetRot)
    {
        isJumping = true;
        transform.rotation = targetRot;
        yield return new WaitForSeconds(0.1f);
        isJumping = false;
    }

    // 이동 및 점프 애니메이션 처리
    IEnumerator RotateThenMove(Vector3 moveDir, Quaternion targetRot)
    {
        isJumping = true;
        transform.rotation = targetRot;
        Vector3 scaledDirection = moveDir * moveDistance;

        if (animator != null)
            animator.SetTrigger("Jump");

        yield return StartCoroutine(MoveWithJump(scaledDirection));
        isJumping = false;
    }

    // 포물선 점프 이동 처리
    IEnumerator MoveWithJump(Vector3 direction)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + direction;

        float timer = 0f;

        while (timer < jumpDuration)
        {
            float t = timer / jumpDuration;
            float height = 4 * jumpHeight * t * (1 - t);  // 포물선 공식
            transform.position = Vector3.Lerp(startPos, endPos, t) + Vector3.up * height;

            timer += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
    }

    // 충돌 처리 (통나무, 차량)
    private void OnCollisionEnter(Collision collision)
    {
        if (isGameOver) return;

        if (collision.gameObject.CompareTag("Log"))
        {
            // 통나무에 올라탐
            currentLog = collision.transform;
            lastLogPosition = currentLog.position;
            Debug.Log("통나무에 올라탔습니다!");
        }
        else if (collision.gameObject.CompareTag("Vehicle"))
        {
            // 차량에 부딪히면 게임오버
            StartCoroutine(GetSquashed());
        }
    }

    // 통나무에서 내릴 때 처리
    private void OnCollisionExit(Collision collision)
    {
        if (isGameOver) return;

        if (collision.gameObject.CompareTag("Log") && collision.transform == currentLog)
        {
            currentLog = null;
            Debug.Log("통나무에서 내렸습니다!");
        }
    }

    // 자식 트리거에서 호출되는 트리거 진입 처리
    public void OnChildTriggerEnter(Collider other, Collider childTrigger)
    {
        if (isGameOver) return;

        if (other.CompareTag("Water"))
        {
            Debug.Log($"물 감지: {childTrigger.name} 트리거가 감지함");
            FallIntoWater();
        }
        else if (other.CompareTag("Vehicle"))
        {
            Debug.Log($"차량 감지: {childTrigger.name} 트리거가 감지함");
            StartCoroutine(GetSquashed());
        }
    }

    // 자식 트리거에서 호출되는 트리거 종료 처리(필요시)
    public void OnChildTriggerExit(Collider other, Collider childTrigger)
    {
        if (isGameOver) return;
        // 필요하면 트리거 종료 시 처리 추가
    }

    // 물에 빠질 때 처리
    IEnumerator FallIntoWater()
    {
        isGameOver = true;
        isJumping = true;

        // Rigidbody가 있다면 물리 비활성화
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        // 물에 빠지는 효과 (아래로 천천히 가라앉음)
        float sinkDuration = 0.8f;
        float timer = 0f;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.down * 3f;   // 물에 빠지는 깊이 조절(2 > 3)

        while (timer < sinkDuration)
        {
            timer += Time.deltaTime;
            float t = timer / sinkDuration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        GameOver();
    }

    // 차량에 깔렸을 때 연출 및 게임오버 처리
    IEnumerator GetSquashed()
    {
        isGameOver = true;
        isJumping = true;

        // 납작해지는 효과
        float squashTime = 0.2f;
        Vector3 startScale = spriteTransform.localScale;
        Vector3 endScale = new Vector3(startScale.x * 1.2f, startScale.y * 0.2f, startScale.z);

        float timer = 0f;
        while (timer < squashTime)
        {
            timer += Time.deltaTime;
            float t = timer / squashTime;
            spriteTransform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }
        spriteTransform.localScale = endScale;
        GameOver();
    }

    void GameOver()
    {
        Debug.Log("Game Over!");
    }
}
