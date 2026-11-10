using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Controla a fase: cenario de cada fase, tentativas, reinicio da bola, gol (vitoria) e derrota
public class GerenciadorFase : MonoBehaviour
{
    // Falso enquanto a bola esta em movimento depois de um chute ou depois que a fase acabou
    public static bool PodeChutar { get; private set; } = true;

    const int UltimaFase = 4;
    const int PremioPorFase = 50;

    [SerializeField] private Rigidbody2D bola;
    [SerializeField] private Forca forca;
    [SerializeField] private Transform posicaoInicial;

    [SerializeField] private TextMeshProUGUI textoTentativas;
    [SerializeField] private TextMeshProUGUI textoMoedas;

    // Texto grande no meio da tela ("GOL!" ou "QUASE!")
    [SerializeField] private TextMeshProUGUI textoMensagem;

    // Cenario de cada fase: o elemento 0 e a fase 1, o 1 e a fase 2...
    // So o cenario da fase escolhida fica ligado.
    [SerializeField] private GameObject[] cenarios;

    // Para onde o jogo volta quando a fase acaba
    [SerializeField] private string cenaSelecao = "SelecaoFases";

    // Abaixo desta altura a bola caiu do mapa
    [SerializeField] private float alturaLimite = -15f;

    int fase;
    int tentativas;
    bool faseAcabou;

    // Moedas pegas nesta fase: so viram moedas do jogador se ele fizer o gol
    int moedasDaFase;

    void Awake()
    {
        // A fase escolhida na tela de selecao define as tentativas e o cenario
        fase = Mathf.Clamp(PlayerPrefs.GetInt("faseAtual", 1), 1, UltimaFase);
        tentativas = fase;
        for (int i = 0; i < cenarios.Length; i++)
            if (cenarios[i] != null) cenarios[i].SetActive(i == fase - 1);

        PodeChutar = true;
        textoMensagem.gameObject.SetActive(false);
        Forca.AoChutar += ChuteFeito;
        AtualizarHud();
    }

    void OnDestroy()
    {
        Forca.AoChutar -= ChuteFeito;
    }

    void AtualizarHud()
    {
        textoTentativas.text = tentativas.ToString();
        // Mostra as moedas salvas mais as pegas nesta fase (que ainda podem ser perdidas)
        textoMoedas.text = (Moedas.Quantidade + moedasDaFase).ToString();
    }

    // Cada chute gasta uma tentativa e espera a bola parar
    void ChuteFeito()
    {
        tentativas--;
        PodeChutar = false;
        AtualizarHud();
        StartCoroutine(EsperarBolaParar());
    }

    IEnumerator EsperarBolaParar()
    {
        // Da um tempo para a bola sair do lugar
        yield return new WaitForSeconds(0.5f);

        // A jogada acaba quando a bola fica parada por meio segundo ou cai do mapa
        float tempoParada = 0;
        while (tempoParada < 0.5f && bola.position.y > alturaLimite && !faseAcabou)
        {
            tempoParada = bola.linearVelocity.magnitude < 0.05f ? tempoParada + Time.deltaTime : 0;
            yield return null;
        }

        if (faseAcabou) yield break;

        if (tentativas > 0)
        {
            ReiniciarBola();
            PodeChutar = true;
        }
        else
        {
            // Nao completou a fase: perde as moedas que pegou nela
            string aviso = moedasDaFase > 0 ? "VOCÊ PERDEU " + moedasDaFase + " MOEDAS" : "TENTE DE NOVO";
            moedasDaFase = 0;
            AtualizarHud();
            StartCoroutine(TerminarFase("QUASE!\n<size=45%>" + aviso + "</size>"));
        }
    }

    // Chamado pelas moedas da fase quando a bola pega uma
    public void ColetarMoeda(int valor)
    {
        moedasDaFase += valor;
        AtualizarHud();
    }

    // Chamado pelo Gol quando a bola entra na rede
    public void MarcarGol()
    {
        if (faseAcabou) return;

        // Salva o premio do gol mais as moedas pegas na fase, e libera a proxima fase
        int premio = PremioPorFase * fase + moedasDaFase;
        Moedas.Quantidade += premio;
        moedasDaFase = 0;
        int proxima = Mathf.Min(fase + 1, UltimaFase);
        if (PlayerPrefs.GetInt("faseLiberada", 1) < proxima)
            PlayerPrefs.SetInt("faseLiberada", proxima);
        PlayerPrefs.Save();

        AtualizarHud();
        StartCoroutine(TerminarFase("GOL!\n<size=45%>+" + premio + " MOEDAS</size>"));
    }

    // Mostra a mensagem e volta para a selecao de fases
    IEnumerator TerminarFase(string mensagem)
    {
        faseAcabou = true;
        PodeChutar = false;
        textoMensagem.text = mensagem;
        textoMensagem.gameObject.SetActive(true);

        // Pequena animacao: o texto cresce rapido e assenta
        var rt = textoMensagem.rectTransform;
        for (float t = 0; t < 0.35f; t += Time.deltaTime)
        {
            float k = t / 0.35f;
            float escala = k < 0.7f ? Mathf.Lerp(0.3f, 1.15f, k / 0.7f) : Mathf.Lerp(1.15f, 1f, (k - 0.7f) / 0.3f);
            rt.localScale = new Vector3(escala, escala, 1);
            yield return null;
        }
        rt.localScale = Vector3.one;

        yield return new WaitForSeconds(2.2f);
        SceneManager.LoadScene(cenaSelecao);
    }

    // Volta a bola para o inicio, parada e com a barra de forca vazia
    void ReiniciarBola()
    {
        bola.linearVelocity = Vector2.zero;
        bola.angularVelocity = 0;
        bola.position = posicaoInicial.position;
        bola.rotation = 0;
        bola.transform.SetPositionAndRotation(posicaoInicial.position, Quaternion.identity);
        forca.Resetar();
    }
}
