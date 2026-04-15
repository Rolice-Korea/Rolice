using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Firebase.Auth;
using Firebase.Firestore;
using Rolice.System.Backend;
using UnityEngine;

namespace Rolice.System.Economy
{
    /// <summary>
    /// Firestore 기반 서버 권위 이코노미.
    ///
    /// Firestore 경로:
    ///   users/{uid}/economy/{currencyKey} → { "balance": N }
    ///   users/{uid}/economy/items         → { "{itemId}": true }
    ///
    /// 쓰기는 서버에 먼저 반영, 성공 후 로컬 캐시(RcPlayerData) 갱신.
    /// 읽기는 로컬 캐시에서 즉시 반환.
    /// </summary>
    public sealed class RcFirestoreEconomyService : IEconomyService
    {
        private const string CollectionName = "users";
        private const string EconomySubcol  = "economy";
        private const string BalanceField   = "balance";
        private const string ItemsDocKey    = "items";

        private readonly RcPlayerState _playerState;

        private FirebaseFirestore Db  => FirebaseFirestore.DefaultInstance;
        private string            Uid => FirebaseAuth.DefaultInstance.CurrentUser?.UserId;

        public RcFirestoreEconomyService(RcPlayerState playerState)
        {
            _playerState = playerState;
        }

        // ─── 재화 ────────────────────────────────────────────────────────────

        public int GetBalance(string currencyKey)
            => _playerState.Data.GetCurrency(currencyKey);

        public async UniTask AddAsync(string currencyKey, int amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "amount는 양수여야 합니다.");

            var docRef = CurrencyDocRef(currencyKey);

            await docRef.SetAsync(
                new Dictionary<string, object> { { BalanceField, FieldValue.Increment(amount) } },
                SetOptions.MergeAll
            );

            // 서버 반영 성공 → 로컬 캐시 갱신
            int current = _playerState.Data.GetCurrency(currencyKey);
            _playerState.Data.SetCurrency(currencyKey, current + amount);
            _playerState.SaveLocal();

            Debug.Log($"[Economy] 추가 완료: {currencyKey} +{amount} → {current + amount}");
        }

        public async UniTask<bool> SpendAsync(string currencyKey, int amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "amount는 양수여야 합니다.");

            var docRef        = CurrencyDocRef(currencyKey);
            bool insufficient = false;
            int  newBalance   = 0;

            await Db.RunTransactionAsync(async transaction =>
            {
                var snapshot = await transaction.GetSnapshotAsync(docRef);
                long balance = snapshot.TryGetValue(BalanceField, out long b) ? b : 0;

                if (balance < amount)
                {
                    insufficient = true;
                    return;
                }

                newBalance = (int)(balance - amount);
                transaction.Update(docRef, new Dictionary<string, object>
                {
                    { BalanceField, FieldValue.Increment(-amount) }
                });
            });

            if (insufficient) return false;

            // 서버 반영 성공 → 로컬 캐시 갱신
            _playerState.Data.SetCurrency(currencyKey, newBalance);
            _playerState.SaveLocal();

            Debug.Log($"[Economy] 차감 완료: {currencyKey} -{amount} → {newBalance}");
            return true;
        }

        // ─── 아이템 ──────────────────────────────────────────────────────────

        public bool HasItem(string itemId)
            => _playerState.Data.HasOwnedItem(itemId);

        public async UniTask AddItemAsync(string itemId)
        {
            var docRef = ItemsDocRef();

            await docRef.SetAsync(
                new Dictionary<string, object> { { itemId, true } },
                SetOptions.MergeAll
            );

            // 서버 반영 성공 → 로컬 캐시 갱신
            _playerState.Data.AddOwnedItem(itemId);
            _playerState.SaveLocal();

            Debug.Log($"[Economy] 아이템 추가 완료: {itemId}");
        }

        // ─── 스타트업 동기화 ─────────────────────────────────────────────────

        /// <summary>
        /// 앱 시작 시 Firestore에서 재화·아이템 데이터를 로컬 캐시에 반영한다.
        /// RcPlayerState.SyncFromCloudAsync() 내부에서 호출된다.
        /// </summary>
        public async UniTask SyncFromCloudAsync()
        {
            var economyCol = Db.Collection(CollectionName).Document(Uid)
                               .Collection(EconomySubcol);

            var snapshot = await economyCol.GetSnapshotAsync();

            foreach (var doc in snapshot.Documents)
            {
                if (doc.Id == ItemsDocKey)
                {
                    foreach (var field in doc.ToDictionary())
                        if (field.Value is bool owned && owned)
                            _playerState.Data.AddOwnedItem(field.Key);
                }
                else
                {
                    if (doc.TryGetValue(BalanceField, out long balance))
                        _playerState.Data.SetCurrency(doc.Id, (int)balance);
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
