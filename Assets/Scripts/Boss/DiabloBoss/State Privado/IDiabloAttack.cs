public interface IDiabloAttack
{
    bool IsFinished { get; }
    void Start(DiabloController ctrl);
    void Tick(DiabloController ctrl);
    void Stop(DiabloController ctrl);
}

