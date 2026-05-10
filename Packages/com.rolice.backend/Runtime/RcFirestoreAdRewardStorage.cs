using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine;

namespace Rolice.System.Backend
{
    /// <summary>
    /// Firestore 서버 타임스탬프 기반 광고 보상 이력 저장소.
    /// 경로: users/{uid}/data/ad_last_reward → { "recordedAt": ServerTimestamp }
    /// </summary>
    public sealed class RcFirestoreAdRewardStorage : IAdRewardStorage
    {
        private const string CollectionName = "users";
        private const string DocKey         = "ad_last_reward";
        private const string TimeField      = "recordedAt";

        private FirebaseFirestore Db  => FirebaseFirestore.DefaultInstance;
        private string            Uid => FirebaseAuth.DefaultInstance.CurrentUser?.UserId;

        public async UniTask RecordRewardAsync()
        {
            if (Uid == null)
            {
                Debug.LogWarning("[AdReward] 기록 실패: 로그인되지 않음");
                return;
            }

            var docRef = Db.Collection(CollectionName).Document(Uid)
                           .Collection("data").Document(DocKey);

            await docRef.SetAsync(new Dictionary<string, object>
            {
                { TimeField, FieldValue.ServerTimestamp }
            });

            Debug.Log("[AdReward] 수령 시각 기록 완료 (서버 타임스탬프)");
        }

        public async UniTask<DateTime?> GetLastRewardTimeAsync()
        {
            if (Uid == null) return null;

            var docRef   = Db.Collection(CollectionName).Document(Uid)
                             .Collection("data").Document(DocKey);
            var snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists) return null;

            if (!snapshot.TryGetValue(TimeField, out Timestamp ts)) return null;

            return ts.ToDateTime(); // UTC DateTime
        }

        public async UniTask ClearAsync()
        {
            if (Uid == null) return;

            var docRef = Db.Collection(CollectionName).Document(Uid)
                           .Collection("data").Document(DocKey);
            await docRef.DeleteAsync();
            Debug.Log("[AdReward] 수령 이력 초기화 완료");
        }
    }
}
