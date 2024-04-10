using UnityEngine;
using System.Collections;

namespace Netmarble.Core
{
    public class UIComponent : MonoBehaviour, IUIComponent
    {
        protected bool _started = false;
        protected bool _composed = false;
        protected bool _reservedValidate = false;
        protected bool _reservedLateUpdateDisplayList = false;

        private void Awake()
        {
            this.Init();
        }

        private void OnEnable()
        {
            this.Enable();
            if (this._composed)
            {
                this.ValidateAsync();
            }
        }

        private void Start()
        {
            this._started = true;
            this.Validate();
        }

        private void OnDisable()
        {
            this.Disable();
        }

        private void OnDestroy()
        {
            this.Destroy();
        }

        private IEnumerator CoroutineValidate(int frameSkip)
        {
            if (frameSkip == 0) yield return null;
            else
            {
                if (frameSkip < 0)
                {
                    yield return null;
                }
                else
                {
                    for (int i = 0; i < frameSkip; ++i) yield return null;
                }
            }

            this._reservedValidate = false;
            this.Validate();
        }

        private IEnumerator CoroutineLateUpdateDisplayList()
        {
            yield return null;
            this.LateUpdateDisplayList();
            this._reservedLateUpdateDisplayList = false;
        }

        private void PreCompose()
        {
            this._composed = true;
            this.Compose();
            this.UpdateDisplayList();
        }

        private void StartComponent(EventData data)
        {
            this.Validate();
        }

        protected virtual void Init()
        {

        }

        protected virtual void Enable()
        {
        }

        protected virtual void ValidateData()
        {
        }

        protected virtual void Compose()
        {
        }

        protected virtual void UpdateDisplayList()
        {
        }

        protected virtual void LateUpdateDisplayList()
        {
        }

        protected virtual void Disable()
        {

        }

        protected virtual void Destroy()
        {
        }

        public virtual void Validate()
        {
            if (this._started)
            {
                this.ValidateData();

                if (this._composed)
                {
                    this.UpdateDisplayList();
                }
                else
                {
                    this.PreCompose();
                }

                if (!this._reservedLateUpdateDisplayList && this.gameObject.activeInHierarchy)
                {
                    this._reservedLateUpdateDisplayList = true;
                    this.StartCoroutine(this.CoroutineLateUpdateDisplayList());
                }
            }

            //Validate Initialize Step
            //1. ValidateData
            //2. Compose
            //3. UpdateDisplayList
            //4. ------- Frame Skip --------
            //5. LateUpdateDisplayList


            //Validate OnEnable Step
            //1. ValidateData
            //2. UpdateDisplayList
            //3. -------- Frame Skip --------
            //4. LateUpdateDisplayList
        }

        public virtual void ValidateAsync(int frameSkip = 0)
        {
            if (this._started)
            {
                if (!this._reservedValidate)
                {
                    this._reservedValidate = true;
                    this.StartCoroutine(this.CoroutineValidate(frameSkip));
                }
            }
        }
    }
}