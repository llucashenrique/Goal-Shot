using UnityEngine;

// Troca a imagem da bola pela que o jogador escolheu na loja
[RequireComponent(typeof(SpriteRenderer))]
public class AparenciaBola : MonoBehaviour
{
    // Mesma ordem dos itens da loja: nova, azul, dourada
    [SerializeField] private Sprite[] bolas;

    void Awake()
    {
        int indice = PlayerPrefs.GetInt("bolaSelecionada", 0);
        if (indice >= 0 && indice < bolas.Length && bolas[indice] != null)
            GetComponent<SpriteRenderer>().sprite = bolas[indice];
    }
}
