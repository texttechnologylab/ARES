using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class NLPHandler : MonoBehaviour
{
    public string json;
    public ExtractJsonScene.Scenes scenes;

    public IEnumerator Post(string inputText, Action<string> response)
        {
            WWWForm form = new WWWForm();
            form.AddField("inputText", inputText);
            form.AddField("model", Settings.model);
            UnityWebRequest webRequest = UnityWebRequest.Post(Settings.apiEndpoint, form);

            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                //Error(webRequest.error);
                response(null);
            }
            else
            {
                response(webRequest.downloadHandler.text);
                json = webRequest.downloadHandler.text;
                ExtractJsonScene ejs = new ExtractJsonScene(json);
                scenes = ejs.scenes;
            }
        }

    public void SendRequest(string inputText)
        {
            // serializable struct

            StartCoroutine(Post(inputText, (response) =>
            {
                if (response == null)
                {
                    return;
                }
                // success
                Debug.Log(response);
            }));
        }
}