public interface IFirebaseService
{
    void PostData(string path, string jsonData, System.Action<bool> callback);
    void GetData(string path, System.Action<string> callback);
}