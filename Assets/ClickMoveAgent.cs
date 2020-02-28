using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ClickMoveAgent : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if( Input.GetMouseButton( 0 ) )
        {
            var randomPosition = Random.insideUnitCircle * 10f;

            navMeshAgent.SetDestination( new Vector3( randomPosition.x, 0f, randomPosition.y ) );
        }
    }
}
