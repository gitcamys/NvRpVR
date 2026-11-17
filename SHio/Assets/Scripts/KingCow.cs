using UnityEngine;

public class KingCow : MonoBehaviour
{
    [SerializeField] private Vector3 finalScale = new Vector3(5f, 5f, 5f); 
    [SerializeField] private float growTime = 2f;                         
    [SerializeField] private float spinSpeed = 90f;                       

    private float timer;

    // roda toda vez que a vaca rei é ativada
    private void OnEnable()
    {
        timer = 0f;
        transform.localScale = Vector3.zero; // cmc invisível de tão pequena
    }

    private void Update()
    {
        timer += Time.deltaTime;

        // vai de 0 a 1 suave conforme o tempo passa
        float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(timer / growTime));

        // cresce até o tamanho final
        transform.localScale = finalScale * k;

        // gira sem parar, tipo um dj
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
    }
}