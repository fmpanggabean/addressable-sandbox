using System;
using UnityEngine;
using UnityEngine.UI;

public class AddressablesImplementation : MonoBehaviour
{
    public Slider loadingBar;

    public bool isClearCacheBeforeRun;

    private void Awake()
    {
        ResourceManager.DownloadProgress += UpdateProgressbar;
        ResourceManager.DownloadCompleted += OnDownloadCompleted;

        OnAwake();
    }

    private void Update()
    {
        OnUpdate();
    }

    private async void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("1");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
        }
    }

    private async void OnAwake()
    {
        if (isClearCacheBeforeRun)
        {
            Caching.ClearCache();
        }

        await ResourceManager.InitializeAddressables();
        await ResourceManager.CatalogUpdateCheck();
        await ResourceManager.CatalogUpdate();
        long size = await ResourceManager.GetDownloadSize();

        Debug.Log($"Download size: {(size/1024).ToString("N2")}KB");
        if (size > 0)
        {
            await ResourceManager.DownloadAssets();
        }

        //await ResourceManager.InstantiateUniqueObject("Set/Set Costume_02 SD Unity-Chan WTD.prefab");
        //ResourceManager.ReleaseInstantiatedUniqueObject("Set/Set Costume_02 SD Unity-Chan WTD.prefab");
        await ResourceManager.LoadScene("Premade Scene", UnityEngine.SceneManagement.LoadSceneMode.Additive);
        await ResourceManager.UnloadScene("Premade Scene");
    }

    private void UpdateProgressbar(float progress)
    {
        loadingBar.value = progress;
    }

    private void OnDownloadCompleted()
    {
        Debug.Log($"Download completed");
    }
}
