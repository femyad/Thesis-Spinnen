using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class IKJoint : MonoBehaviour
{
    [SerializeField]
    private Vector3 axis;
    [SerializeField]
    private float minAngle;
    [SerializeField]
    private float maxAngle;
    [SerializeField]
    private float damping = 1f;    //the damping factor for the joint. 1 means no damping, 0 means full damping


    private Vector3 defaultPos;
    private float defaultAngle;

    public bool UseComfort = false;
    public float ComfortAngle = 0f;

    #region Properties
    //------------------------------------------------------------------------------------------------------------------------------
    //Properties to access attributes
    
    public Vector3 Axis 
    {
        get
        {
            return axis;
        }
    }

    public Vector3 DefaultPos
    {
        get
        {
            return defaultPos;
        }
    }
    public float DefaultAngle
    {
        get
        {
            return defaultAngle;
        }
    }


    public float MinAngle
    {
        get
        {
            return minAngle;
        }
    }

    public float MaxAngle
    {
        get
        {
            return maxAngle;
        }
    }

    public float Damping
    {
        get
        {
            return damping;
        }
    }

    public float CurAngle
    {
        get
        {
            float curAngle = transform.localEulerAngles.x;
            if ( curAngle == 0 ) { curAngle = transform.localEulerAngles.y; }
            if ( curAngle == 0 ) { curAngle = transform.localEulerAngles.z; }
            if ( curAngle > 180 ) { curAngle -= 360; } //convert to [-180,180]
            return curAngle;
        }
    }

    //------------------------------------------------------------------------------------------------------------------------------
    #endregion

    /// <summary>
    /// Set default position and angle
    /// </summary>
    private void Awake()
    {
        defaultPos = transform.localPosition;
        float defaultAngle = transform.localEulerAngles.x;
        if ( defaultAngle == 0 ) { defaultAngle = transform.localEulerAngles.y; }
        if ( defaultAngle == 0 ) { defaultAngle = transform.localEulerAngles.z; }
        if ( defaultAngle > 180 ) { defaultAngle -= 360; }
    }

    /// <summary>
    /// Sets the local rotation of the joint on its axis to a certain angle
    /// </summary>
    public void UpdateRotation( float angle )
    {
        transform.localEulerAngles = axis * angle;
    }
    
    /// <summary>
    /// Adds a certain value to the rotation of the joint on its axis
    /// </summary>
    public void IncreaseRotation( float angle )
    {
        //Debug.Log( transform.localEulerAngles );
        //Debug.Log( "adding " + angle + " degrees to " + curRotation + ", clamped to " + Mathf.Clamp( curRotation + angle, minAngle, maxAngle ) );

        //add new angle to current rotation (clamped to min and max rotation values)
        transform.localEulerAngles = Mathf.Clamp( CurAngle + angle * damping, minAngle, maxAngle ) * axis;
        //transform.Rotate( axis, angle, Space.Self );
    }

    public void RefreshDefaultPos()
    {
        defaultPos = transform.localPosition;
    }

  


}
