using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletraxAI : Enemy
{
    public BoxCollider2D boxCollider;
    public RectInt room;
    [Header("IFrame Stuff")]
    public Color flashColor;
    public Color regularColor;
    public float flashDuration;
    public int numberOfFlashes;
    public SpriteRenderer mySprite;
    // Start is called before the first frame update
    void Start()
    {
        boxCollider = GetComponentInChildren<BoxCollider2D>();
        boxCollider.size = room.size;
        health = 20;
        currentState = EnemyState.idle;
    }

    private void Update()
    {
        switch (currentState)
        {
            case EnemyState.idle:
                Idle();
                break;
        }
    }
    private void Idle()
    {

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            FindObjectOfType<MusicSoundManager>().ChangeMusic("bossMusic");
        }
    }
    //private void OnTriggerExit2D(Collider2D collision)
    //{
    //    if (collision.CompareTag("Player"))
    //    {
    //        FindObjectOfType<MusicSoundManager>().ChangeMusic("dungeonMusic");
    //    }
    //}
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("PlayerAttack"))
        {
            GetHit();
        }
        //if (collision.collider.CompareTag("Enemy"))
        //{
        //    collision.collider.GetComponentInChildren<PolygonCollider2D>().size
        //}
    }
    private IEnumerator FlashCo(bool death)
    {
        int temp = 0;
        while (temp < numberOfFlashes)
        {
            mySprite.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            mySprite.color = regularColor;
            yield return new WaitForSeconds(flashDuration);
            temp++;
        }
        if (death)
        {
            this.gameObject.SetActive(false);
        }

    }

    public void GetHit()
    {
        Debug.Log("Health: " + health);
        health -= 1;
        if (health > 0)
        {
            SoundManager.PlaySound("skeletonHit");
            StartCoroutine(FlashCo(false));
        }
        else
        {
            SoundManager.PlaySound("skeletonDeath");
            StartCoroutine(FlashCo(true));
            FindObjectOfType<GameManager>().EndGame(true);

        }
    }
}
