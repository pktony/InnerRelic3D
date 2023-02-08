/// <summary>
/// 캐릭터가 공통으로 가지는 전투관련 프로퍼티, 델리게이트 함수 
/// </summary>
public interface IBattle
{
    float AttackPower { get; }
    void Attack(IBattle target);
    void TakeDamage(float damage);

    void ParryAction();
    public bool IsParry { get; }
}
