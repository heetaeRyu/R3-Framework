namespace Netmarble.Core
{
    public class Singleton<Type> where Type : new()
    {
        private static Type _inst;

        public static Type Instance
        {
            get
            {
                if (_inst == null)
                {
                    _inst = new Type();
                }

                return _inst;
            }
        }

        public Singleton()
        {

        }

        public virtual void Destroy()
        {

        }
    }
}