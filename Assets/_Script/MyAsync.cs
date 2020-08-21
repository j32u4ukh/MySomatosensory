using System;
using System.Runtime.CompilerServices;
using UnityAsync.Awaiters;
using UnityEngine;
using UnityEngine.Networking;

namespace ETLab
{
    public static class MyAsync
    {
        // UnityWebRequestAsyncOperation 的方法擴充 for await request.SendWebRequest();
        public static UnityWebRequestAsyncOperationAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOperation)
        {
            return new UnityWebRequestAsyncOperationAwaiter(asyncOperation);
        }

        // ResourceRequest 的方法擴充
        public static ResourceRequestAwaiter GetAwaiter(this ResourceRequest asyncOperation)
        {
            return new ResourceRequestAwaiter(asyncOperation);
        }
    }

    public class UnityWebRequestAsyncOperationAwaiter : INotifyCompletion
    {
        UnityWebRequestAsyncOperation _asyncOperation;

        public bool IsCompleted
        {
            get
            {
                return _asyncOperation.isDone;
            }
        }

        public UnityWebRequestAsyncOperationAwaiter(UnityWebRequestAsyncOperation asyncOperation)
        {
            _asyncOperation = asyncOperation;
        }

        // NOTE: 結果はUnityWebRequestからアクセスできるので、ここで返す必要性は無い
        public void GetResult()
        {

        }

        // await 後的部分會封裝成一個 Action continuation 在此執行
        public void OnCompleted(Action continuation)
        {
            _asyncOperation.completed += _ => { continuation(); };
        }
    }
}
