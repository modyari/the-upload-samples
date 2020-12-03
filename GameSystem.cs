using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheUpload.Core
{
    // GameSystem base class without arguments 
    public abstract class GameSystem : MonoBehaviour
    {
        private List<GameFeature> _gameFeatures = new List<GameFeature>();
        protected IGameSystemArgs _arguments;
        // subsystem stack, only the top one gets updated 
        protected Stack<Subsystem> _subsystems;
        public GameSystemType GameSystemType { get; set; }
        
        // when the requested room is done loading
        public Action OnLoadingFinished;
        // when the game system should be exited
        public Action<GameSystemResult> OnFinished;

        public Subsystem CurrentSubsystem
        {
            get
            {
                Subsystem s = null;
                if (_subsystems?.Count > 0)
                {
                    s = _subsystems?.Peek();
                }

                return s;
            }
        }

        public virtual void Initialize(IGameSystemArgs args)
        {
            _subsystems = new Stack<Subsystem>();
            _arguments = args;
        }

        public virtual void Run()
        {
        }

        public virtual void Tick()
        {
            if (CurrentSubsystem != null)
            {
                CurrentSubsystem.Tick();
            }
            UpdateFeatures();
        }

        public virtual void Terminate()
        {
            // Terminate and remove all subsystems 
            while (CurrentSubsystem != null)
            {
                CurrentSubsystem.Terminate();
                _subsystems.Pop();
            }
        }

        public virtual void Finish(GameSystemResult result)
        {
            // exit the gamesystem 
            OnFinished?.Invoke(result);
            Terminate();
        }

        protected virtual void AddFeature(GameFeature feature)
        {
            if(!_gameFeatures.Contains(feature))
            {
                feature.Initialize(_arguments);
                feature.OnFinished += RemoveFeature;
                _gameFeatures.Add(feature);
            }
        }

        protected virtual void RemoveFeature(GameFeature feature)
        {
            if (_gameFeatures.Contains(feature))
            {
                feature.OnFinished -= RemoveFeature;
                feature.Terminate();
                _gameFeatures.Remove(feature);
            }
        }

        protected virtual void AddSubsystem(Subsystem subsystem)
        {
            subsystem.OnSubsystemChange += SubsystemChangeHandler;
            _subsystems.Push(subsystem);
        }

        protected virtual void SubsystemChangeHandler(Subsystem.SubsystemType type)
        {
            if (type == Subsystem.SubsystemType.NONE)
            {
                if (CurrentSubsystem != null)
                {
                    CurrentSubsystem.OnSubsystemChange -= SubsystemChangeHandler;

                    _subsystems.Pop();
                }
            }
        }

        private void UpdateFeatures()
        {
            foreach(var feature in _gameFeatures)
            {
                if (feature.WaitForRun && !feature.IsRunning)
                {
                    continue;
                }
                feature.Tick();
            }
        }

    }

    // gamesystem base class with arguments
    public abstract class GameSystem<T> : GameSystem
    where T : class, IGameSystemArgs
    {
        public T Arguments => (T)_arguments;

        public override void Initialize(IGameSystemArgs args)
        {
            base.Initialize(args);
            Initialize();
        }

        protected virtual void Initialize()
        {

        }
    }
}
