using Cysharp.Threading.Tasks;
using Rolice.Define;
using Rolice.System.Backend;
using Rolice.System.Economy;
using UnityEngine;

/// <summary>
/// 아이템 구매 시 발동되는 효과의 추상 정의.
/// [SerializeReference]로 ShopRow 안에 인라인 직렬화됨.
/// </summary>
public interface IItemEffect
{
    /// <summary>동일 아이템을 반복 구매할 수 있는지.</summary>
    bool IsRepurchasable { get; }

    /// <summary>구매 확정 시 효과 발동.</summary>
    UniTask ApplyAsync(uint itemId);
}

/// <summary>
/// 아이템을 보유 목록에 추가. 스킨류에 사용.
/// 가방에서 별도로 장착 가능.
/// </summary>
[System.Serializable]
public class RcOwnItemEffect : IItemEffect
{
    public bool IsRepurchasable => false;

    public UniTask ApplyAsync(uint itemId)
        => RcBackendServices.Economy.AddItemAsync(itemId.ToString());
}

/// <summary>
/// 구매 즉시 재화 지급. 재화 상품에 사용.
/// </summary>
[System.Serializable]
public class RcAddCurrencyEffect : IItemEffect
{
    [SerializeField] public RcCurrencyId CurrencyId;
    [SerializeField] public int          Amount;

    public bool IsRepurchasable => true;

    public UniTask ApplyAsync(uint itemId)
        => RcBackendServices.Economy.AddAsync(CurrencyId.ToKey(), Amount);
}
