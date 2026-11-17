using UnityEngine;

public class Cow : MonoBehaviour
{
    // os estados possíveis da vaca
    public enum State { Idle, Abducting, Saved, Taken }

    // estado atual (começa parada de boa)
    public State Current = State.Idle;

    // o ovni chama isso quando começa a puxar a vaca
    public void StartAbduction()
    {
        Current = State.Abducting;
    }

    // o ovni chama isso quando a vaca já era
    public void MarkTaken()
    {
        Current = State.Taken;
    }

    // chamado quando o jogador clica na vaca
    public void TrySave()
    {
        // só salva se o ovni já tiver pegado ela
        if (Current == State.Abducting)
        {
            Current = State.Saved;
        }
    }

    // teletransporta a vaca pro lugar do estábulo
    public void GoToSpot(Transform spot)
    {
        transform.position = spot.position;
        transform.rotation = spot.rotation; // vira pro mesmo lado do ponto
    }
}