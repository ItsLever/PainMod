using BepInEx.Unity.IL2CPP;
using UnityEngine;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using System.Collections;
namespace PainMod;

public static class CoroutineHelper
{
    private static readonly CoroutineHelperBehaviour _behaviour;

    static CoroutineHelper()
    {
        _behaviour = IL2CPPChainloader.AddUnityComponent<CoroutineHelperBehaviour>();
    }

    public static void StartCoroutine(IEnumerator coroutineMethod)
    {
        _behaviour.StartCoroutine(coroutineMethod);
    }

    private static Coroutine StartCoroutine(this MonoBehaviour self, IEnumerator coroutine) =>
        self.StartCoroutine(coroutine.WrapToIl2Cpp());

    private class CoroutineHelperBehaviour : MonoBehaviour {}
}