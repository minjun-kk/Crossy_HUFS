using System.Collections;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public float moveDistance = 2f;         // 한 번 이동 시 이동 거리
    public float jumpHeight = 1f;           // 점프 최고 높이
    public float jumpDuration = 0.3f;       // 점프 소요 시간
    
    [Header("스쿼시 앤 스트레치 효과")]
    public Transform spriteTransform;       // 스케일을 변경할 스프라이트 트랜스폼
    public Vector3 squashScale = new Vector3(1.2f, 0.8f, 1f); // 압축 시 스케일 (X축 늘어나고, Y축 줄어듦)
    public float squashDuration = 0.1f;     // 압축/복원 소요 시간

    public LayerMask obstacleLayer;         // 충돌 체크용 레이어 (장애물에 할당)
    public Animator animator;               // Animator 컴포넌트 (점프 애니메이션용)

    private bool isJumping = false;
    private Vector3 originalScale;          // 원래 스케일
    private Vector3 currentMoveDir;         // 현재 이동 방향
    private Quaternion currentTargetRot;    // 현재 회전 방향
    private bool isSquashed = false;        // 압축 상태 여부
    private Coroutine currentSquashCoroutine; // 현재 실행 중인 압축 코루틴
    
    // 이동 방향 벡터 (월드 좌표계 기준)
    private readonly Vector3 FORWARD = new Vector3(0, 0, 1);
    private readonly Vector3 BACKWARD = new Vector3(0, 0, -1);
    private readonly Vector3 LEFT = new Vector3(-1, 0, 0);
    private readonly Vector3 RIGHT = new Vector3(1, 0, 0);

    void Start()
    {
        // 스프라이트 트랜스폼이 지정되지 않았다면 현재 오브젝트 사용
        if (spriteTransform == null)
            spriteTransform = transform;
            
        // 원래 스케일 저장
        originalScale = spriteTransform.localScale;
    }

    void Update()
    {
        if (isJumping) return; // 점프 중에는 입력 무시

        // 이동키를 눌렀을 때 압축 효과 시작 (GetKeyDown 사용)
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

        // 이동키를 뗐을 때 압축 효과 해제 및 이동 시작 (GetKeyUp 사용)
        if (Input.GetKeyUp(KeyCode.W) && isSquashed)
            ApplySquashEffect(false, () => TryMove(currentMoveDir, currentTargetRot));
        else if (Input.GetKeyUp(KeyCode.S) && isSquashed)
            ApplySquashEffect(false, () => TryMove(currentMoveDir, currentTargetRot));
        else if (Input.GetKeyUp(KeyCode.A) && isSquashed)
            ApplySquashEffect(false, () => TryMove(currentMoveDir, currentTargetRot));
        else if (Input.GetKeyUp(KeyCode.D) && isSquashed)
            ApplySquashEffect(false, () => TryMove(currentMoveDir, currentTargetRot));
    }

    // 압축 효과 적용 메서드
    void ApplySquashEffect(bool squash, System.Action onComplete = null)
    {
        // 이미 실행 중인 코루틴이 있으면 중지
        if (currentSquashCoroutine != null)
            StopCoroutine(currentSquashCoroutine);
        
        // 새 코루틴 시작
        currentSquashCoroutine = StartCoroutine(SquashEffect(squash, onComplete));
    }

    // 압축 효과 코루틴
    IEnumerator SquashEffect(bool squash, System.Action onComplete = null)
    {
        Vector3 targetScale = squash ? squashScale : originalScale;
        Vector3 startScale = spriteTransform.localScale;
        
        float timer = 0f;
        
        while (timer < squashDuration)
        {
            timer += Time.deltaTime;
            float t = timer / squashDuration;
            
            // 부드러운 보간
            spriteTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            
            yield return null;
        }
        
        // 최종 스케일 적용
        spriteTransform.localScale = targetScale;
        isSquashed = squash;
        currentSquashCoroutine = null;
        
        // 완료 시 실행할 콜백이 있으면 실행
        if (onComplete != null)
            onComplete();
    }

    // 이동 시도: 충돌 검사 후 이동 코루틴 실행
    void TryMove(Vector3 moveDir, Quaternion targetRot)
    {
        // 이동 방향에 장애물이 있는지 Raycast로 확인
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, moveDir, moveDistance, obstacleLayer))
        {
            // 장애물과 충돌 시 이동하지 않음 (아무 효과 없음)
            return;
        }

        // 이동 가능하면 코루틴 실행
        StartCoroutine(RotateThenMove(moveDir, targetRot));
    }

    // 회전 후 점프 이동
    IEnumerator RotateThenMove(Vector3 moveDir, Quaternion targetRot)
    {
        isJumping = true;

        // 즉시 회전
        transform.rotation = targetRot;

        // 이동 벡터 계산
        Vector3 scaledDirection = moveDir * moveDistance;

        // 점프 애니메이션 트리거 (Animator가 있으면)
        if (animator != null)
            animator.SetTrigger("Jump");

        // 점프 이동 코루틴 실행
        yield return StartCoroutine(MoveWithJump(scaledDirection));

        isJumping = false;
    }

    // 점프 궤적으로 이동 (상승-하강)
    IEnumerator MoveWithJump(Vector3 direction)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + direction;

        float timer = 0f;

        while (timer < jumpDuration)
        {
            float t = timer / jumpDuration;

            // 포물선 궤적 공식: h = 4 * jumpHeight * t * (1 - t)
            float height = 4 * jumpHeight * t * (1 - t);

            // 선형 이동 + 포물선 높이
            transform.position = Vector3.Lerp(startPos, endPos, t) + Vector3.up * height;

            timer += Time.deltaTime;
            yield return null;
        }

        // 최종 위치 보정
        transform.position = endPos;
    }
}
