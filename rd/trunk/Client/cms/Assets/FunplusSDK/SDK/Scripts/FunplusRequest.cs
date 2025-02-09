﻿using UnityEngine;
using System;
using System.Text;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using HSMiniJSON;

namespace Funplus
{

	public delegate void FpRequestSuccess (Dictionary<string, object> data);
	public delegate void FpRequestError (FunplusError error);

	public delegate void FbRequestSuccess (Dictionary<string, object> data);
	public delegate void FbRequestError (string error);

	public class FunplusRequest : MonoBehaviour
	{
		private const string PASSPORT_ENDPOINT = "https://passport.funplusgame.com/client_api.php?ver=3";
		private const string FACEBOOK_ENDPOINT = "https://graph.facebook.com/me";

		public static FunplusRequest Instance { get; set; }

		void Awake ()
		{
			Instance = this;
		}

		public void FpPost (Dictionary<string, string> formData, FpRequestSuccess onSuccess = null, FpRequestError onError = null)
		{
			StartCoroutine (FpRequest (formData, onSuccess, onError));
		}

		public IEnumerator FpRequest (Dictionary<string, string> formData, FpRequestSuccess onSuccess, FpRequestError onError)
		{
			WWW www;

			if (formData == null)
			{
				Debug.LogError ("[funsdk] Failed to call FunplusRequest.Request(), the `formData` parameter must not be null.");
				www = null;
			}
			else
			{
				StringBuilder postData = new StringBuilder ();
				foreach (string key in formData.Keys)
				{
					if (postData.Length > 0)
					{
						postData.Append ("&");
					}
					postData.Append (key).Append ("=").Append (formData [key]);
				}

				Dictionary<string, string> headers = new Dictionary<string, string> ();
				headers.Add ("Authorization", MakeSigV3 (postData.ToString ()));
				www = new WWW (PASSPORT_ENDPOINT, StringEncode (postData.ToString()), headers);
			}

			yield return www;

			if (www == null || www.error != null || !www.isDone)
			{
				Debug.LogWarning ("[funsdk] Failed to request to passport server.");
				if (onError != null)
				{
					onError (FunplusError.E (1207));
				}
			}
			else if (string.IsNullOrEmpty (www.text))
			{
				Debug.LogWarning ("[funsdk] Response from passport server is empty.");
				if (onError != null)
				{
					onError (FunplusError.E (1208));
				}
			}
			else
			{
				Debug.Log (www.text);

				try
				{
					var dict = Json.Deserialize (www.text) as Dictionary<string,object>;
					long status = (long)dict ["status"];
					Dictionary<string, object> data = (Dictionary<string, object>)dict ["data"];

					if (status == 1)
					{
						if (onSuccess != null)
						{
							onSuccess (data);
						}
					}
					else
					{
						Debug.LogError ("[funsdk] Server responses an error.");
						long errorCode = (long)dict ["error"];

						if (onError != null)
						{
							onError (FunplusError.E (errorCode));
						}
					}
				}
				catch (Exception e)
				{
					Debug.LogErrorFormat ("[funsdk] Could not parse response: {0}", e.Message);
					if (onError != null)
					{
						onError (FunplusError.E (1208));
					}
				}
			}
		}

		public void FbGet (string accessToken, FbRequestSuccess onSuccess = null, FbRequestError onError = null)
		{
			StartCoroutine (FbRequest (accessToken, onSuccess, onError));
		}
			
		public IEnumerator FbRequest (string accessToken, FbRequestSuccess onSuccess = null, FbRequestError onError = null)
		{
			WWW www;
			
			if (string.IsNullOrEmpty (accessToken))
			{
				if (onError != null)
				{
					onError ("[funsdk] AccessToken cannot be null, check your configurations.");
				}
				www = null;
			}
			else
			{
				string url = string.Format ("{0}?access_token={1}", FACEBOOK_ENDPOINT, accessToken);
				www = new WWW (url);
			}

			yield return www;

			if (www == null || www.error != null || !www.isDone)
			{
				if (onError != null)
				{
					onError ("[funsdk] failed to request to Facebook");
				}
			}
			else if (string.IsNullOrEmpty (www.text))
			{
				if (onError != null) {
					onError ("[funsdk] Response from Facebook is empty.");
				}
			}
			else
			{
				Debug.Log (www.text);

				try
				{
					var dict = Json.Deserialize (www.text) as Dictionary<string,object>;

					if (dict.ContainsKey("error"))
					{
						if (onError != null)
						{
							onError((string) dict["error"]);
						}
					}
					else
					{
						if (onSuccess != null)
						{
							onSuccess (dict);
						}
					}
				}
				catch (Exception e)
				{
					if (onError != null)
					{
						onError (e.Message);
					}
				}
			}
		}

		private string MakeSigV3 (string postData)
		{
			string cipher = Hmac256 (FunplusSdk.Instance.GameKey, postData);
			return string.Format ("FP {0}:{1}", FunplusSdk.Instance.GameId, cipher);
		}

		private string Hmac256 (string key, string message)
		{
			var hash = new HMACSHA256 (StringEncode (key));
			return System.Convert.ToBase64String (StringEncode (HashEncode ((hash.ComputeHash (StringEncode (message))))));
		}

		private byte[] StringEncode (string s)
		{
			var encoding = new ASCIIEncoding ();
			return encoding.GetBytes (s);
		}

		private string HashEncode (byte[] hash)
		{
			return BitConverter.ToString (hash).Replace ("-", "").ToLower ();
		}

		private byte[] HexDecode (string hex)
		{
			var bytes = new byte[hex.Length / 2];
			for (int i = 0; i < bytes.Length; i++)
			{
				bytes [i] = byte.Parse (hex.Substring (i * 2, 2), NumberStyles.HexNumber);
			}
			return bytes;
		}
	}

}