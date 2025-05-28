using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI")]
    public GameObject gameOverUI;
    public GameObject startUI;
    public RectTransform startImage;
    public GameObject titleImage;
    public Button restartButton;

    [Header("손가락 깜빡임")]
    public Image fingerImage;
    public Sprite fingerUpSprite;
    public Sprite fingerDownSprite;
    public float blinkInterval = 0.5f;

    [Header("블루바 애니메이션")]
    public RectTransform[] blueBars;

    private bool gameStarted = false;
    private bool gameOver = false;
    private bool isBlinking = true;
    private Coroutine blinkCoroutine;

    void Awake()
    {
        // 싱글톤 패턴
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (gameOverUI != null)
            gameOverUI.SetActive(false);

        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(false);
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartGame);
        }

        if (startUI != null)
            startUI.SetActive(true);

        if (titleImage != null)
            titleImage.SetActive(true);

        if (fingerImage != null && fingerDownSprite != null)
        {
            blinkCoroutine = StartCoroutine(BlinkFinger());
        }
    }

    void Update()
    {
        if (!gameStarted && Input.anyKeyDown)
            StartGame();

        // 테스트용: K키로 강제 게임오버
        if (gameStarted && !gameOver && Input.GetKeyDown(KeyCode.K))
            GameOver();
    }

    void StartGame()
    {
        gameStarted = true;
        isBlinking = false;

        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        Debug.Log("게임 시작!");

        Vector2 targetPos = new Vector2(1000f, -300f);
        float duration = 2f;

        if (startImage != null && startUI != null)
        {
            startImage
                .DOAnchorPos(targetPos, duration)
                .SetEase(Ease.InExpo)
                .OnComplete(() => startUI.SetActive(false));
        }

        if (titleImage != null)
            titleImage.SetActive(false);

        // 게임 실행 로직 추가 가능
    }

    public void GameOver()
    {
        if (gameOver) return;
        gameOver = true;
        Debug.Log("게임 오버!");

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
            Debug.Log("UI 활성화됨");

            float baseDelay = 0f;
            float delayStep = 0.2f;

            // 블루바 애니메이션
            foreach (RectTransform bar in blueBars)
            {
                Vector2 startPos = bar.anchoredPosition + new Vector2(-800f, 0);
                bar.anchoredPosition = startPos;

                bar.DOAnchorPosX(bar.anchoredPosition.x + 800f, 0.6f)
                    .SetEase(Ease.OutBack)
                    .SetDelay(baseDelay);

                baseDelay += delayStep;
            }

            if (restartButton != null)
            {
                restartButton.gameObject.SetActive(true);
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(RestartGame);
            }
        }
        else
        {
            Debug.Log("gameOverUI 연결 안 됨!");
        }
    }

    void RestartGame()
    {
        Debug.Log("게임 재시작");

        // 씬 리로드로 완전 초기화
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        // 아래 코드는 씬 리로드가 아니라면 사용
        /*
        gameOver = false;
        gameStarted = false;
        isBlinking = true;

        gameOverUI.SetActive(false);
        restartButton.gameObject.SetActive(false);

        if (titleImage != null)
            titleImage.SetActive(false);

        startUI.SetActive(true);
        if (fingerImage != null && fingerDownSprite != null)
            blinkCoroutine = StartCoroutine(BlinkFinger());
        */
    }

    IEnumerator BlinkFinger()
    {
        while (isBlinking)
        {
            fingerImage.sprite = fingerDownSprite;
            yield return new WaitForSeconds(blinkInterval);
            fingerImage.sprite = fingerUpSprite;
            yield return new WaitForSeconds(blinkInterval);
        }
    }
}
