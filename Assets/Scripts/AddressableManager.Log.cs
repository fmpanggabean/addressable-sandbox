using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class AddressableManager
{
    private void Log(string value)
    {
        log.text += $"[{DateTime.Now.ToString("hh:mm:ss:ff")}] {value}\n";
        Debug.Log($"[{DateTime.Now.ToString("hh:mm:ss:ff")}] {value}\n");
    }

    private void Message(string value)
    {
        msg.text = $"{value}";
    }

    public async void GetTextCommand()
    {
        var input = chatBox.text.Split("#");

        if (input[0].Equals("create-object"))
        {
            InstantiateObjectAtRandom(input[1]);
        }
        else if (input[0].Equals("update"))
        {
            await DownloadDependencies();
        }
        else if (input[0].Equals("delete-object"))
        {
            RemoveInstantiatedObject(input[1]);
        }
        else if (input[0].Equals("delete-scene"))
        {
            await LoadScene(input[1], LoadSceneMode.Single);
        }
        else if (input[0].Equals("create-scene"))
        {
            await LoadScene(input[1], LoadSceneMode.Single);
        }

        chatBox.text = "";
    }
}
