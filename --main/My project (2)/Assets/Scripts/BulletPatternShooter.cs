// 적 탄환 패턴ㄴ
using System.Collections;
using UnityEngine;

public class BulletPatternShooter : MonoBehaviour
{
    public GameObject enemyBulletPrefab;

    public Transform firePoint;
    public Transform player;

    public bool autoStart = true;

    public float patternRestTime = 0.8f;

    public int bulletDamage = 1;

    public float normalBulletSpeed = 5f;
    public float fastBulletSpeed = 8f;
    public float slowBulletSpeed = 2.5f;

    public float wallMinX = -8f;
    public float wallMaxX = 8f;
    public float wallSpawnY = 5.5f;

    public float laserLength = 33f;
    public float laserWidth = 1.8f;
    public float laserWarningTime = 1f;
    public float laserStayTime = 1.45f;
    public int laserDamage = 3;

    private Enemy enemy;

    private bool isRunning = false;

    private Vector3 lastPlayerPosition;
    private Vector3 playerVelocity;

    private int patternIndex = 0;

    private static Sprite whiteBoxSprite;

    void Awake()
    {
        enemy = GetComponent<Enemy>();

        if (firePoint == null)
        {
            firePoint = transform;
        }
    }

    void Start()
    {
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");

            if (foundPlayer != null)
            {
                player = foundPlayer.transform;
                lastPlayerPosition = player.position;
            }
        }

        if (autoStart)
        {
            StartPatterns();
        }
    }

    void Update()
    {
        if (player != null)
        {
            playerVelocity = (player.position - lastPlayerPosition) / Time.deltaTime;
            lastPlayerPosition = player.position;
        }
    }

    public void StartPatterns()
    {
        if (isRunning)
        {
            return;
        }

        isRunning = true;
        StartCoroutine(PatternLoop());
    }

    IEnumerator PatternLoop()
    {
        while (true)
        {
            if (enemyBulletPrefab == null)
            {
                Debug.Log("EnemyBullet 프리팹이 없음");
                yield return new WaitForSeconds(1f);
                continue;
            }

            float hpPercent = 1f;

            if (enemy != null)
            {
                hpPercent = enemy.GetHpPercent();
            }

            if (hpPercent > 0.7f)
            {
                yield return StartCoroutine(PhaseOnePattern());
            }
            else if (hpPercent > 0.4f)
            {
                yield return StartCoroutine(PhaseTwoPattern());
            }
            else
            {
                yield return StartCoroutine(PhaseThreePattern());
            }

            yield return new WaitForSeconds(patternRestTime);
        }
    }

    IEnumerator PhaseOnePattern()
    {
        int pick = patternIndex % 3;
        patternIndex++;

        if (pick == 0)
        {
            yield return StartCoroutine(FanPattern(5, 60f, normalBulletSpeed, 3, 0.25f));
        }
        else if (pick == 1)
        {
            yield return StartCoroutine(AimedPattern(3, normalBulletSpeed, 0.35f));
        }
        else
        {
            yield return StartCoroutine(CrossPattern(4, normalBulletSpeed, 0.35f));
        }
    }

    IEnumerator PhaseTwoPattern()
    {
        int pick = patternIndex % 5;
        patternIndex++;

        if (pick == 0)
        {
            yield return StartCoroutine(FanPattern(7, 80f, normalBulletSpeed, 4, 0.22f));
        }
        else if (pick == 1)
        {
            yield return StartCoroutine(RotatingPattern(8, 8, normalBulletSpeed, 12f, 0.15f));
        }
        else if (pick == 2)
        {
            yield return StartCoroutine(WallPattern(13, 3, normalBulletSpeed, 4, 0.45f));
        }
        else if (pick == 3)
        {
            yield return StartCoroutine(FlowerPattern(6, 6, normalBulletSpeed, 15f, 0.18f));
        }
        else
        {
            yield return StartCoroutine(PredictivePattern(4, fastBulletSpeed, 0.45f, 0.45f));
        }
    }

    IEnumerator PhaseThreePattern()
    {
        int pick = patternIndex % 6;
        patternIndex++;

        if (pick == 0)
        {
            yield return StartCoroutine(RotatingPattern(12, 12, normalBulletSpeed, 14f, 0.12f));
        }
        else if (pick == 1)
        {
            yield return StartCoroutine(MixedSlowFastPattern());
        }
        else if (pick == 2)
        {
            yield return StartCoroutine(LaserPattern());
        }
        else if (pick == 3)
        {
            yield return StartCoroutine(FlowerPattern(8, 8, normalBulletSpeed, 13f, 0.14f));
        }
        else if (pick == 4)
        {
            yield return StartCoroutine(FanPattern(9, 100f, fastBulletSpeed, 4, 0.18f));
        }
        else
        {
            yield return StartCoroutine(AimedSpreadPattern(5, 35f, fastBulletSpeed));
        }
    }

    IEnumerator FanPattern(int bulletCount, float angleRange, float speed, int repeat, float wait)
    {
        for (int r = 0; r < repeat; r++)
        {
            float startAngle = -90f - angleRange / 2f;
            float angleGap = angleRange / (bulletCount - 1);

            for (int i = 0; i < bulletCount; i++)
            {
                float angle = startAngle + angleGap * i;
                Vector2 dir = AngleToDirection(angle);

                FireBullet(dir, speed, bulletDamage);
            }

            yield return new WaitForSeconds(wait);
        }
    }

    IEnumerator RotatingPattern(int bulletCount, int repeat, float speed, float rotateAmount, float wait)
    {
        float startRotate = 0f;

        for (int r = 0; r < repeat; r++)
        {
            for (int i = 0; i < bulletCount; i++)
            {
                float angle = startRotate + 360f / bulletCount * i;
                Vector2 dir = AngleToDirection(angle);

                FireBullet(dir, speed, bulletDamage);
            }

            startRotate = startRotate + rotateAmount;

            yield return new WaitForSeconds(wait);
        }
    }

    IEnumerator CrossPattern(int repeat, float speed, float wait)
    {
        for (int r = 0; r < repeat; r++)
        {
            float offset = 0f;

            if (r % 2 == 1)
            {
                offset = 45f;
            }

            FireBullet(AngleToDirection(0f + offset), speed, bulletDamage);
            FireBullet(AngleToDirection(90f + offset), speed, bulletDamage);
            FireBullet(AngleToDirection(180f + offset), speed, bulletDamage);
            FireBullet(AngleToDirection(270f + offset), speed, bulletDamage);

            yield return new WaitForSeconds(wait);
        }
    }

    IEnumerator AimedPattern(int repeat, float speed, float wait)
    {
        for (int i = 0; i < repeat; i++)
        {
            Vector2 dir = GetDirectionToPlayer();

            FireBullet(dir, speed, bulletDamage);

            yield return new WaitForSeconds(wait);
        }
    }

    IEnumerator AimedSpreadPattern(int bulletCount, float angleRange, float speed)
    {
        Vector2 baseDir = GetDirectionToPlayer();
        float baseAngle = DirectionToAngle(baseDir);

        float startAngle = baseAngle - angleRange / 2f;
        float angleGap = angleRange / (bulletCount - 1);

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = startAngle + angleGap * i;
            Vector2 dir = AngleToDirection(angle);

            FireBullet(dir, speed, bulletDamage);
        }

        yield return new WaitForSeconds(0.35f);
    }

    IEnumerator PredictivePattern(int repeat, float speed, float predictionTime, float wait)
    {
        for (int i = 0; i < repeat; i++)
        {
            Vector2 dir = Vector2.down;

            if (player != null)
            {
                Vector3 predictedPosition = player.position + playerVelocity * predictionTime;
                dir = predictedPosition - GetFirePosition();
            }

            FireBullet(dir, speed, bulletDamage);

            yield return new WaitForSeconds(wait);
        }
    }

    IEnumerator WallPattern(int bulletCount, int gapSize, float speed, int repeat, float wait)
    {
        for (int r = 0; r < repeat; r++)
        {
            int gapStart = Random.Range(0, bulletCount - gapSize);

            for (int i = 0; i < bulletCount; i++)
            {
                bool isGap = i >= gapStart && i < gapStart + gapSize;

                if (isGap)
                {
                    continue;
                }

                float t = 0f;

                if (bulletCount > 1)
                {
                    t = (float)i / (float)(bulletCount - 1);
                }

                float x = Mathf.Lerp(wallMinX, wallMaxX, t);

                Vector3 spawnPos = new Vector3(x, wallSpawnY, 0f);

                FireBulletAt(spawnPos, Vector2.down, speed, bulletDamage);
            }

            yield return new WaitForSeconds(wait);
        }
    }

    IEnumerator FlowerPattern(int bulletCount, int repeat, float speed, float rotateAmount, float wait)
    {
        float rotate = 0f;

        for (int r = 0; r < repeat; r++)
        {
            for (int i = 0; i < bulletCount; i++)
            {
                float angle = rotate + 360f / bulletCount * i;
                Vector2 dir = AngleToDirection(angle);

                FireBullet(dir, speed, bulletDamage);
            }

            rotate = rotate + rotateAmount;

            yield return new WaitForSeconds(wait);
        }
    }

    IEnumerator MixedSlowFastPattern()
    {
        for (int i = 0; i < 12; i++)
        {
            float angle = 360f / 12f * i;
            Vector2 dir = AngleToDirection(angle);

            FireBullet(dir, slowBulletSpeed, bulletDamage);
        }

        yield return new WaitForSeconds(0.45f);

        Vector2 playerDir = GetDirectionToPlayer();
        float playerAngle = DirectionToAngle(playerDir);

        FireBullet(AngleToDirection(playerAngle - 14f), fastBulletSpeed, bulletDamage);
        FireBullet(AngleToDirection(playerAngle), fastBulletSpeed, bulletDamage);
        FireBullet(AngleToDirection(playerAngle + 14f), fastBulletSpeed, bulletDamage);

        yield return new WaitForSeconds(0.35f);
    }

    IEnumerator LaserPattern()
    {
        Vector3 pos = GetFirePosition();

        GameObject warning = CreateBoxObject("레이저 경고선", new Color(1f, 0f, 0f, 0.35f));
        warning.transform.position = pos + Vector3.down * laserLength / 2f;
        warning.transform.localScale = new Vector3(laserWidth * 0.35f, laserLength, 1f);

        yield return new WaitForSeconds(laserWarningTime);

        Destroy(warning);

        GameObject laser = CreateBoxObject("레이저", new Color(1f, 0f, 0f, 0.8f));
        laser.transform.position = pos + Vector3.down * laserLength / 2f;
        laser.transform.localScale = new Vector3(laserWidth, laserLength, 1f);

        BoxCollider2D col = laser.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        LaserDamageToPlayer laserDamageScript = laser.AddComponent<LaserDamageToPlayer>();
        laserDamageScript.damage = laserDamage;

        yield return new WaitForSeconds(laserStayTime);

        Destroy(laser);
    }

    void FireBullet(Vector2 dir, float speed, int damage)
    {
        FireBulletAt(GetFirePosition(), dir, speed, damage);
    }

    void FireBulletAt(Vector3 spawnPosition, Vector2 dir, float speed, int damage)
    {
        GameObject newBullet = Instantiate(enemyBulletPrefab, spawnPosition, Quaternion.identity);

        EnemyBullet enemyBullet = newBullet.GetComponent<EnemyBullet>();

        if (enemyBullet != null)
        {
            enemyBullet.SetBullet(dir, speed, damage);
        }

        float angle = DirectionToAngle(dir);
        newBullet.transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    Vector3 GetFirePosition()
    {
        if (firePoint != null)
        {
            return firePoint.position;
        }

        return transform.position;
    }

    Vector2 GetDirectionToPlayer()
    {
        if (player == null)
        {
            return Vector2.down;
        }

        Vector2 dir = player.position - GetFirePosition();

        if (dir.magnitude <= 0.01f)
        {
            return Vector2.down;
        }

        return dir.normalized;
    }

    Vector2 AngleToDirection(float angle)
    {
        float rad = angle * Mathf.Deg2Rad;

        float x = Mathf.Cos(rad);
        float y = Mathf.Sin(rad);

        return new Vector2(x, y).normalized;
    }

    float DirectionToAngle(Vector2 dir)
    {
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }

    GameObject CreateBoxObject(string objName, Color color)
    {
        GameObject obj = new GameObject(objName);

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = GetWhiteBoxSprite();
        sr.color = color;
        sr.sortingOrder = 50;

        return obj;
    }

    Sprite GetWhiteBoxSprite()
    {
        if (whiteBoxSprite != null)
        {
            return whiteBoxSprite;
        }

        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();

        whiteBoxSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);

        return whiteBoxSprite;
    }
}
