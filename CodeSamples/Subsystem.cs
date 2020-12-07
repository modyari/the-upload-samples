
/*
   A subsystem is a secondary system under GameSystem. Can represent the state of the game during a GameSystem (e.g. gampeplay, pause, dialogue, etc)
   A GameSystem has a stack of subsystems and updates only the top one. In a way it's a FSM containing its own logic implemented in a stack.
*/
using System;
using UnityEngine;

namespace TheUpload.Core
{
    // base subsystem without arguments
    public abstract class Subsystem : MonoBehaviour
    {
        public enum SubsystemType
        {
            NONE,
            Gameplay,
            Pause,
            Cutscene,
        }

        protected ISubsystemArgs _arguments;

        public Action<SubsystemType> OnSubsystemChange;

        public virtual void Initialize(ISubsystemArgs args)
        {
            _arguments = args;
            OnSubsystemChange += SubsystemChangeHandler;
        }

        public virtual void Run()
        {
        }

        public virtual void Tick()
        {
        }

        public virtual void Terminate()
        {
            OnSubsystemChange -= SubsystemChangeHandler;
        }

        protected virtual void PauseHandler()
        {
        }

        protected virtual void SubsystemChangeHandler(Subsystem.SubsystemType type)
        {
            if (type == Subsystem.SubsystemType.Pause)
            {
                PauseHandler();
            }
        }
    }

    // base subsystem with arguments
    public abstract class Subsystem<T> : Subsystem
    where T : class, ISubsystemArgs
    {
        public T Arguments => (T) _arguments;
    }
}
