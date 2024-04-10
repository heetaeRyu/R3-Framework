using System.IO;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Netmarble.Core
{
	public class FileReference
	{
		public static string Read(string path)
		{
			string contents;
			var parent = new FileInfo(path).DirectoryName;
			if (Directory.Exists(parent))
			{
				FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read);
				StreamReader sr = new StreamReader(file);

				contents = sr.ReadToEnd();
				sr.Close();
				file.Close();
				return contents;
			}
			else return null;
		}

		private static bool IsFileOpened(string path)
		{
			bool isFileOpend = false;
			try
			{
				using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None))
				{
					stream.Close();
				}
			}
			catch (IOException)
			{
				isFileOpend = true;
			}

			return isFileOpend;
		}

		public static void Write(string path, string contents, bool isOverride = true)
		{
			if (isOverride)
			{
				// 두개 이상 에디터에서 file io시 file io 익셉션 발생 이슈 수정
				if (!IsFileOpened(path) && File.Exists(path))
				{
					File.Delete(path);
				}
			}

			var parent = new FileInfo(path).DirectoryName;
			Directory.CreateDirectory(parent);
			FileStream file = new FileStream(path, FileMode.Create, FileAccess.Write);
			StreamWriter sw = new StreamWriter(file, Encoding.UTF8);
			sw.Write(contents);
			sw.Close();
			file.Close();

#if UNITY_EDITOR
			AssetDatabase.Refresh();
#endif
		}

		public static void Write(string path, byte[] contents, bool isOverride = true)
		{
			if (isOverride)
			{
				// 두개 이상 에디터에서 file io시 file io 익셉션 발생 이슈 수정
				if (!IsFileOpened(path) && File.Exists(path))
				{
					File.Delete(path);
				}
			}

			var parent = new FileInfo(path).DirectoryName;
			Directory.CreateDirectory(parent);
			FileStream file = new FileStream(path, FileMode.Create, FileAccess.Write);
			StreamWriter sw = new StreamWriter(file, Encoding.UTF8);
			sw.Write(contents);
			sw.Close();
			file.Close();

#if UNITY_EDITOR
			AssetDatabase.Refresh();
#endif
		}

		public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
		{
#if UNITY_EDITOR
			// Get the subdirectories for the specified directory.
			DirectoryInfo dir = new DirectoryInfo(sourceDirName);

			if (!dir.Exists)
			{
				throw new DirectoryNotFoundException(
					"Source directory does not exist or could not be found: "
					+ sourceDirName);
			}

			DirectoryInfo[] dirs = dir.GetDirectories();
			// If the destination directory doesn't exist, create it.
			if (!Directory.Exists(destDirName))
			{
				Directory.CreateDirectory(destDirName);
			}

			// Get the files in the directory and copy them to the new location.
			FileInfo[] files = dir.GetFiles();
			int length = files.Length;
			for (int i = 0; i < length; ++i)
			{
				FileInfo file = files[i];
				string temppath = Path.Combine(destDirName, file.Name);
				file.CopyTo(temppath, false);
			}

			// If copying subdirectories, copy them and their contents to new location.
			if (copySubDirs)
			{
				foreach (DirectoryInfo subdir in dirs)
				{
					string temppath = Path.Combine(destDirName, subdir.Name);
					DirectoryCopy(subdir.FullName, temppath, copySubDirs);
				}
			}
#endif
		}
	}
}