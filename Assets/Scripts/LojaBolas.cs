using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LojaBolas : MonoBehaviour
{
    // Um item da loja e tudo que aparece no card dele
    [System.Serializable]
    public class ItemLoja
    {
        public int preco;
        public GameObject bola;      // imagem da bola (sempre visivel)
        public GameObject cadeado;   // cadeado pequeno, aparece enquanto nao foi comprada
        public GameObject check;     // aparece na bola que esta em uso
        public TextMeshProUGUI textoBarra; // preco, "USAR" ou vazio
    }

    // Cena para onde o botao Voltar leva
    [SerializeField] private string cenaVoltar = "SelecaoFases";

    [SerializeField] private TextMeshProUGUI textoMoedas;
    [SerializeField] private ItemLoja[] itens;

    void Start()
    {
        Atualizar();
    }

    // A primeira bola ja vem liberada; as outras precisam ser compradas
    bool Comprada(int indice)
    {
        return indice == 0 || PlayerPrefs.GetInt("bolaComprada" + indice, 0) == 1;
    }

    // Chamado ao clicar num card: compra se estiver bloqueada, usa se ja for do jogador
    public void ClicarItem(int indice)
    {
        if (!Comprada(indice))
        {
            if (Moedas.Quantidade < itens[indice].preco)
            {
                Debug.Log("Moedas insuficientes para comprar esta bola");
                return;
            }
            Moedas.Quantidade -= itens[indice].preco;
            PlayerPrefs.SetInt("bolaComprada" + indice, 1);
        }

        PlayerPrefs.SetInt("bolaSelecionada", indice);
        PlayerPrefs.Save();
        Atualizar();
    }

    // Mostra cadeado, bola, check e texto da barra conforme o que o jogador tem
    void Atualizar()
    {
        if (textoMoedas != null) textoMoedas.text = Moedas.Quantidade.ToString();

        int selecionada = PlayerPrefs.GetInt("bolaSelecionada", 0);
        for (int i = 0; i < itens.Length; i++)
        {
            bool comprada = Comprada(i);
            bool emUso = comprada && i == selecionada;

            itens[i].bola.SetActive(true);
            itens[i].cadeado.SetActive(!comprada);
            itens[i].check.SetActive(emUso);
            itens[i].textoBarra.text = !comprada ? itens[i].preco.ToString() : emUso ? "" : "USAR";
        }
    }

    // Chamado pelo botao Voltar
    public void Voltar()
    {
        SceneManager.LoadScene(cenaVoltar);
    }
}
