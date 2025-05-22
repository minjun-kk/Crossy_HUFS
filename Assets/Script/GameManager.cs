using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject gameOverUI;
    public GameObject startUI;
    public RectTransform startImage;
    public GameObject titleImage;
    public Button restartButton;

    public Image fingerImage;
    public Sprite fingerUpSprite;
    public Sprite fingerDownSprite;
    public float blinkInterval = 0.5f;

    private bool gameStarted = false;
    private bool gameOver = false;
    private bool isBlinking = true;

    public RectTransform[] blueBars; 

    void Start()
    {   
        if (gameOverUI != null)
          gameOverUI.SetActive(false);
        if (restartButton != null)
        {restartButton.gameObject.SetActive(false); 
         restartButton.onClick.RemoveAllListeners(); 
         restartButton.onClick.AddListener(()=>{
            Debug.Log("재시작 버튼 눌림");
            RestartGame();
            });
        }
    
    StartCoroutine(BlinkFinger());}

    void Update()
    {
        if (!gameStarted && Input.anyKeyDown)
            StartGame();

        if (gameStarted && !gameOver && Input.GetKeyDown(KeyCode.K))
            GameOver();

    }

    void StartGame()
    {
        gameStarted = true;
        isBlinking = false;

        Debug.Log("게임 시작!");

        Vector2 targetPos = new Vector2(1000f, -300f);
        float duration = 2f;

        startImage
            .DOAnchorPos(targetPos, duration)
            .SetEase(Ease.InExpo)
            .OnComplete(() => startUI.SetActive(false));

        // 여기에 게임 실행 로직 추가.
    }

    public void GameOver()
    {
        gameOver = true;
        Debug.Log("게임 오버!");

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
            Debug.Log("UI 활성화됨");

             float baseDelay = 0f;
             float delayStep = 0.2f;

        foreach (RectTransform bar in blueBars)
        {
            // 초기 위치 설정 (왼쪽 바깥)
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
            Debug.Log("gameOverUI 연결 안됨!");
        }
    }

    void RestartGame()
{
    Debug.Log("게임 재시작");

    gameOver = false;
    gameStarted = false;
    isBlinking = true;


    gameOverUI.SetActive(false);
    restartButton.gameObject.SetActive(false);

    //타이틀은 숨기고 손가락만 보이게
    if (titleImage != null)
        titleImage.SetActive(false);

    startUI.SetActive(true);
    StartCoroutine(BlinkFinger());
}

    System.Collections.IEnumerator BlinkFinger()
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
