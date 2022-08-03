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
using UnityEngine.SceneManagement;
using UnityEngine.ResourceManagement.ResourceProviders;

public partial class AddressableManager : MonoBehaviour
{
    public static AddressableManager Instance;

    private IResourceLocator resourceLocator;
    private Dictionary<string, AsyncOperationHandle<GameObject>> asyncGameObjectListInstantiated = new Dictionary<string, AsyncOperationHandle<GameObject>>();
    private Dictionary<string, AsyncOperationHandle<SceneInstance>> asyncSceneDictionary = new Dictionary<string, AsyncOperationHandle<SceneInstance>>();

    public TMP_Text msg;
    public TMP_Text log;
    public TMP_InputField chatBox;

    public bool isClearCacheBeforeRun;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

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
                size += asyncDownloadSize.Result;
            }
        }
        if (size > 0)
        {
            Log($"Download size: {size/1024f} KB. Type update to download additional files.");
        }
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




    public async Task LoadScene(string path, LoadSceneMode loadSceneMode)
    {
        if (asyncSceneDictionary.ContainsKey(path))
        {
            return;
        }

        var asyncLoadScene = Addressables.LoadSceneAsync(path, loadSceneMode);
        await asyncLoadScene.Task;

        if (asyncLoadScene.Status == AsyncOperationStatus.Succeeded)
        {
            asyncSceneDictionary.Add(path, asyncLoadScene);
        }
    }

    public async Task UnloadScene(string path)
    {
        if (asyncSceneDictionary.ContainsKey(path))
        {
            return;
        }

        var asyncUnloadScene = Addressables.UnloadSceneAsync(asyncSceneDictionary[path]);
        await asyncUnloadScene.Task;

        if (asyncUnloadScene.Status == AsyncOperationStatus.Succeeded)
        {
            asyncSceneDictionary.Remove(path);
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
}
