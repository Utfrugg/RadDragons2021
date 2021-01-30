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
    private int castHash;

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
        castHash = Animator.StringToHash("A_GenericCast01");
    }

    // Update is called once per frame
    void Update()
    {

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

    public void StartCastAnim()
    {
        animator.Play(castHash);
    }

    public void AttackSuccess()
    {
        //OnHitEvent.Invoke();
    }

}
