using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace ZForward
{
    public static class Request
    {

        // Post Request
        public static IEnumerator Post(string uri, object postData, Action<UnityWebRequest> callback = null, string xAuthToken = "")
        {
            // using var webRequest = UnityWebRequest.Post(uri, postData);
            var data = string.IsNullOrEmpty(JsonUtility.ToJson(postData))
                ? null
                : Encoding.UTF8.GetBytes(JsonUtility.ToJson(postData));
            
            Debug.Log(Convert.FromBase64String(Convert.ToBase64String(data!)).ToString());
            
            using var webRequest = new UnityWebRequest
            {
                url = uri,
                method = UnityWebRequest.kHttpVerbPOST,
                uploadHandler = new UploadHandlerRaw(data),
                downloadHandler = new DownloadHandlerBuffer(),
                timeout = 60
            };
            webRequest.SetRequestHeader("Accept", "application/json");
            webRequest.SetRequestHeader("Content-Type", "application/json");
            if (xAuthToken.Length > 0)
                webRequest.SetRequestHeader("x-auth-token", xAuthToken);

            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            // var result = ProcessResponse(webRequest);
            callback?.Invoke(webRequest);
        }
        
        // Get Request
        public static IEnumerator Get(string uri, Action<UnityWebRequest> callback = null, string xAuthToken = "")
        {
            using var webRequest = UnityWebRequest.Get(uri);

            if (xAuthToken.Length > 0)
                webRequest.SetRequestHeader("x-auth-token", xAuthToken);
            

            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            // var result = ProcessResponse(webRequest);
            callback?.Invoke(webRequest);
        }
        
        public static IEnumerator DownloadImage(string mediaUrl, string fileName, Action<Texture2D> callback = null)
        {   
            var request = UnityWebRequestTexture.GetTexture(mediaUrl);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ProtocolError ||
                request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(request.error);
            }
            else
            {
                var texture = ((DownloadHandlerTexture) request.downloadHandler).texture;
                texture.name = fileName;
                callback?.Invoke(texture);
            }
        }

        public static IEnumerator DownloadAudio(string mediaUrl, string fileName, Action<AudioClip,byte[]> callback = null)
        {
            var request = UnityWebRequestMultimedia.GetAudioClip(mediaUrl, AudioType.MPEG);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ProtocolError ||
                request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(request.error);
            }
            else
            {
                var audioClip = ((DownloadHandlerAudioClip) request.downloadHandler).audioClip;
                var bytes = ((DownloadHandlerAudioClip) request.downloadHandler).data;
                audioClip.name = fileName;
                callback?.Invoke(audioClip, bytes);
            }
        }

        public static IEnumerator LoadLocalAudio(string mediaUrl, Action<AudioClip> callback = null)
        {
            var request = UnityWebRequestMultimedia.GetAudioClip(mediaUrl, AudioType.AUDIOQUEUE);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(mediaUrl);
                Debug.Log(request.error);
            }
            else
            {
                callback?.Invoke(((DownloadHandlerAudioClip) request.downloadHandler).audioClip);
            }
        }

        // Process Request Response
        private static UnityWebRequest.Result ProcessResponse(UnityWebRequest request)
        {
            switch (request.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                    Debug.LogError(": Error: " + request.downloadHandler.text);
                    return request.result;
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(": Error: " + request.downloadHandler.text);
                    return request.result;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(": HTTP Error: " + request.downloadHandler.text);
                    return request.result;
                case UnityWebRequest.Result.Success:
                    Debug.Log(":\nReceived: " + request.downloadHandler.text);
                    return request.result;
                case UnityWebRequest.Result.InProgress:
                    return request.result;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
    }
}
