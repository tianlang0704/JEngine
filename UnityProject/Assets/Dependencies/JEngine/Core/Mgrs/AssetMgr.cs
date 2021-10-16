using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VEngine;
using Logger = VEngine.Logger;
using Object = UnityEngine.Object;

namespace JEngine.Core
{
    public static class AssetMgr
    {
        private static readonly Dictionary<string, Asset> AssetCache = new Dictionary<string, Asset>();

        public static bool RuntimeMode => !Versions.SimulationMode;

        public static bool Loggable
        {
            get => Logger.Loggable;
            set => Logger.Loggable = value;
        }
        
        public static Object Load(string path,Type type = null)
        {
            var res = GetAssetFromCache(path);
            if (res != null)
            {
                return res;
            }
            type = CheckType(type);
            var req = Asset.Load(path, type);
            CheckError(path, req);
            AssetCache[path] = req;
            return req.asset;
        }
        
        public static Task<Object> LoadAsync(string path,Type type = null)
        {
            var res = GetAssetFromCache(path);
            var tcs = new TaskCompletionSource<Object>();
            if (res != null)
            {
                tcs.SetResult(res);
                return tcs.Task;
            }
            type = CheckType(type);
            var req = Asset.LoadAsync(path, type);
            req.completed += ar =>
            {
                CheckError(path, req);
                AssetCache[path] = ar;
                tcs.SetResult(ar.asset);
            };
            return tcs.Task;
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
            var req = Scene.LoadAsync(path, null, additive);
            while (!req.isDone)
            {
                loadingCallback?.Invoke(req.progress);
                await Task.Delay(10);
            }
            CheckError(path, req);
            finishedCallback?.Invoke(string.IsNullOrEmpty(req.error));
        } 

        private static Object GetAssetFromCache(string path)
        {
            if (AssetCache.TryGetValue(path, out var v))
            {
                return v.asset;
            }

            return null;
        }

        
        private static Type CheckType(Type t)
        {
            if (t == null) return typeof(Object);
            return t;
        }

        private static void CheckError(string path, Loadable req)
        {
            if (req.isDone && !string.IsNullOrEmpty(req.error))
            {
                Log.PrintError($"Error when loading '{path}': {req.error}");
            }
        }

        private static void ReleaseAsset(Asset req)
        {
            req.Release();
        }
    }
}