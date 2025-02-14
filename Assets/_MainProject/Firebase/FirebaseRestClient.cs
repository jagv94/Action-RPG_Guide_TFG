using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class FirebaseRestClient : MonoBehaviour
{
    private static FirebaseRestClient instance;
    private const string DATABASE_URL = "https://tfg-vr-2024-25-default-rtdb.europe-west1.firebasedatabase.app/";

    /**
     * Permite obtener una instancia global de FirebaseRestClient.
     */
    public static FirebaseRestClient Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /**
     * Envía datos a Firebase Realtime Database.
     * @param path Ruta en la base de datos (por ejemplo, "userEvents.json").
     * @param jsonData Datos a enviar en formato JSON.
     */
    public void PostData(string path, string jsonData, Action<bool> callback)
    {
        if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(jsonData))
        {
            Debug.LogError("Path o jsonData no pueden ser nulos o vacíos.");
            callback?.Invoke(false);
            return;
        }

        StartCoroutine(PostDataCoroutine(path, jsonData, callback));
    }

    private IEnumerator PostDataCoroutine(string path, string jsonData, Action<bool> callback)
    {
        string url = DATABASE_URL + path;
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Error al enviar datos a Firebase ({request.responseCode}): {request.error}");
            callback?.Invoke(false);
        }
        else
        {
            Debug.Log("Datos enviados correctamente: " + request.downloadHandler.text);
            callback?.Invoke(true);
        }
    }

    /**
     * Lee datos de Firebase Realtime Database.
     * @param path Ruta en la base de datos (por ejemplo, "userEvents.json").
     */
    public void GetData(string path, Action<string> callback)
    {
        StartCoroutine(GetDataCoroutine(path, callback));
    }

    private IEnumerator GetDataCoroutine(string path, Action<string> callback)
    {
        string url = DATABASE_URL + path;
        UnityWebRequest request = UnityWebRequest.Get(url);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Error al leer datos de Firebase ({request.responseCode}): {request.error}");
            callback?.Invoke(null);
        }
        else
        {
            Debug.Log("Datos recibidos correctamente.");
            callback?.Invoke(request.downloadHandler.text);
        }
    }
}