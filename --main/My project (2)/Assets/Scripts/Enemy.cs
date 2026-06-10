using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int maxHp = 100;
    public int hp = 100;

    public string enemyName = "적";

    public Color hitFlashColor = Color.red;
    public float hitFlashTime = 0.08f;

    private bool isDead = false;

    private SpriteRenderer[] spriteRenderers;
    private Color[] originalColors;

    private Coroutine flashCoroutine;

    void Awake()
    {
        if (hp <= 0)
        {
            hp = maxHp;
        }

        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        originalColors = new Color[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                originalColors[i] = spriteRenderers[i].color;
            }
        }
    }

    void Start()
    {
        Debug.Log(enemyName + "초기 체력: " + hp);

        if (GameUIController.Instance != null)
        {
            GameUIController.Instance.RegisterEnemy(this);
            GameUIController.Instance.UpdateEnemyHealth(hp, maxHp);
            GameUIController.Instance.UpdateCameraPhaseByEnemyHp(GetHpPercent());
        }
    }

    public void Damage(int damage)
    {
        if (isDead)
        {
            return;
        }

        if (damage <= 0)
        {
            return;
        }

        hp = hp - damage;

        if (hp < 0)
        {
            hp = 0;
        }

        Debug.Log(enemyName + " 피해 입음 받은 피해: " + damage + " / 남은 체력: " + hp);

        FlashRed();

        if (GameUIController.Instance != null)
        {
            GameUIController.Instance.UpdateEnemyHealth(hp, maxHp);
            GameUIController.Instance.UpdateCameraPhaseByEnemyHp(GetHpPercent());
        }

        if (hp <= 0)
        {
            Die();
        }
    }

    void FlashRed()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(FlashRedRoutine());
    }

    IEnumerator FlashRedRoutine()
    {
        SetSpriteColors(hitFlashColor);

        yield return new WaitForSeconds(hitFlashTime);

        RestoreSpriteColors();

        flashCoroutine = null;
    }

    void SetSpriteColors(Color color)
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                spriteRenderers[i].color = color;
            }
        }
    }

    void RestoreSpriteColors()
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                spriteRenderers[i].color = originalColors[i];
            }
        }
    }

    void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        Debug.Log(enemyName + " 사망 승리 페이드아웃");

        StartCoroutine(DieRoutine());
    }

    IEnumerator DieRoutine()
    {
        if (GameUIController.Instance != null)
        {
            yield return StartCoroutine(GameUIController.Instance.VictoryFadeOut());
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        Destroy(gameObject);
    }

    public float GetHpPercent()
    {
        if (maxHp <= 0)
        {
            return 0f;
        }

        return (float)hp / (float)maxHp;
    }
}