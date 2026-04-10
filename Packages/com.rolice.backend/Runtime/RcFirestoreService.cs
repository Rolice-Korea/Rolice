using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine;

namespace Rolice.System.Backend
{
    /// <summary>
    /// Firestore 기반 클라우드 저장소.
    /// 경로: users/{uid}/{key} → { "value": json }
    /// </summary>
    public sealed class RcFirestoreService : ICloudSyncService
    {
        private const string CollectionName = "users";
        private const string FieldName      = "value";

        private FirebaseFirestore Db  => FirebaseFirestore.DefaultInstance;
        private string            Uid => FirebaseAuth.DefaultInstance.CurrentUser?.UserId;

        public async UniTask SaveAsync(string key, string json)
        {
            if (Uid == null)
            {
                Debug.LogWarning("[Firestore] 저장 실패: 로그인되지 않음");
                return;
            }

            var docRef = Db.Collection(CollectionName).Document(Uid)
                           .Collection("data").Document(key);

            await docRef.SetAsync(new Dictionary<string, object>
            {
                { FieldName, json }
            });

            Debug.Log($"[Firestore] 저장 완료: {key}");
        }

        public async UniTask<string> LoadAsync(string key)
        {
            if (Uid == null)
            {
                Debug.LogWarning("[Firestore] 로드 실패: 로그인되지 않음");
                return null;
            }

            var docRef   = Db.Collection(CollectionName).Document(Uid)
                             .Collection("data").Document(key);
            var snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                Debug.Log($"[Firestore] 데이터 없음: {key}");
                return null;
            }

            snapshot.TryGetValue(FieldName, out string json);
            Debug.Log($"[Firestore] 로드 완료: {key}");
            return json;
        }
    }
}
