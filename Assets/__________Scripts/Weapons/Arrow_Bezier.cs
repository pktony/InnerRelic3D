using System.Collections;
using System.Collections.Generic;
using Cinemachine.Utility;
using UnityEngine;

public class Arrow_Bezier : Arrow
{
    private Vector3[] bezierPoints;

    Vector3 startPoint;
    Vector3 target;

    [SerializeField] float timeToTarget = 1.0f;
    [SerializeField] float arrowSpeed = 1.0f;
    [SerializeField] float anchorPositionOffset = 3.0f;

    private float currentTime = 0f;
    private float timer = 0f;

    private bool isHit = false;

    protected override void Awake()
    {
        base.Awake();

        bezierPoints = new Vector3[2];
    }

    protected override void Start()
    {
        InitializeBezierPositions();
    }

    private void FixedUpdate()
    {
        currentTime = timer / timeToTarget;

        timer += Time.fixedDeltaTime * arrowSpeed;

        if(!isHit)
            transform.position = CalculateBezierTrajectory(bezierPoints[0], bezierPoints[1], target);
    }


    private void InitializeBezierPositions()
    {
        startPoint = transform.position;
        Transform _target = GameManager.Inst.ArcherController.LockonTarget;
        if(_target != null)
        {
            target = _target.position + Vector3.up;
        }
        RandomBezierPoints();
    }

    private void RandomBezierPoints()
    {
        bezierPoints[0] = GameManager.Inst.ArcherController.transform.position - transform.forward * 5f + Vector3.up * anchorPositionOffset + anchorPositionOffset * Random.insideUnitSphere ;
        bezierPoints[1] = target + anchorPositionOffset * Random.insideUnitSphere;
    }


    private Vector3 CalculateBezierTrajectory(Vector3 anchorPoint1, Vector3 anchorPoint2, Vector3 _target)
    { // 베지어 곡선 경로 계산
        // https://en.wikipedia.org/wiki/B%C3%A9zier_curve
        // Quadratic Bezier Curve
        //  B(t) = (1-t)^2 P_0 + 2(1-t)tP_1 + t^2P_2   ( 0 <= t <= 1)
        // Cubic Bezier Curve
        //  B(t) = (1-t)^3 P_0 + 3(1-t)^2 tP_1 + 3(1-t)t^2P_2 +  t^3P_3   ( 0 <= t <= 1)

        Vector3 pos = Mathf.Pow(1 - currentTime, 3) * startPoint
            + 3 * Mathf.Pow(1 - currentTime, 2) * currentTime * anchorPoint1
            + 3 * (1 - currentTime) * Mathf.Pow(currentTime, 2) * anchorPoint2
            + Mathf.Pow(currentTime, 3) * _target;

        return pos;
    }

    protected override IEnumerator PlayParticles(ParticleSystem particle)
    {
        isHit = true;
        return base.PlayParticles(particle);
    }
}
