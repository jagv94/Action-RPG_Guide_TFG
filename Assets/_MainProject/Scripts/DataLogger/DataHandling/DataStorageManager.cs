using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class DataStorageManager : MonoBehaviour
{
    private static DataStorageManager instance;
    public static DataStorageManager Instance => instance;

    private string localDataPath;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            localDataPath = Application.persistentDataPath + "/eventQueue.json";
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Guarda eventos localmente en un archivo JSON.
    /// </summary>
    public void SaveEvents(List<UserEventData> events)
    {
        try
        {
            if (events.Count == 0) return;

            string jsonData = JsonConvert.SerializeObject(new EventQueueWrapper { events = events }, Formatting.Indented);
            File.WriteAllText(localDataPath, jsonData);
            Debug.Log("Eventos guardados localmente.");
        }
        catch (Exception e)
        {
            Debug.LogError("Error al guardar eventos localmente: " + e.Message);
        }
    }

    /// <summary>
    /// Carga eventos almacenados localmente.
    /// </summary>
    public List<UserEventData> LoadEvents()
    {
        try
        {
            if (!File.Exists(localDataPath)) return new List<UserEventData>();

            string jsonData = File.ReadAllText(localDataPath);
            var eventWrapper = JsonConvert.DeserializeObject<EventQueueWrapper>(jsonData);
            Debug.Log("Eventos cargados desde almacenamiento local.");
            return eventWrapper?.events ?? new List<UserEventData>();
        }
        catch (Exception e)
        {
            Debug.LogError("Error al cargar eventos locales: " + e.Message);
            return new List<UserEventData>();
        }
    }

    /// <summary>
    /// Borra los eventos almacenados después de enviarlos correctamente.
    /// </summary>
    public void ClearLocalStorage()
    {
        try
        {
            if (File.Exists(localDataPath))
            {
                File.Delete(localDataPath);
                Debug.Log("Eventos locales eliminados.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error al eliminar eventos locales: " + e.Message);
        }
    }
}