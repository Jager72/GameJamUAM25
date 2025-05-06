using TMPro.Examples;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayParticleOnKey : MonoBehaviour
{
    public ParticleSystem particle;
    private PlayerInput playerInput;

    public void Update()
    {   
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            particle.Play();
            Debug.Log("Particle played!");
        }
    }
}
