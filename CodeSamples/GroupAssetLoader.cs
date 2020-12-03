using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace TheUpload.Core.Async
{
    /*
    class for loading then reporting progress and result for a group of addressables 
    */
    public class GroupAssetLoader : MonoBehaviour
    {
        private struct LoaderOperationInfo
        {
            public string Name;
            public Action<GameObject> OnComplete;
        }

        public class LoaderOpertaionHandle
        {
            public Action OnCompleted;
        }

        private int _finishedOperations;
        private List<LoaderOperationInfo> _operationInfos;

        public bool IsDone => _operationInfos.Count != 0 && _operationInfos.Count == _finishedOperations;

        public static GroupAssetLoader Init(GameObject obj)
        {
            var comp = obj.AddComponent<GroupAssetLoader>();
            comp._operationInfos = new List<LoaderOperationInfo>();
            return comp;
        }

        public void Add(string name, Action<GameObject> onComplete = null)
        {
            var o = new LoaderOperationInfo() {Name = name, OnComplete = onComplete};
            _operationInfos.Add(o);
        }

        public void Load<T>(string name, Action<T> onComplete = null)
        {
            Addressables.LoadAssetAsync<T>(name).Completed += onLoadDone;

            void onLoadDone(AsyncOperationHandle<T> obj)
            {
                onComplete?.Invoke(obj.Result);
            }
        }

        public LoaderOpertaionHandle LoadAssets()
        {
            foreach (var info in _operationInfos)
            {
                LoadObject(info);
            }
            LoaderOpertaionHandle handle = new LoaderOpertaionHandle();
            StartCoroutine(waitUntilDone());

            IEnumerator waitUntilDone()
            {
                yield return new WaitUntil(()=>IsDone);
                handle.OnCompleted?.Invoke();
                Destroy(this);
            }

            return handle;
        }

        private void LoadObject(LoaderOperationInfo operationInfo)
        {
            Addressables.InstantiateAsync(operationInfo.Name).Completed += objectLoadedHandler;

            void objectLoadedHandler(AsyncOperationHandle<GameObject> handle)
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    operationInfo.OnComplete?.Invoke(handle.Result);
                    _finishedOperations++;
                }
                else
                {
                    Debug.LogError($"Asset loading failure: {handle.DebugName}");
                }
            }
        }
    }
}
