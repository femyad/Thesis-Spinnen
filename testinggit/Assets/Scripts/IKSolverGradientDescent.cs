using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKSolverGradientDescent : MonoBehaviour
{
    public IKJoint[] joints;
    public GameObject target;                   //The target for the end effector

    private float samplingDistance = 2f;        //The amount to update each angle in the chain when searching for the minimum
    private float learningRate = 2f;           //The speed at which to update the final angles
    private float distanceThreshold = 1f;       //The distance to target threshold to stop updating the IK chain
    private int maxIterations = 100;             //The maximum number of iterations to get to the target

    private float[] angles; //= new float[] { 26, 90, -69, 95, 0, 62, -48, 0 };   //the current angles of the joins in this chain
    

    /// <summary>
    /// Initialize angles for this IK chain
    /// </summary>
    private void Start()
    {
        angles = new float[joints.Length];
        for( int i = 0; i < joints.Length; i++ )
        {
            angles[i] = joints[i].CurAngle;
        }
    }

    private void Update()
    {
        if ( target != null )
        {
            InverseKinematics( target.transform.position, angles );
        }
    }

    /// <summary>
    /// Calculates the position (in world coordinates) of the end effector based on an array of input angles for the joints.
    /// Note: this does not actually reposition the joints
    /// </summary>
    private Vector3 ForwardKinematics( float[] angles )
    {
        Vector3 prevPoint = joints[0].transform.position; //world coordinates
        Quaternion rotation = joints[0].transform.parent.rotation; //start with the rotation of the parent node of the IK chain

        for ( int i = 1; i < joints.Length; i++ )
        {
            //Debug.Log( prevPoint + " || " + joints[i-1].transform.position + " || " + joints[i].DefaultPos );

            rotation *= Quaternion.AngleAxis( angles[i - 1], joints[i - 1].Axis );
            Vector3 nextPoint = prevPoint + rotation * joints[i].DefaultPos;

            prevPoint = nextPoint;
        }

        //Debug.Log( prevPoint + " || " + joints[joints.Length-1].transform.position );

        return prevPoint;
    }

    /// <summary>
    /// The error function to be used in the gradient descent. This contains two aspects:
    /// - the distance between the end effector of the chain and a certain target
    /// - the comfort level of the leg. For now, this tries to keep the trochanter rotation as close to 90 as possible
    /// </summary>
    private float ErrorFunction( Vector3 target, float[] angles )
    {
        //Calculate distance
        float distance = TargetDistance( target, angles );

        //Calculate leg comfort level (trochanter angle stored at index 1)
        float comfort = LegComfort( angles );

        //Debug.Log( "target distance = " + distance + " | leg comfort = " + comfort );

        return distance + comfort;
    }

    /// <summary>
    /// Calculates the distance to the target
    /// </summary>
    private float TargetDistance( Vector3 target, float[] angles )
    {
        return Vector3.Distance( ForwardKinematics( angles ), target );
    }

    /// <summary>
    /// Calculates the comfort level of the leg. This uses a couple of assumptions:
    /// - the trochanter rotation should be as close to 90 degrees as possible, and if it does have to rotate, it should rotate outward (>90)
    /// - the tibia prefers inward rotation (> 0)
    /// </summary>
    private float LegComfort( float[] angles )
    {
        /*
        float error = 0;

        for( int i = 0; i < angles.Length; i++ )
        {
            float angle = angles[i];
            IKJoint joint = joints[i];

            if( angle < joint.DefaultAngle )
            {
                if ( joint.DefaultAngle != joint.MinAngle )
                {
                    error += 1 - Mathf.Abs( angle - joint.MinAngle ) / ( joint.DefaultAngle - joint.MinAngle );
                }
            }
            else
            {
                if ( joint.DefaultAngle != joint.MaxAngle )
                {
                    error += 1 - Mathf.Abs( joint.MaxAngle - angle ) / ( joint.MaxAngle - joint.DefaultAngle );
                }
            }
        }

        return error;
        */

        float trochanterComfort = Mathf.Abs( 90 - angles[1] ) / 50;
        if( angles[1] < 90 )
        {
            trochanterComfort *= 10;
        }

        float tibiaComfort = 1 - ( 35 - Mathf.Abs(angles[4]) );

        //Debug.Log( "leg comfort: " + trochanterComfort + " | " + tibiaComfort );

        return trochanterComfort + tibiaComfort;
    }

    /// <summary>
    /// Estimates the partial gradient for the ith joint
    /// </summary>
    private float PartialGradient( Vector3 target, float[] angles, int i )
    {
        //save
        float angle = angles[i];

        //calculate gradient : [f(x+samplingDistance) - f(x)] / h
        float f_x = ErrorFunction( target, angles );
        angles[i] += samplingDistance;
        float f_xd = ErrorFunction( target, angles );

        float gradient = ( f_xd - f_x ) / samplingDistance;

        //restore
        angles[i] = angle;

        return gradient;
    }

    /// <summary>
    /// Calculates the inverse kinematics for the chain towards a specific target. 
    /// </summary>
    private void InverseKinematics( Vector3 target, float[] angles )
    {
        int curIteration = 1;
        float curDist = TargetDistance( target, angles );

        //Only do something if the effector has not yet reached the target
        if ( curDist > distanceThreshold )
        {
            while ( curDist > distanceThreshold && curIteration <= maxIterations )
            {
                //Debug.Log( curIteration + ": dist = " + curDist + " to tgt " + target );

                //Calculate the gradient in each joint and update the angles. We iterate back-to-front to update the most outward joints first
                for ( int i = joints.Length - 1; i >= 0; i-- )
                //for ( int i = 0; i < joints.Length; i++ )
                {
                    float gradient = PartialGradient( target, angles, i );
                    angles[i] -= learningRate * gradient;

                    //Clamp to joint constraints
                    angles[i] = Mathf.Clamp( angles[i], joints[i].MinAngle, joints[i].MaxAngle );

                    //Update iteration counter to prevent the calculation from looping too long
                    curIteration++;
                }
            }

            //Update the joint angles
            Debug.Log( "----> FINAL RESULT" );
            for ( int i = 0; i < joints.Length; i++ )
            {
                Debug.Log( angles[i] );
                joints[i].UpdateRotation( angles[i] );
            }
        }
    }


    #region Debug methods
    //------------------------------------------------------------------------------------------------------------------------------

    private string arrayToString( float[] someArray )
    {
        string result = "[ ";

        for( int i = 0; i < someArray.Length; i++ )
        {
            result += someArray[i];
            if( i < someArray.Length - 1 )
            {
                result += ", ";
            }
        }
        result += " ]";

        return result;
    }

    //------------------------------------------------------------------------------------------------------------------------------
    #endregion
}
