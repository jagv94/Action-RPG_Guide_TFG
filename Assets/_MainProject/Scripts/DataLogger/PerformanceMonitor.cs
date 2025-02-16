using UnityEngine;
using System.Diagnostics;
using System;
using UnityEngine.Profiling;

public class PerformanceMonitor : MonoBehaviour
{
    private static PerformanceMonitor instance;
    public static PerformanceMonitor Instance => instance;

    private Process currentProcess;
    private float lastCPUTime;
    private float lastCheckTime;

    // Calcular rotación de la cabeza
    private Quaternion lastHeadRotation;
    private float headMovementSpeed;
    private Camera mainCamera;

    public float FPS { get; private set; }
    public float CPUUsage { get; private set; }
    public float GPUUsage { get; private set; }
    public float RAMUsage { get; private set; }
    public float HeadMovement { get; private set; }

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

    void Start()
    {
        mainCamera = Camera.main;
        lastHeadRotation = mainCamera.transform.rotation; // Guarda la rotación inicial de la cabeza

        currentProcess = Process.GetCurrentProcess();
        lastCheckTime = Time.realtimeSinceStartup;
        InvokeRepeating(nameof(UpdatePerformanceMetrics), 1.0f, 1.0f);
    }

    private void UpdatePerformanceMetrics()
    {
        FPS = 1.0f / Time.unscaledDeltaTime;
        CPUUsage = CalculateCPUUsage();
        GPUUsage = EstimateGPUUsage();
        RAMUsage = Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f);
        HeadMovement = CalculateHeadMovementSpeed(); // Calcula la velocidad angular del visor en grados por segundo
    }

    private float CalculateCPUUsage()
    {
        TimeSpan totalTime = currentProcess.TotalProcessorTime;
        float currentCPUTime = (float)totalTime.TotalMilliseconds;
        float deltaTime = Time.realtimeSinceStartup - lastCheckTime;
        lastCheckTime = Time.realtimeSinceStartup;

        float cpuUsage = ((currentCPUTime - lastCPUTime) / (deltaTime * System.Environment.ProcessorCount)) * 100f;
        lastCPUTime = currentCPUTime;

        return Mathf.Clamp(cpuUsage, 0, 100);
    }

    private float EstimateGPUUsage()
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            return 0f;

        float estimatedLoad = (1.0f - (FPS / (float)Screen.currentResolution.refreshRateRatio.value)) * 100.0f;
        return Mathf.Clamp(estimatedLoad, 0, 100);
    }

    private float CalculateHeadMovementSpeed()
    {
        Quaternion currentRotation = mainCamera.transform.rotation;
        float angle = Quaternion.Angle(lastHeadRotation, currentRotation);
        lastHeadRotation = currentRotation;

        float newSpeed = angle / Time.deltaTime;
        headMovementSpeed = Mathf.Lerp(headMovementSpeed, newSpeed, 0.5f);
        return headMovementSpeed < 0.1f ? 0f : headMovementSpeed;
    }
}