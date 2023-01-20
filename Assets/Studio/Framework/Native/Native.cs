using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//using LineCorp.Pion.Sharing;
using UnityEngine;
using Studio.Common;
//using Studio.Common.Permission;
//using Studio.Common.Trident;
using Studio.Game;
using System.Runtime.InteropServices;
using Crystal;
using Newtonsoft.Json.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.Experimental.Rendering;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.UI;
using System.Text;

namespace Studio.Framework
{
    public class Native : Singleton<Native>
    {
#if !UNITY_EDITOR && UNITY_ANDROID		
		private const int LAYOUT_IN_DISPLAY_CUTOUT_MODE_DEFAULT = 0;
		private const int LAYOUT_IN_DISPLAY_CUTOUT_MODE_SHORT_EDGES = 1;
		private const int LAYOUT_IN_DISPLAY_CUTOUT_MODE_NEVER = 2;
		private const int LAYOUT_IN_DISPLAY_CUTOUT_MODE_ALWAYS = 3;
#endif		

        #region File
        public static bool CreateDirectory(string path, out string message)
        {
            bool result = false;

            message = "";
            try {
                if (Directory.Exists(path)) 
                {
                    result = true;

                } 
                else 
                {
                    DirectoryInfo info = Directory.CreateDirectory(path);
                    if (info == null || !Directory.Exists(path))
                    {
                        message = (path) + " created  fail";
                        result = false;
                    } 
                    else 
                    {
                        result = true;
                    }
                }

#if UNITY_IOS
                if (result) 
                {
                    UnityEngine.iOS.Device.SetNoBackupFlag(path);
                }
#endif
            } catch (Exception e) {
                message = e.Message;

                string msg = string.Format("CreateDirectory fail, path:{0} message:{2} exception:{1}", path, e.StackTrace, message);
                Debug.LogError(msg);
                result = false;
            }

            return result;
        }
        #endregion
                
        #region Share
        //private LineCorp.Pion.Sharing.ShareComponent shareComponent = null;
        private const string DirName= "projmShare";
        private const string FileName= "projm_share_screencapture.jpg";
        private string ShareMsg;
        private const int SCREENSHOT_HEIGHT = 1334;
        private Func<IEnumerator> _writePermissionRequestor;
        private Func<IEnumerator> _readPermissionRequestor;

        [SerializeField] private Texture2D _texture;
	    
        public struct MSG_START_CAPTURE_UI_CAM_SCREENSHOT
        {
        
        }
    
        public struct MSG_END_CAPTURE_UI_CAM_SCREENSHOT
        {
        
        }
	    
        public struct MSG_CHECK_COMMUNITY_REWARD
        {
        
        }

        public void SetAndroidWritePermissionGuide(Func<IEnumerator> onRequestor)
        {
            if (null != onRequestor)
            {
                _writePermissionRequestor = new Func<IEnumerator>(onRequestor);
            }
        }
	    
        public void SetAndroidReadPermissionGuide(Func<IEnumerator> onRequestor)
        {
            if (null != onRequestor)
            {
                _readPermissionRequestor = new Func<IEnumerator>(onRequestor);
            }
        }

        public void CaptureShare(Action<bool> callback = null, bool crop = true, string shareTextureSuffix = "en", string shareText = "")
        {
	        if (_isInCaptureShareProcess)
		        return;
	        
            StartCoroutine(CaptureShareProcess(callback, crop, shareTextureSuffix, shareText));
        }

	    private bool _isInCaptureShareProcess = false;
	    
        private IEnumerator CaptureShareProcess(Action<bool> callback, bool crop, string shareTextureSuffix, string shareText)
        {
	        _isInCaptureShareProcess = true;
            var possible = false;
#if UNITY_ANDROID
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                yield return new WaitForSeconds(0.5f);				 
                callback.Invoke(true);
				_isInCaptureShareProcess = false;
                yield break;
            }
#endif
            /*
            if (getSDKVersion() > 28)
	        {
		        yield return _readPermissionRequestor?.Invoke();
		        
		        possible = AndroidRuntimePermissions.Permission.Granted == GetPermissionStatus("android.permission.READ_EXTERNAL_STORAGE");
		        
		        if (!possible)
		        {
			        Debug.Log("READ_EXTERNAL_STORAGE(false)");
		        }
	        }
	        else
	        {
		        yield return _writePermissionRequestor?.Invoke();
		        yield return _readPermissionRequestor?.Invoke();

		        var writable = AndroidRuntimePermissions.Permission.Granted == GetPermissionStatus("android.permission.WRITE_EXTERNAL_STORAGE");
		        var readable = AndroidRuntimePermissions.Permission.Granted == GetPermissionStatus("android.permission.READ_EXTERNAL_STORAGE");

		        possible = writable && readable;
		        
		        if (!possible)
		        {
			        Debug.Log($"WRITE_EXTERNAL_STORAGE({writable}), READ_EXTERNAL_STORAGE({readable})");
		        }
	        }
            */
#else
            possible = !Application.isEditor;
#endif
            if (!possible)
            {
                callback?.Invoke(false);
	            _isInCaptureShareProcess = false;
                yield break;
            }

            yield return new WaitForSeconds(0.1f);

            var safeArea = Screen.safeArea;
            Texture2D texture;
	        
            Color[] c;
            Texture2D croppedTexture;
            yield return new WaitForEndOfFrame();
	        var mediaPath = "";
            try
            {
                texture = GetScreenShot(Mathf.RoundToInt(Screen.height * 0.8f), Screen.height);
                var width = Mathf.RoundToInt(safeArea.height * 0.8f);
                var startX = Mathf.RoundToInt((texture.width - width) / 2.0f); 
                Debug.Log($"startX : {startX}, safeArea.height : {safeArea.height}, width : {width} Screen.height : {Screen.height}");
                if (crop)
                {
                    croppedTexture = new Texture2D(width, Mathf.RoundToInt(safeArea.height));
                    c = texture.GetPixels(startX, Mathf.RoundToInt(safeArea.y), width, Mathf.RoundToInt(safeArea.height));
                    croppedTexture.SetPixels(c);
                    croppedTexture.Apply();
                }
                else
                    croppedTexture = texture;
	        

                Texture2D resizedTexture = croppedTexture;
	        
                var shareImgName = $"share_img_{shareTextureSuffix}";
                var shareSprite = Resources.Load<Sprite>(shareImgName);
                var shareTexture = shareSprite.texture;
                if (Screen.height > SCREENSHOT_HEIGHT)
                    resizedTexture = ScaleTexture(croppedTexture, Mathf.RoundToInt(SCREENSHOT_HEIGHT * 0.8f), SCREENSHOT_HEIGHT);
                else
                {
                    if(shareTexture != null)
                        shareTexture = ScaleTexture(shareTexture, Mathf.RoundToInt(shareTexture.width * (Screen.height / (float)SCREENSHOT_HEIGHT)), Mathf.RoundToInt(shareTexture.height * (Screen.height / (float)SCREENSHOT_HEIGHT)));
                }
	        
                if(shareTexture != null)    
                    AddTextureOn(resizedTexture, shareTexture, resizedTexture.width / 2 - shareTexture.width / 2 + 40 , 20);
#if UNITY_ANDROID
            if (getSDKVersion() >= 29)
            {
                NativeGallery.SaveImageToGallery(resizedTexture, "Screenshots", FileName, (success, path) =>
                {
                    mediaPath = path;
                });
                Debug.LogFormat("mediaPath : {0}", mediaPath);
            }
            else
            {
                byte[] imageData = resizedTexture.EncodeToJPG();

                mediaPath = GetSavePath(DirName, FileName);
				Debug.LogFormat("mediaPath : {0}", mediaPath);
	
        		File.WriteAllBytes(mediaPath, imageData);

        		SaveImageToGallery(mediaPath);
			}
#else
	        
                //ios path
                var dir = Path.Combine(Application.persistentDataPath, DirName);
                string msg;
                //icloud backup 제외시킬겸, 폴더 생성 체크겸
                if (CreateDirectory(dir, out msg) == false)
                {
                    Debug.Log("SHARE Fail : Create Dicrectory");
                    yield break;
                }
	        
                mediaPath = Path.Combine(Application.persistentDataPath, DirName, FileName);

                byte[] imageData = resizedTexture.EncodeToPNG();
                File.WriteAllBytes(mediaPath, imageData);

#endif
	            DestroyImmediate(croppedTexture);
	            DestroyImmediate(texture);
	            DestroyImmediate(resizedTexture);
	            
            }
            catch (Exception e)
            {
	            Debug.Log($"message : {e.Message}");
            }
            finally
            {
	            callback?.Invoke(false);
	            _isInCaptureShareProcess = false;
            } 

            yield return new WaitForSeconds(0.5f);
            /*
	        try
	        {
		        Debug.Log($"PionProvider share sharetext: {shareText}");
                
		        Service.GetProvider<Studio.Common.Pion.PionProvider>().Share(new Studio.Common.Pion.Sharing.Model()
		        {
			        text = shareText,
			        url = "",
			        mediaPath = mediaPath,
			        onResult = new System.Action<bool>(result =>
			        {
				        Debug.Log($"Share result: {result}");
				        callback?.Invoke(result);
			        })
		        });
	        }
	        catch (Exception e)
	        {
		        Debug.Log($"message : {e.Message}");
	        }
	        finally
	        {
		        callback?.Invoke(false);
		        _isInCaptureShareProcess = false;
	        }

	        yield return Service.GetProvider<Studio.Common.Pion.PionProvider>().Dispatch();
	        */
	        _isInCaptureShareProcess = false;
	        
	        yield return null;
        }

	    
        public Texture2D GetScreenShot(int width, int height)
        {
            List<Camera> cameras = new List<Camera>(Camera.allCameras);
            // sort cameras using Depth
            cameras.Sort((cam1, cam2) => cam1.depth.CompareTo(cam2.depth));

            var renderTextureFormat = RenderTextureFormat.Default;
#if !UNITY_EDITOR && UNITY_IOS
		    if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGB565))
			    renderTextureFormat = RenderTextureFormat.RGB565;
#endif
            RenderTexture renderTexture = RenderTexture.GetTemporary(width, height, 24, renderTextureFormat);
            RenderTexture.active = renderTexture;
            foreach (Camera cam in cameras)
            {
                if (cam.isActiveAndEnabled)
                {
                    if(cam.name.Equals("FxCamera") || cam.name.Equals("UICamera"))
                        cam.clearFlags = CameraClearFlags.Depth;
                    if(cam.name.Equals("UICamera"))
                        Service.Message.Publish(new MSG_START_CAPTURE_UI_CAM_SCREENSHOT());
                    float prevFov = cam.fieldOfView;
                    cam.targetTexture = renderTexture;
                    cam.Render();
                    cam.targetTexture = null;
                    cam.fieldOfView = prevFov;
                    if(cam.name.Equals("FxCamera") || cam.name.Equals("UICamera"))
                        cam.clearFlags = CameraClearFlags.Nothing;
                    if(cam.name.Equals("UICamera"))
                        Service.Message.Publish(new MSG_END_CAPTURE_UI_CAM_SCREENSHOT());
                }
            }
 
            Texture2D result = new Texture2D(width, height, TextureFormat.ARGB32, false);
            result.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
            result.Apply();
 
            RenderTexture.active = null;
 
            RenderTexture.ReleaseTemporary(renderTexture);
 
            return result;
        }
	    
        private void SetParentCanvasScalerResolution(bool reset)
        {
            var cardInfoView = Service.UIManager.GetView("CardInfoView");
            var cardSetCompleteView = Service.UIManager.GetView("CardSetCompleteView");
            var tournamentResultNormalView = Service.UIManager.GetView("TournamentResultNormalView");
            var tournamentResultRankerView = Service.UIManager.GetView("TournamentResultRankerView");

            if (cardInfoView != null && cardInfoView.isActiveAndEnabled ||
                cardSetCompleteView != null && cardSetCompleteView.isActiveAndEnabled ||
                tournamentResultNormalView != null && tournamentResultNormalView.isActiveAndEnabled ||
                tournamentResultRankerView != null && tournamentResultRankerView.isActiveAndEnabled)
            {
                List<Camera> cameras = new List<Camera>(Camera.allCameras);
                foreach (Camera cam in cameras)
                {
                    if (cam.name.Equals("UICamera") || cam.name.Equals("FxCamera"))
                    {
                        cam.clearFlags = reset ? CameraClearFlags.Nothing :  CameraClearFlags.Depth;
                        if (cam.name.Equals("FxCamera"))
                            continue;
                        var parent = cam.gameObject.transform.parent;
                        if (parent)
                        {
                            var parentCanvasScaler = parent.GetComponent<CanvasScaler>();
                            if (parentCanvasScaler)
                                parentCanvasScaler.referenceResolution = new Vector2(reset ? 750.0f : parentCanvasScaler.referenceResolution.y * 0.8f, parentCanvasScaler.referenceResolution.y);
                        }    
                    }
                }   
            }
        }

#if UNITY_ANDROID
	    private const string MEDIA_STORE_IMAGE_MEDIA = "android.provider.MediaStore$Images$Media";
	    protected static AndroidJavaObject m_Activity;

	    protected static AndroidJavaObject Texture2DToAndroidBitmap(Texture2D a_Texture)
	    {
		    byte[] encodedTexture = a_Texture.EncodeToPNG();
		    using (AndroidJavaClass bitmapFactory = new AndroidJavaClass("android.graphics.BitmapFactory"))
		    {
			    return bitmapFactory.CallStatic<AndroidJavaObject>("decodeByteArray", encodedTexture, 0, encodedTexture.Length);
		    }
	    }

	    protected static AndroidJavaObject Activity
	    {
		    get
		    {
			    if (m_Activity == null)
			    {
				    AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
				    m_Activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
			    }
			    return m_Activity;
		    }
	    }
	    
	    private static int getSDKVersion() 
		{
		    using (AndroidJavaClass version = new AndroidJavaClass("android.os.Build$VERSION"))
			{
				return version.GetStatic<int>("SDK_INT");
			}
	    }

	    private static void SaveImageToGallery( string path)
	    {
		    AndroidJavaObject unityClass = new AndroidJavaClass( "com.unity3d.player.UnityPlayer" );
		    AndroidJavaObject context = unityClass.GetStatic<AndroidJavaObject>( "currentActivity" );
		    AndroidJavaClass mediaScannerConnection = new AndroidJavaClass("android.media.MediaScannerConnection");
		    string[] param = {path};
		    mediaScannerConnection.CallStatic( "scanFile", context, param, null, null);
	    }
	    
	    //use contentResolver (Android Scoped Storage)
	    protected static string SaveImageToGallery(Texture2D a_Texture, string a_Title, string a_Description)
	    {
		    using (AndroidJavaClass mediaClass = new AndroidJavaClass(MEDIA_STORE_IMAGE_MEDIA))
		    {
			    using (AndroidJavaObject contentResolver = Activity.Call<AndroidJavaObject>("getContentResolver"))
			    {
				    AndroidJavaObject image = Texture2DToAndroidBitmap(a_Texture);
				    return mediaClass.CallStatic<string>("insertImage", contentResolver, image, a_Title, a_Description);
			    }
		    }
	    }


	    private static string GetSavePath( string album, string filenameFormatted )
	    {
		    string saveDir;
		    string path;
		    AndroidJavaClass env = new AndroidJavaClass("android.os.Environment");
		    AndroidJavaObject file1 = env.CallStatic<AndroidJavaObject>( "getExternalStorageDirectory");
		    path = file1.Call<string>( "getAbsolutePath");
	
		    if( path.ElementAt( path.Length - 1 ) != Path.DirectorySeparatorChar)
			    path = path + Path.DirectorySeparatorChar;

		    if( album.ElementAt( album.Length - 1 ) == '/' || album.ElementAt( album.Length - 1 ) == '\\' )
			    album = album.Substring( 0, album.Length - 1 );

		    path += album + Path.DirectorySeparatorChar;

		    AndroidJavaObject file2 = new AndroidJavaObject("java.io.File", path);
		    file2.Call<bool>("mkdirs");
		    saveDir = path;
	    
		    if( filenameFormatted.Contains( "{0}" ) )
		    {
			    int fileIndex = 0;
			    string p;
			    do
			    {
				    p = Path.Combine( saveDir, string.Format( filenameFormatted, ++fileIndex ) );
			    } while( File.Exists( p ) );

			    return p;
		    }

		    saveDir = Path.Combine( saveDir, filenameFormatted );
		    return saveDir;
	    }
        /*
		private AndroidRuntimePermissions.Permission GetPermissionStatus(string permission)
		{
			return AndroidRuntimePermissions.CheckPermission(permission);
		}
        */
		public static long getAvailableStorageCapacity(bool preferExternalStorage = true)
		{
			AndroidJavaClass androidNative = new AndroidJavaClass ("com.linecorp.LGPJCOIN.AndroidNative");
			return androidNative.CallStatic<long>("getAvailableStorageCapacity", preferExternalStorage);
		}

        private static string getUsimCountryCode()
        {
            AndroidJavaClass androidNative = new AndroidJavaClass ("com.linecorp.LGPJCOIN.AndroidNative");
			return androidNative.CallStatic<string>("getUsimCountryCode");
        }
#endif

        private static void AddTextureOn(Texture2D texture, Texture2D coverTexture, int startX, int startY)
        {
            int watermarkWidth = coverTexture.width;
            int watermarkHeight = coverTexture.height;

            Color[] coverUpPixels = coverTexture.GetPixels();
            Color[] originalPixels = texture.GetPixels(startX, startY, watermarkWidth, watermarkHeight);
		    
            for(int i = 0; i < coverUpPixels.Length; i++)
            {
			    
                var pixel = coverUpPixels[i];
                if(pixel.a > 0)
                    originalPixels[i] = Color.Lerp(originalPixels[i], pixel, pixel.a / 1);
            }

            texture.SetPixels(startX, startY, watermarkWidth, watermarkHeight, originalPixels);
            texture.Apply();
        }
	    
        private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
        {
            Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, true);
            Color[] rpixels = result.GetPixels(0);
            float incX = (1.0f / (float)targetWidth);
            float incY = (1.0f / (float)targetHeight);
            for (int px = 0; px < rpixels.Length; px++)
            {
                rpixels[px] = source.GetPixelBilinear(incX * ((float)px % targetWidth), incY * ((float)Mathf.Floor(px / targetWidth)));
            }
            result.SetPixels(rpixels, 0);
            result.Apply();
            return result;
        }
	    
        #endregion
	    
        #region Webview
        
        public void ShowWebView(string url, string cookie)
        {/*
            var phase = "SERVICE_RELEASE";
#if SERVICE_ALPHA
			phase = "SERVICE_ALPHA";
		#elif SERVICE_BETA
			phase = "SERVICE_BETA";
		#elif SERVICE_RC
			phase = "SERVICE_RC";
		#else
            phase = "SERVICE_RELEASE";
#endif

            var trident = Service.GetProvider<TridentProvider>();
            if (string.IsNullOrEmpty(cookie))
            {	    
                var tridentCore = trident ? trident.GetService<Core>() : default;
                cookie = tridentCore?.appId;
            }

            string title = "";

#if UNITY_EDITOR
            Debug.LogErrorFormat("InAppWebview will open  url : {0}", url);
            return;
#elif UNITY_IOS
            var userKey = trident.GetService<Auth>()?.userKey;
			_setUUID(userKey);
			_showWebView(url, title, cookie);
#elif UNITY_ANDROID
			AndroidJavaClass androidNative = new AndroidJavaClass("com.linecorp.LGPJCOIN.AndroidNative");
			androidNative.CallStatic("showWebView", new object[]{ url, title, cookie});

#endif
        */}

        #endregion

#if !UNITY_EDITOR && UNITY_IOS
		[DllImport("__Internal")]
		private static extern void _showWebView(string url, string title, string cookie);
		[DllImport("__Internal")]
		private static extern void _setUUID(string uuid);
		[DllImport("__Internal")]
		private static extern void _performDynamicLinksDiagnostics();
		[DllImport("__Internal")]
		private static extern ulong getAvailableStorageCapacity();
        [DllImport("__Internal")]
		private static extern void _openSetting();
        [DllImport("__Internal")]
		private static extern string _getUsimCountryCode();
        
#endif

        public void RenderOutsideSafeArea(bool isActive)
        {
#if !UNITY_EDITOR && UNITY_ANDROID
			if (getSDKVersion() < 28)
				return;
			
			using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
			{
				using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
				{
					var window = activity.Call<AndroidJavaObject>("getWindow");
					var attribute = window.Call<AndroidJavaObject>("getAttributes");

					int cutOutMode = isActive ? LAYOUT_IN_DISPLAY_CUTOUT_MODE_SHORT_EDGES : 
											    LAYOUT_IN_DISPLAY_CUTOUT_MODE_NEVER;

					attribute.Set("layoutInDisplayCutoutMode", cutOutMode);
					activity.Call("runOnUiThread", new AndroidJavaRunnable(() => 
					{
						window.Call("setAttributes", attribute);
					}));
				}
			}
#endif
        }

        public static long GetAvailableStorageCapacity()
        {
            long value = long.MaxValue;
#if !UNITY_EDITOR && UNITY_ANDROID
			value = getAvailableStorageCapacity();
#elif !UNITY_EDITOR && UNITY_IOS
			value = long.Parse(getAvailableStorageCapacity().ToString());
#endif
            Debug.Log($"Available storage capacity : {value}");
            return value;
        }

        public static void OpenSetting()
        {
#if !UNITY_EDITOR && UNITY_IOS
            _openSetting();
#endif
        }

        public static string GetUsimCountryCode()
        {
#if UNITY_EDITOR
            return "ko";
#elif UNITY_ANDROID
            return getUsimCountryCode();
#elif UNITY_IOS
            return _getUsimCountryCode();
#else
            return "ko";
#endif
        }

        public void PerformDynamicLinksDisgnostics()
        {
#if !UNITY_EDITOR && UNITY_IOS
			_performDynamicLinksDiagnostics();
#endif
        }

        protected override void OnDestroy()
        {
#if UNITY_ANDROID
			m_Activity?.Dispose();
#endif
            base.OnDestroy();
        }
	    
        private DateTime _timeWentToBackground;
        private void OnApplicationPause(bool pauseStatus)
        {
            Debug.LogFormat("OnApplicationPause pauseStatus : {0}", pauseStatus);
		    
            if (pauseStatus)
            {
                _timeWentToBackground = DateTime.Now;
                return;
            }
		    
            var deltaTime = DateTime.Now - _timeWentToBackground;
            var stayBackgroundSeconds = Math.Abs(deltaTime.TotalSeconds);
            if (stayBackgroundSeconds >= 30 * 60) // 30 * 60 30min
            {
                Service.Reboot();
            }
        }

	    public void OnDeeplinkCallback(string message)
	    {
		    if (!string.IsNullOrEmpty(message))
		    {
			    Debug.LogFormat("OnDeeplinkCallback message : {0}", message);    
			    if (!message.Contains("ORGANIC"))
				    Service.Message.Publish(new MSG_CHECK_COMMUNITY_REWARD());
		    }
	    }
/*
        public string GetDeeplinkResult()
        {
            return Service.GetProvider<Common.Pion.PionProvider>().GetDeeplinkResult();
        }
	    
        public void ResetDeeplinkResult()
        {
            Service.GetProvider<Common.Pion.PionProvider>().ResetDeeplinkResult();
        }
*/
    }
}