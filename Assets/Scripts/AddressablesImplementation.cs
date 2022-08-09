using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AddressablesImplementation : MonoBehaviour
{
    public Slider loadingBar;
    public TMP_InputField inputField;

    public bool isClearCacheBeforeRun;

    private void Awake()
    {
        ResourceManager.DownloadProgress += UpdateProgressbar;
        ResourceManager.DownloadCompleted += OnDownloadCompleted;

        OnAwake();
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
            //await ResourceManager.DownloadAssets();
        }
    }

    private void UpdateProgressbar(float progress)
    {
        loadingBar.value = progress;
    }

    private void OnDownloadCompleted()
    {
        Debug.Log($"Download completed");
    }

    public async void SendCommand()
    {
        var command = inputField.text.Split(" ", 2);

        if (command[0].Equals("/update"))
        {
            await ResourceManager.DownloadAssets();
        }
        else if (command[0].Equals("/instantiate"))
        {
            await ResourceManager.InstantiateUniqueObject(command[1]);
        }
        else if (command[0].Equals("/release"))
        {
            ResourceManager.ReleaseInstantiatedUniqueObject(command[1]);
        }
        else if (command[0].Equals("/loadScene"))
        {
            await ResourceManager.LoadScene(command[1], UnityEngine.SceneManagement.LoadSceneMode.Additive);
        }
        else if (command[0].Equals("/unloadScene"))
        {
            await ResourceManager.UnloadScene(command[1]);
        }

        inputField.text = "";
    }
}
