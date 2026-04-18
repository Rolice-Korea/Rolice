using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine;

namespace Rolice.System.Backend
{
    /// <summary>
    /// Firestore 기반 서버 권위 이코노미.
    ///
    /// Firestore 경로:
    ///   users/{uid}/economy/{currencyKey} → { "balance": N }
    ///   users/{uid}/economy/items         → { "{itemId}": true }
    ///
    /// 쓰기는 서버에 먼저 반영, 성공 후 로컬 캐시(IPlayerDataCache) 갱신.
    /// 읽기는 로컬 캐시에서 즉시 반환.
    /// </summary>
    public sealed class RcFirestoreEconomyService : IEconomyService
    {
        private const string CollectionName = "users";
        private const string EconomySubcol  = "economy";
        private const string BalanceField   = "balance";
        private const string ItemsDocKey    = "items";

        private readonly IPlayerDataCache _cache;

        private FirebaseFirestore Db  => FirebaseFirestore.DefaultInstance;
        private string            Uid => FirebaseAuth.DefaultInstance.CurrentUser?.UserId;

        public RcFirestoreEconomyService(IPlayerDataCache cache)
        {
            _cache = cache;
        }

        // ─── 재화 ────────────────────────────────────────────────────────────

        public int GetBalance(string currencyKey)
            => _cache.GetCurrency(currencyKey);

        public async UniTask AddAsync(string currencyKey, int amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "amount는 양수여야 합니다.");

            await CurrencyDocRef(currencyKey).SetAsync(
                new Dictionary<string, object> { { BalanceField, FieldValue.Increment(amount) } },
                SetOptions.MergeAll
            );

            _cache.SetCurrency(currencyKey, _cache.GetCurrency(currencyKey) + amount);

            Debug.Log($"[Economy] 추가 완료: {currencyKey} +{amount}");
        }

        public async UniTask<bool> SpendAsync(string currencyKey, int amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "amount는 양수여야 합니다.");

            var  docRef      = CurrencyDocRef(currencyKey);
            bool insufficient = false;
            int  newBalance   = 0;

            await Db.RunTransactionAsync(async transaction =>
            {
                var  snapshot = await transaction.GetSnapshotAsync(docRef);
                long balance  = snapshot.TryGetValue(BalanceField, out long b) ? b : 0;

                if (balance < amount) { insufficient = true; return; }

                newBalance = (int)(balance - amount);
                transaction.Update(docRef, new Dictionary<string, object>
                {
                    { BalanceField, FieldValue.Increment(-amount) }
                });
            });

            if (insufficient) return false;

            _cache.SetCurrency(currencyKey, newBalance);

            Debug.Log($"[Economy] 차감 완료: {currencyKey} -{amount} → {newBalance}");
            return true;
        }

        // ─── 아이템 ──────────────────────────────────────────────────────────

        public bool HasItem(string itemId)
            => _cache.HasOwnedItem(itemId);

        public async UniTask AddItemAsync(string itemId)
        {
            await ItemsDocRef().SetAsync(
                new Dictionary<string, object> { { itemId, true } },
                SetOptions.MergeAll
            );

            _cache.AddOwnedItem(itemId);

            Debug.Log($"[Economy] 아이템 추가 완료: {itemId}");
        }

        // ─── 스타트업 동기화 ─────────────────────────────────────────────────

        public async UniTask SyncFromCloudAsync()
        {
            var snapshot = await Db.Collection(CollectionName).Document(Uid)
                                   .Collection(EconomySubcol).GetSnapshotAsync();

            foreach (var doc in snapshot.Documents)
            {
                if (doc.Id == ItemsDocKey)
                {
                    foreach (var field in doc.ToDictionary())
                        if (field.Value is bool owned && owned)
                            _cache.AddOwnedItem(field.Key);
                }
                else
                {
                    if (doc.TryGetValue(BalanceField, out long balance))
                        _cache.SetCurrency(doc.Id, (int)balance);
                }
            }

            Debug.Log("[Economy] 클라우드 동기화 완료");
        }

        // ─── 헬퍼 ────────────────────────────────────────────────────────────

        private DocumentReference CurrencyDocRef(string currencyKey)
            => Db.Collection(CollectionName).Document(Uid)
                 .Collection(EconomySubcol).Document(currencyKey);

        private DocumentReference ItemsDocRef()
            => Db.Collection(CollectionName).Document(Uid)
                 .Collection(EconomySubcol).Document(ItemsDocKey);
    }
}
