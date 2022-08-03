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
    private Dictionary<string, AsyncOperationHandle<GameObject>> asyncGameObjectListInstantiated = new Dictionary<string, AsyncOperationHandle<GameObject>>();

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
                //if (asyncDownloadSize.Result > 0)
                //{
                //    Log($"{key} with download size of {asyncDownloadSize.Result} B");
                //}
                size += asyncDownloadSize.Result;
            }
        }

        Log($"Download size: {size/1024f} KB. Type update to download additional files.");
    }

    private async Task DownloadDependencies()
    {
        foreach(var locator in Addressables.ResourceLocators)
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






    public async void InstantiateObjectAtRandom(string key)
    {
        Vector3 randomPosition = new Vector3(
            UnityEngine.Random.Range(-4, 4),
            0,
            UnityEngine.Random.Range(-4, 4)
            );

        await InstantiateAsset(key, randomPosition, Quaternion.identity);
    }
    public async Task InstantiateAsset(string key, Vector3 position, Quaternion rotation)
    { 
        AsyncOperationHandle<GameObject> asyncInstantiate = Addressables.InstantiateAsync(key, position, rotation);
        await asyncInstantiate.Task;

        asyncGameObjectListInstantiated.Add(key, asyncInstantiate);

        Log($"{asyncInstantiate.Result.name} instantiated");
    }

    public void RemoveInstantiatedObject(string key)
    {
        if (asyncGameObjectListInstantiated.ContainsKey(key) == false)
        {
            Log($"Delete failed");
            return;
        }

        string deletedName = asyncGameObjectListInstantiated[key].Result.name;
        Addressables.Release(asyncGameObjectListInstantiated[key]);
        
        Log($"{deletedName} deleted");

    }

    public async void GetTextCommand()
    {
        var input = chatBox.text.Split("#");

        if (input[0].Equals("create"))
        {
            InstantiateObjectAtRandom(input[1]);
        } 
        else if (input[0].Equals("update"))
        {
            await DownloadDependencies();
        } 
        else if (input[0].Equals("delete"))
        {
            RemoveInstantiatedObject(input[1]);
        }

        chatBox.text = "";
    }
}
