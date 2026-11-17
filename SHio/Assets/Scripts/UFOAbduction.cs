using System.Collections;
using System.Collections.Generic;
//using TMPro;
using UnityEngine;

public class UFOAbduction : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Cow[] cows;                   // lista com todas as vacas
    [SerializeField] private ParticleSystem beamParticles; // particulas 
    [SerializeField] private ParticleSystem exitBurst;     // explosãozinha qnd o ovni for
    // [SerializeField] private TMP_Text counterText;      // texto do contador
    [SerializeField] private GameObject kingCow;           // a vaca rei gigante HAHAHAAH
    // [SerializeField] private GameObject giantAlien;

    [Header("Estábulo")]
    [SerializeField] private Transform[] barnSpots; // os 3 lugares fixos dentro do estábulo

    [Header("Som")]
    [SerializeField] private AudioClip mooSound;                    // o muuu
    [SerializeField, Range(0f, 1f)] private float mooVolume = 1f;   // volume do muuu

    [Header("Regras")]
    [SerializeField] private int cowsToSave = 3;           // quantas vacas salvar pra ganhar

    [Header("Movimento do OVNI")]
    [SerializeField] private float hoverHeight = 8f;       // altura do ovni em cima da vaca
    [SerializeField] private float flySpeed = 12f;         // velocidade do ovni voando

    [Header("Abdução")]
    [SerializeField] private float abductDuration = 4f;    // tempo que o jogador tem pra salvar
    [SerializeField] private float riseExtra = 1f;         // quão perto da nave a vaca chega
    [SerializeField] private float spinSpeed = 120f;       // vaca girando enquanto sobe 
    [SerializeField] private float rescueDuration = 1f;    // tempo da vaca descendo quando salva
    [SerializeField] private float pauseBetweenCows = 1.5f; // respiro entre uma vaca e outra

    // [Header("Final")]
    // [SerializeField] private float alienGrowTime = 3f;

    [Header("Saída")]
    [SerializeField] private float exitDuration = 2f;      // tempo da fuga final
    [SerializeField] private float exitHeight = 80f;       // quão alto o ovni sobe ao fugir

    private int saved;  // vacas salvas
    private int lost;   // vacas perdidas
    [SerializeField] private float EsperandoOvni =20f;

    private void Start()
    {
        // vaca rei começa escondida
        if (kingCow != null)
        {
            kingCow.SetActive(false);
        }

        // if (giantAlien != null)
        // {
        //     giantAlien.SetActive(false);
        // }

        // UpdateCounter();
        StartCoroutine(GameLoop()); // cmç a resenha
    }

    // o loop principal do jogo
    private IEnumerator GameLoop()
    {
        // espera um pouquinho antes de começar
        yield return new WaitForSeconds(EsperandoOvni);

        // repete até salvar vacas suficientes
        while (saved < cowsToSave)
        {
            Cow next = PickNextCow();

            // vendo se sobrou algo pro beta
            if (next == null)
            {
                break;
            }

            yield return FlyAbove(next.transform);  // voa até a vaca
            yield return Abduct(next);              // tenta abduzir
            yield return new WaitForSeconds(pauseBetweenCows); // respira
        }

        // ganhou? vaca gigante gira
        if (saved >= cowsToSave)
        {
            // if (giantAlien != null)
            // {
            //     giantAlien.SetActive(true);
            //     yield return new WaitForSeconds(alienGrowTime);
            //     giantAlien.SetActive(false);
            // }

            if (kingCow != null)
            {
                kingCow.SetActive(true);
                yield return new WaitForSeconds(1f);
            }
        }

        // ovni vaza
        yield return Leave();
    }

    // escolhe uma vaca aleatória que ainda tá de parada
    private Cow PickNextCow()
    {
        List<Cow> available = new List<Cow>();

        // separa só as vacas paradas (ignora as salvas e as levadas)
        foreach (Cow c in cows)
        {
            if (c != null && c.Current == Cow.State.Idle)
            {
                available.Add(c);
            }
        }

        // ninguém disponível
        if (available.Count == 0)
        {
            return null;
        }

        // sorteia uma
        return available[Random.Range(0, available.Count)];
    }

    // ovni voa até ficar em cima da vaca
    private IEnumerator FlyAbove(Transform cow)
    {
        Vector3 target = cow.position + Vector3.up * hoverHeight;

        // anda até chegar bem perto do ponto
        while (Vector3.Distance(transform.position, target) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, target, flySpeed * Time.deltaTime);
            yield return null;
        }
    }

    // a parte principal: puxa a vaca e espera o jogador salvar
    private IEnumerator Abduct(Cow cow)
    {
        Transform t = cow.transform;

        // pega componentes que podem atrapalhar o movimento (se a vaca tiver)
        cow.TryGetComponent(out Animator anim);
        cow.TryGetComponent(out UnityEngine.AI.NavMeshAgent agent);
        cow.TryGetComponent(out Rigidbody rb);
        bool wasKinematic = rb != null && rb.isKinematic; // guarda como tava pra restaurar depois

        // desliga eles enquanto a vaca flutua
        if (anim) anim.enabled = false;
        if (agent) agent.enabled = false;
        if (rb) rb.isKinematic = true;

        // guarda como a vaca tava no chão (pra devolver se ela for salva)
        Vector3 startPos = t.position;
        Quaternion startRot = t.rotation;
        Vector3 startScale = t.localScale;

        // ponto onde a vaca vai "entrar" na nave
        Vector3 endPos = transform.position - Vector3.up * riseExtra;

        beamParticles.Play();   // liga o raio
        cow.StartAbduction();   // agora dá pra salvar ela

        float p = 0f; // progresso da abdução (0 = chão, 1 = dentro da nave)
        while (p < 1f)
        {
            // o jogador clicou a tempo!
            if (cow.Current == Cow.State.Saved)
            {
                // toca o muuu na posição da vaca (som 3d)
                if (mooSound != null)
                {
                    AudioSource.PlayClipAtPoint(mooSound, t.position, mooVolume);
                }

                // vaca desce de volta pro chão
                yield return Rescue(t, startPos, startRot, startScale);

                // devolve o que tinha desligado
                if (anim) anim.enabled = true;
                if (agent) agent.enabled = true;
                if (rb) rb.isKinematic = wasKinematic;

                // manda a vaca pro lugar fixo do estábulo (se ainda tiver lugar sobrando)
                if (saved < barnSpots.Length)
                {
                    cow.GoToSpot(barnSpots[saved]);
                }

                saved++;
                // UpdateCounter();
                yield break; // sai da função, tá salva
            }

            // avança o progresso conforme o tempo passa
            p += Time.deltaTime / abductDuration;

            // suaviza o movimento (começa e termina devagarinho)
            float eased = Mathf.SmoothStep(0f, 1f, p);

            // vaca sobe pra nave
            t.position = Vector3.Lerp(startPos, endPos, eased);

            // e gira de brinde
            t.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);

            // só nos últimos 30% ela encolhe, pra parecer que entrou na nave
            float shrink = Mathf.InverseLerp(0.7f, 1f, p);
            t.localScale = Vector3.Lerp(startScale, Vector3.zero, shrink);

            yield return null;
        }

        // chegou aqui = o jogador não salvou a tempo, vaca perdida
        cow.MarkTaken();
        cow.gameObject.SetActive(false); // some com ela
        beamParticles.Stop();
        lost++;
        // UpdateCounter();
    }

    // vaca desce de volta pro lugar original
    private IEnumerator Rescue(Transform t, Vector3 groundPos, Quaternion groundRot, Vector3 groundScale)
    {
        beamParticles.Stop(); // desliga o raio

        // guarda de onde ela tá saindo (no ar)
        Vector3 fromPos = t.position;
        Quaternion fromRot = t.rotation;
        Vector3 fromScale = t.localScale;

        float k = 0f;
        while (k < 1f)
        {
            k += Time.deltaTime / rescueDuration;
            float eased = Mathf.SmoothStep(0f, 1f, k);

            // volta suave pra posição, rotação e tamanho de antes
            t.position = Vector3.Lerp(fromPos, groundPos, eased);
            t.rotation = Quaternion.Slerp(fromRot, groundRot, eased);
            t.localScale = Vector3.Lerp(fromScale, groundScale, eased);

            yield return null;
        }
    }

    // // atualiza o texto do contador
    // private void UpdateCounter()
    // {
    //     if (counterText != null)
    //     {
    //         counterText.text = $"Vacas salvas: {saved}/{cowsToSave}\nVacas perdidas: {lost}";
    //     }
    // }

    // ovni vai embora com estilo
    private IEnumerator Leave()
    {
        yield return new WaitForSeconds(0.5f);

        // solta a explosão de partículas
        if (exitBurst != null)
        {
            exitBurst.Play();
        }

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.up * exitHeight; // pra cima, bem longe
        Vector3 startScale = transform.localScale;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / exitDuration;

            // t ao quadrado = começa devagar e vai acelerando, estilo foguete
            float accel = t * t;

            transform.position = Vector3.Lerp(startPos, endPos, accel);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, accel); // vai encolhendo
            transform.Rotate(Vector3.up, 720f * accel * Time.deltaTime, Space.World); // gira cada vez mais rápido

            yield return null;
        }

        // some de vez
        gameObject.SetActive(false);
    }
}