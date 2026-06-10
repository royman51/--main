using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUIController : MonoBehaviour
{
    public static GameUIController Instance;

    public TMP_Text enemyHealthText;
    public TMP_Text myHealthText;
    public Image heartImage;

    public Image fadeImage;
    public float startFadeInTime = 0.7f;
    public float normalFadeTime = 0.45f;
    public Color fadeColor = Color.black;

    public Camera targetCamera;

    public float cameraColorTweenTime = 0.6f;

    public Color phaseColorOne = new Color32(0x0B, 0x05, 0x1C, 0x00);
    public Color phaseColorTwo = new Color32(0x7E, 0x7C, 0x8D, 0x00);
    public Color phaseColorThree = new Color32(0xEF, 0xEE, 0xF5, 0x00);

    public float heartShakeTime = 0.22f;
    public float heartShakePower = 12f;

    private RectTransform heartRect;
    private Vector2 heartStartPos;

    private Coroutine heartShakeCoroutine;
    private Coroutine fadeCoroutine;
    private Coroutine cameraColorCoroutine;

    private int currentCameraPhase = -1;

    private Enemy currentEnemy;
    private PlayerHealthSimple currentPlayer;

    void Awake()
    {
        Instance = this;

        AutoFindUI();

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (heartImage != null)
        {
            heartRect = heartImage.GetComponent<RectTransform>();

            if (heartRect != null)
            {
                heartStartPos = heartRect.anchoredPosition;
            }
        }

        if (fadeImage != null)
        {
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
        }
    }

    void Start()
    {
        if (targetCamera != null)
        {
            targetCamera.backgroundColor = phaseColorOne;
        }

        StartCoroutine(StartFadeInRoutine());
    }

    void Update()
    {
        if (enemyHealthText == null || myHealthText == null || heartImage == null || fadeImage == null)
        {
            AutoFindUI();
        }
    }

    void AutoFindUI()
    {
        if (enemyHealthText == null)
        {
            GameObject obj = GameObject.Find("EnemyHealth");

            if (obj != null)
            {
                enemyHealthText = obj.GetComponent<TMP_Text>();

                if (enemyHealthText == null)
                {
                    enemyHealthText = obj.GetComponentInChildren<TMP_Text>();
                }
            }
        }

        if (myHealthText == null)
        {
            GameObject obj = GameObject.Find("MyHealth");

            if (obj != null)
            {
                myHealthText = obj.GetComponent<TMP_Text>();

                if (myHealthText == null)
                {
                    myHealthText = obj.GetComponentInChildren<TMP_Text>();
                }
            }
        }

        if (heartImage == null)
        {
            GameObject obj = GameObject.Find("HeartImage");

            if (obj != null)
            {
                heartImage = obj.GetComponent<Image>();

                if (heartImage == null)
                {
                    heartImage = obj.GetComponentInChildren<Image>();
                }

                if (heartImage != null)
                {
                    heartRect = heartImage.GetComponent<RectTransform>();

                    if (heartRect != null)
                    {
                        heartStartPos = heartRect.anchoredPosition;
                    }
                }
            }
        }

        if (fadeImage == null)
        {
            GameObject obj = GameObject.Find("FadeImage");

            if (obj != null)
            {
                fadeImage = obj.GetComponent<Image>();

                if (fadeImage == null)
                {
                    fadeImage = obj.GetComponentInChildren<Image>();
                }
            }
        }
    }

    IEnumerator StartFadeInRoutine()
    {
        if (fadeImage == null)
        {
            yield break;
        }

        SetFadeAlpha(1f);

        yield return StartCoroutine(FadeTo(0f, startFadeInTime));
    }

    public void RegisterEnemy(Enemy enemy)
    {
        currentEnemy = enemy;

        if (currentEnemy != null)
        {
            UpdateEnemyHealth(currentEnemy.hp, currentEnemy.maxHp);
            UpdateCameraPhaseByEnemyHp(currentEnemy.GetHpPercent());
        }
    }

    public void RegisterPlayer(PlayerHealthSimple player)
    {
        currentPlayer = player;

        if (currentPlayer != null)
        {
            UpdateMyHealth(currentPlayer.hp, currentPlayer.maxHp);
        }
    }

    public void UpdateEnemyHealth(int hp, int maxHp)
    {
        if (enemyHealthText != null)
        {
            enemyHealthText.text = hp + " / " + maxHp;
        }
    }

    public void UpdateMyHealth(int hp, int maxHp)
    {
        if (myHealthText != null)
        {
            myHealthText.text = hp + " / " + maxHp;
        }
    }

    public void ShakeHeart()
    {
        if (heartRect == null)
        {
            return;
        }

        if (heartShakeCoroutine != null)
        {
            StopCoroutine(heartShakeCoroutine);
        }

        heartShakeCoroutine = StartCoroutine(HeartShakeRoutine());
    }

    IEnumerator HeartShakeRoutine()
    {
        float timer = 0f;

        while (timer < heartShakeTime)
        {
            timer = timer + Time.deltaTime;

            float randomX = Random.Range(-heartShakePower, heartShakePower);
            float randomY = Random.Range(-heartShakePower, heartShakePower);

            heartRect.anchoredPosition = heartStartPos + new Vector2(randomX, randomY);

            yield return null;
        }

        heartRect.anchoredPosition = heartStartPos;
        heartShakeCoroutine = null;
    }

    public IEnumerator FadeOut()
    {
        if (fadeImage == null)
        {
            yield break;
        }

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeTo(1f, normalFadeTime));

        yield return fadeCoroutine;

        fadeCoroutine = null;
    }

    public IEnumerator FadeIn()
    {
        if (fadeImage == null)
        {
            yield break;
        }

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeTo(0f, normalFadeTime));

        yield return fadeCoroutine;

        fadeCoroutine = null;
    }

    public IEnumerator VictoryFadeOut()
    {
        if (fadeImage == null)
        {
            yield break;
        }

        yield return StartCoroutine(FadeOut());
    }

    IEnumerator FadeTo(float targetAlpha, float time)
    {
        if (fadeImage == null)
        {
            yield break;
        }

        Color startColor = fadeImage.color;
        Color targetColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, targetAlpha);

        float timer = 0f;

        if (time <= 0f)
        {
            fadeImage.color = targetColor;
            yield break;
        }

        while (timer < time)
        {
            timer = timer + Time.deltaTime;

            float t = timer / time;

            fadeImage.color = Color.Lerp(startColor, targetColor, t);

            yield return null;
        }

        fadeImage.color = targetColor;
    }

    void SetFadeAlpha(float alpha)
    {
        if (fadeImage == null)
        {
            return;
        }

        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
    }

    public void UpdateCameraPhaseByEnemyHp(float enemyHpPercent)
    {
        int newPhase = 0;

        if (enemyHpPercent > 0.7f)
        {
            newPhase = 0;
        }
        else if (enemyHpPercent > 0.4f)
        {
            newPhase = 1;
        }
        else
        {
            newPhase = 2;
        }

        if (newPhase == currentCameraPhase)
        {
            return;
        }

        currentCameraPhase = newPhase;

        Color targetColor = phaseColorOne;

        if (newPhase == 0)
        {
            targetColor = phaseColorOne;
        }
        else if (newPhase == 1)
        {
            targetColor = phaseColorTwo;
        }
        else
        {
            targetColor = phaseColorThree;
        }

        StartCameraColorTween(targetColor);
    }

    void StartCameraColorTween(Color targetColor)
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null)
        {
            return;
        }

        if (cameraColorCoroutine != null)
        {
            StopCoroutine(cameraColorCoroutine);
        }

        cameraColorCoroutine = StartCoroutine(CameraColorTweenRoutine(targetColor));
    }

    IEnumerator CameraColorTweenRoutine(Color targetColor)
    {
        Color startColor = targetCamera.backgroundColor;

        float timer = 0f;

        if (cameraColorTweenTime <= 0f)
        {
            targetCamera.backgroundColor = targetColor;
            yield break;
        }

        while (timer < cameraColorTweenTime)
        {
            timer = timer + Time.deltaTime;

            float t = timer / cameraColorTweenTime;

            targetCamera.backgroundColor = Color.Lerp(startColor, targetColor, t);

            yield return null;
        }

        targetCamera.backgroundColor = targetColor;

        cameraColorCoroutine = null;
    }
}