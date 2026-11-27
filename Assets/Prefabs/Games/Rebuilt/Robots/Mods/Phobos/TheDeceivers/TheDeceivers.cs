using Games.Rebuilt.Robots;
using Games.Rebuilt.Components;
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
    [SerializeField] private GenericJoint intakeSlide;
    
    [Header("PID")] 
    [SerializeField] private PidConstants turretPID;
    [SerializeField] private PidConstants hoodPid;
    [SerializeField] private PidConstants intakeSlidePid;

    [Header("Hood Interpolation")]
    [SerializeField] private RebuiltInterpolation allianceZoneHoodInterpolation;
    [SerializeField] private RebuiltInterpolation passZoneHoodInterpolation;
    // PIDs and mechanism values: add serialized tuning fields here.

    // Runtime state: add mechanism targets and toggles here.
    // swmRef, swmSettings, rb, and driveController are inherited.
    private float hoodTarget;
    private float intakeTarget;

    protected override void Init()
    {
        base.Init();
        turret.SetPid(turretPID);
        hood.SetPid(hoodPid);
        intakeSlide.SetPid(intakeSlidePid);
        hoodTarget = 0f;
        intakeTarget = -12;

        allianceZoneHoodInterpolation.Initialize(this);
        passZoneHoodInterpolation.Initialize(this);

        Scheduler.AddLateRecursive(UpdatePid);
        Scheduler.AddSubsystems(UpdateJoints);
        Scheduler.AddProtectedSystem(UpdateTarget);
    }

    private void UpdatePid()
    {
        // TODO: Update each joint's PID constants for live tuning.
        hood.UpdatePid(hoodPid);
        intakeSlide.UpdatePid(intakeSlidePid);
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
        turret.SetDefaultMap(GenericTurret.DefaultMap.Hub);
        allianceZoneHoodInterpolation.SetDefaultMap(DefaultMap.Hub);

        if (OuttakeAction)
        {
            hoodTarget = allianceZoneHoodInterpolation.Interpolate();
        }
        else
        {
            hoodTarget = 0;
        }
    }

    protected override void NeutralZone()
    {
        // TODO: Choose a pass target, turret / hood targets, and shooter speed.
        // Debug.Log("NeutralZone called");
        turret.SetPassMapClosest();
        passZoneHoodInterpolation.SetPassMapClosest();

        if (OuttakeAction)
        {
            hoodTarget = passZoneHoodInterpolation.Interpolate();
        }
        else
        {
            hoodTarget = 0f;
        }
    }

    protected override void Intaking()
    {
        // TODO: Run the intake and indexer without feeding the shooter.
        if (IntakeAction)
        {
            intakeTarget = 0.02f;
        }
    }

    protected override void IntakingAndShooting()
    {
        // TODO: Run the intake, indexer, and shooter feed together.
    }

    protected override void Shooting()
    {
        // TODO: Feed fuel into the shooter.
        
    }

    private void UpdateTarget()
    {
      hood.SetTargetAngle(hoodTarget).withAxis(JointAxis.Z);
      intakeSlide.SetLinearTarget(intakeTarget).withAxis(JointAxis.Z).flipDirection();
    }  

    protected override void Idling()
    {
        // TODO: Stop feeding and choose idle roller behavior.
        hoodTarget = 0f;
    }

    protected override bool Aligned()
    {
        // TODO: Return whether turret / hood alignment and flywheel speed permit shooting.
        return Mathf.Abs(turret.GetError()) < 5;
    }
}
