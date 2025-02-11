using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class FirebaseRestClient : MonoBehaviour
{
    private string databaseURL = "https://tfg-vr-2024-25-default-rtdb.europe-west1.firebasedatabase.app/"; // Reemplaza con tu URL de Firebase.

    /**
     * Envía datos a Firebase Realtime Database.
     * @param path Ruta en la base de datos (por ejemplo, "userEvents.json").
     * @param data Datos a enviar en formato JSON.
     */
    public void PostData(string path, string jsonData)
    {
        StartCoroutine(PostDataCoroutine(path, jsonData));
    }

    private IEnumerator PostDataCoroutine(string path, string jsonData)
    {
        string url = databaseURL + path;
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error al enviar datos: " + request.error);
        }
        else
        {
            Debug.Log("Datos enviados correctamente: " + request.downloadHandler.text);
        }
    }

    /**
     * Lee datos de Firebase Realtime Database.
     * @param path Ruta en la base de datos (por ejemplo, "userEvents.json").
     */
    public void GetData(string path)
    {
        StartCoroutine(GetDataCoroutine(path));
    }

    private IEnumerator GetDataCoroutine(string path)
    {
        string url = databaseURL + path;

        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error al leer datos: " + request.error);
        }
        else
        {
            Debug.Log("Datos recibidos: " + request.downloadHandler.text);
        }
    }
}