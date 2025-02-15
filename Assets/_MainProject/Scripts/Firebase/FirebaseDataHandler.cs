using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;

public class FirebaseDataHandler : MonoBehaviour, IFirebaseService
{
    private static FirebaseDataHandler instance;
    public static FirebaseDataHandler Instance => instance;

    private string databaseUrl;

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

    private void Start()
    {
        databaseUrl = FirebaseManager.Instance.DatabaseURL;
    }

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
        string url = databaseUrl + path;
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

    public void GetData(string path, Action<string> callback)
    {
        StartCoroutine(GetDataCoroutine(path, callback));
    }

    private IEnumerator GetDataCoroutine(string path, Action<string> callback)
    {
        string url = databaseUrl + path;
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