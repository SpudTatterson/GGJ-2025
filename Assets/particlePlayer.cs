using UnityEngine;

public class particlePlayer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] Attacker attacker;
    [SerializeField] AttackerAOE AOE;
    public void playParticles()
    {
        attacker.particles.Play();

    }
     public void AOEplayParticles()
    {
        AOE.particles.Play();

    }


}
