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

    public bool isClearCacheBeforeRun;

    private void Awake()
    {
        if (isClearCacheBeforeRun)
        {
            ClearCache();
        }

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

        AsyncOperationHandle<List<string>> asyncCatalogCheck = Addressables.CheckForCatalogUpdates(false);
        await asyncCatalogCheck.Task;

        Log($"Catalog count: {asyncCatalogCheck.Result.Count}");
        Addressables.Release(asyncCatalogCheck);
    }

    private void Log(string value)
    {
        log.text += $"[{DateTime.Now.ToString("hh:mm:ss:ff")}] {value}\n";
    }

    private void Message(string value)
    {
        msg.text = $"{value}";
    }
}
