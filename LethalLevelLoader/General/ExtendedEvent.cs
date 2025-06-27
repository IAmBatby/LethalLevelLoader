using System;
using System.Collections.Generic;
using System.Text;

namespace LethalLevelLoader
{
    public class ExtendedEvent
    {
        protected event Action onEvent;
        public bool HasListeners => (Listeners != 0);

        public virtual int Listeners => listeners.Count;
        private List<Action> listeners = new List<Action>();

        public void Invoke() { onEvent?.Invoke(); }

        public void AddListener(Action listener)
        {
            onEvent += listener;
            listeners.Add(listener);
        }
        public void RemoveListener(Action listener)
        {
            onEvent -= listener;
            listeners.Remove(listener);
        }

        public virtual void ClearListeners()
        {
            foreach (Action listener in listeners)
                onEvent -= listener;
            listeners.Clear();
        }
    }

    public delegate void ParameterEvent<T>(T param);
    public class ExtendedEvent<T> : ExtendedEvent
    {
        public override int Listeners => base.Listeners + paramListeners.Count;
        private event ParameterEvent<T> onParameterEvent;
        private List<ParameterEvent<T>> paramListeners = new List<ParameterEvent<T>>();

        public void Invoke(T param)
        {
            onParameterEvent?.Invoke(param);
            Invoke();
        }

        public void AddListener(ParameterEvent<T> listener)
        {
            onParameterEvent += listener;
            paramListeners.Add(listener);
        }
        public void RemoveListener(ParameterEvent<T> listener)
        {
            onParameterEvent -= listener;
            paramListeners.Remove(listener);
        }

        public override void ClearListeners()
        {
            base.ClearListeners();
            foreach (ParameterEvent<T> listener in paramListeners)
                onParameterEvent -= listener;
            paramListeners.Clear();
        }
    }

    public enum StepType { Before, On, After }
    public class ExtendedStepEvent
    {
        private ExtendedEvent Before = new ExtendedEvent();
        private ExtendedEvent On = new ExtendedEvent();
        private ExtendedEvent After = new ExtendedEvent();

        public virtual void Invoke()
        {
            Before.Invoke();
            On.Invoke();
            After.Invoke();
        }

        public void AddListener(Action action, StepType stepType)
        {
            if (stepType == StepType.Before) Before.AddListener(action);
            if (stepType == StepType.On) On.AddListener(action);
            if (stepType == StepType.After) After.AddListener(action);
        }

        public void RemoveListener(Action action, StepType stepType)
        {
            if (stepType == StepType.Before) Before.RemoveListener(action);
            if (stepType == StepType.On) On.RemoveListener(action);
            if (stepType == StepType.After) After.RemoveListener(action);
        }

        public virtual void ClearListeners()
        {
            Before.ClearListeners();
            On.ClearListeners();
            After.ClearListeners();
        }
    }

    public class ExtendedStepEvent<T> : ExtendedStepEvent
    {
        private ExtendedEvent<T> ParamBefore = new ExtendedEvent<T>();
        private ExtendedEvent<T> ParamOn = new ExtendedEvent<T>();
        private ExtendedEvent<T> ParamAfter = new ExtendedEvent<T>();

        public void Invoke(T param)
        {
            ParamBefore.Invoke(param);
            ParamOn.Invoke(param);
            ParamAfter.Invoke(param);
            base.Invoke();
        }

        public void AddListener(ParameterEvent<T> action, StepType stepType)
        {
            if (stepType == StepType.Before) ParamBefore.AddListener(action);
            if (stepType == StepType.On) ParamOn.AddListener(action);
            if (stepType == StepType.After) ParamAfter.AddListener(action);
        }

        public void RemoveListener(ParameterEvent<T> action, StepType stepType)
        {
            if (stepType == StepType.Before) ParamBefore.RemoveListener(action);
            if (stepType == StepType.On) ParamOn.RemoveListener(action);
            if (stepType == StepType.After) ParamAfter.RemoveListener(action);
        }

        public override void ClearListeners()
        {
            ParamBefore.ClearListeners();
            ParamOn.ClearListeners();
            ParamAfter.ClearListeners();
            base.ClearListeners();
        }
    }
}
