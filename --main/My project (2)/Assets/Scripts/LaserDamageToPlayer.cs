using System.Collections.Generic;
using UnityEngine;

public class LaserDamageToPlayer : MonoBehaviour
{
    public int damage = 3;
    public string targetTag = "Player";

    private List<GameObject> alreadyHitObjects = new List<GameObject>();

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(targetTag))
        {
            return;
        }

        if (alreadyHitObjects.Contains(other.gameObject))
        {
            return;
        }

        alreadyHitObjects.Add(other.gameObject);

        PlayerHealthSimple playerHp = other.GetComponent<PlayerHealthSimple>();

        if (playerHp == null)
        {
            playerHp = other.GetComponentInParent<PlayerHealthSimple>();
        }

        if (playerHp != null)
        {
            playerHp.Damage(damage);
        }
    }
}