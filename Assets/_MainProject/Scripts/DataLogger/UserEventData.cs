using System;
using Newtonsoft.Json;

[Serializable]
public class UserEventData
{
    [JsonProperty("timestamp")] public string timestamp; // Marca de tiempo que identifica un evento.
    [JsonProperty("userID")] public string userID; // ID único anónimo del usuario.
    [JsonProperty("sessionID")] public string sessionID; // ID único de cada sesión.
    [JsonProperty("eventType")] public string eventType; // Identificador del tipo de evento. Ej: "click_button" o "teleport".
    [JsonProperty("targetObject")] public string targetObject; // Receptor del evento.
    [JsonProperty("duration")] public float duration; // Duración del evento.
    [JsonProperty("timeSinceLastEvent")] public float timeSinceLastEvent; // Intervalo de tiempo desde el evento prévio.
    [JsonProperty("totalSessionTime")] public float totalSessionTime; // Tiempo total desde que se empezó una sesión hasta que se capturó el evento.
    [JsonProperty("headMovement")] public float headMovement; // Movimiento de cabeza en VR en grados por segundo.
    [JsonProperty("gazeTarget")] public string gazeTarget; // Objeto o interfaz donde el usuario fija la mirada en VR.
    [JsonProperty("teleportUsage")] public int teleportUsage; // Número de veces que el usuario usa teletransporte en VR.
    [JsonProperty("fps")] public float fps; // Cuántos FPS tiene la aplicación en cada momento.
    [JsonProperty("cpuUsage")] public float cpuUsage; // Carga de CPU en porcentaje.
    [JsonProperty("gpuUsage")] public float gpuUsage; // Carga de GPU en porcentaje.
    [JsonProperty("ramUsage")] public float ramUsage; // Carga de GPU en porcentaje.
    [JsonProperty("cpu")] public string cpu; // Modelo del procesador del usuario.
    [JsonProperty("gpu")] public string gpu; // Modelo de la tarjeta gráfica del usuario.
    [JsonProperty("ram")] public string ram; // Cantidad de memoria RAM en el sistema.
    [JsonProperty("os")] public string os; // Sistema operativo del usuario.
    [JsonProperty("vr_headset")] public string vr_headset; // Nombre del visor VR en uso.
    [JsonProperty("frustrationRate")] public int frustrationRate; // Veces que el usuario repite una acción sin éxito.
    [JsonProperty("helpAccessed")] public bool helpAccessed; // true si el usuario ha abierto la sección de ayuda/controles.

    public UserEventData(string userID, string sessionID, string eventType, string targetObject, float duration,
        float timeSinceLastEvent, float totalSessionTime, float headMovement, string gazeTarget,
        int teleportUsage, float fps, float cpuUsage, float gpuUsage, float ramUsage, string cpu, string gpu, string ram,
        string os, string vr_headset, int frustrationRate, bool helpAccessed)
    {
        this.timestamp = DateTime.UtcNow.ToString("o");
        this.userID = userID;
        this.sessionID = sessionID;
        this.eventType = eventType;
        this.targetObject = targetObject;
        this.duration = duration;
        this.timeSinceLastEvent = timeSinceLastEvent;
        this.totalSessionTime = totalSessionTime;
        this.headMovement = headMovement;
        this.gazeTarget = gazeTarget;
        this.teleportUsage = teleportUsage;
        this.fps = fps;
        this.cpuUsage = cpuUsage;
        this.gpuUsage = gpuUsage;
        this.ramUsage = ramUsage;
        this.cpu = cpu;
        this.gpu = gpu;
        this.ram = ram;
        this.os = os;
        this.vr_headset = vr_headset;
        this.frustrationRate = frustrationRate;
        this.helpAccessed = helpAccessed;
    }
}