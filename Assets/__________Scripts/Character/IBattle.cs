public interface IBattle
{
    float AttackPower { get; }
    void Attack(IBattle target);
    void TakeDamage(float damage);
}
