using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Rotacao : MonoBehaviour
{

    // Posição da Seta
    [SerializeField]private Transform posStart;

    // Seta
    [SerializeField]private Image setaImg;

    // Angulo da seta em graus (o script Forca usa esse valor)
    public float zRotate;
    // Permite ou bloqueia a rotacao da seta
    public bool liberaRot = false;

    // Avisa o script Forca que o chute pode ser feito
    public bool liberaTiro = false;

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
        LimitaRotacao();
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

    // Movimento vertical do mouse aumenta ou diminui o angulo
    void InputDeRotacao()
    {
        // if(Input.GetKey(KeyCode.UpArrow))
        // {
        //     zRotate += 2.5f;
        // }

        //  if(Input.GetKey(KeyCode.UpArrow))
        // {
        //     zRotate -= 2.5f;
        // }

        if(liberaRot == true)
        {
        // Le quanto o mouse se moveu neste frame
        float moveX = Input.GetAxis ("Mouse X");
        float moveY = Input.GetAxis ("Mouse Y");

        // Mouse para cima sobe a seta, mouse para baixo desce
        if(zRotate < 90)
        {
            if(moveY > 0)
            {
                zRotate += 2.5f;
            }  
        }

        if(zRotate > 0)
        {
            if(moveY < 0)
            {
                zRotate -= 2.5f;
            }
        }
        }
    }

    // Mantem o angulo entre 0 e 90 graus
    void LimitaRotacao()
    {
        if (zRotate >= 90)
        {
            zRotate = 90;
        }

        if (zRotate <= 0)
        {
            zRotate = 0;
        }
    }

    // Clicou e segurou na bola: libera a mira
    void OnMouseDown()
    {
       liberaRot = true;
    }

    // Soltou o mouse: trava a mira e libera o chute
    void OnMouseUp()
    {
       liberaRot = false;
       liberaTiro = true;
    }

}
