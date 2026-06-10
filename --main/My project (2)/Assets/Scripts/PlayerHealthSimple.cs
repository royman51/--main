using System.Collections;
using UnityEngine;

public class PlayerHealthSimple : MonoBehaviour
{
    public int maxHp = 10;
    public int hp = 10;

    public float invincibleTime = 0.75f;
    public float blinkSpeed = 0.1f;

    public float respawnInvincibleTime = 1f;
    public bool clearBulletsOnDeath = true;

    private bool isDead = false;
    private bool isInvincible = false;

    private SpriteRenderer[] spriteRenderers;
    private Collider2D[] colliders;

    private Coroutine invincibleCoroutine;
    private Coroutine deathCoroutine;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private Player playerController;

    void Awake()
    {
        if (hp <= 0)
        {
            hp = maxHp;
        }

        startPosition = transform.position;
        startRotation = transform.rotation;

        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        colliders = GetComponentsInChildren<Collider2D>(true);

        playerController = GetComponent<Player>();
    }

    void Start()
    {
        if (GameUIController.Instance != null)
        {
            GameUIController.Instance.RegisterPlayer(this);
            GameUIController.Instance.UpdateMyHealth(hp, maxHp);
        }
    }

    public void Damage(int damage)
    {
        if (isDead)
        {
            return;
        }

        if (isInvincible)
        {
            Debug.Log("플레이어가 무적 상태라 피해를 받지 않음");
            return;
        }

        if (damage <= 0)
        {
            return;
        }

        int beforeHp = hp;

        hp = hp - damage;

        if (hp < 0)
        {
            hp = 0;
        }

        Debug.Log("플레이어 피해 입음 받은 피해: " + damage + " / 남은 체력: " + hp);

        if (GameUIController.Instance != null)
        {
            GameUIController.Instance.UpdateMyHealth(hp, maxHp);

            if (hp < beforeHp)
            {
                GameUIController.Instance.ShakeHeart();
            }
        }

        if (hp <= 0)
        {
            Die();
            return;
        }

        StartInvincible(invincibleTime);
    }

    void Die()
    {
        if (isDead)
        {
            return;
        }

        if (deathCoroutine != null)
        {
            StopCoroutine(deathCoroutine);
        }

        deathCoroutine = StartCoroutine(DeathAndRespawnRoutine());
    }

    IEnumerator DeathAndRespawnRoutine()
    {
        isDead = true;
        isInvincible = true;

        Debug.Log("플레이어 사망 초기화 시작");

        if (playerController != null)
        {
            playerController.SetControlEnabled(false);
        }

        SetCollidersEnabled(false);

        if (GameUIController.Instance != null)
        {
            yield return StartCoroutine(GameUIController.Instance.FadeOut());
        }
        else
        {
            yield return new WaitForSeconds(0.4f);
        }

        if (clearBulletsOnDeath)
        {
            ClearAllProjectiles();
        }

        transform.position = startPosition;
        transform.rotation = startRotation;

        hp = maxHp;

        if (GameUIController.Instance != null)
        {
            GameUIController.Instance.UpdateMyHealth(hp, maxHp);
        }

        SetSpritesVisible(true);
        SetCollidersEnabled(true);

        if (GameUIController.Instance != null)
        {
            yield return StartCoroutine(GameUIController.Instance.FadeIn());
        }
        else
        {
            yield return new WaitForSeconds(0.4f);
        }

        if (playerController != null)
        {
            playerController.SetControlEnabled(true);
        }

        isDead = false;
        isInvincible = false;

        StartInvincible(respawnInvincibleTime);

        deathCoroutine = null;
    }

    void ClearAllProjectiles()
    {
        Bullet[] playerBullets = FindObjectsOfType<Bullet>();
        EnemyBullet[] enemyBullets = FindObjectsOfType<EnemyBullet>();

        for (int i = 0; i < playerBullets.Length; i++)
        {
            if (playerBullets[i] != null)
            {
                Destroy(playerBullets[i].gameObject);
            }
        }

        for (int i = 0; i < enemyBullets.Length; i++)
        {
            if (enemyBullets[i] != null)
            {
                Destroy(enemyBullets[i].gameObject);
            }
        }
    }

    void StartInvincible(float time)
    {
        if (invincibleCoroutine != null)
        {
            StopCoroutine(invincibleCoroutine);
        }

        invincibleCoroutine = StartCoroutine(InvincibleBlink(time));
    }

    IEnumerator InvincibleBlink(float time)
    {
        isInvincible = true;

        float timer = 0f;

        while (timer < time)
        {
            SetSpritesVisible(false);
            yield return new WaitForSeconds(blinkSpeed);

            SetSpritesVisible(true);
            yield return new WaitForSeconds(blinkSpeed);

            timer = timer + blinkSpeed * 2f;
        }

        SetSpritesVisible(true);

        isInvincible = false;
        invincibleCoroutine = null;
    }

    void SetSpritesVisible(bool visible)
    {
        if (spriteRenderers == null)
        {
            return;
        }

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                spriteRenderers[i].enabled = visible;
            }
        }
    }

    void SetCollidersEnabled(bool enabledValue)
    {
        if (colliders == null)
        {
            return;
        }

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
            {
                colliders[i].enabled = enabledValue;
            }
        }
    }

    public bool IsInvincible()
    {
        return isInvincible;
    }

    public bool IsDead()
    {
        return isDead;
    }
}