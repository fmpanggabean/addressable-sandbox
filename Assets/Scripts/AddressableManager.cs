using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public partial class AddressableManager : MonoBehaviour
{
    public static AddressableManager Instance;

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
        Caching.ClearCache();
        await IntializeAddressables();
        var catalog = await CatalogUpdateCheck();
        await CatalogUpdate(catalog);

        List<string> keys = await RetrievePrimaryKey();
        long size = await GetDownloadSize(keys);
        await DownloadAssets(keys);
    }

    private async Task DownloadAssets(List<string> keys)
    {
        var downloadHandle = Addressables.DownloadDependenciesAsync(keys, Addressables.MergeMode.Union, false);
        
        while (!downloadHandle.IsDone)
        {
            Log($"Download: {downloadHandle.GetDownloadStatus().Percent * 100}%");
            await Task.Yield();
        }
        Log($"Downloaded: {(downloadHandle.GetDownloadStatus().DownloadedBytes/1024).ToString("G")} KB");

        Release(downloadHandle);
    }

    private async Task<long> GetDownloadSize(List<string> keys)
    {
        Dictionary<string, long> downloadList = new Dictionary<string, long>();

        var downloadSizeHandle = Addressables.GetDownloadSizeAsync(keys);
        await downloadSizeHandle.Task;

        long size = downloadSizeHandle.Result;
        Log($"Download size: {size/1024} KB");
        Release(downloadSizeHandle);

        //foreach(var key in keys)
        //{
        //    var downloadSizeHandle = Addressables.GetDownloadSizeAsync(key);
        //    await downloadSizeHandle.Task;
        //    long size = downloadSizeHandle.Result;

        //    if (size > 0)
        //    {
        //        downloadList.Add(key, size);
        //        Log($"Key: {key}, Size: {size}");
        //    }

        //    Release(downloadSizeHandle);
        //}

        return size;
    }

    private async Task<List<string>> RetrievePrimaryKey()
    {
        List<string> keys = new List<string>();

        foreach (var locator in Addressables.ResourceLocators)
        {
            //Log($"Locator: {locator.LocatorId}");

            foreach (var key in locator.Keys)
            {
                //Log($"Key: {key.ToString()}");

                IList<IResourceLocation> resourceLocations;
                locator.Locate(key, null, out resourceLocations);

                foreach (var location in resourceLocations)
                {
                    string primary = location.PrimaryKey;

                    if (keys.Contains(primary))
                    {
                        break;
                    }

                    keys.Add(primary);
                }
            }
        }

        return keys;
    }

    private async Task CatalogUpdate(List<string> list)
    {
        if (list.Count == 0)
        {
            Log($"Catalog need no update");
            return;
        }

        var catalogUpdateHandle = Addressables.UpdateCatalogs(list, false);
        await catalogUpdateHandle.Task;

        if (catalogUpdateHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Log($"Catalog update failed");
        }
        else
        {
            Log($"Catalog update success");
        }

        Release(catalogUpdateHandle);
    }

    private async Task<List<string>> CatalogUpdateCheck()
    {
        List<string> catalogUpdate = new List<string>();
        var catalogUpdateCheckHandle = Addressables.CheckForCatalogUpdates(false);
        await catalogUpdateCheckHandle.Task;


        if (catalogUpdateCheckHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Log($"Catalog update check failed");
        }
        else
        {
            Log($"Catalog update check success");
            catalogUpdate.AddRange(catalogUpdateCheckHandle.Result);
        }

        Release(catalogUpdateCheckHandle);
        return catalogUpdate;
    }

    private async Task IntializeAddressables()
    {
        await Addressables.InitializeAsync(true).Task;
    }

    private void Release(AsyncOperationHandle value)
    {
        Addressables.Release(value);
    }
}
