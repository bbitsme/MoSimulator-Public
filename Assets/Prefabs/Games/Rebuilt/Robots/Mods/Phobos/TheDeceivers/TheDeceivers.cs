using Games.Rebuilt.Robots;
using MoSimLib;
using RobotFramework.Components;
using RobotFramework.Controllers.PidSystems;
using RobotFramework.Enums;
using UnityEngine;

public class TheDeceivers : RebuiltTurretRobot
{
    // Joints and rollers: add serialized turret / hood / intake / shooter references here.

    [Header("Joints")] 
    [SerializeField] private GenericTurret turret;
    [SerializeField] private GenericJoint hood;
    
    [Header("PID")] 
    [SerializeField] private PidConstants turretPID;
    [SerializeField] private PidConstants hoodPid;

    [Header("Hood Aiming (degrees, 0-5)")]
    [SerializeField] private AnimationCurve hubAngleByDistance =
        AnimationCurve.Linear(1.5f, 0f, 6f, 5f);
    [SerializeField] private AnimationCurve passAngleByDistance =
        AnimationCurve.Linear(1.5f, 0f, 10f, 5f);

    private bool passing;
    private readonly System.Collections.Generic.List<Transform> nodes =
        new System.Collections.Generic.List<Transform>();

    // PIDs and mechanism values: add serialized tuning fields here.

    // Runtime state: add mechanism targets and toggles here.
    // swmRef, swmSettings, rb, and driveController are inherited.
    private float hoodTarget;

    protected override void Init()
    {
        base.Init();
        turret.SetPid(turretPID);
        hood.SetPid(hoodPid);
        hoodTarget = 0;

        Scheduler.AddLateRecursive(UpdatePid);
        Scheduler.AddSubsystems(UpdateJoints);
        Scheduler.AddSubsystems(Shooter);
        Scheduler.AddProtectedSystem(UpdateTarget);
    }

    private void UpdatePid()
    {
        // TODO: Update each joint's PID constants for live tuning.
        hood.UpdatePid(hoodPid);

    }

    private void UpdateJoints()
    {
        // TODO: Apply mechanism targets to the joints.
        turret.Align(RotateAction.ReadValue<float>());
        float err = turret.GetError();
        // Debug.Log($"turret error: {err}");
    }

    protected override void HandleSwm()
    {
        // Assign the inherited swmRef (with a parent) and swmSettings in the Inspector.
        // SOTMSettings is a struct; only the Rigidbody and Transform references can be null.
        if (rb != null && swmRef != null && swmRef.parent != null)
        {
            base.HandleSwm();
        }
    }

    protected override void AllianceZone()
    {
        // TODO: Aim the turret / hood at the hub and choose shooter speed.
        // Debug.Log("AllianceZone called");
        passing = false;
        turret.SetDefaultMap(GenericTurret.DefaultMap.Hub);
    }

    protected override void NeutralZone()
    {
        // TODO: Choose a pass target, turret / hood targets, and shooter speed.
        // Debug.Log("NeutralZone called");
        passing = true;
        turret.SetPassMapClosest();
    }

    protected override void Intaking()
    {
        // TODO: Run the intake and indexer without feeding the shooter.
    }

    protected override void IntakingAndShooting()
    {
        // TODO: Run the intake, indexer, and shooter feed together.
    }

    protected override void Shooting()
    {
        // TODO: Feed fuel into the shooter.
        
    }

    private float GetTargetDistance()
    {
        if (nodes.Count == 0)
        {
            int layer = LayerMask.NameToLayer("AutoAngleNodes");
            foreach (Transform t in FindObjectsOfType<Transform>())
                if (t.gameObject.layer == layer) nodes.Add(t);
        }
    
        string alliance = gameObject.CompareTag("BlueRobot") ? "Blue" : "Red";
        string kind = passing ? "Pass" : "Hub";
    
        Vector3 me = hood.transform.position; me.y = 0;
        float best = -1f;
    
        foreach (Transform n in nodes)
        {
            if (n == null) continue;
            if (!n.name.Contains(kind) || !n.name.Contains(alliance)) continue;
            Vector3 p = n.position; p.y = 0;
            float d = Vector3.Distance(me, p);
            if (best < 0f || d < best) best = d;
        }
        return best < 0f ? 0f : best;
    }


    private void Shooter()
    {
        if (OuttakeAction)
        {
            AnimationCurve curve = passing ? passAngleByDistance : hubAngleByDistance;
            hoodTarget = Mathf.Clamp(curve.Evaluate(GetTargetDistance()), 0f, 5f);
        }
        else
        {
            hoodTarget = 0;
        }
    }

    private void UpdateTarget()
    {
      hood.SetTargetAngle(hoodTarget).withAxis(JointAxis.Z);
    }  

    protected override void Idling()
    {
        // TODO: Stop feeding and choose idle roller behavior.
    }

    protected override bool Aligned()
    {
        // TODO: Return whether turret / hood alignment and flywheel speed permit shooting.
        return Mathf.Abs(turret.GetError()) < 5;
    }
}
