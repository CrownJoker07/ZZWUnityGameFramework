using System;
using UnityEngine;

public class Test : MonoBehaviour
{
    private void Awake()
    {
        MonoSingletonTestDontDestroyOnLoad monoSingletonTestDontDestroyOnLoad =
            MonoSingletonTestDontDestroyOnLoad.instance;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
