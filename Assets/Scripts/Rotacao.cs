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

    // Angulo 
    public float zRotate;
    void Start()
    {
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

    void RotacaoSeta()
    {
        setaImg.rectTransform.eulerAngles = new Vector3(0,0,zRotate);
    }

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
