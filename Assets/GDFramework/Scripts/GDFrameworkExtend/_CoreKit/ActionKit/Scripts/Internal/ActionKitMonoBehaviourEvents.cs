/****************************************************************************
 * Copyright (c) 2016 - 2022 liangxiegame UNDER MIT License
 *
 * https://qframework.cn
 * https://github.com/liangxiegame/QFramework
 * https://gitee.com/liangxiegame/QFramework
 ****************************************************************************/

using System.Collections;
using GDFramework_Core.Scripts.GDFrameworkCore;
using QFramework;
using UnityEngine;

namespace GDFrameworkExtend._CoreKit.ActionKit
{
    [MonoSingletonPath("QFramework/ActionKit/GlobalMonoBehaviourEvents")]
    internal class ActionKitMonoBehaviourEvents : MonoSingleton<ActionKitMonoBehaviourEvents>
    {
        internal readonly EasyEvent OnUpdate = new();
        internal readonly EasyEvent OnFixedUpdate = new();
        internal readonly EasyEvent OnLateUpdate = new();
        internal readonly EasyEvent OnGUIEvent = new();
        internal readonly EasyEvent<bool> OnApplicationFocusEvent = new();
        internal readonly EasyEvent<bool> OnApplicationPauseEvent = new();
        internal readonly EasyEvent OnApplicationQuitEvent = new();

        private void Awake()
        {
            hideFlags = HideFlags.HideInHierarchy;
        }

        private void Update()
        {
            OnUpdate?.Trigger();
        }

        private void OnGUI()
        {
            OnGUIEvent?.Trigger();
        }

        private void FixedUpdate()
        {
            OnFixedUpdate?.Trigger();
        }

        private void LateUpdate()
        {
            OnLateUpdate?.Trigger();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            OnApplicationFocusEvent?.Trigger(hasFocus);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            OnApplicationPauseEvent?.Trigger(pauseStatus);
        }

        protected override void OnApplicationQuit()
        {
            OnApplicationQuitEvent?.Trigger();
            base.OnApplicationQuit();
        }

        public void ExecuteCoroutine(IEnumerator coroutine, System.Action onFinish)
        {
            StartCoroutine(DoExecuteCoroutine(coroutine, onFinish));
        }

        private IEnumerator DoExecuteCoroutine(IEnumerator coroutine, System.Action onFinish)
        {
            yield return coroutine;
            onFinish?.Invoke();
        }
    }
}