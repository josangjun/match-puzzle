#if !FINAL_BUILD
#define __ENABLE_LOG__
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
#if UNITY_2018_3_OR_NEWER
using UnityEngine.Networking;
#endif

/// <summary>
/// Davinci - A powerful, esay-to-use image downloading and caching library for Unity in Run-Time
/// v 1.2
/// Developed by ShamsDEV.com
/// copyright (c) ShamsDEV.com All Rights Reserved.
/// Licensed under the MIT License.
/// https://github.com/shamsdev/davinci
/// </summary>
public class Davinci : MonoBehaviour
{
    private static bool ENABLE_GLOBAL_LOGS = true;

    private bool enableLog = false;
    
    private bool cached = true;

    private enum RendererType
    {
        none,
        uiImage,
        renderer,
        sprite,
        rawImage
    }

    private RendererType rendererType = RendererType.none;
    private Component targetObj;
    private string url = null;

    private Texture loadingPlaceholder, errorPlaceholder;

    private UnityAction onStartAction,
        onDownloadedAction,
        onLoadedAction,
        onEndAction;

    private Func<byte[], Texture2D> onLoadingAction;

    private UnityAction<int> onDownloadProgressChange;
    private UnityAction<string> onErrorAction;
    /*
    private static Dictionary<string, Davinci> underProcessDavincies
        = new Dictionary<string, Davinci>();
    */
    private string uniqueHash;
    private int progress;

    private bool success = false;

    static string filePath =
#if UNITY_EDITOR
        $"{Application.dataPath}/.."
#else
        Application.persistentDataPath
#endif
         + "/Resource/images/";

    public static int expireSeconds { get; private set; } = 30 * 60;

    public static void SetFilePath(string filePath)
    {
        Davinci.filePath = filePath;
    }

    public static void SetExpireTime(int seconds)
    {
        expireSeconds = seconds;
        ClearAllExpiredFiles(seconds);
    }

    private enum FinishAction
    {
        DestroyComponent,
        DestroyGameObject
    }

    private FinishAction finishAction;

    private Davinci setFinishAction(FinishAction action)
    {
        finishAction = action;
        return this;
    }

    /// <summary>
    /// Get instance of davinci class
    /// </summary>
    public static Davinci get()
    {
        return new GameObject("Davinci").AddComponent<Davinci>().setFinishAction(FinishAction.DestroyGameObject);
    }

    /// <summary>
    /// Set image url for download.
    /// </summary>
    /// <param name="url">Image Url</param>
    /// <returns></returns>
    public Davinci load(string url)
    {
        Log("[Davinci] Url set : " + url);

        this.url = url;
        return this;
    }

    public Davinci into(RawImage image)
    {
        Log("[Davinci] Target as RawImage set : " + image);

        rendererType = RendererType.rawImage;
        this.targetObj = image;
        return this;
    }

    /// <summary>
    /// Set target Image component.
    /// </summary>
    /// <param name="image">target Unity UI image component</param>
    /// <returns></returns>
    public Davinci into(Image image)
    {
        Log("[Davinci] Target as UIImage set : " + image);

        rendererType = RendererType.uiImage;
        this.targetObj = image;
        return this;
    }

    /// <summary>
    /// Set target Renderer component.
    /// </summary>
    /// <param name="renderer">target renderer component</param>
    /// <returns></returns>
    public Davinci into(Renderer renderer)
    {
        Log("[Davinci] Target as Renderer set : " + renderer);

        rendererType = RendererType.renderer;
        this.targetObj = renderer;
        return this;
    }

    public Davinci into(SpriteRenderer spriteRenderer)
    {
        Log("[Davinci] Target as SpriteRenderer set : " + spriteRenderer);

        rendererType = RendererType.sprite;
        this.targetObj = spriteRenderer;
        return this;
    }

#region Actions
    public Davinci withStartAction(UnityAction action)
    {
        this.onStartAction = action;

        Log("[Davinci] On start action set : " + action);

        return this;
    }

    public Davinci withDownloadedAction(UnityAction action)
    {
        this.onDownloadedAction = action;

        Log("[Davinci] On downloaded action set : " + action);

        return this;
    }

    public Davinci withDownloadProgressChangedAction(UnityAction<int> action)
    {
        this.onDownloadProgressChange = action;

        Log("[Davinci] On download progress changed action set : " + action);

        return this;
    }

    public Davinci withLoadingAction(Func<byte[], Texture2D> action)
    {
        this.onLoadingAction = action;
        return this;
    }

    public Davinci withLoadedAction(UnityAction action)
    {
        this.onLoadedAction = action;

        Log("[Davinci] On loaded action set : " + action);

        return this;
    }

    public Davinci withErrorAction(UnityAction<string> action)
    {
        this.onErrorAction = action;

        Log("[Davinci] On error action set : " + action);

        return this;
    }

    public Davinci withEndAction(UnityAction action)
    {
        this.onEndAction = action;

        Log("[Davinci] On end action set : " + action);

        return this;
    }
#endregion

    /// <summary>
    /// Show or hide logs in console.
    /// </summary>
    /// <param name="enable">'true' for show logs in console.</param>
    /// <returns></returns>
    public Davinci setEnableLog(bool enableLog)
    {
        this.enableLog = enableLog;

        Log("[Davinci] Logging enabled : " + enableLog);

        return this;
    }

    /// <summary>
    /// Set the sprite of image when davinci is downloading and loading image
    /// </summary>
    /// <param name="loadingPlaceholder">loading texture</param>
    /// <returns></returns>
    public Davinci setLoadingPlaceholder(Texture loadingPlaceholder)
    {
        this.loadingPlaceholder = loadingPlaceholder;

        Log("[Davinci] Loading placeholder has been set.");

        return this;
    }

    /// <summary>
    /// Set image sprite when some error occurred during downloading or loading image
    /// </summary>
    /// <param name="errorPlaceholder">error texture</param>
    /// <returns></returns>
    public Davinci setErrorPlaceholder(Texture errorPlaceholder)
    {
        this.errorPlaceholder = errorPlaceholder;

        Log("[Davinci] Error placeholder has been set.");

        return this;
    }

    /// <summary>
    /// Enable cache
    /// </summary>
    /// <returns></returns>
    public Davinci setCached(bool cached)
    {
        this.cached = cached;

        Log("[Davinci] Cache enabled : " + cached);

        return this;
    }

    /// <summary>
    /// Start davinci process.
    /// </summary>
    public void start()
    {
        if (url == null)
        {
            error("Url has not been set. Use 'load' funtion to set image url.");
            return;
        }

        try
        {
            Uri uri = new Uri(url);
            this.url = uri.AbsoluteUri;
        }
        catch (Exception)
        {
            error("Url is not correct.");
            return;
        }
        
        Log("[Davinci] Start Working.");

        if (loadingPlaceholder != null)
            SetLoadingImage();

        if (onStartAction != null)
            onStartAction.Invoke();

        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }

        uniqueHash = CreateMD5(url);
        
        {
            var path = filePath + uniqueHash;
            if (File.Exists(path))
            {
                var time = File.GetLastWriteTime(path);
                var ts = DateTime.Now - time;
                
                Log($"File:{path}, LastWriteTime:{time}, {ts.TotalSeconds}, hit:{ts.TotalSeconds < expireSeconds}");

                if (ts.TotalSeconds < expireSeconds)
                {
                    if (onDownloadedAction != null)
                        onDownloadedAction.Invoke();

                    loadSpriteToImage();
                    return;
                }
                else
                {
                    File.Delete(path);
                }
            }

            //underProcessDavincies.Add(uniqueHash, this);
            StopAllCoroutines();
            StartCoroutine(Downloader());
        }
    }

    public static bool IsCached(string url)
    {
        var uniqueHash = CreateMD5(url);
        var path = filePath + uniqueHash;
        if (File.Exists(path))
        {
            var time = File.GetLastWriteTime(path);
            var ts = DateTime.Now - time;

            if (ts.TotalSeconds < expireSeconds)
                return true;
        }
        return false;
    }

    [System.Diagnostics.Conditional("__ENABLE_LOG__")]
    private void Log(string msg)
    {
        if (enableLog)
            Debug.Log($"[{gameObject.GetInstanceID():x}], {msg}", this);
    }

    [System.Diagnostics.Conditional("__ENABLE_LOG__")]
    private void LogWarning(string msg)
    {
        if (enableLog)
            Debug.LogWarning($"[{gameObject.GetInstanceID():x}], {msg}", this);
    }

    private void LogError(string msg)
    {
        Debug.LogError($"[{gameObject.GetInstanceID():x}], {msg}", this);
    }

    private IEnumerator Downloader()
    {
        Log("[Davinci] Download started.");

#if UNITY_2018_3_OR_NEWER
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();
#else
        var www = new WWW(url);
#endif

        while (!www.isDone)
        {
            if (www.error != null)
            {
                error("Error while downloading the image : " + www.error);
                yield break;
            }

#if UNITY_2018_3_OR_NEWER
            progress = Mathf.FloorToInt(www.downloadProgress * 100);
#else
            progress = Mathf.FloorToInt(www.progress * 100);
#endif
            if (onDownloadProgressChange != null)
                onDownloadProgressChange.Invoke(progress);

            Log("[Davinci] Downloading progress : " + progress + "%");

            yield return null;
        }
        byte[] fileData = null;
#if UNITY_2018_3_OR_NEWER
        if (www.error == null)
        {
            fileData = www.downloadHandler.data;
            File.WriteAllBytes(filePath + uniqueHash, fileData);
        }
        else
        {
            error("Error while downloading the image : " + www.error);
        }
#else
        if (www.error == null)
            File.WriteAllBytes(filePath + uniqueHash, www.bytes);
#endif

        www.Dispose();
        www = null;

        if (onDownloadedAction != null)
            onDownloadedAction.Invoke();

        loadSpriteToImage(fileData);

        //underProcessDavincies.Remove(uniqueHash);
    }

    private void loadSpriteToImage(byte[] fileData = null)
    {
        progress = 100;
        if (onDownloadProgressChange != null)
            onDownloadProgressChange.Invoke(progress);

        Log("[Davinci] Downloading progress : " + progress + "%");
        
        if (!File.Exists(filePath + uniqueHash))
        {
            error("Loading image file has been failed.");
            return;
        }
        
        StopAllCoroutines();
        ImageLoader(null, fileData);
    }

    private void SetLoadingImage()
    {
        switch (rendererType)
        {
            case RendererType.renderer:
                Renderer renderer = (Renderer)targetObj;
                renderer.material.mainTexture = loadingPlaceholder;
                break;

            case RendererType.uiImage:
                Image image = (Image)targetObj;
                Sprite sprite = Sprite.Create((Texture2D)loadingPlaceholder,
                    new Rect(0, 0, loadingPlaceholder.width, loadingPlaceholder.height),
                    new Vector2(0.5f, 0.5f));
                image.sprite = sprite;

                break;

            case RendererType.sprite:
                SpriteRenderer spriteRenderer = (SpriteRenderer)targetObj;
                Sprite spriteImage = Sprite.Create((Texture2D)loadingPlaceholder,
                    new Rect(0, 0, loadingPlaceholder.width, loadingPlaceholder.height),
                    new Vector2(0.5f, 0.5f));

                spriteRenderer.sprite = spriteImage;
                break;

            case RendererType.rawImage:
                var rawImage = (RawImage)targetObj;
                rawImage.texture = loadingPlaceholder;
                break;
        }
    }

    private void OnDestroy()
    {
        if (success == false)
            LogWarning($"[Davinci] stopped");
    }

    private void ImageLoader(Texture texture = null, byte[] fileData = null)
    {
        Log("[Davinci] Start loading image.");

        if (texture == null)
        {
            if (fileData == null)
            {
                var path = filePath + uniqueHash;
                if (File.Exists(path))
                    fileData = File.ReadAllBytes(path);
                else
                    LogError("[Davinci] file does not exist : " + path);
            }
            
            if (onLoadingAction != null)
            {
                texture = onLoadingAction.Invoke(fileData);
            }
            else if (rendererType != RendererType.none)
            {
                LogWarning($"[Davinci] onLoadingAction is null.");
                var texture2d = new Texture2D(2, 2);
                if (texture2d.LoadImage(fileData, true)) //..this will auto-resize the texture dimensions.
                {
                    texture = texture2d;
                }
                else
                {
                    LogError("[Davinci] LoadImage failed.");
                    texture = errorPlaceholder;
                }
            }
        }

        if (targetObj != null)
            switch (rendererType)
            {
                case RendererType.renderer:
                    Renderer renderer = targetObj.GetComponent<Renderer>();

                    if (renderer == null || renderer.material == null)
                        break;

                    renderer.material.mainTexture = texture;

                    break;

                case RendererType.uiImage:
                    Image image = (Image)targetObj;

                    if (image == null)
                        break;

                    Sprite sprite = Sprite.Create((Texture2D)texture,
                        new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                    image.sprite = sprite;
                    
                    break;

                case RendererType.sprite:
                    SpriteRenderer spriteRenderer = (SpriteRenderer)targetObj;

                    if (spriteRenderer == null)
                        break;

                    Sprite spriteImage = Sprite.Create((Texture2D)texture,
                        new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                    spriteRenderer.sprite = spriteImage;
                    break;

                case RendererType.rawImage:
                    var rawImage = (RawImage)targetObj;
                    rawImage.texture = texture;

                    break;
            }

        if (onLoadedAction != null)
            onLoadedAction.Invoke();

        if (enableLog)
            Debug.Log("[Davinci] Image has been loaded.");

        success = true;
        finish();
    }

    public static string CreateMD5(string input)
    {
        // Use input string to calculate MD5 hash
        using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
        {
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Convert the byte array to hexadecimal string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }

    private void error(string message)
    {
        success = false;
#if !FINAL_BUILD
        LogWarning("[Davinci] Error : " + message);
#endif
        if (onErrorAction != null)
            onErrorAction.Invoke(message);

        if (errorPlaceholder != null)
            ImageLoader(errorPlaceholder);
        else finish();
    }

    private void finish()
    {
        Log("[Davinci] Operation has been finished.");

        if (!cached)
        {
            try
            {
                File.Delete(filePath + uniqueHash);
            }
            catch (Exception ex)
            {
                if (enableLog)
                    LogError($"[Davinci] Error while removing cached file: {ex.Message}");
            }
        }

        if (onEndAction != null)
            onEndAction.Invoke();

        Invoke("destroyer", 1/30f);
    }

    private void destroyer()
    {
        if (finishAction == FinishAction.DestroyGameObject)
            Destroy(gameObject);
        else
            Destroy(this);
    }


    /// <summary>
    /// Clear a certain cached file with its url
    /// </summary>
    /// <param name="url">Cached file url.</param>
    /// <returns></returns>
    public static void ClearCache(string url)
    {
        try
        {
            File.Delete(filePath + CreateMD5(url));

            if (ENABLE_GLOBAL_LOGS)
                Debug.Log($"[Davinci] Cached file has been cleared: {url}");
        }
        catch (Exception ex)
        {
            if (ENABLE_GLOBAL_LOGS)
                Debug.LogError($"[Davinci] Error while removing cached file: {ex.Message}");
        }
    }

    /// <summary>
    /// Clear all davinci cached files
    /// </summary>
    /// <returns></returns>
    public static void ClearAllCachedFiles()
    {
        try
        {
            if (Directory.Exists(filePath))
                Directory.Delete(filePath, true);

            if (ENABLE_GLOBAL_LOGS)
                Debug.Log("[Davinci] All Davinci cached files has been cleared.");
        }
        catch (Exception ex)
        {
            if (ENABLE_GLOBAL_LOGS)
                Debug.LogError($"[Davinci] Error while removing cached file: {ex.Message}");
        }
    }

    public static async void ClearAllExpiredFiles(int seconds)
    {
        try
        {
            if (!Directory.Exists(filePath))
                return;
            
            var files = Directory.GetFiles(filePath);
            var now = DateTime.Now;
            var startTime = Time.realtimeSinceStartup;

            for (var i = 0; i < files.Length; ++i)
            {
                var file = files[i];
                var time = File.GetLastWriteTime(file);
                var ts = now - time;
                var expired = ts.TotalSeconds > seconds;
                //Debug.Log($"File cache:{file}, {time}, {expired}");
                if (expired)
                    File.Delete(file);
                
                var dt = Time.realtimeSinceStartup - startTime;
                if (dt > 0.1f)
                {
                    await System.Threading.Tasks.Task.Yield();
                    startTime = Time.realtimeSinceStartup;
                }
            }
        }
        catch (Exception ex)
        {
            if (ENABLE_GLOBAL_LOGS)
                Debug.LogError($"[Davinci] Error while removing cached file: {ex.Message}");
        }
    }
}