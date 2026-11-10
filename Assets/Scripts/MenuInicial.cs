using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuInicial : MonoBehaviour
{
    // Nome da cena do jogo (precisa estar na lista de cenas do build)
    [SerializeField] private string cenaDoJogo = "SampleScene";

    // Painel com os icones que sobem da engrenagem
    [SerializeField] private GameObject painelConfiguracoes;

    // Icones do painel, de baixo para cima (ordem em que sobem)
    [SerializeField] private RectTransform[] botoesPainel;

    // Ponto de onde os icones saem (posicao da engrenagem dentro do painel)
    [SerializeField] private Vector2 origemAnimacao;

    // Engrenagem: o painel de icones sempre fica em cima dela
    [SerializeField] private RectTransform engrenagem;

    // Icone do som: fica apagado com o som ligado e forte com o som mudo
    [SerializeField] private Image iconeSom;

    // Janela de informacoes (creditos)
    [SerializeField] private GameObject painelInfo;

    // Link aberto pelo botao do Facebook
    [SerializeField] private string linkFacebook = "";

    const float DuracaoAnimacao = 0.3f;
    const float AtrasoEntreIcones = 0.07f;

    Vector2[] destinos;
    bool painelAberto;
    Coroutine animacao;

    void Awake()
    {
        // Se a engrenagem mudou de lugar, a coluna de icones vai junto
        if (engrenagem != null)
            painelConfiguracoes.GetComponent<RectTransform>().anchoredPosition = engrenagem.anchoredPosition;

        // Guarda onde cada icone fica com o painel aberto
        destinos = new Vector2[botoesPainel.Length];
        for (int i = 0; i < botoesPainel.Length; i++)
            destinos[i] = botoesPainel[i].anchoredPosition;
    }

    void Start()
    {
        // Comeca com tudo fechado e o som como foi salvo da ultima vez
        painelConfiguracoes.SetActive(false);
        if (painelInfo != null) painelInfo.SetActive(false);
        AplicarSom(PlayerPrefs.GetInt("som", 1) == 1);
    }

    // Chamado pelo botao Jogar
    public void Jogar()
    {
        SceneManager.LoadScene(cenaDoJogo);
    }

    // Chamado pela engrenagem: os icones sobem ou descem
    public void AlternarConfiguracoes()
    {
        painelAberto = !painelAberto;
        if (animacao != null) StopCoroutine(animacao);
        animacao = StartCoroutine(AnimarPainel(painelAberto));
    }

    IEnumerator AnimarPainel(bool abrir)
    {
        // Se estava fechado, os icones partem de dentro da engrenagem
        if (abrir && !painelConfiguracoes.activeSelf)
        {
            foreach (var botao in botoesPainel)
            {
                botao.anchoredPosition = origemAnimacao;
                botao.localScale = new Vector3(0.3f, 0.3f, 1);
            }
        }
        painelConfiguracoes.SetActive(true);

        int total = botoesPainel.Length;
        var inicio = new Vector2[total];
        var escalaInicio = new float[total];
        for (int i = 0; i < total; i++)
        {
            inicio[i] = botoesPainel[i].anchoredPosition;
            escalaInicio[i] = botoesPainel[i].localScale.x;
        }

        float tempoTotal = DuracaoAnimacao + AtrasoEntreIcones * (total - 1);
        for (float t = 0; t < tempoTotal + Time.unscaledDeltaTime; t += Time.unscaledDeltaTime)
        {
            for (int i = 0; i < total; i++)
            {
                // Abrindo, o de baixo sobe primeiro; fechando, o de cima desce primeiro
                int ordem = abrir ? i : total - 1 - i;
                float k = Mathf.Clamp01((t - ordem * AtrasoEntreIcones) / DuracaoAnimacao);
                float curva = abrir ? SaidaComQuique(k) : k * k;

                var alvo = abrir ? destinos[i] : origemAnimacao;
                botoesPainel[i].anchoredPosition = Vector2.LerpUnclamped(inicio[i], alvo, curva);
                float escala = Mathf.LerpUnclamped(escalaInicio[i], abrir ? 1f : 0.3f, curva);
                botoesPainel[i].localScale = new Vector3(escala, escala, 1);
            }
            yield return null;
        }

        if (!abrir) painelConfiguracoes.SetActive(false);
        animacao = null;
    }

    // Passa um pouco do ponto e volta, como um quique
    static float SaidaComQuique(float k)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1;
        return 1 + c3 * Mathf.Pow(k - 1, 3) + c1 * Mathf.Pow(k - 1, 2);
    }

    // Chamado pelo icone do som: liga ou desliga todo o audio do jogo
    public void AlternarSom()
    {
        bool ligado = AudioListener.volume == 0;
        AplicarSom(ligado);
        PlayerPrefs.SetInt("som", ligado ? 1 : 0);
    }

    void AplicarSom(bool ligado)
    {
        AudioListener.volume = ligado ? 1 : 0;
        if (iconeSom != null) iconeSom.color = new Color(1, 1, 1, ligado ? 0.45f : 1f);
    }

    // Chamado pelo icone "i"
    public void AbrirInfo()
    {
        painelInfo.SetActive(true);
    }

    // Chamado ao clicar em qualquer lugar da janela de informacoes
    public void FecharInfo()
    {
        painelInfo.SetActive(false);
    }

    // Chamado pelo icone "f"
    public void AbrirFacebook()
    {
        if (string.IsNullOrEmpty(linkFacebook))
        {
            Debug.Log("Link do Facebook ainda nao foi configurado no MenuInicial");
            return;
        }
        Application.OpenURL(linkFacebook);
    }

    // Fecha o jogo (no editor, para o Play)
    public void Sair()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
