using System;
using UnityEngine;

public class IDGenerator : MonoBehaviour
{
    private static IDGenerator instance;
    public static IDGenerator Instance => instance;

    public string UserID { get; private set; }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UserID = GenerateUserID();
    }

    private string GenerateUserID()
    {
        string hashBase = SystemInfo.deviceModel + SystemInfo.processorType + SystemInfo.graphicsDeviceName + SystemInfo.systemMemorySize;
        int hashCode = hashBase.GetHashCode(); // Hash único basado en el hardware
        string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        return $"id-{hashCode:X8}-{timestamp}"; // Formato similar a IPv6
    }
}