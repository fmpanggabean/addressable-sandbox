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
            Log($"Catalog update completed!");
        }
        else
        {
            Log($"Catalog already up to date!");
        }

        Addressables.Release(asyncCatalogCheck);
    }

    private async Task UpdateCatalog(List<string> result)
    {
        AsyncOperationHandle asyncUpdate = Addressables.UpdateCatalogs(result);
        await asyncUpdate.Task;
    }

    private void Log(string value)
    {
        log.text += $"[{DateTime.Now.ToString("hh:mm:ss:ff")}] {value}\n";
    }

    private void Message(string value)
    {
        msg.text = $"{value}";
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Vector3 randomPosition = new Vector3(
                UnityEngine.Random.Range(-4, 4),
                0,
                UnityEngine.Random.Range(-4, 4)
                );
            Addressables.InstantiateAsync("Assets/FreeDragons/Prefab/DragonBoarPBR/GoldBoarPBR.prefab", randomPosition, Quaternion.identity);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Vector3 randomPosition = new Vector3(
                UnityEngine.Random.Range(-4, 4),
                0,
                UnityEngine.Random.Range(-4, 4)
                );
            Addressables.InstantiateAsync("Assets/FreeDragons/Prefab/DragonBoarPBR/BlueBoarPBR.prefab", randomPosition, Quaternion.identity);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Vector3 randomPosition = new Vector3(
                UnityEngine.Random.Range(-4, 4),
                0,
                UnityEngine.Random.Range(-4, 4)
                );
            Addressables.InstantiateAsync("Assets/FreeDragons/Prefab/DragonBoarPBR/RedBoarPBR.prefab", randomPosition, Quaternion.identity);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Vector3 randomPosition = new Vector3(
                UnityEngine.Random.Range(-4, 4),
                0,
                UnityEngine.Random.Range(-4, 4)
                );
            Addressables.InstantiateAsync("Assets/FreeDragons/Prefab/DragonBoarPBR/GreenBoarPBR.prefab", randomPosition, Quaternion.identity);
        }
    }
}
