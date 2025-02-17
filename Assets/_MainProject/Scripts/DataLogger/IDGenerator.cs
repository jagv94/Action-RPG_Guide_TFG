using System;
using UnityEngine;

public static class IDGenerator
{
    public static string GenerateUserID()
    {
        string hashBase = SystemInfo.deviceModel + SystemInfo.processorType + SystemInfo.graphicsDeviceName + SystemInfo.systemMemorySize;
        int hashCode = hashBase.GetHashCode(); // Hash único basado en el hardware
        string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        return $"id-{hashCode:X8}-{timestamp}"; // Formato similar a IPv6
    }
}