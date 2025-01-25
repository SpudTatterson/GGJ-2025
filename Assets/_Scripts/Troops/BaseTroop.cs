using UnityEngine;
using UnityEngine.AI;

public abstract class BaseTroop : MonoBehaviour
{
    [SerializeField] protected Damageable healthSystem;
    [SerializeField] protected NavMeshAgent agent;

    public Animator[] animators { get; protected set; }


    protected virtual void Awake()
    {
        animators = GetComponentsInChildren<Animator>();
    }
    public void SendToPosition(Vector3 position)
    {
        agent.SetDestination(position);
    }
}
