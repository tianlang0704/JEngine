//
// Updater.cs
//
// Author:
//       fjy <jiyuan.feng@live.com>
//
// Copyright (c) 2020 fjy
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using JEngine.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace JEngine
{
    public interface IUpdater
    {
        void OnStart();

        void OnMessage(string msg);

        void OnProgress(float progress);

        void OnVersion(string ver);

        void OnClear();
    }

    [RequireComponent(typeof(NetworkMonitor))]
    public class Updater : MonoBehaviour, IUpdater, INetworkMonitorListener
    {
        enum Step
        {
            Wait,
            Copy,
            Coping,
            Versions,
            Prepared,
            Download,
        }

        private Step _step;

        [SerializeField] private string gameScene = "Assets/HotUpdateResources/Scene/Game.unity";
        [Tooltip("离线模式")] [SerializeField] public bool offline;
        public static Action<string,Action<float>> OnAssetsInitialized;
        public IUpdater listener { get; set; }
        private NetworkMonitor _monitor;

        private void Start()
        {
            _monitor = gameObject.GetComponent<NetworkMonitor>();
            _monitor.listener = this;
        }
        
        public void StartUpdate()
        {
            OnStart();
            Checking();
        }
        
        public void OnStart()
        {
            if (listener != null) {
                listener.OnStart();
            }
        }
        
        private async void LoadGameScene()
        {
            OnMessage("正在初始化");
            var handle = Addressables.InitializeAsync(true);
            await handle.Task;
            if (handle.Status == AsyncOperationStatus.Succeeded) {
                OnProgress(0);
                OnMessage("加载游戏场景");
                OnAssetsInitialized?.Invoke(gameScene, OnProgress);
            } else {
                var mb = MessageBox.Show("提示", "初始化异常错误：" + handle.OperationException + "请联系技术支持");
                await mb;
                Quit();
            }
        }

        private void OnDestroy()
        {
            MessageBox.Dispose();
        }

        private void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private bool _reachabilityChanged;
        public void OnReachablityChanged(NetworkReachability reachability)
        {
            if (_step == Step.Wait) {
                return;
            }
            _reachabilityChanged = true;

            if (reachability == NetworkReachability.NotReachable) {
                MessageBox.Show("提示！", "找不到网络，请确保手机已经联网", "确定", "退出").onComplete += delegate(MessageBox.EventId id) {
                    if (id == MessageBox.EventId.Ok) {
                        StartUpdate();
                        _reachabilityChanged = false;
                    } else {
                        Quit();
                    }
                };
            } else {
                StartUpdate();
                _reachabilityChanged = false;
                MessageBox.CloseAll();
            }
        }
        
        private void OnUpdate(long progress, long size, float speed)
        {
            OnMessage(
                string.Format("下载中...{0}/{1}, 速度：{2}",
                    progress,
                    size,
                    speed));

            OnProgress(progress * 1f / size);
        }

        public void Clear()
        {
            MessageBox.Show("提示", "清除数据后所有数据需要重新下载，请确认！", "清除").onComplete += id => {
                if (id != MessageBox.EventId.Ok)
                    return;
                OnClear();
            };
        }

        public void OnClear()
        {
            OnMessage("数据清除完毕");
            OnProgress(0);
            _step = Step.Wait;
            _reachabilityChanged = false;
            Caching.ClearCache();

            if (listener != null) {
                listener.OnClear();
            }
        }

        private async void Checking()
        {
            if (_step == Step.Wait) {
                _step = Step.Copy;
            }

            if (_step == Step.Copy) {
                _step = Step.Coping;
            }

            if (_step == Step.Coping) {
                _step = Step.Versions;
            }

            List<string> toUpdate = null;
            if (_step == Step.Versions) {
                toUpdate = await RequestVersions();
                _step = Step.Prepared;
            }

            if (_step == Step.Prepared) {
                Download(toUpdate);
            }
        }

        private async Task<List<string>> RequestVersions()
        {
            if (offline) {
                OnComplete();
                return null;
            }
            OnMessage("正在获取版本信息...");
            if (Application.internetReachability == NetworkReachability.NotReachable) {
                var mb = MessageBox.Show("提示", "请检查网络连接状态", "重试", "退出");
                await mb;
                if (mb.isOk) {
                    StartUpdate();
                } else {
                    Quit();
                }

                return null;
            }
            var catalogHandle = Addressables.CheckForCatalogUpdates(false);
            await catalogHandle.Task;
            if (catalogHandle.Status != AsyncOperationStatus.Succeeded) {
                var mb = MessageBox.Show("提示", string.Format("获取服务器版本失败：{0}", catalogHandle.OperationException), "重试", "退出");
                await mb;
                if (mb.isOk) {
                    StartUpdate();
                } else {
                    Quit();
                }

                return null;
            }
            return catalogHandle.Result;
        }

        private async void Download(List<string> toUpdate)
        {
            OnMessage("正在检查版本信息...");
            var sizeHandle = Addressables.GetDownloadSizeAsync(toUpdate);
            await sizeHandle.Task;
            var totalSize = sizeHandle.Result;
            if (totalSize > 0) {
                var tips = string.Format("发现内容更新，总计需要下载 {0} 内容", totalSize);
                var mb = MessageBox.Show("提示", tips, "下载", "退出");
                await mb;
                if (mb.isOk) {
                    _step = Step.Download;
                    var downloadHandle = Addressables.DownloadDependenciesAsync(toUpdate);
                    long lastBytes = 0;
                    int interval = 100;
                    int factor = 1000 / interval;
                    while (downloadHandle.IsDone) {
                        var status = downloadHandle.GetDownloadStatus();
                        var deltaBytes = (status.DownloadedBytes - lastBytes) * factor;
                        OnUpdate(status.DownloadedBytes, totalSize, deltaBytes);
                        await Task.Delay(interval);
                    }
                    OnComplete();
                } else {
                    Quit();
                }
            } else {
                OnComplete();
            }
        }

        public void OnMessage(string msg)
        {
            if (listener != null) {
                listener.OnMessage(msg);
            }
        }

        public void OnProgress(float progress)
        {
            if (listener != null) {
                listener.OnProgress(progress);
            }
        }

        public void OnVersion(string ver)
        {
            if (listener != null) {
                listener.OnVersion(ver);
            }
        }

        private void OnComplete()
        {
            OnProgress(1);
            OnMessage("更新完成");
            LoadGameScene();
        }
    }
}
