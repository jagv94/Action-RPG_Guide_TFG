using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Management;

public class UserEventLogger : MonoBehaviour
{
    private static UserEventLogger instance;
    public static UserEventLogger Instance => instance;

    private ConcurrentQueue<UserEventData> eventQueue = new ConcurrentQueue<UserEventData>();

    private float lastEventTime;
    private float sessionStartTime;

    public string UserID { get; private set; }
    public string SessionID { get; private set; }

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
        sessionStartTime = Time.time;
        lastEventTime = sessionStartTime;
        UserID = IDGenerator.GenerateUserID();
        SessionID = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Registra un evento con información relevante.
    /// </summary>
    /// <param name="eventType">Tipo de evento realizado.</param>
    /// <param name="targetObject">Elemento con el que interactuó el usuario.</param>
    /// <param name="duration">Duración del evento (en caso de eventos prolongados).</param>
    public void LogEvent(string eventType, string targetObject, float duration = 0f)
    {
        if (string.IsNullOrWhiteSpace(eventType))
        {
            Debug.LogError("Evento inválido: eventType no puede estar vacío.");
            return;
        }

        float currentTime = Time.time;
        float timeSinceLastEvent = currentTime - lastEventTime;
        float totalSessionTime = currentTime - sessionStartTime;
        lastEventTime = currentTime;

        UserEventData data = new UserEventData(
            userID: UserID,
            sessionID: SessionID,
            eventType: eventType,
            targetObject: targetObject,
            duration: duration,
            timeSinceLastEvent: timeSinceLastEvent,
            totalSessionTime: totalSessionTime,
            missedClicks: 0, // Podría actualizarse con lógica adicional
            headMovement: PerformanceMonitor.Instance.HeadMovement,
            gazeTarget: string.IsNullOrEmpty(targetObject) ? "AirClick" : targetObject,
            teleportUsage: TeleportTracker.Instance.TeleportCount, // Debe ser actualizado en otro módulo
            fps: PerformanceMonitor.Instance.FPS,
            cpuUsage: PerformanceMonitor.Instance.CPUUsage,
            gpuUsage: PerformanceMonitor.Instance.GPUUsage,
            ramUsage: PerformanceMonitor.Instance.RAMUsage,
            cpu: SystemInfo.processorType,
            gpu: SystemInfo.graphicsDeviceName,
            ram: $"{SystemInfo.systemMemorySize} MB",
            os: SystemInfo.operatingSystem,
            vr_headset: XRGeneralSettings.Instance.Manager.activeLoader?.name ?? "None",
            frustrationRate: ActionFailureTracker.Instance.FailedAttempts,
            helpAccessed: SettingsTracker.Instance.SettingsVisits > 0
        );

        // Validar los datos antes de agregarlos a la cola
        if (!ValidateEventData(data))
        {
            Debug.LogError("Datos no válidos, evento descartado.");
            return;
        }

        eventQueue.Enqueue(data);
    }

    /// <summary>
    /// Retorna los eventos registrados y vacía la lista.
    /// </summary>
    public List<UserEventData> GetAndClearEventQueue()
    {
        List<UserEventData> batch = new List<UserEventData>();
        while (eventQueue.TryDequeue(out UserEventData data))
        {
            batch.Add(data);
        }
        return batch;
    }

    private bool ValidateEventData(UserEventData data)
    {
        if (string.IsNullOrEmpty(data.userID) || string.IsNullOrEmpty(data.sessionID) ||
            string.IsNullOrEmpty(data.eventType) || string.IsNullOrEmpty(data.timestamp) ||
            string.IsNullOrEmpty(data.gazeTarget))
        {
            return false;
        }

        if (!DateTime.TryParse(data.timestamp, out DateTime timestamp) || timestamp == default(DateTime))
        {
            return false;
        }

        if (data.duration < 0 || data.timeSinceLastEvent < 0 || data.totalSessionTime < 0)
        {
            return false;
        }

        return true;
    }
}