using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestInteraction : MonoBehaviour, IInteractable
{
    #region boolean definition
    private bool exampleBool;

    public bool ExampleBool
    {
        get { return exampleBool; }
    }
    #endregion

    public delegate void InteractionDelegate();

    public InteractionDelegate interactionDelegate;

    private void OnEnable()
    {
        interactionDelegate = new InteractionDelegate(TestMethod);
        interactionDelegate += TestMethodTwo;
    }

    private void OnDisable()
    {
        interactionDelegate -= TestMethod;
        interactionDelegate -= TestMethodTwo;
    }
    private void Start()
    {
        interactionDelegate = new InteractionDelegate(TestMethod);
        interactionDelegate += TestMethodTwo;
    }
    public void Activate()
    {
        interactionDelegate.Invoke();
    }

    private void TestMethod()
    {
        Debug.Log("First method has been executed.");
    }

    private void TestMethodTwo()
    {
        Debug.Log("Second method has been executed.");
    }
}
