using UnityEngine;

public class FpsSetter : MonoBehaviour
{
    private void Start()
    {
        Application.targetFrameRate = 1000;
    }
}