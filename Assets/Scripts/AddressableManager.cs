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
    public TMP_InputField chatBox;

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

        await UpdateCheck();
    }

    private async Task UpdateCheck()
    {
        AsyncOperationHandle<List<string>> asyncCatalogCheck = Addressables.CheckForCatalogUpdates(false);
        await asyncCatalogCheck.Task;

        Log($"Catalog count: {asyncCatalogCheck.Result.Count}");

        if (asyncCatalogCheck.Result.Count > 0)
        {
            Log($"Updating catalog");
            await UpdateCatalog(asyncCatalogCheck.Result);
        }
        else
        {
            Log($"Catalog already up to date!");
        }

        Addressables.Release(asyncCatalogCheck);
    }

    private async Task UpdateCatalog(List<string> result)
    {
        AsyncOperationHandle asyncUpdate = Addressables.UpdateCatalogs(result, false);
        //await asyncUpdate.Task;

        while(asyncUpdate.PercentComplete < 1f)
        {
            Message($"Load {asyncUpdate.PercentComplete*100}%");
            await Task.Yield();
        }
        Message($"Load {asyncUpdate.PercentComplete * 100}%");

        Log($"Catalog update completed!");
        Addressables.Release(asyncUpdate);
    }

    private void Log(string value)
    {
        log.text += $"[{DateTime.Now.ToString("hh:mm:ss:ff")}] {value}\n";
        Debug.Log($"[{DateTime.Now.ToString("hh:mm:ss:ff")}] {value}\n");
    }

    private void Message(string value)
    {
        msg.text = $"{value}";
    }

    public async Task InstantiateAsset(string path)
    {        
        Vector3 randomPosition = new Vector3(
            UnityEngine.Random.Range(-4, 4),
            0,
            UnityEngine.Random.Range(-4, 4)
            );

        AsyncOperationHandle<GameObject> asyncInstantiate;
        asyncInstantiate  = Addressables.InstantiateAsync(path, randomPosition, Quaternion.identity);
        //await asyncInstantiate.Task;
        while (asyncInstantiate.PercentComplete < 1f)
        {
            Message($"Load {path} {asyncInstantiate.PercentComplete * 100}%");
            await Task.Yield();
        }

        Log($"{asyncInstantiate.Result.name} instantiated");
    }

    public async void CreateObject()
    {
        await InstantiateAsset(chatBox.text);

        chatBox.text = "";
    }
}
