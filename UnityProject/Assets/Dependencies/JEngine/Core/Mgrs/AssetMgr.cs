using System;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using Object = UnityEngine.Object;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace JEngine.Core
{
    public static class AssetMgr
    {
        private static readonly Dictionary<string, AsyncOperationHandle> AssetCache = new Dictionary<string, AsyncOperationHandle>();
        // private static readonly Dictionary<string, BundleRequest> BundleCache = new Dictionary<string, BundleRequest>();

        // public static bool RuntimeMode => Assets.runtimeMode;
        public static bool RuntimeMode => false;

        public static bool Loggable
        {
            // get => Assets.loggable;
            // set => Assets.loggable = value;
            get;
            set;
        }

        public static string Error(string path)
        {
            return AssetCache.ContainsKey(path) ? (AssetCache[path].Status == AsyncOperationStatus.Failed ? "Failed" : "???") : "";
        }

        public static AsyncOperationStatus State(string path)
        {
            return AssetCache.ContainsKey(path) ? AssetCache[path].Status : AsyncOperationStatus.None;
        }

        public static float Progress(string path)
        {
            return AssetCache.ContainsKey(path) ? AssetCache[path].PercentComplete : 0;
        }
        
        public static T Load<T>(string path) where T:class
        {
            var req = LoadAsyncHandle<T>(path);
            req.WaitForCompletion();
            return req.Result as T;
        }
        
        public static Task<object> LoadAsync(string path)
        {
            var req = LoadAsyncHandle<Object>(path);
            return req.Task;
        }

        public static AsyncOperationHandle LoadAsyncHandle<T>(string key) where T:class
        {
            var res = GetHandleFromCache(key);
            if (res != null) {
                return res.Value;
            }
            var req = Addressables.LoadAssetAsync<T>(key);
            CheckError(key, req);
            AssetCache[key] = req;
            return req;
        }

        public static void Unload(string path, bool ignore = false)
        {
            if (AssetCache.TryGetValue(path, out var req))
            {
                ReleaseAsset(req);
            }
            else if (!ignore)
            {
                Log.PrintError($"Resource '{path}' has not loaded yet");
            }
        }

        public static async void LoadSceneAsync(string path, bool additive, Action<float> loadingCallback = null,
            Action<bool> finishedCallback = null)
        {
            var req = Addressables.LoadSceneAsync(path, additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
            while (!req.IsDone)
            {
                loadingCallback?.Invoke(req.PercentComplete);
                await Task.Delay(10);
            }
            CheckError(path, req);
            finishedCallback?.Invoke(req.Status == AsyncOperationStatus.Succeeded);
        }

        public static AssetBundle LoadBundle(string path)
        {
            // var res = GetBundleFromCache(path);
            // if (res != null)
            // {
            //     return res;
            // }
            // var req = Assets.LoadBundle(path);
            // CheckError(path, req);
            // BundleCache[path] = req;
            // return req.assetBundle;
            return null;
        }
        
        public static Task<AssetBundle> LoadBundleAsync(string path)
        {
            // var res = GetBundleFromCache(path);
            // var tcs = new TaskCompletionSource<AssetBundle>();
            // if (res != null)
            // {
            //     tcs.SetResult(res);
            //     return tcs.Task;
            // }
            // var req = Assets.LoadBundleAsync(path);
            // req.completed += ar =>
            // {
            //     CheckError(path, req);
            //     AssetCache[path] = ar;
            //     tcs.SetResult(((BundleRequest) ar).assetBundle);
            // };
            // return tcs.Task;
            return null;
        }

        public static void UnloadBundle(string path, bool ignore = false)
        {
            // if (BundleCache.TryGetValue(path, out var req))
            // {
            //     ReleaseAsset(req);
            // }
            // else if (!ignore)
            // {
            //     Log.PrintError($"Bundle '{path}' has not loaded yet");
            // }
        }

        public static void RemoveUnusedAssets()
        {
            // Assets.RemoveUnusedAssets();
        }
        
        private static AsyncOperationHandle? GetHandleFromCache(string path)
        {
            if (AssetCache.TryGetValue(path, out var v)) {
                return v;
            }
            return null;
        }

        private static AssetBundle GetBundleFromCache(string path)
        {
            // if (BundleCache.TryGetValue(path, out var v))
            // {
            //     return v.assetBundle;
            // }

            // return null;
            return null;

        }

        private static Type CheckType(Type t)
        {
            if (t == null) return typeof(Object);
            return t;
        }

        private static void CheckError(string path, AsyncOperationHandle req)
        {
            if (req.IsDone && req.Status == AsyncOperationStatus.Failed)
            {
                Log.PrintError($"Resource '{path}' load failed: {req.OperationException}");
            }
        }

        private static void ReleaseAsset(AsyncOperationHandle req)
        {
            Addressables.Release(req);
        }
    }
}