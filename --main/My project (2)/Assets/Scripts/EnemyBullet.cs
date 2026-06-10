using System.Collections;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 5f;
    public int damage = 1;

    public float deleteTime = 8f;

    public string targetTag = "Player";

    public float fadeInTime = 0.08f;
    public float fadeOutTime = 0.12f;

    public bool useRandomRotation = true;

    private Vector2 moveDirection = Vector2.down;

    private bool isDisappearing = false;
    private Vector3 targetScale;

    private Collider2D[] colliders;

    void Start()
    {
        targetScale = transform.localScale;

        colliders = GetComponentsInChildren<Collider2D>();

        if (useRandomRotation)
        {
            float randomZ = Random.Range(0f, 360f);
            transform.rotation = Quaternion.Euler(0f, 0f, randomZ);
        }

        StartCoroutine(BulletLife());
    }

    void Update()
    {
        if (isDisappearing)
        {
            return;
        }

        transform.position = transform.position + (Vector3)(moveDirection.normalized * speed * Time.deltaTime);
    }

    public void SetBullet(Vector2 dir, float newSpeed, int newDamage)
    {
        moveDirection = dir.normalized;
        speed = newSpeed;
        damage = newDamage;
    }

    IEnumerator BulletLife()
    {
        yield return StartCoroutine(FadeIn());

        float waitTime = deleteTime - fadeInTime - fadeOutTime;

        if (waitTime < 0)
        {
            waitTime = 0;
        }

        yield return new WaitForSeconds(waitTime);

        StartDisappear();
    }

    IEnumerator FadeIn()
    {
        float timer = 0f;

        transform.localScale = Vector3.zero;

        while (timer < fadeInTime)
        {
            timer = timer + Time.deltaTime;

            float t = timer / fadeInTime;

            transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);

            yield return null;
        }

        transform.localScale = targetScale;
    }

    IEnumerator FadeOut()
    {
        float timer = 0f;

        Vector3 startScale = transform.localScale;

        while (timer < fadeOutTime)
        {
            timer = timer + Time.deltaTime;

            float t = timer / fadeOutTime;

            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            yield return null;
        }

        Destroy(gameObject);
    }

    void StartDisappear()
    {
        if (isDisappearing)
        {
            return;
        }

        isDisappearing = true;

        DisableColliders();

        StartCoroutine(FadeOut());
    }

    void DisableColliders()
    {
        if (colliders == null)
        {
            return;
        }

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
            {
                colliders[i].enabled = false;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDisappearing)
        {
            return;
        }

        if (other.CompareTag(targetTag))
        {
            PlayerHealthSimple playerHp = other.GetComponent<PlayerHealthSimple>();

            if (playerHp == null)
            {
                playerHp = other.GetComponentInParent<PlayerHealthSimple>();
            }

            if (playerHp != null)
            {
                playerHp.Damage(damage);
            }

            StartDisappear();
        }
    }
}