using UnityEngine;

// Area dentro da rede: quando a bola entra, e gol
public class Gol : MonoBehaviour
{
    [SerializeField] private GerenciadorFase gerenciador;

    void OnTriggerEnter2D(Collider2D outro)
    {
        // So conta se for a bola (ela tem o script Forca)
        if (outro.GetComponent<Forca>() != null)
            gerenciador.MarcarGol();
    }
}
