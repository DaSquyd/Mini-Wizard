using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossScript : Entity
{

    [Header("Movement")]
    public float movementSpeed = 2.0f;

    [Header("Slamming")]
    public GameObject punchBox;
    public float slamSeconds = 4.0f;
    public float slamRate = 0.5f;

    [Header("Throwing")]
    public float throwSeconds = 5.0f;
    public float throwRate = 0.5f;

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

    protected override void Start()
    {
        base.Start();
        enemyState = EnemyState.Idle;
    }

    protected override void Update()
    {
        base.Update();

        if (player == null && FindObjectOfType<PlayerController>() != null)
            player = FindObjectOfType<PlayerController>().gameObject;

        transform.LookAt(new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z), Vector3.up);
        disToPlayer = Vector3.Distance(transform.position, player.transform.position);

        if (player != null)
        {
            switch (enemyState)
            {
                case EnemyState.Idle:
                    //rotate towards 
                    transform.position = Vector3.MoveTowards(transform.position, new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z), Time.deltaTime * movementSpeed);
                    if (Vector3.Distance(transform.position, player.transform.position) >= 12.5f && Time.time > nextThrow)
                        enemyState = EnemyState.Throw;
                    if (Vector3.Distance(transform.position, player.transform.position) < 5.0f && Time.time > nextSlam)
                        enemyState = EnemyState.Slam;
                    break;
                case EnemyState.Slam:
                    //Punch the player
                    //do the things
                    nextSlam = Time.time + slamSeconds + 1.0f / slamRate;
                    StartCoroutine(Slam());
                    break;
                case EnemyState.Throw:
                    //Throw a big rock
                    nextThrow = Time.time + throwSeconds + 1.0f / throwRate;
                    StartCoroutine(Throw());
                    break;
                case EnemyState.Die:
                    //Play death animation
                    break;
            }
        }
    }

    private IEnumerator Slam()
    {
        yield return new WaitForSecondsRealtime(slamSeconds);

        enemyState = EnemyState.Idle;
    }

    private IEnumerator Throw()
    {
        yield return new WaitForSecondsRealtime(throwSeconds);

        enemyState = EnemyState.Idle;
    }

}
