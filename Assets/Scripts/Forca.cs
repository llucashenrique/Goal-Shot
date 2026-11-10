using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
public class Forca : MonoBehaviour
{
    // Avisa o GerenciadorFase que um chute aconteceu (para contar a tentativa)
    public static event System.Action AoChutar;

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
            // Sem forca carregada nao conta como chute
            if(force > 0)
            {
                bola.AddForce(new Vector2(x,y));
                AoChutar?.Invoke();
            }
            // Desliga para o chute acontecer uma vez so
            rot.liberaTiro = false;
        }
    }

    // Zera a forca e esvazia a seta (usado quando a bola volta para o inicio)
    public void Resetar()
    {
        force = 0;
        seta2Img.fillAmount = 0;
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
