using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public static class ResourceManager
{
    public static event Action<float> DownloadProgress;
    public static event Action DownloadCompleted;

    private static List<string> updatedCatalog = new List<string>();
    private static List<string> primaryKeys = new List<string>();

    public static async Task InitializeAddressables()
    {
        await Addressables.InitializeAsync(true).Task;
    }

    private static void Release(AsyncOperationHandle value)
    {
        Addressables.Release(value);
    }

    public static async Task CatalogUpdateCheck()
    {
        List<string> catalogUpdate = new List<string>();
        var catalogUpdateCheckHandle = Addressables.CheckForCatalogUpdates(false);
        await catalogUpdateCheckHandle.Task;


        if (catalogUpdateCheckHandle.Status != AsyncOperationStatus.Succeeded)
        {
            //TODO 
        }
        else
        {
            //TODO
            catalogUpdate.AddRange(catalogUpdateCheckHandle.Result);
        }

        Release(catalogUpdateCheckHandle);
        updatedCatalog.AddRange(catalogUpdate);
    }

    public static async Task CatalogUpdate()
    {
        if (updatedCatalog.Count == 0)
        {
            return;
        }

        var catalogUpdateHandle = Addressables.UpdateCatalogs(updatedCatalog, false);
        await catalogUpdateHandle.Task;

        if (catalogUpdateHandle.Status != AsyncOperationStatus.Succeeded)
        {
            //TODO
        }
        else
        {
            //TODO
        }

        Release(catalogUpdateHandle);
    }

    private static void RetrievePrimaryKey()
    {
        List<string> keys = new List<string>();

        foreach (var locator in Addressables.ResourceLocators)
        {
            foreach (var key in locator.Keys)
            {
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
        primaryKeys.AddRange(keys);
    }

    public static async Task<long> GetDownloadSize()
    {
        RetrievePrimaryKey();

        var downloadSizeHandle = Addressables.GetDownloadSizeAsync(primaryKeys);
        await downloadSizeHandle.Task;

        long sizeInBytes = downloadSizeHandle.Result;

        Release(downloadSizeHandle);

        return sizeInBytes;
    }

    public static async Task DownloadAssets()
    {
        var downloadHandle = Addressables.DownloadDependenciesAsync(primaryKeys, Addressables.MergeMode.Union, false);

        while (!downloadHandle.IsDone)
        {
            DownloadProgress?.Invoke(downloadHandle.GetDownloadStatus().Percent);
            await Task.Yield();
        }
        DownloadCompleted?.Invoke();

        Release(downloadHandle);
    }
}
