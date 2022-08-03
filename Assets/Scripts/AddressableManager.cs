using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using TMPro;
using UnityEngine.ResourceManagement.ResourceLocations;

public partial class AddressableManager : MonoBehaviour
{
    //private AsyncOperationHandle<IResourceLocator> asyncOperationHandle;
    private IResourceLocator resourceLocator;

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
        resourceLocator = await Addressables.InitializeAsync(true).Task;

        await CheckCatalogUpdate();
        if (catalogForUpdate.Count > 0)
        {
            await UpdateCatalog();
        }
        
        await GetDownloadSize();
    }

    private async Task GetDownloadSize()
    {
        long size = 0;

        foreach (var locator in Addressables.ResourceLocators)
        {
            foreach (var key in locator.Keys)
            {
                AsyncOperationHandle<long> asyncDownloadSize = Addressables.GetDownloadSizeAsync(key);
                await asyncDownloadSize.Task;
                if (asyncDownloadSize.Result > 0)
                {
                    Log($"{key} with download size of {asyncDownloadSize.Result} B");
                }
                size += asyncDownloadSize.Result;
            }
        }

        Log($"Download size: {size} B");
    }

    private async Task DownloadDependencies()
    {
        foreach(var locator in updatedLocators)
        {
            foreach(var key in locator.Keys)
            {
                Log($"Attempting to download {key}...");
                AsyncOperationHandle asyncDownload = Addressables.DownloadDependenciesAsync(key);

                DateTime start = DateTime.Now;
                while (!asyncDownload.IsDone)
                {
                    Message($"Download {key} {asyncDownload.PercentComplete*100}%");
                    await Task.Yield();
                }
                Message($"Download {key} {asyncDownload.PercentComplete * 100}%");

                DateTime finished = DateTime.Now;
                Log($"Download complete for {key} in {finished - start} seconds");
            }
        }
    }






    public async Task InstantiateAsset(string key)
    {        
        Vector3 randomPosition = new Vector3(
            UnityEngine.Random.Range(-4, 4),
            0,
            UnityEngine.Random.Range(-4, 4)
            );

        AsyncOperationHandle<GameObject> asyncInstantiate;
        asyncInstantiate  = Addressables.InstantiateAsync(key, randomPosition, Quaternion.identity);
        await asyncInstantiate.Task;

        Log($"{asyncInstantiate.Result.name} instantiated");
    }

    public async void GetTextCommand()
    {
        string input = chatBox.text;

        if (input.Contains("/create "))
        {
            input = input.Split("/create ")[1];
            Log($"{input}");
            await InstantiateAsset(input);
        } else if (input.Contains("/update"))
        {
            await DownloadDependencies();
        }

        chatBox.text = "";
    }
}
