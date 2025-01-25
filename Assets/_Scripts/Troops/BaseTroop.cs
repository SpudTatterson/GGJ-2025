using UnityEngine;
using UnityEngine.AI;

public abstract class BaseTroop : MonoBehaviour
{
    [SerializeField] protected Damageable healthSystem;
    [SerializeField] protected NavMeshAgent agent;
    

    public void SendToPosition(Vector3 position)
    {
        agent.SetDestination(position);
    }
}
