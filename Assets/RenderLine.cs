using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderLine : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.DrawLine( Vector3.left, Vector3.right, Color.red, 5f );    
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
