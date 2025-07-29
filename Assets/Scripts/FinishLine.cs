using UnityEngine;

public class FinishLine : MonoBehaviour
{
    [SerializeField] private GameObject finishLineGo;
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        Time.timeScale = 0;
        finishLineGo.SetActive(true);
    }
}
