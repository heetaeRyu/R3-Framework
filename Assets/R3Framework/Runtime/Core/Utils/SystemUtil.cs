using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Netmarble.Core
{
	public enum ExceptionType
	{
		MethodNotImplemented,
		UnSupportedInputType
	}

	public static class SystemUtil
	{
		private static List<UnityEngine.Object> _dontDestroyList = new List<UnityEngine.Object>();

		public static void OpenURL(string url)
		{
#if UNITY_WEBGL
		Application.ExternalEval("window.open('" + url + "','_blank')");
#else
			Application.OpenURL(url);
#endif
		}

		//마지막의 "/"은 제외하고 리턴
		public static string AbsPath
		{
			get
			{
#if UNITY_EDITOR
				string absPath = Application.dataPath;
				return absPath.Substring(0, absPath.LastIndexOf("/"));
#else
				return Application.dataPath;
#endif
			}
		}

		public static Dictionary<string, string[]> EnvironmentArgumentDic
		{
			get
			{
				Dictionary<string, string[]> dic = new Dictionary<string, string[]>();
				List<string> list = new List<string>();
				string[] args = Environment.GetCommandLineArgs();
				if (args != null)
				{
					int length = args.Length;
					for (int i = 0; i < length; ++i)
					{
						string value = args[i];
						string key = GetKey(value);
						if (key != null)
						{
							int index = i + 1;
							list.Clear();
							while (index < length)
							{
								string arg = args[index];
								if (GetKey(arg) == null)
								{
									list.Add(arg);
									index++;
								}
								else
								{
									i = index - 1;
									break;
								}
							}

							dic.Add(key, list.ToArray());
						}
					}
				}

				return dic;
			}
		}

		private static string GetKey(string value)
		{
			string key = null;
			if (value.IndexOf("-") == 0)
				key = value.Substring(1, value.Length - 1);
			return key;
		}

		public static bool IsDiskFull(Exception ex)
		{
			const long ERROR_HANDLE_DISK_FULL = 0x27;
			const long ERROR_DISK_FULL = 0x70;
			long errorCode = System.Runtime.InteropServices.Marshal.GetHRForException(ex) & 0xFFFF;

			return (errorCode == ERROR_HANDLE_DISK_FULL || errorCode == ERROR_DISK_FULL);
		}

		public static string GetMD5(string path)
		{
			if (!File.Exists(path))
			{
				return string.Empty;
			}

			try
			{
				StringBuilder sb = new StringBuilder();
				byte[] byteResult = null;

				using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					byteResult = (new MD5CryptoServiceProvider()).ComputeHash(fs);
				}

				if (byteResult != null)
				{
					for (int i = 0; i < byteResult.Length; i++)
					{
						sb.Append(byteResult[i].ToString("X2"));
					}
				}

				return sb.ToString();
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}

			return string.Empty;
		}

		public static string GetVersionFormat(int version, int masterVersion = 0)
		{
			var arr = version.ToString().ToCharArray();
			var length = arr.Length;
			var ver = "";
			var versionList = new List<int>();
			for (int i = 0; i < length; ++i)
			{
				if (i != 0 && i % 3 == 0)
				{
					versionList.Add(int.Parse(ver));
					ver = "";
				}

				ver = arr[length - (i + 1)].ToString() + ver;
			}

			if (ver != "") versionList.Add(int.Parse(ver));

			ver = "";
			length = 3;
			var list = new string[length];
			for (int i = 0; i < length; ++i)
			{
				var versionValue = versionList.Count <= i ? 0 : versionList[i];
				var versionStr = i == 2 ? (versionValue + masterVersion).ToString() : versionValue.ToString();
				list[length - (i + 1)] = versionStr;
			}

			return string.Join(".", list);
		}

		public static int GetVersionNumber(string version)
		{
			var list = version.Split('.');
			var versionStr = "";
			foreach (var ver in list)
				versionStr += int.Parse(ver).ToString("D3");

			var verNum = int.Parse(versionStr);
			Console.WriteLine(verNum);
			return verNum;
		}

#if UNITY_EDITOR
		public static string ProjectName
		{
			get
			{
				string path = SystemUtil.AbsPath;
				int lastIndex = path.LastIndexOf("/") + 1;

				return path.Substring(lastIndex, path.Length - lastIndex);
			}
		}
#endif

		public static string TimeStamp
		{
			get
			{
				System.DateTime time = System.DateTime.Now;
				string year = time.Year.ToString().Substring(2, 2);
				return $"{year:D2}{time.Month:D2}{time.Day:D2}_{time.Hour:D2}{time.Minute:D2}";
			}
		}

		public static string TieStampWithSec
		{
			get
			{
				DateTime time = DateTime.Now;
				string year = time.Year.ToString().Substring(2, 2);
				return $"{year:D2}{time.Month:D2}{time.Day:D2}_{time.Hour:D2}{time.Minute:D2}{time.Second:D2}";
			}
		}

		public static string currentProjectName
		{
			get
			{
				string[] s = Application.dataPath.Split('/');
				return s[s.Length - 2];
			}
		}

		public static Exception GetException(ExceptionType type, object obj, string extendDesc = null)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append("[").Append(obj.ToString()).Append("] ");
			switch (type)
			{
				case ExceptionType.MethodNotImplemented:
					builder.Append("The method was not implement yet.");
					break;
				case ExceptionType.UnSupportedInputType:
					builder.Append("This type of device is not support input type.");
					break;
			}

			if (extendDesc != null)
			{
				builder.Append(" ").Append(extendDesc);
			}

			return new Exception(builder.ToString());
		}

#if UNITY_EDITOR
		public static string androidSDKPath
		{
			get { return EditorPrefs.GetString("AndroidSdkRoot"); }
			set { EditorPrefs.SetString("AndroidSdkRoot", value); }
		}

		public static string adbPath
		{
			get
			{
				string path = androidSDKPath + "/platform-tools/adb";
#if UNITY_EDITOR_WIN
				path += ".exe";
#endif
				return path;
			}
		}

		public static string jdkPath
		{
			get => EditorPrefs.GetString("JdkPath");
			set => EditorPrefs.SetString("JdkPath", value);
		}
#endif

#if UNITY_EDITOR
		public static void ShowTypeAssembly(Type type, string target = null)
		{
			Assembly assembly = type.Assembly; //typeof(UnityEditor.EditorWindow).Assembly; //Get Type to EditorWindow
			Type[] types = assembly.GetTypes();
			if (target == null)
			{
				foreach (Type ty in types)
				{
					Debug.Log(ty.ToString());
				}
			}
			else
			{
				foreach (Type ty in types)
				{
					string typeStr = ty.ToString();
					if (typeStr.IndexOf(target) != -1)
					{
						Debug.Log(typeStr);
					}
				}
			}
		}

		public static void ShowMemberNames(Type classType)
		{
			MemberInfo[] mInfos = classType.GetMembers();
			foreach (MemberInfo mInfo in mInfos)
			{
				Debug.Log("Name : " + mInfo.Name + ", Type :" + mInfo.MemberType);
			}
		}

		public static void ShowMemberNames(string typeName)
		{
			ShowMemberNames(Type.GetType(typeName));
		}

		public static void ShowPropertyNames(Type classType)
		{
			PropertyInfo[] pInfos = classType.GetProperties();
			foreach (PropertyInfo pInfo in pInfos)
			{
				Debug.Log("Name : " + pInfo.Name + ", Type :" + pInfo.PropertyType + ", CanRead : " + pInfo.CanRead +
				          ", CanWrite : " + pInfo.CanWrite);
			}
		}

		public static void ShowPropertyNames(string typeName)
		{
			ShowPropertyNames(Type.GetType(typeName));
		}

		public static void ShowFieldNames(Type classType)
		{
			FieldInfo[] fInfos = classType.GetFields();
			foreach (FieldInfo fInfo in fInfos)
			{
				Debug.Log("Name : " + fInfo.Name + ", Type :" + fInfo.FieldType);
			}
		}

		public static void ShowFieldNames(string typeName)
		{
			ShowFieldNames(Type.GetType(typeName));
		}

		public static void ShowMethodNames(Type classType)
		{
			MethodInfo[] mInfos = classType.GetMethods();
			int pLength;
			ParameterInfo pInfo;
			string paramStr;
			foreach (MethodInfo mInfo in mInfos)
			{
				ParameterInfo[] pInfos = mInfo.GetParameters();
				pLength = pInfos.Length;
				paramStr = "";
				for (int i = 0; i < pLength; ++i)
				{
					pInfo = pInfos[i];
					if (i != 0)
					{
						paramStr += ", ";
					}
					else
					{
						paramStr += " ";
					}

					paramStr += "[" + pInfo.ParameterType + "]" + pInfo.Name;

					if (i == pLength - 1)
					{
						paramStr += " ";
					}
				}

				Debug.Log(mInfo.Name + " (" + paramStr + ") : " + mInfo.ReturnType);
			}
		}

		public static void ShowMethodNames(string typeName)
		{
			ShowMemberNames(Type.GetType(typeName));
		}

#endif
		public static void GenerateFolder(string path)
		{
			string[] folders = path.Split(new[] { '/' });
			string pathAdd = "";
			int length = folders.Length;
			for (int i = 0; i < length; ++i)
			{
				pathAdd += folders[i] + "/";
				if (!Directory.Exists(pathAdd))
				{
					Directory.CreateDirectory(pathAdd);
				}
			}
		}

		public static string GetGameObjectPath(GameObject target)
		{
			string path = target.name;
			Transform parent = target.transform.parent;
			while (parent != null)
			{
				path = parent.gameObject.name + "/" + path;
				parent = parent.transform.parent;
			}

			return path;
		}

		public static string GetLanguageToCode(SystemLanguage language, string defaultCode = "en")
		{
			string code;
			switch (language)
			{
				case SystemLanguage.Afrikaans:
					code = "af";
					break;
				case SystemLanguage.Arabic:
					code = "ar";
					break;
				case SystemLanguage.Basque:
					code = "eu";
					break;
				case SystemLanguage.Belarusian:
					code = "be";
					break;
				case SystemLanguage.Bulgarian:
					code = "bg";
					break;
				case SystemLanguage.Catalan:
					code = "ca";
					break;
				case SystemLanguage.Chinese:
					code = "zh";
					break;
				case SystemLanguage.Czech:
					code = "cs";
					break;
				case SystemLanguage.Danish:
					code = "da";
					break;
				case SystemLanguage.Dutch:
					code = "nl";
					break;
				case SystemLanguage.English:
					code = "en";
					break;
				case SystemLanguage.Estonian:
					code = "et";
					break;
				case SystemLanguage.Faroese:
					code = "fo";
					break;
				case SystemLanguage.Finnish:
					code = "fi";
					break;
				case SystemLanguage.French:
					code = "fr";
					break;
				case SystemLanguage.German:
					code = "de";
					break;
				case SystemLanguage.Greek:
					code = "el";
					break;
				case SystemLanguage.Hebrew:
					code = "he";
					break;
				case SystemLanguage.Hungarian:
					code = "hu";
					break;
				case SystemLanguage.Icelandic:
					code = "is";
					break;
				case SystemLanguage.Indonesian:
					code = "id";
					break;
				case SystemLanguage.Italian:
					code = "it";
					break;
				case SystemLanguage.Japanese:
					code = "ja";
					break;
				case SystemLanguage.Korean:
					code = "ko";
					break;
				case SystemLanguage.Latvian:
					code = "lv";
					break;
				case SystemLanguage.Lithuanian:
					code = "lt";
					break;
				case SystemLanguage.Norwegian:
					code = "nb";
					break;
				case SystemLanguage.Polish:
					code = "pl";
					break;
				case SystemLanguage.Portuguese:
					code = "pt";
					break;
				case SystemLanguage.Romanian:
					code = "ro";
					break;
				case SystemLanguage.Russian:
					code = "ru";
					break;
				case SystemLanguage.SerboCroatian:
					code = "sr";
					break;
				case SystemLanguage.Slovak:
					code = "sk";
					break;
				case SystemLanguage.Slovenian:
					code = "sl";
					break;
				case SystemLanguage.Spanish:
					code = "es";
					break;
				case SystemLanguage.Swedish:
					code = "sv";
					break;
				case SystemLanguage.Thai:
					code = "th";
					break;
				case SystemLanguage.Turkish:
					code = "tr";
					break;
				case SystemLanguage.Ukrainian:
					code = "uk";
					break;
				default:
					code = defaultCode;
					break;
			}

			return code;
		}

		public static SystemLanguage GetCodeToLanguage(string code)
		{
			SystemLanguage language;
			switch (code)
			{
				case "af":
					language = SystemLanguage.Afrikaans;
					break;
				case "ar":
					language = SystemLanguage.Arabic;
					break;
				case "eu":
					language = SystemLanguage.Basque;
					break;
				case "be":
					language = SystemLanguage.Belarusian;
					break;
				case "bg":
					language = SystemLanguage.Bulgarian;
					break;
				case "ca":
					language = SystemLanguage.Catalan;
					break;
				case "zh":
					language = SystemLanguage.Chinese;
					break;
				case "cs":
					language = SystemLanguage.Czech;
					break;
				case "da":
					language = SystemLanguage.Danish;
					break;
				case "nl":
					language = SystemLanguage.Dutch;
					break;
				case "en":
					language = SystemLanguage.English;
					break;
				case "et":
					language = SystemLanguage.Estonian;
					break;
				case "fo":
					language = SystemLanguage.Faroese;
					break;
				case "fi":
					language = SystemLanguage.Finnish;
					break;
				case "fr":
					language = SystemLanguage.French;
					break;
				case "de":
					language = SystemLanguage.German;
					break;
				case "el":
					language = SystemLanguage.Greek;
					break;
				case "he":
					language = SystemLanguage.Hebrew;
					break;
				case "hu":
					language = SystemLanguage.Hungarian;
					break;
				case "is":
					language = SystemLanguage.Icelandic;
					break;
				case "id":
					language = SystemLanguage.Indonesian;
					break;
				case "it":
					language = SystemLanguage.Italian;
					break;
				case "ja":
					language = SystemLanguage.Japanese;
					break;
				case "ko":
					language = SystemLanguage.Korean;
					break;
				case "lv":
					language = SystemLanguage.Latvian;
					break;
				case "lt":
					language = SystemLanguage.Lithuanian;
					break;
				case "nb":
					language = SystemLanguage.Norwegian;
					break;
				case "pl":
					language = SystemLanguage.Polish;
					break;
				case "pt":
					language = SystemLanguage.Portuguese;
					break;
				case "ro":
					language = SystemLanguage.Romanian;
					break;
				case "ru":
					language = SystemLanguage.Russian;
					break;
				case "sr":
					language = SystemLanguage.SerboCroatian;
					break;
				case "sk":
					language = SystemLanguage.Slovak;
					break;
				case "sl":
					language = SystemLanguage.Slovenian;
					break;
				case "es":
					language = SystemLanguage.Spanish;
					break;
				case "sv":
					language = SystemLanguage.Swedish;
					break;
				case "th":
					language = SystemLanguage.Thai;
					break;
				case "tr":
					language = SystemLanguage.Turkish;
					break;
				case "uk":
					language = SystemLanguage.Ukrainian;
					break;
				default:
					language = SystemLanguage.Unknown;
					break;
			}

			return language;
		}
	}
}