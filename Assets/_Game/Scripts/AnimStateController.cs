using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class AnimStateController : MonoBehaviour
{
    // Initialize
    private Animator animator;


    // Hash
    private int velocityHash;
    private int diggingHash;
    private int deathHash;
    private int jumpHash;

    public ParticleSystem DigParticleSystem;
    public GameObject digDirtPile;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Increase performance
        velocityHash = Animator.StringToHash("Velocity");
        diggingHash = Animator.StringToHash("A_Digging");
        deathHash = Animator.StringToHash("A_GenericDeath01");
        jumpHash = Animator.StringToHash("A_Jump");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CreateDiggyHoley()
    {
        Instantiate(digDirtPile, transform.position, transform.rotation);
    }

    public void DoDigParticlesPlay()
    {
        DigParticleSystem.Play(true);
    }

    public void UpdateMoveAnim(float vel)
    {
        animator.SetFloat(velocityHash, vel);
    }

    public void StartDiggingAnim()
    {
        animator.Play(diggingHash);
    }

    public void StartDeathAnim()
    {
        animator.Play(deathHash);
    }

    public void StartJumpAnim()
    {
        animator.Play(jumpHash);
    }

    public void AttackSuccess()
    {
        //OnHitEvent.Invoke();
    }

}
