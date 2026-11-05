public interface IMovementMode {
    DuckState State { get; }
    void Enter();
    void Exit();
    void Tick(float dt);
}