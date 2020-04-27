using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossScript : Entity
{

    public Slider healthBar;

    [Header("Movement")]
    public float movementSpeed = 2.0f;

    [Header("Slamming")]
    public float slamSeconds = 4.0f;
    public float slamRate = 0.5f;

    [Header("Throwing")]
    public GameObject throwPlace;
    public GameObject throwItem;
    public float throwSeconds = 5.0f;
    public float throwRate = 0.5f;
    public float throwForce = 5.0f;

    private enum EnemyState
    {
        Idle,
        Slam,
        Throw,
        Die
    };

    [DebugDisplay]
    private EnemyState enemyState;
    private GameObject player;
    private float nextThrow;
    private float nextSlam;
    [DebugDisplay]
    private float disToPlayer;
    private Animator animator;
    [DebugDisplay]
    private bool once;
    private bool punched;

    protected override void OnStart()
    {
        enemyState = EnemyState.Idle;
        animator = GetComponent<Animator>();
        nextSlam = Time.time + slamSeconds + 1.0f / slamRate;
        nextThrow = Time.time + throwSeconds + 1.0f / throwRate;
        MaxHealth = 300;
        Health = 300;
        healthBar.maxValue = MaxHealth;
    }

    protected override void OnUpdate(float deltaTime)
    {
        if (player == null && FindObjectOfType<PlayerController>() != null)
            player = FindObjectOfType<PlayerController>().gameObject;

        if (player != null)
        {
            transform.LookAt(new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z), Vector3.up);
        }
        disToPlayer = Vector3.Distance(transform.position, player.transform.position);

        animator.SetBool("Walking", enemyState == EnemyState.Idle);

        if (player != null)
        {
            switch (enemyState)
            {
                case EnemyState.Idle:
                    //rotate towards 
                    if (Vector3.Distance(transform.position, player.transform.position) > 7.0f)
                        transform.position = Vector3.MoveTowards(transform.position, new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z), deltaTime * movementSpeed);
                    if (Vector3.Distance(transform.position, player.transform.position) >= 15.0f && Time.time > nextThrow)
                        enemyState = EnemyState.Throw;
                    if (Vector3.Distance(transform.position, player.transform.position) < 10.0f && Time.time > nextSlam)
                        enemyState = EnemyState.Slam;
                    break;
                case EnemyState.Slam:
                    //Punch the player
                    //do the things
                    animator.SetTrigger("Slam");
                    nextSlam = Time.time + slamSeconds + 1.0f / slamRate;
                    StartCoroutine(Slam());
                    break;
                case EnemyState.Throw:
                    //Throw a big rock
                    if (!once)
                        ThrowRock();
                    nextThrow = Time.time + throwSeconds + 1.0f / throwRate;
                    StartCoroutine(Throw());
                    break;
                case EnemyState.Die:
                    //Play death animation
                    animator.SetBool("Death", true);
                    enabled = false;
                    break;
            }
        }

        healthBar.value = Health;
    }

    private IEnumerator Slam()
    {
        yield return new WaitForSecondsRealtime(slamSeconds);
        enemyState = EnemyState.Idle;
    }

    private IEnumerator Throw()
    {
        yield return new WaitForSecondsRealtime(throwSeconds);
        once = false;
        enemyState = EnemyState.Idle;
    }

    private void ThrowRock()
    {
        animator.SetTrigger("Throw");
        GameObject throwObject = Instantiate(throwItem, throwPlace.transform.position, Quaternion.identity);
        throwObject.transform.localScale = new Vector3(0.4f, 0.5f, 1.0f);
        Vector3 direction = (player.transform.position - transform.position).normalized;
        throwObject.AddComponent<BoxCollider>();
        Rigidbody throwRB = throwObject.AddComponent<Rigidbody>();
        throwRB.AddForce(direction * throwForce, ForceMode.Force);
        Destroy(throwObject, 5.0f);
        once = true;
    }

}
