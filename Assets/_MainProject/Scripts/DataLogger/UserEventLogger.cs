using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Management;

public class UserEventLogger : MonoBehaviour
{
    private static UserEventLogger instance;
    public static UserEventLogger Instance => instance;

    private UserEventBasicData basicData;
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

            UserID = PlayerPrefs.GetString("UserID", "");
            if (UserID == "")
            {
                UserID = IDGenerator.GenerateUserID();
                PlayerPrefs.SetString("UserID", UserID.ToString());
            }

            SessionID = Guid.NewGuid().ToString();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (!LoggerManager.Instance.Logger)
        {
            return;
        }

        sessionStartTime = Time.time;
        lastEventTime = sessionStartTime;
    }

    public void LogBasicEvent(string eventType, string targetObject)
    {
        if (!LoggerManager.Instance.Logger)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(eventType))
        {
            Debug.LogError("Evento inv�lido: eventType no puede estar vac�o.");
            return;
        }

        basicData = new UserEventBasicData(
            userID: UserID,
            sessionID: SessionID,
            eventType: eventType,
            targetObject: targetObject,
            cpu: SystemInfo.processorType,
            gpu: SystemInfo.graphicsDeviceName,
            ram: $"{SystemInfo.systemMemorySize} MB",
            os: SystemInfo.operatingSystem,
            vr_headset: XRGeneralSettings.Instance != null &&
                XRGeneralSettings.Instance.Manager != null &&
                XRGeneralSettings.Instance.Manager.activeLoader != null ?
                XRGeneralSettings.Instance.Manager.activeLoader.name : "None"
        );

        // Validar los datos antes de agregarlos a la cola
        if (!ValidateEventData(basicData))
        {
            Debug.LogError("Datos no v�lidos, evento descartado.");
            return;
        }
    }

    /// <summary>
    /// Registra un evento con informaci�n relevante.
    /// </summary>
    /// <param name="eventType">Tipo de evento realizado.</param>
    /// <param name="targetObject">Elemento con el que interactu� el usuario.</param>
    /// <param name="duration">Duraci�n del evento (en caso de eventos prolongados).</param>
    public void LogEvent(string eventType, string targetObject, float duration = 0f)
    {
        if (!LoggerManager.Instance.Logger)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(eventType))
        {
            Debug.LogError("Evento inv�lido: eventType no puede estar vac�o.");
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
            headMovement: PerformanceMonitor.Instance != null ? PerformanceMonitor.Instance.HeadMovement : 0f,
            gazeTarget: string.IsNullOrEmpty(targetObject) ? "AirClick" : targetObject,
            teleportUsage: TeleportTracker.Instance != null ? TeleportTracker.Instance.TeleportCount : 0,
            fps: PerformanceMonitor.Instance != null ? PerformanceMonitor.Instance.FPS : 0,
            cpuUsage: PerformanceMonitor.Instance != null ? PerformanceMonitor.Instance.CPUUsage : 0f,
            gpuUsage: PerformanceMonitor.Instance != null ? PerformanceMonitor.Instance.GPUUsage : 0f,
            ramUsage: PerformanceMonitor.Instance != null ? PerformanceMonitor.Instance.RAMUsage : 0f,
            frustrationRate: ActionFailureTracker.Instance != null ? ActionFailureTracker.Instance.FailedAttempts : 0,
            helpAccessed: UIInteractionLogger.Instance != null ? UIInteractionLogger.Instance.SettingsVisits : 0
        );


        // Validar los datos antes de agregarlos a la cola
        if (!ValidateEventData(data))
        {
            Debug.LogError("Datos no v�lidos, evento descartado.");
            return;
        }

        eventQueue.Enqueue(data);
    }

    public UserEventBasicData GetBasicEvent()
    {
        return basicData;
    }

    public List<UserEventData> GetEventQueue()
    {
        List<UserEventData> batch = new List<UserEventData>();
        foreach (UserEventData data in eventQueue)
        {
            batch.Add(data);
        }
        return batch;
    }

    /// <summary>
    /// Retorna los eventos registrados y vac�a la lista.
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

    private bool ValidateEventData(UserEventBasicData data)
    {
        if (string.IsNullOrEmpty(data.userID) || string.IsNullOrEmpty(data.sessionID) ||
            string.IsNullOrEmpty(data.eventType) || string.IsNullOrEmpty(data.timestamp))
        {
            return false;
        }

        if (!DateTime.TryParse(data.timestamp, out DateTime timestamp) || timestamp == default(DateTime))
        {
            return false;
        }

        return true;
    }
}