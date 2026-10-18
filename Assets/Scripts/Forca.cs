using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
public class Forca : MonoBehaviour
{
    private Rigidbody2D bola;
    // Intensidade do chute
    public float force = 0;
    // Usado para ler o angulo (zRotate) da seta
    private Rotacao rot;

    // Seta verde que funciona como barra de forca (Image Type Filled)
    public Image seta2Img;

    void Start()
    {
       bola = GetComponent<Rigidbody2D> ();
       rot = GetComponent<Rotacao> (); 
    }

    void Update()
    {
        AplicaForca();
        ControlaForca();
    }

    void AplicaForca()
    {
        // Divide a forca em X e Y pelo angulo (Deg2Rad converte graus para radianos)
        float x = force * Mathf.Cos(rot.zRotate * Mathf.Deg2Rad);
        float y = force * Mathf.Sin(rot.zRotate * Mathf.Deg2Rad);

        // Chuta quando o mouse e solto na bola (liberaTiro do script Rotacao)
        if(rot.liberaTiro == true)
        {
            bola.AddForce(new Vector2(x,y));
            // Desliga para o chute acontecer uma vez so
            rot.liberaTiro = false;
        }
    }

    // Carrega a forca enquanto a bola esta segurada
    void ControlaForca()
    {
        if(rot.liberaRot == true)
        {
          float moveX = Input.GetAxis ("Mouse X");

          // Arrastar para a esquerda enche a barra, para a direita esvazia
          // A forca vai de 0 a 1000 conforme o preenchimento da seta
          if(moveX < 0)
            {
              seta2Img.fillAmount += 1 * Time.deltaTime;
              force = seta2Img.fillAmount * 1000;  
            }

            if(moveX > 0)
            {
              seta2Img.fillAmount -= 1 * Time.deltaTime;
              force = seta2Img.fillAmount * 1000; 
            }
        }
    }
}
