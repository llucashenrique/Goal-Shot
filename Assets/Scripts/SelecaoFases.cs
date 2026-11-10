using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelecaoFases : MonoBehaviour
{
    // Cena onde a fase e jogada (precisa estar na lista de cenas do build)
    [SerializeField] private string cenaDoJogo = "SampleScene";

    // Cena do menu inicial, para o botao Voltar
    [SerializeField] private string cenaMenu = "TelaInicial";

    // Botoes das fases, em ordem (fase 1, fase 2, ...)
    [SerializeField] private Button[] botoesFases;

    // Texto do contador de moedas, no topo da tela
    [SerializeField] private TextMeshProUGUI textoMoedas;

    void Start()
    {
        // Mostra quantas moedas o jogador tem guardadas
        if (textoMoedas != null) textoMoedas.text = Moedas.Quantidade.ToString();

        // So libera ate a ultima fase desbloqueada (a fase 1 sempre esta liberada)
        int liberada = PlayerPrefs.GetInt("faseLiberada", 1);
        for (int i = 0; i < botoesFases.Length; i++)
            botoesFases[i].interactable = i + 1 <= liberada;
    }

    // Chamado pelos botoes das fases, com o numero da fase
    public void AbrirFase(int numero)
    {
        // Guarda a fase escolhida para o Level Manager saber qual montar
        PlayerPrefs.SetInt("faseAtual", numero);
        SceneManager.LoadScene(cenaDoJogo);
    }

    // Chamado pelo botao Loja
    public void AbrirLoja()
    {
        SceneManager.LoadScene("LojaBolas");
    }

    // Chamado pelo botao Voltar
    public void Voltar()
    {
        SceneManager.LoadScene(cenaMenu);
    }
}
