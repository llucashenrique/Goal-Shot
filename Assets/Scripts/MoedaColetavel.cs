using System.Collections;
using UnityEngine;

// Moeda dentro da fase: flutua girando e, quando a bola encosta, da moedas ao jogador
public class MoedaColetavel : MonoBehaviour
{
    [SerializeField] private int valor = 10;

    Vector3 posicaoInicial;
    bool coletada;

    void Start()
    {
        posicaoInicial = transform.position;
    }

    void Update()
    {
        if (coletada) return;

        // Sobe e desce devagar e "gira" achatando no eixo X
        transform.position = posicaoInicial + Vector3.up * Mathf.Sin(Time.time * 2.5f) * 0.12f;
        float giro = Mathf.Abs(Mathf.Cos(Time.time * 3f));
        transform.localScale = new Vector3(Mathf.Max(giro, 0.15f), 1, 1);
    }

    void OnTriggerEnter2D(Collider2D outro)
    {
        // So a bola pega a moeda (ela tem o script Forca)
        if (coletada || outro.GetComponent<Forca>() == null) return;

        coletada = true;
        FindFirstObjectByType<GerenciadorFase>().ColetarMoeda(valor);
        StartCoroutine(Sumir());
    }

    // Cresce e fica transparente antes de desaparecer
    IEnumerator Sumir()
    {
        var sprite = GetComponent<SpriteRenderer>();
        for (float t = 0; t < 0.25f; t += Time.deltaTime)
        {
            float k = t / 0.25f;
            transform.localScale = Vector3.one * Mathf.Lerp(1f, 1.8f, k);
            sprite.color = new Color(1, 1, 1, 1 - k);
            yield return null;
        }
        gameObject.SetActive(false);
    }
}
