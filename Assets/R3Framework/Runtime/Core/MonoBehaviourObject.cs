using UnityEngine;

namespace Netmarble.Core
{
	public class MonoBehaviourObject : MonoBehaviour
	{
		private static MonoBehaviourObject _instance;

		public static MonoBehaviourObject To
		{
			get
			{
				if (!_instance)
				{
					GameObject game = new GameObject("MonoBehaviourObject")
					{
						isStatic = true
					};
					_instance = game.AddComponent<MonoBehaviourObject>();
					_instance.GameObject = game;

					if (Application.isPlaying) DontDestroyOnLoad(game);
				}

				return _instance;
			}
		}

		public GameObject GameObject { get; private set; }
	}
}