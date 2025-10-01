using UnityEngine;
using System.Collections;

public class PowerDashObserver : MonoBehaviour
{
    public PlayerController player;
    public GameObject dashMarkPrefab;

    private int dashCount = 0;
    private float dashResetTime = 1f;
    private float dashTimer;

    private void Update()
    {
        if (player.stateMachine.CurrentState == player.DashState)
        {
            dashTimer = dashResetTime;
            dashCount++;

            if (dashCount >= 2)
            {
                dashCount = 0;
                StartCoroutine(SpawnDashMarkWithDelay());
            }
        }
        else
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
                dashCount = 0;
        }
    }

    private IEnumerator SpawnDashMarkWithDelay()
    {
        yield return new WaitForSeconds(0.5f); // Delay de 0.1s

        Instantiate(dashMarkPrefab, player.transform.position, Quaternion.identity);
    }
}
