using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    protected Rigidbody rigid;
    private GameObject model;
    protected ParticleSystem hitParticle;
    protected ParticleSystem groundHitParticle;

    protected virtual void Awake()
    {
        rigid = GetComponent<Rigidbody>();

        model = transform.GetChild(0).gameObject;
        hitParticle = transform.GetChild(1).GetComponent<ParticleSystem>();
        groundHitParticle = transform.GetChild(2).GetComponent<ParticleSystem>();
    }

    protected virtual void Start()
    {
        rigid.AddForce(GameManager.Inst.ArcherController.CurrentVelocity.magnitude * transform.forward, ForceMode.Impulse);
    }

    private void FixedUpdate()
    {
        if(rigid.velocity.sqrMagnitude > 0)
            rigid.MoveRotation(Quaternion.LookRotation(rigid.velocity));
    }


    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Ground"))
        {
            StartCoroutine(PlayParticles(groundHitParticle));
        }
        else if (collider.CompareTag("Enemy"))
        {
            StartCoroutine(PlayParticles(hitParticle));
        }
        else
            Destroy(this.gameObject);


    }

    protected virtual IEnumerator PlayParticles(ParticleSystem particle )
    {
        model.SetActive(false);
        rigid.velocity = Vector3.zero;
        particle.Play();
        yield return new WaitForSeconds(particle.main.duration);
        Destroy(this.gameObject);
    }
}
