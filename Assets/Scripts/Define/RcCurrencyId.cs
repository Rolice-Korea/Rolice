namespace Rolice.Define
{
    public enum RcCurrencyId
    {
        Star  = 0,
        Heart = 1,
        Gem   = 2,
    }

    public static class RcCurrencyIdExtensions
    {
        /// <summary>
        /// 저장 키 변환. int 값이 아닌 이름 문자열로 저장해 세이브 안전성 확보.
        /// </summary>
        public static string ToKey(this RcCurrencyId id) => id.ToString();
    }
}
