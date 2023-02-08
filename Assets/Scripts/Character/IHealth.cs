/// <summary>
/// 플레이어 및 적이 공통으로 가지는 체력관련 프로퍼티, 델리게이트
/// </summary>
public interface IHealth
{
    float HP { get; set; }
    float MaxHP { get; }

    public System.Action onHealthChange { get; set; }
}
