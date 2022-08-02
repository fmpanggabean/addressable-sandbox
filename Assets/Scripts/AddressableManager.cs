using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using TMPro;

public class AddressableManager : MonoBehaviour
{
    private AsyncOperationHandle<IResourceLocator> asyncOperationHandle;

    public TMP_Text msg;
    public TMP_Text log;

    private void Awake()
    {
        ClearCache();

        OnAwake();
    }

    private void ClearCache()
    {
        Caching.ClearCache();
        Log($"Cache cleared.");
    }

    private async void OnAwake()
    {
        asyncOperationHandle = Addressables.InitializeAsync(false);
        await asyncOperationHandle.Task;

        float downloadSize = await GetDownloadSizeInKB();
    }

    private async Task<float> GetDownloadSizeInKB()
    {
        long size = 0;

        foreach (var key in asyncOperationHandle.Result.Keys)
        {
            AsyncOperationHandle<long> f = Addressables.GetDownloadSizeAsync(key);
            await f.Task;
            size += f.Result;
        }

        size /= 1024;
        Log($"Download Size: {size} KB");

        return size;
    }

    private void Log(string msg)
    {
        log.text += $"[{DateTime.Now.ToString("hh:mm:ss:ff")}] {msg}\n";
    }
}
