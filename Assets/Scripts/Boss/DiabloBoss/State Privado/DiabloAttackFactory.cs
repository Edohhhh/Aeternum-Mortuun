public static class DiabloAttackFactory
{
    public static IDiabloAttack Create(int id)
    {
        switch (id)
        {
            case 1: return new DiabloAttack_XPlus();
            case 2: return new DiabloAttack_Chess();
            case 3: return new DiabloAttack_SpawnTornado();
            case 4: return new DiabloAttack_Walls();
            case 5: return new DiabloAttack_RotatingX();
            case 6: return new DiabloAttack_AirPunch();
            default: return new DiabloAttack_Placeholder("Default");
        }
    }
}

// Placeholder mínimo para que compile y puedas probar el flujo
public class DiabloAttack_Placeholder : IDiabloAttack
{
    private float t;
    private readonly string name;
    public bool IsFinished { get; private set; }

    public DiabloAttack_Placeholder(string n) { name = n; }

    public void Start(DiabloController ctrl)
    {
        t = 0f;
        IsFinished = false;
        // Debug.Log($"[DIABLO] Ataque placeholder: {name}");
    }

    public void Tick(DiabloController ctrl)
    {
        t += UnityEngine.Time.deltaTime;
        if (t >= 0.6f) IsFinished = true; // dura 0.6s y termina
    }

    public void Stop(DiabloController ctrl)
    {
        IsFinished = true;
    }
}
