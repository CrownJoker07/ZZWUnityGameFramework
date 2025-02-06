using System;
using UnityEngine;

public class Test : MonoBehaviour
{
    private void Awake()
    {

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MonoSingletonTestDontDestroyOnLoad monoSingletonTestDontDestroyOnLoad =
            MonoSingletonTestDontDestroyOnLoad.Instance; 
        
        MonoSingletonTestAwake monoSingletonTestAwake = MonoSingletonTestAwake.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
