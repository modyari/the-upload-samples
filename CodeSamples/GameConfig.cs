/*
  Scriptable object containing internal game configs like scene names and UI/scene animations
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheUpload.Core
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Game/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [SerializeField]
        private string _storySceneName;
        [SerializeField]
        private string _hackingSceneName;
        [SerializeField]
        private string _cutsceneSceneName;
        [SerializeField]
        private string _editorSceneName;
        [SerializeField]
        private float _loadingFadeDuration;
        [SerializeField]
        private float _loadingLongFadeDuration;
        [SerializeField]
        private AnimationCurve _loadingFadeAnimationCurve;
        [SerializeField]
        private float _uiFadeDuration;
        [SerializeField]
        private AnimationCurve _uiFadeCurve;
        [SerializeField]
        private AnimationCurve _cameraMovementCurve;

        public float LoadingFadeDuration => _loadingFadeDuration;
        public float LoadingLongFadeDuration => _loadingLongFadeDuration;
        public AnimationCurve LoadingFadeAnimationCurve => _loadingFadeAnimationCurve;
        public float UIFadeDuration => _uiFadeDuration;
        public AnimationCurve UIFadeCurve => _uiFadeCurve;
        public AnimationCurve CameraMovementCurve => _cameraMovementCurve;

        public string GetSceneName(GameSystemType type)
        {
            var name = "";
            switch (type)
            {
                case GameSystemType.Story:
                    name = _storySceneName;
                    break;
                case GameSystemType.Cutscene:
                    name = _cutsceneSceneName;
                    break;
                case GameSystemType.Editor:
                    name = _editorSceneName;
                    break;
                case GameSystemType.Hacking:
                    name = _hackingSceneName;
                    break;
            }
            return name;
        }
    }
}
