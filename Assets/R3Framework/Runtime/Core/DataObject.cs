using UnityEngine;

namespace Netmarble.Core
{
	public class DataObject : MonoBehaviour
	{
		private static DataObject _instance;

		public static DataObject To
		{
			get
			{
				if (!_instance)
				{
					GameObject game = new GameObject("DataObject")
					{
						isStatic = true
					};
					_instance = game.AddComponent<DataObject>();
					_instance.GameObject = game;

					if (Application.isPlaying) DontDestroyOnLoad(game);
				}

				return _instance;
			}
		}

		public GameObject GameObject { get; private set; }
	}
}