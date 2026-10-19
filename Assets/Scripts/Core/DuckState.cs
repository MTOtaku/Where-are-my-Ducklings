// Isso aqui é tudo pro animator, ja pensando na transição de estados. não precisa mudar
public enum DuckState {
    Idle = 0, Balancing = 1, Fallen = 2, GettingUp = 3,
    Airborne = 10,
    FlyNeutral = 20, FlyGlide = 21, FlyDive = 22,
    Floating = 30, Aiming = 31, Diving = 32
}