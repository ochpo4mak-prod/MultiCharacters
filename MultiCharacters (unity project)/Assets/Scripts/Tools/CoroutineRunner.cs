using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CoroutineRunner : IDisposable
{
    private readonly MonoBehaviour runner;
    private readonly List<IEnumerator> activeCoroutines;

    public CoroutineRunner(MonoBehaviour runner)
    {
        this.runner = runner;
        activeCoroutines = new List<IEnumerator>();
    }

    public void Start(IEnumerator coroutine)
    {
        activeCoroutines.Add(coroutine);
        runner.StartCoroutine(coroutine);
    }

    public void Stop(IEnumerator coroutine)
    {
        if (activeCoroutines.Contains(coroutine))
        {
            runner.StopCoroutine(coroutine);
            activeCoroutines.Remove(coroutine);
        }
    }

    public void StopAll()
    {
        foreach (IEnumerator coroutine in activeCoroutines)
            runner.StopCoroutine(coroutine);

        activeCoroutines.Clear();
    }

    public void Dispose() => StopAll();
}
