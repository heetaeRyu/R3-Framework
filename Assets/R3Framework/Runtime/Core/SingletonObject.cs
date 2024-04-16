using UnityEngine;

namespace Netmarble.Core
{
    public class SingletonObject<Type> : MonoBehaviour where Type : MonoBehaviour
    {
        /************************************************************************
         *	 	 	 	 	Private Variable Declaration	 	 	 	 	 	*
         ************************************************************************/
        private static bool _isQuitting = false;
        private static object _lock = new object();
        private static Type _instance;

        /************************************************************************
         *	 	 	 	 	Protected Variable Declaration	 	 	 	 	 	*
         ************************************************************************/


        /************************************************************************
         *	 	 	 	 	Public Variable Declaration	 	 	 	 	 		*
         ************************************************************************/


        /************************************************************************
         *	 	 	 	 	Getter & Setter Declaration	 	 	 	 	 		*
         ************************************************************************/
        public static Type Instance
        {
            get
            {
                if (_isQuitting)
                {
                    return null;
                }

                lock (_lock)
                {
                    if (!_instance)
                    {
                        _instance = (Type)FindObjectOfType(typeof(Type));

                        if (FindObjectsOfType(typeof(Type)).Length > 1)
                        {
                            return _instance;
                        }

                        if (!_instance)
                        {
                            if (!_instance)
                            {
                                _instance = MonoBehaviourObject.To.GameObject.AddComponent<Type>();
                            }
                        }

                        return _instance;
                    }
                }

                return _instance;
            }
        }

        /************************************************************************
         *	 	 	 	 	Initialize & Destroy Declaration	 	 	 		*
         ************************************************************************/


        /************************************************************************
         *	 	 	 	 	Life Cycle Method Declaration	 	 	 	 	 	*
         ************************************************************************/
        protected virtual void OnDestroy()
        {
            _isQuitting = true;
        }


        /************************************************************************
         *	 	 	 	 	Coroutine Declaration	 	  			 	 		*
         ************************************************************************/


        /************************************************************************
         *	 	 	 	 	Private Method Declaration	 	 	 	 	 		*
         ************************************************************************/


        /************************************************************************
         *	 	 	 	 	Protected Method Declaration	 	 	 	 	 	*
         ************************************************************************/


        /************************************************************************
         *	 	 	 	 	Public Method Declaration	 	 	 	 	 		*
         ************************************************************************/
    }
}