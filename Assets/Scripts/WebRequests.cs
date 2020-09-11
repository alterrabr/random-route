using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class WebRequests : MonoBehaviour
{
    //Upload generated path to Dropbox app folder
    public static IEnumerator Upload(string path)
    {
        using (UnityWebRequest www = UnityWebRequest.Post("https://content.dropboxapi.com/2/files/upload", path))
        {
            www.SetRequestHeader("Authorization", "Bearer 5nKP-t1rTtwAAAAAAAABf_L3tok1c7Y4_dKk1uyTFl30h0D8H-V7NbirsXH4FFtX");
            www.SetRequestHeader("Dropbox-API-Arg", "{\"path\": \"/path.json\",\"mode\":\"overwrite\",\"autorename\":false,\"mute\":false,\"strict_conflict\": false}");
            www.SetRequestHeader("Content-Type", "application/octet-stream");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(www.error);
            }
            else
            {
                File.Delete(Application.dataPath + "/path.json");
                Debug.Log("Path upload complete!");
            }
        }
    }

    //Download path to Dropbox app folder
    public static IEnumerator Download()
    {
        using (UnityWebRequest www = UnityWebRequest.Post("https://content.dropboxapi.com/2/files/download", ""))
        {
            www.SetRequestHeader("Authorization", "Bearer 5nKP-t1rTtwAAAAAAAABf_L3tok1c7Y4_dKk1uyTFl30h0D8H-V7NbirsXH4FFtX");
            www.SetRequestHeader("Dropbox-API-Arg", "{\"path\": \"/path.json\"}");
            www.SetRequestHeader("Content-Type", "text/plain; charset=utf-8");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(www.error);
            }
            else
            {
                //Write downloaded path to local file
                string path = System.Uri.UnescapeDataString(www.downloadHandler.text);
                File.WriteAllText(Application.dataPath + "/path.json", path);

                //And load scene with path
                SceneManager.LoadScene("Path");
                Debug.Log("Path download complete!");
            }
        }
    }
}
