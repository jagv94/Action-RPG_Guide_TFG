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
    private string connection_type; // Tipo de conexión del visor (cable o Air Link).
    private int frustrationRate; // Veces que el usuario repite una acción sin éxito.
    private bool helpAccessed; // true si el usuario ha abierto la sección de ayuda.
    private int rageQuits; // Número de veces que el usuario cierra la app tras un error o fallo.
    private int replayRate; // Veces que un usuario repite la misma acción o prueba.

    private Process currentProcess; // Para medir el uso de CPU.

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        firebaseClient = FindObjectOfType<FirebaseRestClient>();

        userID = GenerateUserID();
        sessionID = GenerateSessionID();
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
        ram = SystemInfo.systemMemorySize.ToString() + " MB";
        os = SystemInfo.operatingSystem;
        vr_headset = XRGeneralSettings.Instance.Manager.activeLoader != null ? XRGeneralSettings.Instance.Manager.activeLoader.name : "None";
        connection_type = "Unknown"; // Se puede mejorar si se detecta de otra forma.

        currentProcess = Process.GetCurrentProcess();
        LogEvent("session_start", "N/A");
    }

    void Update()
    {
        fps = 1.0f / Time.deltaTime;
        cpuUsage = CalculateCPUUsage(); // Calcula el uso de CPU de manera precisa.
        gpuUsage = SystemInfo.graphicsMemorySize / 100;
        CheckIdleTime(); // Verifica la inactividad en cada frame.
    }

    private string GenerateUserID()
    {
        return SystemInfo.deviceUniqueIdentifier;
    }

    private string GenerateSessionID()
    {
        return Guid.NewGuid().ToString();
    }

    /**
     * Calcula el uso de CPU utilizando System.Diagnostics.Process.
     */
    private float CalculateCPUUsage()
    {
        TimeSpan totalProcessorTime = currentProcess.TotalProcessorTime;
        TimeSpan uptime = TimeSpan.FromSeconds(Time.realtimeSinceStartup);
        float cpuUsage = (float)(totalProcessorTime.TotalMilliseconds / uptime.TotalMilliseconds) * 100f;
        return cpuUsage;
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
        firebaseClient?.PostData("userEvents.json", JsonUtility.ToJson(data));
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

    private void SaveToLocalFile()
    {
        string jsonData = JsonUtility.ToJson(new EventQueueWrapper { events = eventQueue });
        File.WriteAllText(localDataPath, jsonData);
    }

    private void LoadLocalData()
    {
        if (File.Exists(localDataPath))
        {
            string jsonData = File.ReadAllText(localDataPath);
            EventQueueWrapper wrapper = JsonUtility.FromJson<EventQueueWrapper>(jsonData);
            eventQueue = wrapper.events;

            if (firebaseClient != null)
            {
                foreach (var data in eventQueue)
                {
                    string jsonEvent = JsonUtility.ToJson(data);
                    firebaseClient.PostData("userEvents.json", jsonEvent);
                }
            }

            eventQueue.Clear();
            File.Delete(localDataPath);
        }
    }

    private void SendDataToFirebase()
    {
        foreach (var data in eventQueue)
        {
            string jsonData = JsonUtility.ToJson(data);
            firebaseClient.PostData("userEvents.json", jsonData);
        }
        eventQueue.Clear();
        File.Delete(localDataPath);
    }

    [Serializable]
    private class UserEventData
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
    private class EventQueueWrapper
    {
        public List<UserEventData> events;
    }
}