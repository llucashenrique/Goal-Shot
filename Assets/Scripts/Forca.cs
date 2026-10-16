using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
public class Forca : MonoBehaviour
{
    private Rigidbody2D bola;
    // Intensidade do chute
    private float force = 1000f;
    // Usado para ler o angulo (zRotate) da seta
    private Rotacao rot;

    void Start()
    {
       bola = GetComponent<Rigidbody2D> ();
       rot = GetComponent<Rotacao> (); 
    }

    void Update()
    {
        AplicaForca();
    }

    void AplicaForca()
    {
        // Divide a forca em X e Y pelo angulo (Deg2Rad converte graus para radianos)
        float x = force * Mathf.Cos(rot.zRotate * Mathf.Deg2Rad);
        float y = force * Mathf.Sin(rot.zRotate * Mathf.Deg2Rad);

        // Chuta ao soltar a barra de espaco
        if(Input.GetKeyUp(KeyCode.Space))
        {
            bola.AddForce(new Vector2(x,y));
        }
    }
}
