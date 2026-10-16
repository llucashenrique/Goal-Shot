using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Rotacao : MonoBehaviour
{

    // Posição da Seta
    [SerializeField]private Transform posStart;

    // Seta
    [SerializeField]private Image setaImg;

    // Angulo da seta em graus (o script Forca usa esse valor)
    public float zRotate;
    void Start()
    {
        // Bola e seta comecam na posicao inicial (StartPos)
        PosicionaSeta();
        PosicionaBola();
    }

    void Update()
    {
        RotacaoSeta();
        InputDeRotacao();
    }

    void PosicionaSeta()
    {
        setaImg.rectTransform.position = posStart.position;
    }

    void PosicionaBola()
    {
        this.gameObject.transform.position = posStart.position;
    }

    // Gira a seta no eixo Z para o angulo do zRotate
    void RotacaoSeta()
    {
        setaImg.rectTransform.eulerAngles = new Vector3(0,0,zRotate);
    }

    // Setas do teclado aumentam ou diminuem o angulo
    void InputDeRotacao()
    {
        if(Input.GetKey(KeyCode.UpArrow))
        {
            zRotate += 2.5f;
        }

         if(Input.GetKey(KeyCode.UpArrow))
        {
            zRotate -= 2.5f;
        }
    }
}
