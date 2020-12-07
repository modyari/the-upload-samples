/*
The Game Controller, sitting at the launch scene. Gathers arguments and loads the right scene and initializes the GameSystem within with the right arguments.
Placeholder / was supposed to go through further refactoring 
*/

using System;
using System.Collections;
using System.Linq;
using TheUpload.Core.Async;
using TheUpload.Core.Data;
using UnityEngine;
using UnityEngine.SceneManagement;
using TheUpload.Game;
using TheUpload.Game.Cutscene;
using TheUpload.Game.Hacking;
using TheUpload.Game.Story;

namespace TheUpload.Core
{
    public class GameController : MonoBehaviour
    {
        [Header("GameSystem")]
        [SerializeField]
        private GameSystemType _gameSystemType;

        [Header("Properties")]
        [SerializeField]
        private CameraController _camera;
        [SerializeField]
        private InputSystem _input;
        [SerializeField]
        private SoundSystem _sound;
        [SerializeField]
        private GameConfig _gameConfig;
        [SerializeField]
        private string _roomName;

        private GameSystem _gameSystem;
        private Services _services;

        private void Awake()
        {
            Initialize();
            StartCoroutine(LoadGameSystemAsync(_roomName, _gameSystemType));
        }

        private void Initialize()
        {
            var dataService = new DataService();
            _services = new Services()
            {
                GameConfig = _gameConfig,
                Input = _input,
                Camera = _camera,
                DataService = dataService,
                Sound = _sound
            };
        }

        private void Update()
        {
            if (_gameSystem != null)
            {
                _gameSystem.Tick();
            }
        }

        private IEnumerator LoadGameSystemAsync(string roomName, GameSystemType gameSystemType, bool showOverlay = true, bool isLongFade = false)
        {
            var t = 0.0f;
            GameSystem prevGameSystem = _gameSystem;
            if (showOverlay)
            {
                yield return _camera.ShowLoadOverlay(isLongFade: isLongFade);
            }
            var sceneName = _gameConfig.GetSceneName(gameSystemType);
            if (prevGameSystem != null)
            {
                yield return UnloadGameSystem(prevGameSystem);
            }
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
            _gameSystem = GameObject.FindObjectOfType<GameSystem>();
            if (_gameSystem != null)
            {
                _gameSystem.OnLoadingFinished += loadingFinishedHandler;
                _gameSystem.OnFinished += GameSystemFinishHandler;
                switch (gameSystemType)
                {
                    case GameSystemType.Story:
                        var args = new StoryGameSystemArgs()
                        {
                            Services = _services,
                            RoomName = roomName
                        };
                        _gameSystem.Initialize(args);
                        break;
                    case GameSystemType.Hacking:
                        var hArgs = new HackingGameSystemArgs()
                        {
                            Services = _services,
                            RoomName = roomName
                        };
                        _gameSystem.Initialize(hArgs);
                        break;
                    case GameSystemType.Cutscene:
                        var cArgs = new CutsceneGameSystemArgs()
                        {
                            Services = _services,
                            RoomName = roomName
                        };
                        _gameSystem.Initialize(cArgs);
                        break;
                }
            }

            void loadingFinishedHandler()
            {
                _gameSystem.OnLoadingFinished -= loadingFinishedHandler;
                StartCoroutine(hideLoadingThenRun());
                IEnumerator hideLoadingThenRun()
                {
                    if (showOverlay)
                    {
                        yield return _camera.HideLoadOverlay(isLongFade: isLongFade);
                    }
                    _gameSystem.Run();
                }
            }
        }

        private void GameSystemFinishHandler(GameSystemResult result)
        {
            StartCoroutine(LoadGameSystemAsync(result.RoomName, result.GameSystemType, result.ShowOverlay, result.IsLongFadeDuration));
        }

        private IEnumerator UnloadGameSystem(GameSystem gameSystem)
        {
            var sceneName = _gameConfig.GetSceneName(gameSystem.GameSystemType);
            yield return SceneManager.UnloadSceneAsync(sceneName, UnloadSceneOptions.None);
        }
    }
}
