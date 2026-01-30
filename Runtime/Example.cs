using System;
using UnityEngine;
using UnityEngine.Events;

public class Example : MonoBehaviour, ITest
{
    [SerializeReference, TypeFilter(typeof(ITest))]
    public ITest TestsInt;

    [SerializeReference, TypeFilter(typeof(ITest))]
    public object[] TestsObj;
}

[Serializable]
public class FirstTest : ITest
{
    public string Name;
    public Vector3 Position;
    public ScriptableObject Object;
}

public class SecondTest : ITest
{
    public Color Color;
    public UnityEvent Event;
}

public class FailedTest : MonoBehaviour, ITest
{
}

public interface ITest
{
}