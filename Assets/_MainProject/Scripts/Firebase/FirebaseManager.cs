using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    private static FirebaseManager instance;
    public static FirebaseManager Instance => instance;

    private const string DATABASE_URL = "https://tfg-vr-2024-25-default-rtdb.europe-west1.firebasedatabase.app/";

    public string DatabaseURL => DATABASE_URL;

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
}