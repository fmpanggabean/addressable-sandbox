using System;
using UnityEngine;

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
}
