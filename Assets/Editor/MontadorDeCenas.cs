using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Monta as cenas do Goal Shot automaticamente.
// Roda sozinho quando a Unity recompila e a Versao abaixo mudou,
// ou pelo menu Goal Shot > Montar Tela Inicial.
[InitializeOnLoad]
public static class MontadorDeCenas
{
    // Aumente este numero para a tela ser montada de novo
    const int Versao = 11;
    const int VersaoSelecao = 5;
    const int VersaoLoja = 3;
    const int VersaoBola = 2;
    const string MaterialBola = "Assets/Fisica/Bola.physicsMaterial2D";
    const int VersaoFase = 5;
    const float TamanhoMoeda = 0.6f;
    const string ImagemCaixa = "Assets/Items/caixa.png";
    const string ImagemBarril = "Assets/Items/barril.png";
    const string ImagemGrama = "Assets/Items/grama.png";
    const string ImagemGol = "Assets/Items/gol.png";

    // Gol encostado no lado direito da tela, com 2,7 unidades de altura
    const float AlturaGol = 2.7f;
    const float XGol = 7.1f;
    const string ImagemBarraVerde = "Assets/Items/barra verde.png";
    const string MapaCampo = "Assets/Tiled/Campo.tmx";
    const string ImagemTiles = "Assets/Tiled/Tiles.png";

    // Mapa do Campo: 14 x 5 tiles de 1,28 unidades, com o canto de cima em (-8.96, 1.5).
    // O topo do chao fica em y = -3.62.
    static readonly Vector3 PosicaoCampo = new Vector3(-8.96f, 1.5f, 0);
    const float TamanhoTile = 1.28f;
    const float TopoDoChao = -3.62f;

    const string CenaLoja = "Assets/Scenes/LojaBolas.unity";
    const string PrefabBola = "Assets/Prefabs/bola de futebol.prefab";
    const string ImagemPainelLoja = "Assets/Items/painel loja.png";
    const string ImagemBarra = "Assets/Items/barra.png";
    const string ImagemCadeado = "Assets/Items/cadeado.png";
    const string ImagemCheck = "Assets/Items/check.png";

    // Bolas da loja, na ordem dos cards; a primeira ja vem liberada
    static readonly string[] BolasLoja = { "Assets/Items/bola nova.png", "Assets/Items/bola azul.png", "Assets/Items/bola dourada.png" };
    static readonly int[] PrecosBolas = { 0, 150, 400 };

    // Tamanho da bola no jogo, em unidades (o mesmo da imagem antiga, para o colisor continuar certo)
    const float TamanhoBolaJogo = 1.254f;

    const string CenaTelaInicial = "Assets/Scenes/TelaInicial.unity";
    const string CenaSelecaoFases = "Assets/Scenes/SelecaoFases.unity";
    const string CenaJogo = "Assets/Scenes/SampleScene.unity";
    const int QuantidadeFases = 4;

    const string ImagemPainel = "Assets/Items/painel novo.png";
    const string ImagemCard = "Assets/Items/card novo.png";
    const string ImagemMoeda = "Assets/Items/moeda.png";

    const string IconeConfiguracao = "Assets/Items/configuração.png";
    const string IconeInfo = "Assets/Items/inf.png";
    const string IconeSom = "Assets/Items/som.png";
    const string IconeFacebook = "Assets/Items/face.png";

    const string ArquivoFonte = "Assets/Fonts/LuckiestGuy-Regular.ttf";
    const string FonteTMP = "Assets/Fonts/LuckiestGuy SDF.asset";
    const string MaterialContorno = "Assets/Fonts/LuckiestGuy Contorno.mat";

    static readonly Vector2 Resolucao = new Vector2(1280, 720);
    static readonly Color VerdeBotao = new Color(0.22f, 0.72f, 0.2f);
    static readonly Color BordaBotao = new Color(0.05f, 0.3f, 0.08f);
    static readonly Color Bege = new Color(1f, 0.93f, 0.78f);

    // Posicoes iniciais (centro da tela = 0,0; tela de 1280 x 720).
    // So valem na primeira montagem: depois vale a posicao que estiver na cena.
    static readonly Vector2 PosJogar = new Vector2(0, -300);
    static readonly Vector2 PosEngrenagem = new Vector2(375, -340);
    const float TamanhoEngrenagem = 90;
    const float TamanhoIcone = 70;
    const float EspacoEntreIcones = 100;

    // Fonte usada nos textos (preenchida em PrepararFonte)
    static TMP_FontAsset fonte;
    static Material contorno;

    static string ChaveVersao => "GoalShot.TelaInicial.Versao." + Application.dataPath;
    static string ChaveVersaoSelecao => "GoalShot.SelecaoFases.Versao." + Application.dataPath;
    static string ChaveVersaoLoja => "GoalShot.LojaBolas.Versao." + Application.dataPath;
    static string ChaveVersaoBola => "GoalShot.BolaJogo.Versao." + Application.dataPath;
    static string ChaveVersaoFase => "GoalShot.Fase.Versao." + Application.dataPath;

    static MontadorDeCenas()
    {
        EditorApplication.delayCall += MontarSeMudou;

        // Se compilou durante o Play, monta quando o Play terminar
        EditorApplication.playModeStateChanged += estado =>
        {
            if (estado == PlayModeStateChange.EnteredEditMode)
                EditorApplication.delayCall += MontarSeMudou;
        };
    }

    static void MontarSeMudou()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        if (EditorPrefs.GetInt(ChaveVersao, 0) < Versao && MontarTelaInicial())
            EditorPrefs.SetInt(ChaveVersao, Versao);

        if (EditorPrefs.GetInt(ChaveVersaoSelecao, 0) < VersaoSelecao && MontarSelecaoFases())
            EditorPrefs.SetInt(ChaveVersaoSelecao, VersaoSelecao);

        if (EditorPrefs.GetInt(ChaveVersaoBola, 0) < VersaoBola && AtualizarBolaDoJogo())
            EditorPrefs.SetInt(ChaveVersaoBola, VersaoBola);

        if (EditorPrefs.GetInt(ChaveVersaoLoja, 0) < VersaoLoja && MontarLoja())
            EditorPrefs.SetInt(ChaveVersaoLoja, VersaoLoja);

        if (EditorPrefs.GetInt(ChaveVersaoFase, 0) < VersaoFase && MontarFase())
            EditorPrefs.SetInt(ChaveVersaoFase, VersaoFase);
    }

    [MenuItem("Goal Shot/Montar HUD da Fase")]
    static void MontarFasePeloMenu()
    {
        if (MontarFase())
            EditorPrefs.SetInt(ChaveVersaoFase, VersaoFase);
    }

    // Cena do jogo: HUD (tentativas e moedas) e o GerenciadorFase.
    // Nao mexe no mapa, na bola nem no Canvas da seta.
    static bool MontarFase()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false;
        var cena = EditorSceneManager.OpenScene(CenaJogo);
        PrepararFonte();

        if (!TrocarMapa()) return false;
        var cenarios = CriarCenarios();

        // HUD em um Canvas proprio, separado do Canvas da seta (que e World Space)
        var hudAntigo = GameObject.Find("HUD");
        if (hudAntigo != null) Object.DestroyImmediate(hudAntigo);
        var hud = new GameObject("HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = hud.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        ConfigurarScaler(hud.GetComponent<CanvasScaler>());

        // Esquerda: bola + barra verde com as tentativas. Direita: moeda + barra amarela.
        var tentativas = CriarBarraComIcone(hud.transform, "Tentativas", CarregarIcone(ImagemBarraVerde), CarregarBola(BolasLoja[0]), new Vector2(-490, 305), "1");
        var moedas = CriarBarraComIcone(hud.transform, "Moedas", CarregarIcone(ImagemBarra), CarregarIcone(ImagemMoeda), new Vector2(490, 305), Moedas.Iniciais.ToString());

        // Mensagem grande no meio ("GOL!" / "QUASE!"), escondida ate a fase acabar
        var caixaMensagem = new GameObject("Mensagem", typeof(RectTransform));
        caixaMensagem.transform.SetParent(hud.transform, false);
        var crt = caixaMensagem.GetComponent<RectTransform>();
        crt.anchoredPosition = new Vector2(0, 60);
        crt.sizeDelta = new Vector2(1100, 400);
        var mensagem = CriarTexto(caixaMensagem.transform, "Texto", "GOL!", 150, Vector2.zero, Vector2.zero);
        mensagem.gameObject.SetActive(false);

        // GerenciadorFase: liga a bola, a forca, o ponto inicial e os textos do HUD
        var gerenciadorGo = GameObject.Find("GerenciadorFase");
        if (gerenciadorGo == null) gerenciadorGo = new GameObject("GerenciadorFase");
        var gerenciador = gerenciadorGo.GetComponent<GerenciadorFase>();
        if (gerenciador == null) gerenciador = gerenciadorGo.AddComponent<GerenciadorFase>();

        var forca = Object.FindFirstObjectByType<Forca>();
        var inicio = GameObject.Find("StartPos");
        if (forca == null || inicio == null)
        {
            Debug.LogWarning("Goal Shot: nao achei a bola (script Forca) ou o StartPos na cena do jogo");
            return false;
        }

        var so = new SerializedObject(gerenciador);
        so.FindProperty("bola").objectReferenceValue = forca.GetComponent<Rigidbody2D>();
        so.FindProperty("forca").objectReferenceValue = forca;
        so.FindProperty("posicaoInicial").objectReferenceValue = inicio.transform;
        so.FindProperty("textoTentativas").objectReferenceValue = tentativas;
        so.FindProperty("textoMoedas").objectReferenceValue = moedas;
        so.FindProperty("textoMensagem").objectReferenceValue = mensagem;
        var listaCenarios = so.FindProperty("cenarios");
        listaCenarios.arraySize = cenarios.Length;
        for (int i = 0; i < cenarios.Length; i++)
            listaCenarios.GetArrayElementAtIndex(i).objectReferenceValue = cenarios[i];
        so.ApplyModifiedPropertiesWithoutUndo();

        CriarGol(gerenciador);

        GarantirEventSystem();
        EditorSceneManager.MarkSceneDirty(cena);
        EditorSceneManager.SaveScene(cena);
        Debug.Log("Goal Shot: fase montada com o Campo e os obstaculos (versao " + VersaoFase + ")");
        return true;
    }

    // Tira o TileLab (que tem caixas e barril desenhados) e coloca o Campo, so com o chao
    static bool TrocarMapa()
    {
        var campo = AssetDatabase.LoadAssetAtPath<GameObject>(MapaCampo);
        if (campo == null)
        {
            AssetDatabase.ImportAsset(MapaCampo, ImportAssetOptions.ForceSynchronousImport);
            campo = AssetDatabase.LoadAssetAtPath<GameObject>(MapaCampo);
        }
        if (campo == null)
        {
            Debug.LogWarning("Goal Shot: nao consegui carregar o mapa Campo.tmx");
            return false;
        }

        var antigo = GameObject.Find("TileLab");
        if (antigo != null) Object.DestroyImmediate(antigo);

        var atual = GameObject.Find("Campo");
        if (atual == null)
        {
            atual = (GameObject)PrefabUtility.InstantiatePrefab(campo);
            atual.name = "Campo";
        }
        atual.transform.position = PosicaoCampo;
        return true;
    }

    // Um cenario por fase. So o da fase escolhida fica ligado no jogo.
    //   Fase 1: campo limpo
    //   Fase 2: 1 obstaculo  (torre de caixas no meio)
    //   Fase 3: 2 obstaculos (muro de caixas e torre de barris perto do gol)
    //   Fase 4: 3 obstaculos (caixas, barril e muro de grama na frente do gol)
    static GameObject[] CriarCenarios()
    {
        foreach (var nome in new[] { "Obstaculos", "Cenarios" })
        {
            var antigo = GameObject.Find(nome);
            if (antigo != null) Object.DestroyImmediate(antigo);
        }
        var raiz = new GameObject("Cenarios");

        var caixa = CarregarBloco(ImagemCaixa);
        var barril = CarregarBloco(ImagemBarril);
        var grama = CarregarBloco(ImagemGrama);

        GameObject Fase(int numero)
        {
            var go = new GameObject("Fase " + numero);
            go.transform.SetParent(raiz.transform, false);
            return go;
        }

        // Coluna de blocos em cima do chao: x e o centro, altura em quantidade de blocos
        void Pilha(GameObject fase, string nome, Sprite sprite, float x, int altura)
        {
            var grupo = new GameObject(nome);
            grupo.transform.SetParent(fase.transform, false);
            for (int i = 0; i < altura; i++)
            {
                var bloco = new GameObject(nome + " " + (i + 1), typeof(SpriteRenderer), typeof(BoxCollider2D));
                bloco.transform.SetParent(grupo.transform, false);
                bloco.transform.position = new Vector3(x, TopoDoChao + TamanhoTile * (i + 0.5f), 0);
                bloco.GetComponent<SpriteRenderer>().sprite = sprite;
                bloco.GetComponent<BoxCollider2D>().size = sprite.bounds.size;
            }
        }

        // Moeda para pegar: x e y sao a posicao no mundo
        var spriteMoeda = CarregarComTamanho(ImagemMoeda, TamanhoMoeda, usarLargura: true);
        int contadorMoedas = 0;
        void Moeda(GameObject fase, float x, float y)
        {
            var moeda = new GameObject("Moeda " + (++contadorMoedas), typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(MoedaColetavel));
            moeda.transform.SetParent(fase.transform, false);
            moeda.transform.position = new Vector3(x, y, 0);
            var render = moeda.GetComponent<SpriteRenderer>();
            render.sprite = spriteMoeda;
            render.sortingOrder = 500;
            var colisor = moeda.GetComponent<CircleCollider2D>();
            colisor.isTrigger = true;
            colisor.radius = TamanhoMoeda * 0.45f;
        }

        // Fase 1: uma moeda no meio do caminho da bola
        var fase1 = Fase(1);
        Moeda(fase1, 0f, -1.2f);

        // Fase 2: uma em cima da torre de caixas e outra no alto, mais para a direita
        var fase2 = Fase(2);
        Pilha(fase2, "Caixas", caixa, 0.8f, 2);
        Moeda(fase2, 0.8f, -0.3f);
        Moeda(fase2, 3.6f, 0.4f);

        // Fase 3: uma acima do muro de caixas e outra acima dos barris
        var fase3 = Fase(3);
        Pilha(fase3, "Caixas", caixa, -1.2f, 3);
        Pilha(fase3, "Barris", barril, 3.2f, 2);
        Moeda(fase3, -1.2f, 1.0f);
        Moeda(fase3, 3.2f, -0.3f);

        // Fase 4: uma em cima do barril e outra acima do muro de grama
        var fase4 = Fase(4);
        Pilha(fase4, "Caixas", caixa, -2.2f, 2);
        Pilha(fase4, "Barril", barril, 0.9f, 1);
        Pilha(fase4, "Muro de Grama", grama, 4.0f, 2);
        Moeda(fase4, 0.9f, -1.6f);
        Moeda(fase4, 4.0f, -0.3f);

        return new[] { fase1, fase2, fase3, fase4 };
    }

    // Bloco de obstaculo do tamanho de um tile do mapa (1,28 unidades)
    static Sprite CarregarBloco(string caminho)
    {
        return CarregarComTamanho(caminho, TamanhoTile, usarLargura: true);
    }

    // Importa a imagem com o "Pixels Per Unit" que deixa ela com o tamanho pedido no jogo
    static Sprite CarregarComTamanho(string caminho, float tamanho, bool usarLargura)
    {
        var sprite = CarregarIcone(caminho);
        var importador = AssetImporter.GetAtPath(caminho) as TextureImporter;
        if (sprite == null || importador == null) return sprite;

        float pixels = usarLargura ? sprite.texture.width : sprite.texture.height;
        float ppu = Mathf.Round(pixels / tamanho);
        if (!Mathf.Approximately(importador.spritePixelsPerUnit, ppu))
        {
            importador.spritePixelsPerUnit = ppu;
            importador.SaveAndReimport();
            sprite = AssetDatabase.LoadAssetAtPath<Sprite>(caminho);
        }
        return sprite;
    }

    // Gol visto de lado, encostado na direita: a bola entra pela frente (esquerda),
    // bate no travessao se vier alta e fica presa na rede inclinada.
    static void CriarGol(GerenciadorFase gerenciador)
    {
        var antigo = GameObject.Find("Gol");
        if (antigo != null) Object.DestroyImmediate(antigo);

        var sprite = CarregarComTamanho(ImagemGol, AlturaGol, usarLargura: false);
        var gol = new GameObject("Gol", typeof(SpriteRenderer));
        gol.transform.position = new Vector3(XGol, TopoDoChao + AlturaGol / 2, 0);
        var render = gol.GetComponent<SpriteRenderer>();
        render.sprite = sprite;
        render.sortingOrder = 1000; // na frente da bola, para ela aparecer "dentro" da rede

        // Pontos da imagem (em pixels) convertidos para unidades, com o centro da imagem como origem
        float ppu = sprite.pixelsPerUnit;
        var tamanho = new Vector2(sprite.texture.width, sprite.texture.height);
        Vector2 Ponto(float px, float py) => new Vector2(px - tamanho.x / 2, tamanho.y / 2 - py) / ppu;

        // Travessao (em cima) e a rede inclinada (atras), sem colisor na frente para a bola entrar
        var estrutura = new GameObject("Travessao e Rede", typeof(EdgeCollider2D));
        estrutura.transform.SetParent(gol.transform, false);
        var borda = estrutura.GetComponent<EdgeCollider2D>();
        borda.points = new[] { Ponto(70, 10), Ponto(205, 10), Ponto(510, 460) };
        borda.edgeRadius = 0.05f;

        // Ponta da trave (a bolinha no alto do poste da frente): a bola rebate se acertar ali
        var ponta = new GameObject("Ponta da Trave", typeof(CircleCollider2D));
        ponta.transform.SetParent(gol.transform, false);
        var circulo = ponta.GetComponent<CircleCollider2D>();
        circulo.offset = Ponto(87, 22);
        circulo.radius = 20 / ppu;

        // Area dentro da rede que conta o gol
        var area = new GameObject("Area do Gol", typeof(BoxCollider2D), typeof(Gol));
        area.transform.SetParent(gol.transform, false);
        var caixa = area.GetComponent<BoxCollider2D>();
        caixa.isTrigger = true;
        var cantoA = Ponto(120, 200);
        var cantoB = Ponto(330, 450);
        caixa.offset = (cantoA + cantoB) / 2;
        caixa.size = new Vector2(Mathf.Abs(cantoB.x - cantoA.x), Mathf.Abs(cantoB.y - cantoA.y));

        var so = new SerializedObject(area.GetComponent<Gol>());
        so.FindProperty("gerenciador").objectReferenceValue = gerenciador;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // Barra (imagem) com um icone saindo pela esquerda e um numero no meio
    static TextMeshProUGUI CriarBarraComIcone(Transform pai, string nome, Sprite barra, Sprite icone, Vector2 posicao, string valor)
    {
        var go = new GameObject(nome, typeof(RectTransform));
        go.transform.SetParent(pai, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = posicao;
        rt.sizeDelta = new Vector2(250, 84);

        CriarImagem(go.transform, "Barra", barra, Vector2.zero, new Vector2(250, 84));
        CriarImagem(go.transform, "Icone", icone, new Vector2(-112, 0), new Vector2(86, 86));
        return CriarTexto(go.transform, "Valor", valor, 46, new Vector2(22, -4), new Vector2(-70, 0));
    }

    [MenuItem("Goal Shot/Montar Loja de Bolas")]
    static void MontarLojaPeloMenu()
    {
        if (MontarLoja())
            EditorPrefs.SetInt(ChaveVersaoLoja, VersaoLoja);
    }

    // So para testar a loja enquanto o jogo ainda nao da moedas
    [MenuItem("Goal Shot/Teste: Ganhar 500 Moedas")]
    static void GanharMoedasTeste()
    {
        Moedas.Quantidade += 500;
        Debug.Log("Goal Shot: agora voce tem " + Moedas.Quantidade + " moedas");
    }

    // Trava de novo as bolas compradas (mantendo moedas e fases), para mostrar a compra na apresentacao
    [MenuItem("Goal Shot/Teste: Bloquear Bolas da Loja")]
    static void BloquearBolasTeste()
    {
        for (int i = 1; i < BolasLoja.Length; i++)
            PlayerPrefs.DeleteKey("bolaComprada" + i);
        PlayerPrefs.SetInt("bolaSelecionada", 0);
        PlayerPrefs.Save();
        Debug.Log("Goal Shot: bola azul e dourada bloqueadas de novo; bola padrao em uso. Voce tem " + Moedas.Quantidade + " moedas");
    }

    [MenuItem("Goal Shot/Teste: Liberar Todas as Fases")]
    static void LiberarFasesTeste()
    {
        PlayerPrefs.SetInt("faseLiberada", 4);
        PlayerPrefs.Save();
        Debug.Log("Goal Shot: as 4 fases foram liberadas");
    }

    [MenuItem("Goal Shot/Teste: Zerar Progresso")]
    static void ZerarProgressoTeste()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("Goal Shot: moedas, bolas compradas e fases liberadas foram zerados");
    }

    // Importa a bola com um tamanho que deixa ela igual a bola antiga no jogo
    static Sprite CarregarBola(string caminho)
    {
        var sprite = CarregarIcone(caminho);
        var importador = AssetImporter.GetAtPath(caminho) as TextureImporter;
        if (sprite == null || importador == null) return sprite;

        float ppu = Mathf.Round(sprite.texture.width / TamanhoBolaJogo);
        if (!Mathf.Approximately(importador.spritePixelsPerUnit, ppu))
        {
            importador.spritePixelsPerUnit = ppu;
            importador.SaveAndReimport();
            sprite = AssetDatabase.LoadAssetAtPath<Sprite>(caminho);
        }
        return sprite;
    }

    // Coloca a bola nova no prefab do jogo e o script que troca pela bola escolhida na loja
    static bool AtualizarBolaDoJogo()
    {
        var sprites = BolasLoja.Select(CarregarBola).ToArray();
        if (sprites.Any(s => s == null)) return false;

        var raiz = PrefabUtility.LoadPrefabContents(PrefabBola);
        raiz.GetComponent<SpriteRenderer>().sprite = sprites[0];

        // Fisica da bola:
        // - material com quique, para rebater na trave e nos obstaculos em vez de cair reto
        // - resistencia (damping), para chute fraco parar no caminho em vez de rolar para sempre
        // - deteccao continua, para chute forte nao atravessar obstaculo
        var corpo = raiz.GetComponent<Rigidbody2D>();
        corpo.sharedMaterial = CriarMaterialBola();
        corpo.linearDamping = 0.35f;
        corpo.angularDamping = 1.5f;
        corpo.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        corpo.interpolation = RigidbodyInterpolation2D.Interpolate;

        var aparencia = raiz.GetComponent<AparenciaBola>();
        if (aparencia == null) aparencia = raiz.AddComponent<AparenciaBola>();
        var so = new SerializedObject(aparencia);
        var lista = so.FindProperty("bolas");
        lista.arraySize = sprites.Length;
        for (int i = 0; i < sprites.Length; i++)
            lista.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
        so.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(raiz, PrefabBola);
        PrefabUtility.UnloadPrefabContents(raiz);
        Debug.Log("Goal Shot: bola do jogo trocada pela bola nova");
        return true;
    }

    static PhysicsMaterial2D CriarMaterialBola()
    {
        var material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(MaterialBola);
        if (material == null)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Fisica")) AssetDatabase.CreateFolder("Assets", "Fisica");
            material = new PhysicsMaterial2D("Bola");
            AssetDatabase.CreateAsset(material, MaterialBola);
        }
        material.bounciness = 0.55f; // quanto a bola quica (0 = nada, 1 = sem perder forca)
        material.friction = 0.5f;    // atrito com o chao e os obstaculos
        EditorUtility.SetDirty(material);
        AssetDatabase.SaveAssets();
        return material;
    }

    // Loja: painel com 3 cards de bola, moedas no topo e Voltar
    static bool MontarLoja()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false;
        var cena = AbrirOuCriarCena(CenaLoja);

        PrepararFonte();
        ConfigurarCamera();
        Object.FindFirstObjectByType<Camera>().backgroundColor = new Color(0.42f, 0.42f, 0.44f);

        var canvasAntigo = Object.FindFirstObjectByType<Canvas>();
        if (canvasAntigo != null) Object.DestroyImmediate(canvasAntigo.gameObject);
        var canvas = CriarCanvas();
        var loja = canvas.gameObject.AddComponent<LojaBolas>();

        // Painel com o titulo "LOJA DE BOLAS" ja na imagem
        var painel = new GameObject("QuadroLoja", typeof(RectTransform), typeof(Image), typeof(Shadow));
        painel.transform.SetParent(canvas.transform, false);
        var prt = painel.GetComponent<RectTransform>();
        prt.anchoredPosition = new Vector2(40, -15);
        prt.sizeDelta = new Vector2(840, 560);
        var pimg = painel.GetComponent<Image>();
        pimg.sprite = CarregarIcone(ImagemPainelLoja);
        pimg.preserveAspect = true;
        pimg.raycastTarget = false;
        var psombra = painel.GetComponent<Shadow>();
        psombra.effectColor = new Color(0, 0, 0, 0.35f);
        psombra.effectDistance = new Vector2(0, -12);

        var textoMoedas = CriarContadorMoedas(canvas.transform, new Vector2(480, 305));

        // Cards das bolas: uma fileira de 3 na parte bege do painel
        var card = CarregarIcone(ImagemCard);
        var barra = CarregarIcone(ImagemBarra);
        var cadeado = CarregarIcone(ImagemCadeado);
        var check = CarregarIcone(ImagemCheck);

        var so = new SerializedObject(loja);
        so.FindProperty("textoMoedas").objectReferenceValue = textoMoedas;
        var itens = so.FindProperty("itens");
        itens.arraySize = BolasLoja.Length;

        for (int i = 0; i < BolasLoja.Length; i++)
        {
            var item = CriarCardFase(painel.transform, "Bola" + (i + 1), card, "", new Vector2(-230 + i * 230, -38), 1.25f);
            Object.DestroyImmediate(item.transform.Find("Numero").gameObject);

            // Bola sempre visivel; cadeado pequeno no canto de baixo da bola
            var bola = CriarImagem(item.transform, "Bola", CarregarBola(BolasLoja[i]), new Vector2(0, 28), new Vector2(125, 125));
            var trava = CriarImagem(item.transform, "Cadeado", cadeado, new Vector2(42, -2), new Vector2(90, 60));
            var barraGo = CriarImagem(item.transform, "Barra", barra, new Vector2(0, -68), new Vector2(200, 67));
            var texto = CriarTexto(barraGo.transform, "Texto", PrecosBolas[i].ToString(), 38, new Vector2(0, -3), Vector2.zero);
            var marca = CriarImagem(item.transform, "Check", check, new Vector2(0, -66), new Vector2(46, 38));

            // Estado inicial (o mesmo que o jogador ve na primeira vez):
            // a primeira bola em uso com check; as outras com cadeado e preco
            bool padrao = i == 0;
            trava.SetActive(!padrao);
            marca.SetActive(padrao);
            texto.text = padrao ? "" : PrecosBolas[i].ToString();

            while (item.onClick.GetPersistentEventCount() > 0)
                UnityEventTools.RemovePersistentListener(item.onClick, 0);
            UnityEventTools.AddIntPersistentListener(item.onClick, loja.ClicarItem, i);

            var dados = itens.GetArrayElementAtIndex(i);
            dados.FindPropertyRelative("preco").intValue = PrecosBolas[i];
            dados.FindPropertyRelative("bola").objectReferenceValue = bola;
            dados.FindPropertyRelative("cadeado").objectReferenceValue = trava;
            dados.FindPropertyRelative("check").objectReferenceValue = marca;
            dados.FindPropertyRelative("textoBarra").objectReferenceValue = texto;
        }
        so.ApplyModifiedPropertiesWithoutUndo();

        var voltar = CriarBotao(canvas.transform, "BotaoVoltar", "VOLTAR", new Vector2(-530, -305), new Vector2(220, 76), 50, new Color(0.85f, 0.33f, 0.24f), -6);
        LigarClique(voltar, loja.Voltar);

        GarantirEventSystem();
        EditorSceneManager.MarkSceneDirty(cena);
        EditorSceneManager.SaveScene(cena, CenaLoja);
        ConfigurarListaDeCenas();
        Debug.Log("Goal Shot: Loja de Bolas montada (versao " + VersaoLoja + ")");
        return true;
    }

    // Abre a cena se ela existir; senao cria uma vazia so com a camera
    static UnityEngine.SceneManagement.Scene AbrirOuCriarCena(string caminho)
    {
        if (System.IO.File.Exists(caminho))
            return EditorSceneManager.OpenScene(caminho);

        var cena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var cameraNova = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraNova.tag = "MainCamera";
        var cam = cameraNova.GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5;
        return cena;
    }

    // Imagem simples (sem clique) dentro de outro objeto
    static GameObject CriarImagem(Transform pai, string nome, Sprite sprite, Vector2 posicao, Vector2 tamanho)
    {
        var go = new GameObject(nome, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(pai, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = posicao;
        rt.sizeDelta = tamanho;
        var imagem = go.GetComponent<Image>();
        imagem.sprite = sprite;
        imagem.preserveAspect = true;
        imagem.raycastTarget = false;
        return go;
    }

    [MenuItem("Goal Shot/Montar Tela Inicial")]
    static void MontarPeloMenu()
    {
        if (MontarTelaInicial())
            EditorPrefs.SetInt(ChaveVersao, Versao);
    }

    [MenuItem("Goal Shot/Montar Seleção de Fases")]
    static void MontarSelecaoPeloMenu()
    {
        if (MontarSelecaoFases())
            EditorPrefs.SetInt(ChaveVersaoSelecao, VersaoSelecao);
    }

    // Tela com o quadro bege, o titulo, a grade de fases e o botao Voltar
    static bool MontarSelecaoFases()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false;

        // Abre a cena, ou cria uma nova so com a camera
        UnityEngine.SceneManagement.Scene cena;
        if (System.IO.File.Exists(CenaSelecaoFases))
        {
            cena = EditorSceneManager.OpenScene(CenaSelecaoFases);
        }
        else
        {
            cena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraNova = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraNova.tag = "MainCamera";
            var cam = cameraNova.GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5;
        }

        PrepararFonte();
        ConfigurarCamera();
        Object.FindFirstObjectByType<Camera>().backgroundColor = new Color(0.42f, 0.42f, 0.44f);

        // Recria o Canvas do zero para nao sobrar nada de versoes antigas
        var canvasAntigo = Object.FindFirstObjectByType<Canvas>();
        if (canvasAntigo != null) Object.DestroyImmediate(canvasAntigo.gameObject);
        var canvas = CriarCanvas();
        var tela = canvas.gameObject.AddComponent<SelecaoFases>();

        // Painel quadrado no centro (imagem painel)
        var painel = new GameObject("QuadroFases", typeof(RectTransform), typeof(Image), typeof(Shadow));
        painel.transform.SetParent(canvas.transform, false);
        var prt = painel.GetComponent<RectTransform>();
        prt.anchoredPosition = new Vector2(0, -20);
        prt.sizeDelta = new Vector2(840, 560);
        var pimg = painel.GetComponent<Image>();
        pimg.sprite = CarregarIcone(ImagemPainel);
        pimg.preserveAspect = true;
        pimg.raycastTarget = false;
        var psombra = painel.GetComponent<Shadow>();
        psombra.effectColor = new Color(0, 0, 0, 0.35f);
        psombra.effectDistance = new Vector2(0, -12);

        // Topo: Loja no canto esquerdo e moedas no canto direito (o titulo ja vem no painel)
        var loja = CriarBotao(canvas.transform, "BotaoLoja", "LOJA", new Vector2(-480, 305), new Vector2(255, 78), 56, new Color(0.36f, 0.32f, 0.72f), -6);
        LigarClique(loja, tela.AbrirLoja);
        var textoMoedas = CriarContadorMoedas(canvas.transform, new Vector2(480, 305));

        // Cards das fases: uma fileira de 4 na parte bege do painel
        var card = CarregarIcone(ImagemCard);
        var botoes = new Button[QuantidadeFases];
        for (int i = 0; i < QuantidadeFases; i++)
        {
            var pos = new Vector2(-262 + i * 175, -38);
            var botao = CriarCardFase(painel.transform, "Fase" + (i + 1), card, (i + 1).ToString(), pos);

            // Fase bloqueada fica cinza
            var cores = botao.colors;
            cores.disabledColor = new Color(0.55f, 0.55f, 0.55f, 1);
            botao.colors = cores;

            while (botao.onClick.GetPersistentEventCount() > 0)
                UnityEventTools.RemovePersistentListener(botao.onClick, 0);
            UnityEventTools.AddIntPersistentListener(botao.onClick, tela.AbrirFase, i + 1);
            botoes[i] = botao;
        }

        // Voltar, no canto de baixo do quadro
        var voltar = CriarBotao(canvas.transform, "BotaoVoltar", "VOLTAR", new Vector2(-505, -300), new Vector2(230, 80), 52, new Color(0.85f, 0.33f, 0.24f), -6);
        LigarClique(voltar, tela.Voltar);

        // Liga os botoes e o contador no script da tela
        var so = new SerializedObject(tela);
        so.FindProperty("textoMoedas").objectReferenceValue = textoMoedas;
        var lista = so.FindProperty("botoesFases");
        lista.arraySize = botoes.Length;
        for (int i = 0; i < botoes.Length; i++)
            lista.GetArrayElementAtIndex(i).objectReferenceValue = botoes[i];
        so.ApplyModifiedPropertiesWithoutUndo();

        GarantirEventSystem();
        EditorSceneManager.MarkSceneDirty(cena);
        EditorSceneManager.SaveScene(cena, CenaSelecaoFases);
        ConfigurarListaDeCenas();
        Debug.Log("Goal Shot: Selecao de Fases montada (versao " + VersaoSelecao + ")");
        return true;
    }

    static bool MontarTelaInicial()
    {
        // Abre a TelaInicial (pergunta antes se outra cena tiver mudancas nao salvas)
        var cena = EditorSceneManager.GetActiveScene();
        if (cena.path != CenaTelaInicial)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false;
            cena = EditorSceneManager.OpenScene(CenaTelaInicial);
        }

        PrepararFonte();
        ConfigurarCamera();
        AjustarFundo();

        var canvas = CriarCanvas();

        // Remove o que versoes antigas criaram e nao existe mais
        foreach (var antigo in new[] { "Button", "BotaoSair" })
        {
            var t = canvas.transform.Find(antigo);
            if (t != null) Object.DestroyImmediate(t.gameObject);
        }

        var menu = canvas.GetComponent<MenuInicial>();
        if (menu == null) menu = canvas.gameObject.AddComponent<MenuInicial>();

        // Mantem a posicao que o JOGAR e a engrenagem ja tiverem na cena
        var posJogar = PosicaoAtual(canvas.transform, "BotaoJogar", PosJogar);
        var posEngrenagem = PosicaoAtual(canvas.transform, "BotaoConfiguracoes", PosEngrenagem);

        // Botao principal, abaixo da bola
        var jogar = CriarBotao(canvas.transform, "BotaoJogar", "JOGAR", posJogar, new Vector2(400, 115), 105, VerdeBotao, -20);
        LigarClique(jogar, menu.Jogar);

        // Coluna de icones acima da engrenagem: f, som e i (de baixo para cima)
        var painel = CriarPainelIcones(canvas.transform, posEngrenagem, out var origem);
        var face = CriarBotaoIcone(painel.transform, "BotaoFacebook", CarregarIcone(IconeFacebook), new Vector2(0, origem.y + EspacoEntreIcones), TamanhoIcone);
        var som = CriarBotaoIcone(painel.transform, "BotaoSom", CarregarIcone(IconeSom), new Vector2(0, origem.y + EspacoEntreIcones * 2), TamanhoIcone);
        var info = CriarBotaoIcone(painel.transform, "BotaoInfo", CarregarIcone(IconeInfo), new Vector2(0, origem.y + EspacoEntreIcones * 3), TamanhoIcone);
        LigarClique(face, menu.AbrirFacebook);
        LigarClique(som, menu.AlternarSom);
        LigarClique(info, menu.AbrirInfo);

        // Engrenagem por cima do painel, para os icones sairem de tras dela
        var engrenagem = CriarBotaoIcone(canvas.transform, "BotaoConfiguracoes", CarregarIcone(IconeConfiguracao), posEngrenagem, TamanhoEngrenagem);
        LigarClique(engrenagem, menu.AlternarConfiguracoes);

        // Janela de informacoes, por cima de tudo
        var painelInfo = CriarPainelInfo(canvas.transform);
        LigarClique(painelInfo.GetComponent<Button>(), menu.FecharInfo);

        // Ordem de desenho: jogar, icones, engrenagem e, por ultimo, a janela de info
        jogar.transform.SetAsLastSibling();
        painel.transform.SetAsLastSibling();
        engrenagem.transform.SetAsLastSibling();
        painelInfo.transform.SetAsLastSibling();

        // Liga tudo no MenuInicial
        var so = new SerializedObject(menu);
        so.FindProperty("painelConfiguracoes").objectReferenceValue = painel;
        so.FindProperty("cenaDoJogo").stringValue = "SelecaoFases";
        so.FindProperty("origemAnimacao").vector2Value = origem;
        so.FindProperty("engrenagem").objectReferenceValue = engrenagem.GetComponent<RectTransform>();
        so.FindProperty("iconeSom").objectReferenceValue = som.GetComponent<Image>();
        so.FindProperty("painelInfo").objectReferenceValue = painelInfo;
        var botoes = so.FindProperty("botoesPainel");
        var ordem = new[] { face, som, info };
        botoes.arraySize = ordem.Length;
        for (int i = 0; i < ordem.Length; i++)
            botoes.GetArrayElementAtIndex(i).objectReferenceValue = ordem[i].GetComponent<RectTransform>();
        so.ApplyModifiedPropertiesWithoutUndo();

        // Na cena tudo fica fechado; abre ao clicar
        painel.SetActive(false);
        painelInfo.SetActive(false);

        GarantirEventSystem();
        ConfigurarListaDeCenas();

        EditorSceneManager.MarkSceneDirty(cena);
        EditorSceneManager.SaveScene(cena);
        Debug.Log("Goal Shot: Tela Inicial montada (versao " + Versao + ")");
        return true;
    }

    // Garante que a imagem esta importada como um sprite unico (sem cortes)
    static Sprite CarregarIcone(string caminho)
    {
        var importador = AssetImporter.GetAtPath(caminho) as TextureImporter;
        if (importador != null &&
            (importador.textureType != TextureImporterType.Sprite || importador.spriteImportMode != SpriteImportMode.Single))
        {
            importador.textureType = TextureImporterType.Sprite;
            importador.spriteImportMode = SpriteImportMode.Single;
            importador.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(caminho);
    }

    // Cria (uma vez) a fonte TMP da Luckiest Guy e um material com contorno
    static void PrepararFonte()
    {
        fonte = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FonteTMP);
        if (fonte == null)
        {
            var ttf = AssetDatabase.LoadAssetAtPath<Font>(ArquivoFonte);
            if (ttf == null) return;

            fonte = TMP_FontAsset.CreateFontAsset(ttf);
            fonte.name = "LuckiestGuy SDF";
            AssetDatabase.CreateAsset(fonte, FonteTMP);
            fonte.atlasTexture.name = "LuckiestGuy SDF Atlas";
            fonte.material.name = "LuckiestGuy SDF Material";
            AssetDatabase.AddObjectToAsset(fonte.atlasTexture, fonte);
            AssetDatabase.AddObjectToAsset(fonte.material, fonte);
            AssetDatabase.SaveAssets();
        }

        contorno = AssetDatabase.LoadAssetAtPath<Material>(MaterialContorno);
        if (contorno == null)
        {
            contorno = new Material(fonte.material);
            AssetDatabase.CreateAsset(contorno, MaterialContorno);
        }
        contorno.EnableKeyword("OUTLINE_ON");
        contorno.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.22f);
        contorno.SetColor(ShaderUtilities.ID_OutlineColor, BordaBotao);
        EditorUtility.SetDirty(contorno);
    }

    static void ConfigurarCamera()
    {
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera == null) return;

        // Camera centralizada no fundo; se sair do lugar, o fundo desalinha dos botoes
        camera.transform.position = new Vector3(0, 0, -10);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
    }

    // Faz a imagem de fundo cobrir a camera inteira, sem distorcer
    static void AjustarFundo()
    {
        var fundo = GameObject.Find("tela inicial");
        var camera = Object.FindFirstObjectByType<Camera>();
        if (fundo == null || camera == null) return;

        var sprite = fundo.GetComponent<SpriteRenderer>();
        if (sprite == null || sprite.sprite == null) return;

        float alturaTela = camera.orthographicSize * 2;
        float larguraTela = alturaTela * Resolucao.x / Resolucao.y;
        var tamanho = sprite.sprite.bounds.size;
        float escala = Mathf.Max(larguraTela / tamanho.x, alturaTela / tamanho.y);

        fundo.transform.position = Vector3.zero;
        fundo.transform.localScale = new Vector3(escala, escala, 1);
    }

    static Canvas CriarCanvas()
    {
        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
            canvas = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<Canvas>();

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        ConfigurarScaler(canvas.GetComponent<CanvasScaler>());
        return canvas;
    }

    static void ConfigurarScaler(CanvasScaler scaler)
    {
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = Resolucao;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        // 1 = acompanha a altura, igual a camera; assim os botoes nao saem
        // do lugar em relacao ao fundo quando a janela nao for 16:9
        scaler.matchWidthOrHeight = 1f;
    }

    // Area invisivel acima da engrenagem que segura os icones.
    // "origem" e a posicao da engrenagem dentro dela (de onde os icones saem).
    // Card de fase: imagem do card com o numero da fase no centro.
    // A imagem tem muito espaco transparente dos lados, entao a area clicavel
    // (o proprio botao) e so o quadrado visivel, e a imagem fica num filho.
    static Button CriarCardFase(Transform pai, string nome, Sprite sprite, string numero, Vector2 posicao, float escala = 1)
    {
        var go = new GameObject(nome, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(pai, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = posicao;
        rt.sizeDelta = new Vector2(160, 165) * escala;
        go.GetComponent<Image>().color = new Color(1, 1, 1, 0); // area de clique invisivel

        var visual = new GameObject("Imagem", typeof(RectTransform), typeof(Image), typeof(Shadow));
        visual.transform.SetParent(go.transform, false);
        var vrt = visual.GetComponent<RectTransform>();
        vrt.sizeDelta = new Vector2(270, 180) * escala;
        var imagem = visual.GetComponent<Image>();
        imagem.sprite = sprite;
        imagem.preserveAspect = true;
        imagem.raycastTarget = false;
        var sombra = visual.GetComponent<Shadow>();
        sombra.effectColor = new Color(0, 0, 0, 0.3f);
        sombra.effectDistance = new Vector2(0, -5);

        // O cinza de "bloqueado" e o escurecer ao passar o mouse vao na imagem
        var botao = go.GetComponent<Button>();
        botao.targetGraphic = imagem;

        CriarTexto(go.transform, "Numero", numero, 100, new Vector2(0, -6), Vector2.zero);
        return botao;
    }

    // Barra amarela com a moeda na esquerda e a quantidade de moedas
    static TextMeshProUGUI CriarContadorMoedas(Transform pai, Vector2 posicao)
    {
        var barra = new GameObject("MoedasPainel", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Shadow));
        barra.transform.SetParent(pai, false);
        var rt = barra.GetComponent<RectTransform>();
        rt.anchoredPosition = posicao;
        rt.sizeDelta = new Vector2(250, 70);

        var imagem = barra.GetComponent<Image>();
        imagem.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        imagem.type = Image.Type.Sliced;
        imagem.color = new Color(1f, 0.83f, 0.18f);
        imagem.raycastTarget = false;
        var borda = barra.GetComponent<Outline>();
        borda.effectColor = new Color(0.55f, 0.33f, 0.05f);
        borda.effectDistance = new Vector2(3, -3);
        var sombra = barra.GetComponent<Shadow>();
        sombra.effectColor = new Color(0, 0, 0, 0.35f);
        sombra.effectDistance = new Vector2(0, -6);

        // Moeda saindo pela esquerda da barra
        var moeda = new GameObject("Moeda", typeof(RectTransform), typeof(Image));
        moeda.transform.SetParent(barra.transform, false);
        var mrt = moeda.GetComponent<RectTransform>();
        mrt.anchorMin = mrt.anchorMax = new Vector2(0, 0.5f);
        mrt.anchoredPosition = new Vector2(5, 0);
        mrt.sizeDelta = new Vector2(105, 105);
        var mimg = moeda.GetComponent<Image>();
        mimg.sprite = CarregarIcone(ImagemMoeda);
        mimg.preserveAspect = true;
        mimg.raycastTarget = false;

        return CriarTexto(barra.transform, "Quantidade", Moedas.Iniciais.ToString(), 52, new Vector2(28, -5), new Vector2(-60, 0));
    }

    // Texto branco com contorno ocupando o pai inteiro, com deslocamento e reducao de largura
    static TextMeshProUGUI CriarTexto(Transform pai, string nome, string texto, float tamanho, Vector2 deslocamento, Vector2 reducao)
    {
        var go = new GameObject(nome, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(pai, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = reducao;
        rt.anchoredPosition = deslocamento;

        var tmp = go.GetComponent<TextMeshProUGUI>();
        if (fonte != null) { tmp.font = fonte; tmp.fontSharedMaterial = contorno; }
        tmp.text = texto;
        tmp.fontSize = tamanho;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        return tmp;
    }

    // Posicao que o objeto ja tem na cena, ou a padrao se ele ainda nao existir
    static Vector2 PosicaoAtual(Transform pai, string nome, Vector2 padrao)
    {
        var t = pai.Find(nome);
        return t != null ? ((RectTransform)t).anchoredPosition : padrao;
    }

    static GameObject CriarPainelIcones(Transform pai, Vector2 posEngrenagem, out Vector2 origem)
    {
        // Recria do zero para nao sobrar nada de versoes antigas
        var existente = pai.Find("PainelConfiguracoes");
        if (existente != null) Object.DestroyImmediate(existente.gameObject);

        float altura = EspacoEntreIcones * 3 + TamanhoIcone;
        var go = new GameObject("PainelConfiguracoes", typeof(RectTransform));
        go.transform.SetParent(pai, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0);
        rt.anchoredPosition = posEngrenagem;
        rt.sizeDelta = new Vector2(TamanhoIcone + 30, altura);

        origem = Vector2.zero;
        return go;
    }

    static Button CriarBotaoIcone(Transform pai, string nome, Sprite sprite, Vector2 posicao, float tamanho)
    {
        var existente = pai.Find(nome);
        var go = existente != null ? existente.gameObject : new GameObject(nome, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(pai, false);
        go.transform.localScale = Vector3.one;

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        if (pai.name == "PainelConfiguracoes")
        {
            // Dentro do painel a posicao conta a partir da base dele (onde esta a engrenagem)
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0);
        }
        rt.anchoredPosition = posicao;
        rt.sizeDelta = new Vector2(tamanho, tamanho);

        var imagem = go.GetComponent<Image>();
        imagem.sprite = sprite;
        imagem.type = Image.Type.Simple;
        imagem.preserveAspect = true;
        imagem.color = Color.white;

        return go.GetComponent<Button>();
    }

    // Janela escura com os creditos; clicar em qualquer lugar fecha
    static GameObject CriarPainelInfo(Transform pai)
    {
        var existente = pai.Find("PainelInfo");
        if (existente != null) Object.DestroyImmediate(existente.gameObject);

        var go = new GameObject("PainelInfo", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(pai, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = new Color(0, 0, 0, 0.6f);

        var cartao = new GameObject("Cartao", typeof(RectTransform), typeof(Image), typeof(Outline));
        cartao.transform.SetParent(go.transform, false);
        var crt = cartao.GetComponent<RectTransform>();
        crt.anchorMin = crt.anchorMax = crt.pivot = new Vector2(0.5f, 0.5f);
        crt.sizeDelta = new Vector2(680, 400);
        var cimg = cartao.GetComponent<Image>();
        cimg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        cimg.type = Image.Type.Sliced;
        cimg.color = Bege;
        cimg.raycastTarget = false;
        var borda = cartao.GetComponent<Outline>();
        borda.effectColor = BordaBotao;
        borda.effectDistance = new Vector2(5, -5);

        var texto = new GameObject("Texto", typeof(RectTransform), typeof(TextMeshProUGUI));
        texto.transform.SetParent(cartao.transform, false);
        var trt = texto.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(30, 25);
        trt.offsetMax = new Vector2(-30, -25);

        var tmp = texto.GetComponent<TextMeshProUGUI>();
        if (fonte != null) { tmp.font = fonte; tmp.fontSharedMaterial = fonte.material; }
        tmp.fontSize = 30;
        tmp.color = BordaBotao;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        tmp.text =
            "<size=64>GOAL SHOT</size>\n\n" +
            "Carlos Eduardo Costa Belorio\n" +
            "Lucas Henrique Peres Lino\n" +
            "Raul Barbosa Pucci\n\n" +
            "<size=22>Fonte Luckiest Guy (Apache 2.0) - toque para fechar</size>";

        return go;
    }

    // deslocamentoTexto: negativo desce o texto dentro do botao
    static Button CriarBotao(Transform pai, string nome, string texto, Vector2 posicao, Vector2 tamanho, float tamanhoFonte, Color cor, float deslocamentoTexto = 0)
    {
        var existente = pai.Find(nome);
        var go = existente != null
            ? existente.gameObject
            : new GameObject(nome, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Shadow));
        go.transform.SetParent(pai, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = posicao;
        rt.sizeDelta = tamanho;

        var imagem = go.GetComponent<Image>();
        imagem.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        imagem.type = Image.Type.Sliced;
        imagem.color = cor;

        // Sombra embaixo do botao (o Outline tambem herda de Shadow, por isso o filtro)
        var sombra = go.GetComponents<Shadow>().FirstOrDefault(s => s.GetType() == typeof(Shadow));
        if (sombra == null) sombra = go.AddComponent<Shadow>();
        sombra.effectColor = new Color(0, 0, 0, 0.45f);
        sombra.effectDistance = new Vector2(0, -6);

        // Borda verde escura em volta do botao
        var borda = go.GetComponent<Outline>();
        if (borda == null) borda = go.AddComponent<Outline>();
        borda.effectColor = BordaBotao;
        borda.effectDistance = new Vector2(3, -3);

        var textoT = go.transform.Find("Texto");
        var textoGo = textoT != null ? textoT.gameObject : new GameObject("Texto", typeof(RectTransform), typeof(TextMeshProUGUI));
        textoGo.transform.SetParent(go.transform, false);

        var trt = textoGo.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = trt.offsetMax = new Vector2(0, deslocamentoTexto);

        var tmp = textoGo.GetComponent<TextMeshProUGUI>();
        if (fonte != null)
        {
            tmp.font = fonte;
            tmp.fontSharedMaterial = contorno;
            tmp.fontStyle = FontStyles.Normal;
        }
        else
        {
            tmp.font = TMP_Settings.defaultFontAsset;
            tmp.fontStyle = FontStyles.Bold;
        }
        tmp.text = texto;
        tmp.fontSize = tamanhoFonte;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return go.GetComponent<Button>();
    }

    // Liga o clique do botao a um metodo do MenuInicial (sem duplicar)
    static void LigarClique(Button botao, UnityEngine.Events.UnityAction acao)
    {
        while (botao.onClick.GetPersistentEventCount() > 0)
            UnityEventTools.RemovePersistentListener(botao.onClick, 0);
        UnityEventTools.AddPersistentListener(botao.onClick, acao);
    }

    static void GarantirEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    // TelaInicial primeiro, depois a selecao de fases e o jogo, mantendo as outras cenas
    static void ConfigurarListaDeCenas()
    {
        var nossas = new[] { CenaTelaInicial, CenaSelecaoFases, CenaLoja, CenaJogo }
            .Where(System.IO.File.Exists).ToArray();
        var outras = EditorBuildSettings.scenes.Where(c => !nossas.Contains(c.path));
        EditorBuildSettings.scenes = nossas
            .Select(caminho => new EditorBuildSettingsScene(caminho, true))
            .Concat(outras).ToArray();
    }
}
