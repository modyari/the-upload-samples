/*
HackingGameSystem is launched with the proper arguments by GameController. 
It loads the correct room from addressables, which contains all the hack levels for the room. 
It then passes the room to HackFlowSubsystem to control the flow and logic of these levels.
It uses DialogueSubystem to display dialogue when a level in the room requests it.
*/
using System;
using System.Collections;
using TheUpload.Core;
using TheUpload.Core.Async;
using TheUpload.Game.Dialogue;
using TheUpload.Game.Dialogue.Audio;
using TheUpload.Game.Hacking.Subsystems;
using UnityEngine;
using UnityEngine.InputSystem;
using Subsystem = TheUpload.Core.Subsystem;

namespace TheUpload.Game.Hacking
{
    // the hacking game system
    public class HackingGameSystem : GameSystem<HackingGameSystemArgs>
    {
        [SerializeField]
        private HackFlowSubsystem _hackFlowSubsystem;
        [SerializeField]
        private DialogueSubsystem _dialogueSubsystem;
        [SerializeField]
        private HackWindow _hackWindow;
        [SerializeField]
        private AudioClip _glitchAudioClip;
        [SerializeField]
        private Canvas _canvasUI;

        private HackRoom _currentRoom;

        public override void Initialize(IGameSystemArgs args)
        {
            base.Initialize(args);
            GameSystemType = GameSystemType.Hacking;
            this.StartGameCoroutine(SetCanvasCameraAsync());
            var groupAssetLoader = GroupAssetLoader.Init(this.gameObject);
            groupAssetLoader.Add(Arguments.RoomName, (rO) =>
            {
                _currentRoom = rO.GetComponent<HackRoom>();
                if (!string.IsNullOrEmpty(_currentRoom.DialogueAudioName))
                {
                    groupAssetLoader.Load<DialogueRoomAudio>(_currentRoom.DialogueAudioName, (r1) =>
                    {
                        Arguments.Services.Sound.SetDialogueAudio(r1);
                    });
                }
            });
            groupAssetLoader.LoadAssets().OnCompleted += finishedLoading;
            _hackWindow.Initialize(Arguments);
            void finishedLoading()
            {
                OnLoadingFinished?.Invoke();
            }
        }

        private IEnumerator SetCanvasCameraAsync()
        {
            _canvasUI.worldCamera = Arguments.Services.Camera.CameraGlitchController.Camera;
            yield return new WaitUntil(() => !Arguments.Services.Camera.CameraGlitchController.gameObject.activeInHierarchy);
            _canvasUI.worldCamera = Arguments.Services.Camera.Camera;

        }

        public override void Run()
        {
            base.Run();

            var fArgs = new HackFlowSubsystemArgs()
            {
                Services = Arguments.Services,
                Room = _currentRoom,
                HackWindow = _hackWindow
            };
            _hackFlowSubsystem.Initialize(fArgs);
            _hackFlowSubsystem.OnNewLevelSpawned += NewLevelHandler;
            _hackFlowSubsystem.OnLevelsFinished += LevelsFinishedHandler;
            AddSubsystem(_hackFlowSubsystem);
            _hackFlowSubsystem.Run();
            var dArgs = new DialogueSubsystemArgs()
            {
                Services = Arguments.Services,
                Room = _currentRoom,
            };
            _dialogueSubsystem.Initialize(dArgs);
        }

        private void NewLevelHandler(HackLevel level)
        {
            if (level.PlayNodeOnRun)
            {
                RunDialogueNode(level.NodeToPlayOnRun);
            }
        }

// for debug
#if UNITY_EDITOR
        private void Update()
        {
            if (Keyboard.current.aKey.wasPressedThisFrame)
            {
                LevelsFinishedHandler();
            }
        }
#endif
        private void LevelsFinishedHandler()
        {
            var result = new GameSystemResult(){GameSystemType = GameSystemType.Story, RoomName = _currentRoom.NextRoomName, ShowOverlay = true};
            CurrentSubsystem.OnSubsystemChange?.Invoke(Subsystem.SubsystemType.NONE);
            if (!string.IsNullOrEmpty(_currentRoom.KeyToAddWhenFinished))
            {
                Arguments.Services.DataService.AddKey(_currentRoom.KeyToAddWhenFinished);
            }
            this.StartGameCoroutine(finishSystemAsync());

            IEnumerator finishSystemAsync()
            {
                if (!string.IsNullOrEmpty(_currentRoom.NodeToPlayOnFinish))
                {
                    RunDialogueNode(_currentRoom.NodeToPlayOnFinish);
                    yield return new WaitForSeconds(2);
                }
                //TODO: refactor this to appropriate feature
                Arguments.Services.Sound.StopMusic();
                Arguments.Services.Camera.CameraGlitchController.SetProperties(true, 3, 4);
                Arguments.Services.Camera.SetGlitchActive(true);
                Arguments.Services.Sound.PlayOneShot(_glitchAudioClip, 9f);
                yield return new WaitForSeconds(1);
                Finish(result);
                Arguments.Services.Camera.SetGlitchActive(false);
                Arguments.Services.Sound.StopSoundEffects();
            }
        }

        private void RunDialogueNode(string nodeName)
        {
            AddSubsystem(_dialogueSubsystem);
            _dialogueSubsystem.PlayNode(nodeName);
        }
    }
}
