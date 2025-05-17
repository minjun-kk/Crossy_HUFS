using System.Collections;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public float moveDistance = 2f;         // 한 번 이동 시 이동 거리
    public float jumpHeight = 1f;           // 점프 최고 높이
    public float jumpDuration = 0.1f;       // 점프 소요 시간 (더 빠르게)

    [Header("스쿼시 앤 스트레치 효과")]
    public Transform spriteTransform;       // 스케일을 변경할 스프라이트 트랜스폼
    public Vector3 squashScale = new Vector3(1.2f, 0.8f, 1f); // 압축 시 스케일
    public float squashDuration = 0.05f;    // 압축/복원 소요 시간 (더 빠르게)

    public LayerMask obstacleLayer;         // 충돌 체크용 레이어
    public Animator animator;               // Animator 컴포넌트

    private bool isJumping = false;
    private Vector3 originalScale;
    private Vector3 currentMoveDir;
    private Quaternion currentTargetRot;
    private bool isSquashed = false;
    private Coroutine currentSquashCoroutine;

    private bool isGameOver = false; // 게임오버 상태 플래그
    
    // 이동 방향 벡터
    private readonly Vector3 FORWARD = new Vector3(0, 0, 1);
    private readonly Vector3 BACKWARD = new Vector3(0, 0, -1);
    private readonly Vector3 LEFT = new Vector3(-1, 0, 0);
    private readonly Vector3 RIGHT = new Vector3(1, 0, 0);

    void Start()
    {
        if (spriteTransform == null)
            spriteTransform = transform;
        originalScale = spriteTransform.localScale;
    }

    void Update()
    {
        if (isJumping || isGameOver) return; // 게임오버 시 입력 무시

        // 이동키를 눌렀을 때 압축 효과 시작
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

        // 이동키를 뗐을 때 압축 효과 해제 및 이동 시작
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
        if (currentSquashCoroutine != null)
            StopCoroutine(currentSquashCoroutine);
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
            spriteTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        spriteTransform.localScale = targetScale;
        isSquashed = squash;
        currentSquashCoroutine = null;
        if (onComplete != null)
            onComplete();
    }

    // 이동 시도: 충돌 검사 후 이동 코루틴 실행
    void TryMove(Vector3 moveDir, Quaternion targetRot)
    {
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, moveDir, moveDistance, obstacleLayer))
        {
            return;
        }
        StartCoroutine(RotateThenMove(moveDir, targetRot));
    }

    // 회전 후 점프 이동
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

    // 점프 궤적으로 이동
    IEnumerator MoveWithJump(Vector3 direction)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + direction;
        float timer = 0f;

        while (timer < jumpDuration)
        {
            float t = timer / jumpDuration;
            float height = 4 * jumpHeight * t * (1 - t);
            transform.position = Vector3.Lerp(startPos, endPos, t) + Vector3.up * height;
            timer += Time.deltaTime;
            yield return null;
        }
        transform.position = endPos;
    }
}

private void OnTriggerEnter(Collider other)
    {
        if (isGameOver) return;
        }

        // 게임오버 처리 (UI 등)
        GameOver();
    }

    void GameOver()
    {
        // 게임오버 처리: 입력 차단, UI 표시 등
        // 예: GameManager.Instance.GameOver();
        // 또는 씬 리로드 등
        Debug.Log("Game Over!");
    }
}
