using System.Collections;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public float moveDistance = 2f;         // 한 번 이동 시 이동 거리
    public float jumpHeight = 1f;           // 점프 최고 높이
    public float jumpDuration = 0.1f;       // 점프 소요 시간(0.3 > 0.1)

    [Header("스쿼시 앤 스트레치 효과")]
    public Transform spriteTransform;                           // 스케일을 변경할 스프라이트 트랜스폼
    public Vector3 squashScale = new Vector3(1.2f, 0.8f, 1f);   // 압축 시 스케일
    public float squashDuration = 0.1f;                         // 압축/복원 소요 시간

    public LayerMask obstacleLayer;                             // 충돌 체크용 레이어
    public Animator animator;                                   // Animator 컴포넌트

    private bool isJumping = false;                             // 추가적인 이동이나 점프 방지
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

    void TryMove(Vector3 moveDir, Quaternion targetRot)
    {
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, moveDir, moveDistance, obstacleLayer))
        {
            return;
        }

        StartCoroutine(RotateThenMove(moveDir, targetRot));
    }

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

    // === [여기서부터 게임종료 조건건 추가] ===

    private void OnTriggerEnter(Collider other)
    {
        if (isGameOver) return;

        if (other.CompareTag("Water"))
        {
            StartCoroutine(FallIntoWater());
        }
        else if (other.CompareTag("Vehicle"))
        {
            StartCoroutine(GetSquashed());
        }
    }

    IEnumerator FallIntoWater()
    {
        isGameOver = true;
        isJumping = true;

        // 물에 빠지는 효과과 (아래로 천천히 가라앉음)
        float sinkDuration = 0.8f;
        float timer = 0f;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.down * 2f;

        while (timer < sinkDuration)
        {
            timer += Time.deltaTime;
            float t = timer / sinkDuration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        // 게임오버 처리 (UI 등)
        GameOver();
    }

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
