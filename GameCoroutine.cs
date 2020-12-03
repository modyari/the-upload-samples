using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheUpload.Core.Async
{   
    /*
    A custom coroutine processor that allows pausing, 
    simply extends MonoBehaviour so you can use it by calling this.StartGameCoroutine(..) from any MonoBehaviour
    when the game is paused, ShouldPauseAll is set to false and processing is paused
    */
    public class GameCoroutine : CustomYieldInstruction
    {
        private IEnumerator _yieldInstruction;
        private static bool _shouldPauseAll;

        public static bool ShouldPauseAll
        {
            get => _shouldPauseAll;
            set
            {
                if (_shouldPauseAll != value)
                {
                    OnPauseStateUpdate?.Invoke(value);
                }
                _shouldPauseAll = value;
            }
        }

        public static event Action<bool> OnPauseStateUpdate;
        public override bool keepWaiting => ShouldPauseAll;
        public Coroutine MainCoroutine { get; }

        public GameCoroutine(MonoBehaviour mono, IEnumerator yield)
        {
            _yieldInstruction = yield;
            MainCoroutine = mono.StartCoroutine(Coroutine(_yieldInstruction));
        }

        private IEnumerator Coroutine(IEnumerator yield)
        {
            while (true)
            {
                while (keepWaiting)
                {
                    yield return null;
                }

                var moveNext = yield.MoveNext();
                if (moveNext)
                {
                    if (yield.Current == null)
                    {
                        yield return null;
                    }
                    else
                    {
                        if (yield.Current is YieldInstruction || yield.Current is CustomYieldInstruction)

                        {
                            yield return yield.Current;
                            continue;
                        }
                        yield return Coroutine(yield.Current as IEnumerator);
                    }
                }
                else
                {
                    yield break;
                }
            }
        }
    }

    public static class GameCoroutineExtension
    {
        public static Coroutine StartGameCoroutine(this MonoBehaviour mono, IEnumerator yield)
        {
            return (new GameCoroutine(mono, yield)).MainCoroutine;
        }
    }
}
