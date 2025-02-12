using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.XR.Management;

/**
 * AdvancedDataLogger
 * 
 * Este script registra datos sobre la sesión del usuario, la interacción con la UI,
 * el rendimiento del dispositivo y el comportamiento en VR. Los datos se almacenan
 * localmente y se envían a Firebase en lotes para optimizar el rendimiento.
 */
public class AdvancedDataLogger : MonoBehaviour
{
    private FirebaseRestClient firebaseClient;
    private string userID; // ID único anónimo del usuario.
    private string sessionID; // ID único de cada sesión.
    private string localDataPath;
    private List<UserEventData> eventQueue = new List<UserEventData>();
    private float sessionStartTime;
    private float lastEventTime;
    private int sessionCount; // Número total de sesiones iniciadas por un usuario.
    private string firstSessionTimestamp; // Fecha y hora de la primera vez que el usuario usó la aplicación.
    private string lastSessionTimestamp; // Fecha y hora de la última sesión del usuario.
    private bool abandonedSession; // true si el usuario cerró la app antes de interactuar.
    private float idleStartTime = -1;
    private float idleThreshold = 30f; // Tiempo antes de considerar al usuario inactivo.
    private float nextPerformanceUpdate = 0f;
    private float performanceUpdateInterval = 1.0f; // Actualiza CPU y GPU cada 1s
    private int missedClicks; // Veces que el usuario hace clic en una zona sin botones.
    private float headMovement; // Movimiento de cabeza en VR en grados por segundo.
    private string gazeTarget; // Objeto o interfaz donde el usuario fija la mirada en VR.
    private int teleportUsage; // Número de veces que el usuario usa teletransporte en VR.
    private float fps; // Cuántos FPS tiene la aplicación en cada momento.
    private float cpuUsage; // Carga de CPU en porcentaje.
    private float gpuUsage; // Carga de GPU en porcentaje.
    private string cpu; // Modelo del procesador del usuario.
    private string gpu; // Modelo de la tarjeta gráfica del usuario.
    private string ram; // Cantidad de memoria RAM en el sistema.
    private string os; // Sistema operativo del usuario.
    private string vr_headset; // Nombre del visor VR en uso.
    private int frustrationRate; // Veces que el usuario repite una acción sin éxito.
    private bool helpAccessed; // true si el usuario ha abierto la sección de ayuda.
    private int rageQuits; // Número de veces que el usuario cierra la app tras un error o fallo.
    private int replayRate; // Veces que un usuario repite la misma acción o prueba.
    private int retryCount = 3; //Reintentos posibles al enviar datos a Firebase.

    private Process currentProcess; // Para medir el uso de CPU.
    private float lastCPUTime = 0f;
    private float lastCheckTime = 0f;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        // Obtiene o crea una instancia de FirebaseRestClient
        firebaseClient = FirebaseRestClient.Instance;

        userID = GenerateUserID();
        sessionID = Guid.NewGuid().ToString();
        currentProcess = Process.GetCurrentProcess();
        sessionStartTime = Time.time;
        lastEventTime = sessionStartTime;
        localDataPath = Application.persistentDataPath + "/eventQueue.json";

        sessionCount = PlayerPrefs.GetInt("sessionCount", 0) + 1;
        PlayerPrefs.SetInt("sessionCount", sessionCount);
        PlayerPrefs.Save();

        if (!PlayerPrefs.HasKey("firstSessionTimestamp"))
        {
            firstSessionTimestamp = DateTime.UtcNow.ToString("o");
            PlayerPrefs.SetString("firstSessionTimestamp", firstSessionTimestamp);
        }
        else
        {
            firstSessionTimestamp = PlayerPrefs.GetString("firstSessionTimestamp");
        }

        lastSessionTimestamp = DateTime.UtcNow.ToString("o");
        PlayerPrefs.SetString("lastSessionTimestamp", lastSessionTimestamp);
        PlayerPrefs.Save();

        abandonedSession = true;
        cpu = SystemInfo.processorType;
        gpu = SystemInfo.graphicsDeviceName;
        ram = $"{SystemInfo.systemMemorySize} MB";
        os = SystemInfo.operatingSystem;

        vr_headset = XRGeneralSettings.Instance.Manager.activeLoader?.name ?? "None";

        LogEvent("session_start", "N/A");
        LoadLocalData(); // Cargar datos locales al inicio para enviarlos a Firebase.
    }

    void Update()
    {
        fps = 1.0f / Time.deltaTime;
        if (Time.time >= nextPerformanceUpdate)
        {
            cpuUsage = CalculateCPUUsage();
            gpuUsage = EstimateGPUUsage();
            nextPerformanceUpdate = Time.time + performanceUpdateInterval;
        }

        CheckIdleTime(); // Verifica la inactividad en cada frame.
    }

    private string GenerateUserID()
    {
        string hashBase = SystemInfo.deviceModel + SystemInfo.processorType + SystemInfo.graphicsDeviceName + SystemInfo.systemMemorySize;
        int hashCode = hashBase.GetHashCode(); // Hash único basado en el hardware
        string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        return $"id-{hashCode:X8}-{timestamp}"; // Formato similar a IPv6
    }

    /**
     * Calcula el uso de CPU utilizando System.Diagnostics.Process.
     */
    private float CalculateCPUUsage()
    {
        float deltaTime = Time.time - lastCheckTime;
        lastCheckTime = Time.time;

        float currentCPUTime = (float)currentProcess.TotalProcessorTime.TotalMilliseconds;
        float cpuUsage = ((currentCPUTime - lastCPUTime) / deltaTime) * 100f;
        lastCPUTime = currentCPUTime;

        return Mathf.Clamp(cpuUsage, 0, 100);
    }

    private float EstimateGPUUsage()
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            return 0f;

        float gpuLoad = (1.0f / fps) * 100.0f; // Cuanto menor es el FPS, mayor carga tiene la GPU
        return Mathf.Clamp(gpuLoad, 0, 100);
    }

    /**
     * Verifica si el usuario está inactivo utilizando XRDevice.userPresence.
     */
    private void CheckIdleTime()
    {
        if (Time.time - lastEventTime > idleThreshold)
        {
            if (idleStartTime < 0)
            {
                idleStartTime = Time.time;
                LogEvent("idle_start", "N/A");
            }
        }
        else if (idleStartTime >= 0)
        {
            float idleDuration = Time.time - idleStartTime;
            LogEvent("idle_end", "N/A", idleDuration);
            idleStartTime = -1;
        }
    }

    /**
     * Registra un evento con información relevante.
     * @param eventType Tipo de evento realizado.
     * @param targetObject Elemento con el que interactuó el usuario.
     * @param duration Duración del evento (en caso de eventos prolongados).
     */
    public void LogEvent(string eventType, string targetObject, float duration = 0f)
    {
        float currentTime = Time.time;
        float timeSinceLastEvent = currentTime - lastEventTime;
        float totalSessionTime = currentTime - sessionStartTime;
        lastEventTime = currentTime;
        abandonedSession = false;

        UserEventData data = new UserEventData
        {
            timestamp = DateTime.UtcNow.ToString("o"),
            userID = userID,
            sessionID = sessionID,
            eventType = eventType,
            targetObject = targetObject,
            duration = duration,
            timeSinceLastEvent = timeSinceLastEvent,
            totalSessionTime = totalSessionTime,
            headMovement = UnityEngine.Random.Range(0f, 10f),
            gazeTarget = targetObject,
            teleportUsage = eventType == "teleport" ? teleportUsage + 1 : teleportUsage,
            fps = fps,
            cpuUsage = cpuUsage,
            gpuUsage = gpuUsage,
            frustrationRate = eventType == "error" ? frustrationRate + 1 : frustrationRate,
            helpAccessed = eventType == "help" || helpAccessed,
            rageQuits = eventType == "rage_quit" ? rageQuits + 1 : rageQuits,
            replayRate = eventType == "replay" ? replayRate + 1 : replayRate
        };

        eventQueue.Add(data);
        SaveToLocalFile();
    }

    private void SaveToLocalFile()
    {
        if (eventQueue.Count == 0) return;

        File.WriteAllText(localDataPath, JsonUtility.ToJson(new EventQueueWrapper { events = eventQueue }));
    }

    private void LoadLocalData()
    {
        if (File.Exists(localDataPath))
        {
            string jsonData = File.ReadAllText(localDataPath);
            eventQueue.AddRange(JsonUtility.FromJson<EventQueueWrapper>(jsonData).events);

            SendDataToFirebase(() => // Enviar todos los eventos cargados a Firebase.
            {
                File.Delete(localDataPath); // Solo borra el archivo tras confirmar el envío
            });
        }
    }

    private void SendDataToFirebase(Action onSuccess = null)
    {
        if (eventQueue.Count == 0) return;

        firebaseClient.PostData("userEvents.json", JsonUtility.ToJson(new EventQueueWrapper { events = eventQueue }), (success) =>
        {
            if (success)
            {
                retryCount = 3;
                eventQueue.Clear();
                onSuccess?.Invoke(); // Solo borra el archivo si el envío fue exitoso
            }
            else if (retryCount > 0)
            {
                UnityEngine.Debug.LogWarning("Reintentando envío a Firebase...");
                retryCount--;
                SendDataToFirebase(); // Reintenta hasta 3 veces
            }
        });
    }

    [Serializable]
    public class UserEventData
    {
        public string timestamp;
        public string userID;
        public string sessionID;
        public string eventType;
        public string targetObject;
        public float duration;
        public float timeSinceLastEvent;
        public float totalSessionTime;
        public float headMovement;
        public string gazeTarget;
        public int teleportUsage;
        public float fps;
        public float cpuUsage;
        public float gpuUsage;
        public int frustrationRate;
        public bool helpAccessed;
        public int rageQuits;
        public int replayRate;
    }

    [Serializable]
    public class EventQueueWrapper
    {
        public List<UserEventData> events;
    }
}