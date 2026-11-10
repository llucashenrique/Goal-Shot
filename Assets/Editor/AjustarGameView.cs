using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// Deixa a aba Game em 16:9 com o menor zoom, para a tela aparecer inteira.
// Roda uma vez sozinho e fica no menu Goal Shot > Ajustar Visualizacao do Game.
// A Unity nao tem API publica para isso, por isso o uso de reflection.
[InitializeOnLoad]
public static class AjustarGameView
{
    const int Versao = 1;
    static string Chave => "GoalShot.GameView.Versao." + Application.dataPath;

    static AjustarGameView()
    {
        EditorApplication.delayCall += AjustarSeFaltar;

        // Se compilou durante o Play, ajusta quando o Play terminar
        EditorApplication.playModeStateChanged += estado =>
        {
            if (estado == PlayModeStateChange.EnteredEditMode)
                EditorApplication.delayCall += AjustarSeFaltar;
        };
    }

    static void AjustarSeFaltar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (EditorPrefs.GetInt(Chave, 0) >= Versao) return;
        if (Ajustar()) EditorPrefs.SetInt(Chave, Versao);
    }

    [MenuItem("Goal Shot/Ajustar Visualização do Game")]
    static void AjustarPeloMenu()
    {
        Ajustar();
    }

    static bool Ajustar()
    {
        const BindingFlags Todos = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        try
        {
            var assembly = typeof(Editor).Assembly;
            var tipoGameView = assembly.GetType("UnityEditor.GameView");
            var gameView = EditorWindow.GetWindow(tipoGameView, false, null, false);

            // Procura a opcao "16:9 Aspect" na lista de resolucoes
            var tipoTamanhos = assembly.GetType("UnityEditor.GameViewSizes");
            var singleton = typeof(ScriptableSingleton<>).MakeGenericType(tipoTamanhos);
            var tamanhos = singleton.GetProperty("instance", Todos).GetValue(null);
            var grupo = tipoTamanhos.GetProperty("currentGroup", Todos).GetValue(tamanhos);
            var textos = (string[])grupo.GetType().GetMethod("GetDisplayTexts", Todos).Invoke(grupo, null);
            int indice = Array.FindIndex(textos, t => t.StartsWith("16:9"));
            if (indice < 0)
            {
                Debug.LogWarning("Goal Shot: opcao 16:9 nao encontrada na aba Game");
                return false;
            }
            tipoGameView.GetMethod("SizeSelectionCallback", Todos).Invoke(gameView, new object[] { indice, null });

            // Menor zoom possivel = a tela inteira cabe na janela
            var minimo = tipoGameView.GetProperty("minScale", Todos);
            var snapZoom = tipoGameView.GetMethod("SnapZoom", Todos, null, new[] { typeof(float) }, null);
            if (minimo != null && snapZoom != null)
                snapZoom.Invoke(gameView, new object[] { (float)minimo.GetValue(gameView) });

            gameView.Repaint();
            Debug.Log("Goal Shot: aba Game ajustada para 16:9 com a tela inteira visivel");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning("Goal Shot: nao consegui ajustar a aba Game sozinho (" + e.Message + "). Ajuste na mao: 16:9 Aspect e Scale no minimo.");
            return false;
        }
    }
}
