using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKSolverCCD : MonoBehaviour
{
    public IKJoint[] joints;                    //The movable joints
    public Transform effector;                  //The end effector for this IK chain
    public GameObject target;                   //The target for the end effector

    private float distanceThreshold = 0.5f;     //The distance to target threshold to stop updating the IK chain
    private int maxIterations = 100;             //The maximum number of iterations to get to the target

    private int jointCtr = 0;                   //DEBUG, used to calculate a specific joint in the IK chain



    // Update is called once per frame
    void Update()
    {
        InverseKinematics();

        /*
        if( Input.GetKeyDown( KeyCode.Space ) )
        {
            IKStep( jointCtr++ );

            if( jointCtr >= joints.Length )
            {
                jointCtr = 0;
            }
        }
        */
    }

    /// <summary>
    /// Calculates the inverse kinematics for the chain towards a specific target.
    /// </summary>
    private void InverseKinematics()
    {
        int iterations = 0;

        while( Vector3.Distance( effector.position, target.transform.position ) > distanceThreshold && iterations < maxIterations )
        {
            IKStep();
            iterations++;
        }

        Debug.Log( "tgt reached after " + iterations + " iterations, dist " + Vector3.Distance( effector.position, target.transform.position ) );
    }

    /// <summary>
    /// Performs a full cycle through all joints of the chain
    /// </summary>
    private void IKStep( int jointCtr = 0 )
    {
        //IKJoint joint = joints[jointCtr];

        //foreach( IKJoint joint in joints )
        for( int i = 0; i < joints.Length; i++ )
        {
            IKJoint joint = joints[i];

            Vector3 targetProjection = ProjectOnJointPlane( target.transform.position, joint );
            Vector3 effectorProjection = ProjectOnJointPlane( effector.transform.position, joint );

            //DEBUG
            /*
            int lineTime = 30;
            Debug.DrawLine( target.transform.position, targetProjection, Color.blue, lineTime, false );
            Debug.DrawLine( effector.transform.position, effectorProjection, Color.green, lineTime, false );
            Debug.DrawLine( joint.transform.position, targetProjection, Color.yellow, lineTime, false );
            Debug.DrawLine( joint.transform.position, effectorProjection, Color.yellow, lineTime, false );
            Debug.DrawLine( joint.transform.position, effector.transform.position, Color.red, lineTime, false );
            */

            Vector3 jointToTgt = targetProjection - joint.transform.position;
            Vector3 jointToEffector = effectorProjection - joint.transform.position;
            float angle = Vector3.SignedAngle( jointToTgt, jointToEffector, joint.transform.TransformDirection( joint.Axis ) );
            joint.IncreaseRotation( -angle );

            //DEBUG
            /*
            Debug.Log( "joint " + jointCtr );
            Debug.Log( targetProjection );
            Debug.Log( effector.transform.position );
            Debug.Log( joint.transform.position );
            Debug.Log( angle );
            Debug.Log( "---------------------------------------" );
            */
            
        }
    }

    /// <summary>
    /// Projects a certain point onto a plane that runs though a joint, perpendicular to the joint's rotation axis
    /// math summary: https://www.youtube.com/watch?v=r5VCChxnLnQ
    /// </summary>
    private Vector3 ProjectOnJointPlane( Vector3 point, IKJoint joint )
    {
        // - create a vector from the point to the joint
        // - project this vector onto the joint's rotation axis (which we convert to world space first)
        Vector3 projectedPtJ = Vector3.Project( point - joint.transform.position, joint.transform.TransformDirection( joint.Axis ) );

        //subtract this vector from the point to end up with the projected point
        return point - projectedPtJ;
    }
}
