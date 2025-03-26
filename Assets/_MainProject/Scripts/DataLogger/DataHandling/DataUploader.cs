using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Threading.Tasks;

public class DataUploader : MonoBehaviour
{
    private static DataUploader instance;
    public static DataUploader Instance => instance;

    private IFirebaseService firebaseService;
    private float uploadInterval = 10f;
    private int batchSizeThreshold = 0; //Por defecto a 20
    private bool isUploading = false;

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
        firebaseService = FirebaseDataHandler.Instance;

        //Evento que marca cuando se empieza una sesión.
        UserEventLogger.Instance.LogBasicEvent("init_event", "Session Started");
        UploadBasicData();

        // Cargar eventos almacenados y enviarlos
        List<UserEventData> storedEvents = DataStorageManager.Instance.LoadEvents();
        if (storedEvents.Count > 0)
        {
            Debug.Log("Enviando eventos almacenados...");
            UploadData();
        }

        StartCoroutine(UploadRoutine());
    }

    private IEnumerator UploadRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(uploadInterval);
            if (UserEventLogger.Instance.GetEventQueue().Count >= batchSizeThreshold)
            {
                UploadData();
            }
        }
    }

    private async void UploadBasicData()
    {
        if (isUploading) return;

        isUploading = true;

        // Obtener eventos en memoria y eventos locales
        List<UserEventBasicData> basicData = new List<UserEventBasicData> { UserEventLogger.Instance.GetBasicEvent() };
        basicData.AddRange(DataStorageManager.Instance.LoadBasicEvents());

        if (basicData.Count == 0)
        {
            isUploading = false;
            return;
        }

        string jsonData = JsonConvert.SerializeObject(new BasicEventQueueWrapper { events = basicData });
        string timestamp = DateTime.UtcNow.ToString("yyyyMMddTHHmmssfffZ");
        string path = $"userEvents/{UserEventLogger.Instance.UserID}/{UserEventLogger.Instance.SessionID}.json";

        bool success = await TrySendDataAsync(path, jsonData);

        if (!success)
        {
            Debug.LogError("No se pudo enviar a Firebase.");
        }
        else
        {
            Debug.Log("Eventos enviados con éxito.");
            DataStorageManager.Instance.ClearLocalStorage(); // Borra eventos locales tras el envío
        }

        isUploading = false;
    }

    private async void UploadData()
    {
        if (isUploading) return;

        isUploading = true;

        // Obtener eventos en memoria y eventos locales
        List<UserEventData> batch = UserEventLogger.Instance.GetAndClearEventQueue();
        batch.AddRange(DataStorageManager.Instance.LoadEvents());

        if (batch.Count == 0)
        {
            isUploading = false;
            return;
        }

        string jsonData = JsonConvert.SerializeObject(new EventQueueWrapper { events = batch });
        string timestamp = DateTime.UtcNow.ToString("yyyyMMddTHHmmssfffZ");
        string path = $"userEvents/{UserEventLogger.Instance.UserID}/{UserEventLogger.Instance.SessionID}/{timestamp}.json";

        bool success = await TrySendDataAsync(path, jsonData);

        if (!success)
        {
            Debug.LogError("No se pudo enviar a Firebase. Guardando eventos localmente...");
            DataStorageManager.Instance.SaveEvents(batch); // Guarda los eventos fallidos
        }
        else
        {
            Debug.Log("Eventos enviados con éxito.");
            DataStorageManager.Instance.ClearLocalStorage(); // Borra eventos locales tras el envío
        }

        isUploading = false;
    }

    private async Task<bool> TrySendDataAsync(string path, string jsonData)
    {
        TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

        firebaseService.PostData(path, jsonData, success =>
        {
            taskCompletionSource.SetResult(success);
        });

        return await taskCompletionSource.Task;
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            UserEventLogger.Instance.LogEvent("app_paused", "application", 0f);
            UploadData();
        }
    }
}