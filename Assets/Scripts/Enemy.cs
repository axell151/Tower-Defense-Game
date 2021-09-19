using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int maxHealth = 1;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private SpriteRenderer healthBar;
    [SerializeField] private SpriteRenderer healthFill;

    private int currentHealth;

    public Vector3 TargetPosition { get; private set; }
    public int CurrentPathIndex { get; private set; }

    private void OnEnable()
    {
        currentHealth = maxHealth;
        healthFill.size = healthBar.size;
    }

    public void MoveToTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, TargetPosition, moveSpeed * Time.deltaTime);
    }

    public void SetTargetPosition (Vector3 targetPosition)
    {
        TargetPosition = targetPosition;
        healthBar.transform.parent = null;
        Vector3 distance = TargetPosition - transform.position;
        if(Mathf.Abs(distance.y) > Mathf.Abs(distance.x))
        {
            if(distance.y > 0)
            {
                //atas
                transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 90f));
            }
            else
            {
                //bawah
                transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, -90f));
            }
        }
        else
        {
            if(distance.x > 0)
            {
                //kanan
                transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
            }
            else
            {
                //kiri
                transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 180f));
            }
        }
        healthBar.transform.parent = transform;
    }

    public void ReduceEnemyHealth(int Damage)
    {
        currentHealth -= Damage;
        AudioPlayer.Instance.PlaySFX("hit-enemy");
        if(currentHealth <= 0)
        {
            currentHealth = 0;
            gameObject.SetActive(false);
            AudioPlayer.Instance.PlaySFX("enemy-die");
        }
        float healthPercentage = (float)currentHealth / maxHealth;
        healthFill.size = new Vector2(healthPercentage * healthBar.size.x, healthBar.size.y);
    }

    public void SetCurrentPathIndex(int currentIndex)
    {
        CurrentPathIndex = currentIndex;
    }
}
