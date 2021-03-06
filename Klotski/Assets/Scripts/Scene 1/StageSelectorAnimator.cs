﻿using Common;
using UnityEngine;

namespace Scene_1
{
    public class StageSelectorAnimator : MonoBehaviour
    {
        [SerializeField] private Animator[] animators;

        public void Back()
        {
            PlayHideAnimation();
            GetComponent<SceneLoader>().LoadPreviousSceneDelay((float) 0.33);
        }

        public void LoadStage(StageConfig stageConfig)
        {
            Store.NextStageConfig = stageConfig;
            PlayHideAnimation();
            GetComponent<SceneLoader>().LoadNextSceneDelay((float) 0.33);
        }

        private void PlayHideAnimation()
        {
            foreach (var animator in animators)
            {
                animator.Play(animator.gameObject.name + " Hide");
            }
        }
    }
}