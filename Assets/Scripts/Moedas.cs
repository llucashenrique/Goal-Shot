using UnityEngine;

// Guarda as moedas do jogador. Todas as telas leem e gastam moedas por aqui.
public static class Moedas
{
    // Quantas moedas o jogador tem na primeira vez que abre o jogo
    public const int Iniciais = 1000;

    public static int Quantidade
    {
        get => PlayerPrefs.GetInt("moedas", Iniciais);
        set
        {
            PlayerPrefs.SetInt("moedas", value);
            PlayerPrefs.Save();
        }
    }
}
