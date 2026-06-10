using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    // 불렛 속도 및 데미지
    public float speed = 12f;
    public int damage = 1;

    public float deleteTime = 3f;

    // 멋진 페이드인 & 아웃 기능
    public float fadeInTime = 0.08f;
    public float fadeOutTime = 0.12f;

    public bool useRandomRotation = true;

    private Vector3 targetScale;
    private bool isDisappearing = false;

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


        // 무작위 회전된 방향이 실제 불렛 개체 이동에 영향을 주던 문제를 (아마) 수정함
        transform.position = transform.position + Vector3.up * speed * Time.deltaTime;
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

        if (other.gameObject.tag == "Enemy")
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();

            if (enemy == null)
            {
                enemy = other.gameObject.GetComponentInParent<Enemy>();
            }

            if (enemy != null)
            {
                enemy.Damage(damage);
            }
            else
            {
                Debug.Log("Enemy 스크가 업슴");
            }

            StartDisappear();
        }
    }
}