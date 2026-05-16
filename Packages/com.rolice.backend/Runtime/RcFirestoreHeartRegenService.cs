using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine;

namespace Rolice.System.Backend
{
    /// <summary>
    /// Firestore 기반 하트 시간 회복 서비스.
    ///
    /// Firestore 경로:
    ///   users/{uid}/economy/Heart        → { "balance": N }           (IEconomyService와 공유)
    ///   users/{uid}/data/heart_regen     → { "lastRegenAt": Timestamp }
    ///
    /// SyncRegenAsync / SpendHeartAsync 모두 cross-document 트랜잭션으로 원자 처리.
    /// 쓰기 성공 후 IPlayerDataCache(로컬) 갱신.
    /// </summary>
    public sealed class RcFirestoreHeartRegenService : IHeartRegenService
    {
        private const string UsersCollection  = "users";
        private const string EconomySubcol    = "economy";
        private const string DataSubcol       = "data";
        private const string HeartDocKey      = "Heart";
        private const string RegenDocKey      = "heart_regen";
        private const string BalanceField     = "balance";
        private const string LastRegenAtField = "lastRegenAt";
        private const int    RegenIntervalSec = 600;

        private readonly IPlayerDataCache _cache;

        private FirebaseFirestore Db  => FirebaseFirestore.DefaultInstance;
        private string            Uid => FirebaseAuth.DefaultInstance.CurrentUser?.UserId;

        public RcFirestoreHeartRegenService(IPlayerDataCache cache)
        {
            _cache = cache;
        }

        // ─── SyncRegenAsync ──────────────────────────────────────────────────

        public async UniTask SyncRegenAsync(int maxHearts)
        {
            if (Uid == null)
            {
                Debug.LogWarning("[HeartRegen] SyncRegen 실패: 로그인되지 않음");
                return;
            }

            var heartDocRef = HeartDocRef();
            var regenDocRef = RegenDocRef();

            bool isFirstTime  = false;
            int  heartsAdded  = 0;
            int  newBalance   = 0;

            await Db.RunTransactionAsync(async transaction =>
            {
                isFirstTime = false;
                heartsAdded = 0;

                var heartSnap = await transaction.GetSnapshotAsync(heartDocRef);
                var regenSnap = await transaction.GetSnapshotAsync(regenDocRef);

                int balance = heartSnap.TryGetValue(BalanceField, out long b) ? (int)b : 0;

                // 신규 유저: heart_regen 문서 없음 → 최대치로 초기화
                if (!regenSnap.Exists)
                {
                    isFirstTime = true;
                    newBalance  = maxHearts;

                    transaction.Set(heartDocRef,
                        new Dictionary<string, object> { { BalanceField, (long)maxHearts } },
                        SetOptions.MergeAll);

                    transaction.Set(regenDocRef,
                        new Dictionary<string, object> { { LastRegenAtField, FieldValue.ServerTimestamp } });
                    return;
                }

                // lastRegenAt 없으면 타임스탬프만 초기화하고 종료
                if (!regenSnap.TryGetValue(LastRegenAtField, out Timestamp lastRegenTs))
                {
                    transaction.Set(regenDocRef,
                        new Dictionary<string, object> { { LastRegenAtField, FieldValue.ServerTimestamp } });
                    newBalance = balance;
                    return;
                }

                // 최대치 이상이면 회복 스킵 (초과 구매분 보호)
                if (balance >= maxHearts)
                {
                    newBalance = balance;
                    return;
                }

                DateTime lastRegen      = lastRegenTs.ToDateTime(); // UTC
                double   elapsedSeconds = (DateTime.UtcNow - lastRegen).TotalSeconds;
                int      heartsToAdd    = (int)(elapsedSeconds / RegenIntervalSec);

                if (heartsToAdd <= 0)
                {
                    newBalance = balance;
                    return;
                }

                int canAdd        = Math.Min(heartsToAdd, maxHearts - balance);
                newBalance        = balance + canAdd;
                heartsAdded       = canAdd;
                DateTime newRegen = lastRegen.AddSeconds(canAdd * RegenIntervalSec);

                transaction.Set(heartDocRef,
                    new Dictionary<string, object> { { BalanceField, (long)newBalance } },
                    SetOptions.MergeAll);

                transaction.Set(regenDocRef,
                    new Dictionary<string, object>
                    {
                        { LastRegenAtField, Timestamp.FromDateTime(DateTime.SpecifyKind(newRegen, DateTimeKind.Utc)) }
                    });
            });

            if (isFirstTime)
            {
                _cache.SetCurrency(HeartDocKey, newBalance);
                Debug.Log($"[HeartRegen] 신규 유저 초기화: Heart={maxHearts}");
            }
            else if (heartsAdded > 0)
            {
                _cache.SetCurrency(HeartDocKey, newBalance);
                Debug.Log($"[HeartRegen] 회복 적용: +{heartsAdded} → {newBalance}");
            }
        }

        // ─── SpendHeartAsync ─────────────────────────────────────────────────

        public async UniTask<bool> SpendHeartAsync(int maxHearts)
        {
            if (Uid == null)
            {
                Debug.LogWarning("[HeartRegen] Spend 실패: 로그인되지 않음");
                return false;
            }

            var heartDocRef = HeartDocRef();
            var regenDocRef = RegenDocRef();

            bool insufficient = false;
            bool wasFull      = false;
            int  newBalance   = 0;

            await Db.RunTransactionAsync(async transaction =>
            {
                insufficient = false;
                wasFull      = false;

                var heartSnap = await transaction.GetSnapshotAsync(heartDocRef);
                int balance   = heartSnap.TryGetValue(BalanceField, out long b) ? (int)b : 0;

                if (balance < 1)
                {
                    insufficient = true;
                    return;
                }

                wasFull    = balance >= maxHearts;
                newBalance = balance - 1;

                transaction.Set(heartDocRef,
                    new Dictionary<string, object> { { BalanceField, (long)newBalance } },
                    SetOptions.MergeAll);

                // 최대치에서 소모: 타이머 시작 (서버 타임스탬프)
                if (wasFull)
                {
                    transaction.Set(regenDocRef,
                        new Dictionary<string, object> { { LastRegenAtField, FieldValue.ServerTimestamp } });
                }
            });

            if (insufficient) return false;

            _cache.SetCurrency(HeartDocKey, newBalance);
            Debug.Log($"[HeartRegen] 소모 완료: Heart={newBalance}{(wasFull ? " (타이머 시작)" : "")}");
            return true;
        }

        // ─── GetLastRegenAtAsync ─────────────────────────────────────────────

        public async UniTask<DateTime?> GetLastRegenAtAsync()
        {
            if (Uid == null) return null;

            var snap = await RegenDocRef().GetSnapshotAsync();
            if (!snap.Exists) return null;
            if (!snap.TryGetValue(LastRegenAtField, out Timestamp ts)) return null;

            return ts.ToDateTime(); // UTC
        }

        // ─── 헬퍼 ────────────────────────────────────────────────────────────

        private DocumentReference HeartDocRef()
            => Db.Collection(UsersCollection).Document(Uid)
                 .Collection(EconomySubcol).Document(HeartDocKey);

        private DocumentReference RegenDocRef()
            => Db.Collection(UsersCollection).Document(Uid)
                 .Collection(DataSubcol).Document(RegenDocKey);
    }
}
