using UnityEngine;
using UnityEngine.InputSystem;

public class CowClicker : MonoBehaviour
{
    [SerializeField] private Camera cam;                
    [SerializeField] private float maxDistance = 200f;  // até onde o clique alcança
    [SerializeField] private float aimRadius = 0.5f;    // folga da mira do controle

    [SerializeField] private InputActionReference saveAction;

    // roda quando o script liga
    private void OnEnable()
    {
        // se esqueceu de ligar a ação, avisa e sai
        if (saveAction == null)
        {
            return;
        }

        saveAction.action.performed += OnSave; // quando apertar, chama o OnSave
        saveAction.action.Enable();            // garante que a ação tá ligada
    }

    // roda quando o script desliga
    private void OnDisable()
    {
        if (saveAction == null)
        {
            return;
        }

        saveAction.action.performed -= OnSave; // para de escutar
    }

    // chamado quando o jogador aperta qualquer botão da ação
    private void OnSave(InputAction.CallbackContext ctx)
    {

        // se esqueceu de arrastar a câmera, pega a principal
        if (cam == null)
        {
            cam = Camera.main;
        }

        // descobre qual aparelho apertou
        if (ctx.control.device is Mouse)
        {
            ShootFromMouse();
        }
        else
        {
            ShootFromCenter();
        }
    }

    // mouse: raio sai de onde o cursor tá
    private void ShootFromMouse()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePos);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            CheckCow(hit);
        }
    }

    // controle: raio sai do centro da tela (onde fica a mirinha)
    private void ShootFromCenter()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        // spherecast = raio gordinho, perdoa mira torta
        RaycastHit hit;
        if (Physics.SphereCast(ray, aimRadius, out hit, maxDistance))
        {
            CheckCow(hit);
        }
    }

    // vê se o que o raio acertou é uma vaca
    private void CheckCow(RaycastHit hit)
    {
        Debug.Log("acertou: " + hit.collider.name); // teste: mostra o que o raio bateu

        // procura o script Cow no que acertou (ou no pai)
        Cow cow = hit.collider.GetComponentInParent<Cow>();

        // achou vaca? tenta salvar
        if (cow != null)
        {
            Debug.Log("estado da vaca: " + cow.Current);
            cow.TrySave();
        }
    }
}