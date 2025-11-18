using UnityEngine;
using Object = UnityEngine.Object;

[CreateAssetMenu(fileName = "BurnOnHitPowerUp", menuName = "PowerUps/Burn On Enemy Hit")]
public class BurnOnHitPowerUp : PowerUp
{
    public int damagePerSecond = 5;
    public float burnDuration = 5f;
    public float cooldownPerEnemy = 10f;

    public GameObject burnVfxPrefab;

    private GameObject observerInstance;

    public override void Apply(PlayerController player)
    {
        // Si ya existe un observer, solo lo configuramos
        BurnOnHitObserver observer = BurnOnHitObserver.Instance;
        if (observer == null)
        {
            // Crear observer global
            observerInstance = new GameObject("BurnOnHitObserver");
            observer = observerInstance.AddComponent<BurnOnHitObserver>();
        }
        else
        {
            observerInstance = observer.gameObject;
        }

        observer.damagePerSecond = damagePerSecond;
        observer.duration = burnDuration;
        observer.cooldownPerEnemy = cooldownPerEnemy;
        observer.burnVfxPrefab = burnVfxPrefab;

        // Primera pasada de hooks (el autoscan luego se ocupa del resto)
        observer.AttachHooksToExistingEnemies();

        // El observer se marca DontDestroyOnLoad en su Awake
    }

    public override void Remove(PlayerController player)
    {
        // Si querés que al quitar la perk se destruya el sistema de burn:
        if (BurnOnHitObserver.Instance != null)
        {
            Object.Destroy(BurnOnHitObserver.Instance.gameObject);
        }
        observerInstance = null;
    }
}
