/*
 The subsystem responsible for handling the hacking flow when there's no pausing or dialogue
 A hack room consists of several levels, when a level ends successfully, the next level is loaded, which may or may not play a piece of dialogue
 When all levels are done, the GameSystem (HackGameSystem) is informed and finishes. The game then loads the next appropriate GameSystem. 
*/
using System;
using System.Collections;
using System.Collections.Generic;
using TheUpload.Core;
using TheUpload.Game.Hacking.Subsystems;
using UnityEngine;
using UnityEngine.InputSystem;
using InputSystem = TheUpload.Core.InputSystem;

namespace TheUpload.Game.Hacking.Subsystems
{
	public class HackFlowSubsystem : TheUpload.Core.Subsystem<HackFlowSubsystemArgs>
    {
        private int _levelIndex;
        private HackLevel _currentLevel;

        private InputSystem Input => Arguments.Services.Input;
        private HackWindow HackWindow => Arguments.HackWindow;
        private HackRoom Room => Arguments.Room;
        private List<HackLevel> Levels => Room.Levels;
        private bool IsLastLevel => _levelIndex == Levels.Count - 1;

        public event Action<HackLevel> OnNewLevelSpawned;
        public event Action OnLevelsFinished;
        
        public override void Run()
        {
            base.Run();
            SpawnLevel();   
        }

        public override void Tick()
        {
            base.Tick();
            if (_currentLevel != null)
            {
                _currentLevel.Tick();
            }
	    //debug purposes
#if UNITY_EDITOR
            if (Keyboard.current.aKey.wasPressedThisFrame)
            {
                _levelIndex = 4;
                SpawnLevel();
            }
#endif
        }

        private void SpawnLevel(bool isReset = false)
        {
            if (_currentLevel != null)
            {
                _currentLevel.OnGoalReached -= NextLevel;
                _currentLevel.OnReset -= resetLevel;
                Destroy(_currentLevel.gameObject);
            }
            HackWindow.Expand(isReset, () =>
            {
                _currentLevel = Instantiate(Levels[_levelIndex]);
                _currentLevel.gameObject.SetActive(true);
                _currentLevel.Initialize(Arguments);
                _currentLevel.OnGoalReached += NextLevel;
                _currentLevel.OnReset += resetLevel;
                OnNewLevelSpawned?.Invoke(_currentLevel);
                Levels[_levelIndex].PlayNodeOnRun = false;
            });

            void resetLevel()
            {
                SpawnLevel(true);
            }
        }

        private void NextLevel()
        {
            if (!IsLastLevel)
            {
                _currentLevel.OnGoalReached -= NextLevel;
                _levelIndex++;
                SpawnLevel();
            }
            else
            {
                _currentLevel.Stop();
                OnLevelsFinished?.Invoke();
            }

        }

        private void DestroyLevel()
        {
            if (_currentLevel != null)
            {
                Destroy(_currentLevel.gameObject);
            }
        }
    }
}
