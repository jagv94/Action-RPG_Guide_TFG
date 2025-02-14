using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.XR.Management;
using Newtonsoft.Json;
using System.Collections;
using System.Threading.Tasks;

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
    private float ramUsage; // Carga de GPU en porcentaje.
    private string cpu; // Modelo del procesador del usuario.
    private string gpu; // Modelo de la tarjeta gráfica del usuario.
    private string ram; // Cantidad de memoria RAM en el sistema.
    private string os; // Sistema operativo del usuario.
    private string vr_headset; // Nombre del visor VR en uso.
    private int frustrationRate; // Veces que el usuario repite una acción sin éxito.
    private bool helpAccessed; // true si el usuario ha abierto la sección de ayuda.
    private int rageQuits; // Número de veces que el usuario cierra la app tras un error o fallo.
    private int replayRate; // Veces que un usuario repite la misma acción o prueba.

    private Process currentProcess; // Para medir el uso de CPU.
    private float lastCPUTime = 0f;
    private float lastCheckTime = 0f;

    private readonly object eventQueueLock = new object();
    private int maxBatchSize = 10;  // Máximo de eventos antes de enviar
    private float maxBatchTime = 10f; // Tiempo máximo antes de enviar (en segundos)
    private int retryCount = 3; // Número máximo de intentos de reenvío
    private float retryDelay = 5f; // Tiempo entre reintentos (segundos)
    private bool isSending = false;  // Evita múltiples envíos simultáneos
    private float lastSaveTime = 0f;
    private float saveInterval = 10f; // Tiempo en segundos para el guardado periódico

    // Calcular rotación de la cabeza
    private Quaternion lastHeadRotation;
    private float headMovementSpeed;
    private Camera mainCamera;


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
        InvokeRepeating(nameof(SaveToLocalFileIfNeeded), saveInterval, saveInterval);
        mainCamera = Camera.main;
        lastHeadRotation = mainCamera.transform.rotation; // Guarda la rotación inicial de la cabeza

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

        StartCoroutine(BatchSendRoutine());  // Iniciar el ciclo de envíos
    }

    void Update()
    {
        InvokeRepeating(nameof(UpdatePerformanceMetrics), 1.0f, 1.0f);

        // Calcula la velocidad angular en grados por segundo
        headMovement = CalculateHeadMovementSpeed();

        CheckIdleTime(); // Verifica la inactividad en cada frame.

        // Enviar datos a Firebase cada 10 segundos
        if (Time.time - lastEventTime > 10f && eventQueue.Count > 0)
        {
            SendDataToFirebase();
        }
    }

    private void OnApplicationQuit()
    {
        if (abandonedSession)
        {
            LogEvent("session_abandoned", "N/A");
        }
        else
        {
            LogEvent("session_end", "N/A", Time.time - sessionStartTime);
        }
    }

    private void OnEnable()
    {
        Application.quitting += OnAppQuit;
    }

    private void OnDisable()
    {
        Application.quitting -= OnAppQuit;
    }

    private void OnAppQuit()
    {
        SaveToLocalFileIfNeeded();
        if (eventQueue.Count > 0)
        {
            SendDataToFirebase();
        }
    }


    private void UpdatePerformanceMetrics()
    {
        fps = 1.0f / Time.deltaTime;
        cpuUsage = CalculateCPUUsage();
        gpuUsage = EstimateGPUUsage();
        ramUsage = GetCPUUsageFromProfiler();
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
        TimeSpan totalTime = currentProcess.TotalProcessorTime;
        float totalMilliseconds = (float)totalTime.TotalMilliseconds;

        float cpuUsage = ((totalMilliseconds - lastCPUTime) / Environment.ProcessorCount);
        lastCPUTime = totalMilliseconds;

        return Mathf.Clamp(cpuUsage, 0, 100);
    }

    private float EstimateGPUUsage()
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            return 0f;

        float gpuLoad = (1.0f / fps) * 100.0f; // Cuanto menor es el FPS, mayor carga tiene la GPU
        return Mathf.Clamp(gpuLoad, 0, 100);
    }

    private float GetCPUUsageFromProfiler()
    {
        return Profiler.GetTotalReservedMemoryLong() / (1024f * 1024f); // En MB
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
            SendDataToFirebase(); // Enviar datos a Firebase cuando el usuario vuelve a estar activo
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
            missedClicks = missedClicks,
            headMovement = headMovement, // Registra el movimiento de cabeza en grados por segundo
            gazeTarget = targetObject,
            teleportUsage = eventType == "teleport" ? teleportUsage + 1 : teleportUsage,
            fps = fps,
            cpuUsage = cpuUsage,
            gpuUsage = gpuUsage,
            ramUsage = ramUsage,
            cpu = cpu,
            gpu = gpu,
            ram = ram,
            os = os,
            vr_headset = vr_headset,
            frustrationRate = eventType == "error" ? frustrationRate + 1 : frustrationRate,
            helpAccessed = eventType == "help" || helpAccessed,
            rageQuits = eventType == "rage_quit" ? rageQuits + 1 : rageQuits,
            replayRate = eventType == "replay" ? replayRate + 1 : replayRate
        };

        // Validar los datos antes de agregarlos a la cola
        if (!ValidateEventData(data))
        {
            UnityEngine.Debug.LogError("Datos no válidos, no se agregarán a la cola.");
            return;
        }

        lock (eventQueueLock)
        {
            eventQueue.Add(data);
        }

        // Guardar solo si se cumple una de estas condiciones
        if (eventQueue.Count >= maxBatchSize || Time.time - lastSaveTime > saveInterval)
        {
            SaveToLocalFileIfNeeded();
        }

        // Si ya alcanzamos el número de eventos, enviamos inmediatamente
        if (eventQueue.Count >= maxBatchSize)
        {
            SendDataToFirebase();
        }
    }

    private void SaveToLocalFile()
    {
        try
        {
            if (eventQueue.Count == 0) return;
            string jsonData = JsonConvert.SerializeObject(new EventQueueWrapper { events = eventQueue }, Formatting.Indented);
            File.WriteAllText(localDataPath, jsonData);
        }
        catch (IOException e)
        {
            UnityEngine.Debug.LogError("Error al guardar datos locales: " + e.Message);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("Error inesperado al guardar datos locales: " + e.Message);
        }
    }

    private async void SaveToLocalFileIfNeeded()
    {
        if (eventQueue.Count == 0) return;

        await Task.Run(() =>
        {
            try
            {
                string jsonData;
                lock (eventQueueLock)
                {
                    jsonData = JsonConvert.SerializeObject(new EventQueueWrapper { events = eventQueue }, Formatting.Indented);
                }
                File.WriteAllText(localDataPath, jsonData);

                lock (eventQueueLock)
                {
                    eventQueue.Clear(); // Limpiar la cola tras guardar
                }

                UnityEngine.Debug.Log("Eventos guardados exitosamente.");
                lastSaveTime = Time.time;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("Error al guardar eventos: " + e.Message);
            }
        });
    }

    private void LoadLocalData()
    {
        try
        {
            if (File.Exists(localDataPath))
            {
                string jsonData = File.ReadAllText(localDataPath);
                var eventWrapper = JsonConvert.DeserializeObject<EventQueueWrapper>(jsonData);
                if (eventWrapper != null && eventWrapper.events != null)
                {
                    lock (eventQueueLock)
                    {
                        eventQueue.AddRange(eventWrapper.events);
                    }
                }

                SendDataToFirebase();
            }
        }
        catch (IOException e)
        {
            UnityEngine.Debug.LogError("Error al cargar datos locales: " + e.Message);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("Error inesperado al cargar datos locales: " + e.Message);
        }
    }

    private bool ValidateEventData(UserEventData data)
    {
        // Verifica que los campos esenciales no estén vacíos o nulos
        if (string.IsNullOrEmpty(data.userID) ||
            string.IsNullOrEmpty(data.sessionID) ||
            string.IsNullOrEmpty(data.eventType) ||
            string.IsNullOrEmpty(data.timestamp))
        {
            return false;
        }

        // Verifica que el timestamp sea válido y no sea el valor predeterminado
        DateTime timestamp;
        bool isValidTimestamp = DateTime.TryParse(data.timestamp, out timestamp);

        if (!isValidTimestamp || timestamp == default(DateTime))
        {
            return false;
        }

        // Verifica otros campos numéricos
        if (data.duration < 0 || data.timeSinceLastEvent < 0 || data.totalSessionTime < 0)
        {
            return false;
        }

        return true;
    }

    private async void SendDataToFirebase()
    {
        if (isSending || eventQueue.Count == 0) return;

        isSending = true;

        List<UserEventData> batch;
        lock (eventQueueLock)
        {
            batch = new List<UserEventData>(eventQueue);
            eventQueue.Clear();
        }

        string jsonData = JsonConvert.SerializeObject(new EventQueueWrapper { events = batch });

        // Generar la ruta basada en userID, sessionID y timestamp del lote
        string timestamp = DateTime.UtcNow.ToString("yyyyMMddTHHmmssfffZ");
        string path = $"userEvents/{userID}/{sessionID}/{timestamp}.json";

        bool success = await TrySendDataAsync(path, jsonData, retryCount);

        if (!success)
        {
            UnityEngine.Debug.LogError("No se pudo enviar a Firebase tras varios intentos. Guardando localmente.");
            lock (eventQueueLock)
            {
                eventQueue.AddRange(batch); // Restaurar eventos no enviados
            }
            SaveToLocalFile();
        }
        else
        {
            UnityEngine.Debug.Log("Datos enviados exitosamente.");
            PlayerPrefs.SetInt("failedAttempts", 0); // Reiniciar intentos fallidos
            PlayerPrefs.Save();
            if (File.Exists(localDataPath)) File.Delete(localDataPath);
        }

        isSending = false;
    }

    private IEnumerator BatchSendRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(maxBatchTime);  // Espera X segundos

            if (eventQueue.Count > 0)  // Si hay eventos pendientes, enviarlos
            {
                SendDataToFirebase();
            }
        }
    }

    private async Task<bool> TrySendDataAsync(string path, string jsonData, int retryCount)
    {
        for (int attempt = 0; attempt <= retryCount; attempt++)
        {
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

            firebaseClient.PostData(path, jsonData, success =>
            {
                taskCompletionSource.SetResult(success);
            });

            bool success = await taskCompletionSource.Task;
            if (success) return true;

            UnityEngine.Debug.LogWarning($"Intento {attempt + 1} de {retryCount} fallido. Reintentando en {retryDelay}s...");
            await Task.Delay(TimeSpan.FromSeconds(retryDelay));
        }

        return false;
    }

    [Serializable]
    public class UserEventData
    {
        [JsonProperty("timestamp")] public string timestamp;
        [JsonProperty("userID")] public string userID;
        [JsonProperty("sessionID")] public string sessionID;
        [JsonProperty("eventType")] public string eventType;
        [JsonProperty("targetObject")] public string targetObject;
        [JsonProperty("duration")] public float duration;
        [JsonProperty("timeSinceLastEvent")] public float timeSinceLastEvent;
        [JsonProperty("totalSessionTime")] public float totalSessionTime;
        [JsonProperty("missedClicks")] public int missedClicks;
        [JsonProperty("headMovement")] public float headMovement;
        [JsonProperty("gazeTarget")] public string gazeTarget;
        [JsonProperty("teleportUsage")] public int teleportUsage;
        [JsonProperty("fps")] public float fps;
        [JsonProperty("cpuUsage")] public float cpuUsage;
        [JsonProperty("gpuUsage")] public float gpuUsage;
        [JsonProperty("ramUsage")] public float ramUsage;
        [JsonProperty("cpu")] public string cpu;
        [JsonProperty("gpu")] public string gpu;
        [JsonProperty("ram")] public string ram;
        [JsonProperty("os")] public string os;
        [JsonProperty("vr_headset")] public string vr_headset;
        [JsonProperty("frustrationRate")] public int frustrationRate;
        [JsonProperty("helpAccessed")] public bool helpAccessed;
        [JsonProperty("rageQuits")] public int rageQuits;
        [JsonProperty("replayRate")] public int replayRate;
    }

    [Serializable]
    public class EventQueueWrapper
    {
        public List<UserEventData> events;
    }
}