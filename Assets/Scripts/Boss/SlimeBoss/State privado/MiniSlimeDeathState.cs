//using System.Collections;
//using UnityEngine;

//public class MiniSlimeDeathState : State<EnemyInputs>
//{
//    private MiniSlimeController controller;
//    private Transform transform;
//    private MonoBehaviour mono;
//    private Coroutine shakeCoroutine;
//    private float duration = 1f; // Tiempo antes de morir

//    public MiniSlimeDeathState(MiniSlimeController controller)
//    {
//        this.controller = controller;
//        this.transform = controller.transform;
//        this.mono = controller.GetComponent<MonoBehaviour>();
//    }

//    public override void Awake()
//    {
//        base.Awake();
//        shakeCoroutine = mono.StartCoroutine(ShakeAndDie());
//    }

//    public override void Sleep()
//    {
//        base.Sleep();
//        if (shakeCoroutine != null)
//        {
//            mono.StopCoroutine(shakeCoroutine);
//        }
//    }

//    private IEnumerator ShakeAndDie()
//    {
//        Vector3 originalPos = transform.position;

//        float timer = 0f;
//        while (timer < duration)
//        {
//            timer += Time.deltaTime;
//            transform.position = originalPos + (Vector3)Random.insideUnitCircle * 0.05f;
//            yield return null;
//        }

//        transform.position = originalPos;

//        controller.Die();
//    }
//}
