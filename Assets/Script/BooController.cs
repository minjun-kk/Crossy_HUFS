using System.Collections;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public float moveDistance = 2f;
    public float jumpHeight = 1f;
    public float jumpDuration = 0.1f;

    [Header("스쿼시 앤 스트레치 효과")]
    public Transform spriteTransform;
    public Vector3 squashScale = new Vector3(1.2f, 0.8f, 1f);
    public float squashDuration = 0.05f;

    public LayerMask obstacleLayer;
    public Animator animator;

    private bool isJumping = false;
    private Vector3 originalScale;
    private Vector3 currentMoveDir;
    private Quaternion currentTargetRot;
    private bool isSquashed = false;
    private Coroutine currentSquashCoroutine;

    private bool isGameOver = false;
    [HideInInspector] public Transform currentLog = null;
    [HideInInspector] public Vector3 lastLogPosition;

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
        SetupChildTriggerZones();
    }

    void Update()
    {
        if (isJumping || isGameOver) return;

        HandleLogMovement();

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

    // 자식 트리거 존 자동 설정
    void SetupChildTriggerZones()
    {
        Collider[] childColliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in childColliders)
        {
            if (col.isTrigger && col.gameObject != this.gameObject)
            {
                TriggerForwarder forwarder = col.gameObject.GetComponent<TriggerForwarder>();
                if (forwarder == null)
                {
                    forwarder = col.gameObject.AddComponent<TriggerForwarder>();
                }
                forwarder.parentController = this;
            }
        }
    }

    // 통나무와 함께 이동하는 로직
    void HandleLogMovement()
    {
        if (currentLog != null && !isJumping)
        {
            Vector3 logMovement = currentLog.position - lastLogPosition;
            transform.position += logMovement;
            lastLogPosition = currentLog.position;
        }
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
        if (currentLog != null)
        {
            currentLog = null;
        }

        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, moveDir, moveDistance, obstacleLayer))
        {
            StartCoroutine(RotateOnly(targetRot));
            return;
        }
        StartCoroutine(RotateThenMove(moveDir, targetRot));
    }

    IEnumerator RotateOnly(Quaternion targetRot)
    {
        isJumping = true;
        transform.rotation = targetRot;
        yield return new WaitForSeconds(0.1f);
        isJumping = false;
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

    // === 충돌 처리 ===
    private void OnCollisionEnter(Collision collision)
    {
        if (isGameOver) return;

        if (collision.gameObject.CompareTag("Log"))
        {
            currentLog = collision.transform;
            lastLogPosition = currentLog.position;
            Debug.Log("통나무에 올라탔습니다!");
        }
        else if (collision.gameObject.CompareTag("Vehicle"))
        {
            StartCoroutine(GetSquashed());
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (isGameOver) return;

        if (collision.gameObject.CompareTag("Log") && collision.transform == currentLog)
        {
            currentLog = null;
            Debug.Log("통나무에서 내렸습니다!");
        }
    }

    // === 트리거 처리 (자식에서 호출됨) ===
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

    public void OnChildTriggerExit(Collider other, Collider childTrigger)
    {
        if (isGameOver) return;
        // 필요시 트리거 종료 처리 로직 추가
    }

    // === 게임오버 처리 ===
    public void FallIntoWater()
    {
        if (isGameOver) return;
        StartCoroutine(FallIntoWaterCoroutine());
    }

    IEnumerator FallIntoWaterCoroutine()
    {
        isGameOver = true;
        isJumping = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        float sinkDuration = 0.5f;
        float timer = 0f;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.down * 10f;

        while (timer < sinkDuration)
        {
            timer += Time.deltaTime;
            float t = timer / sinkDuration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        // GameManager의 GameOver 호출
        if (GameManager.Instance != null)
            GameManager.Instance.GameOver();
        GameOver();
    }

    IEnumerator GetSquashed()
    {
        isGameOver = true;
        isJumping = true;

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
        // GameManager의 GameOver 호출
        if (GameManager.Instance != null)
            GameManager.Instance.GameOver();
        GameOver();
    }

    void GameOver()
    {
        Debug.Log("Game Over!");
    }
}

// === 보조 컴포넌트: 트리거 전달자 ===
public class TriggerForwarder : MonoBehaviour
{
    [HideInInspector] public Controller parentController;

    private void OnTriggerEnter(Collider other)
    {
        if (parentController != null)
        {
            parentController.OnChildTriggerEnter(other, GetComponent<Collider>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (parentController != null)
        {
            parentController.OnChildTriggerExit(other, GetComponent<Collider>());
        }
    }
}
