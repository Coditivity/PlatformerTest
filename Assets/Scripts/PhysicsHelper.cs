using UnityEngine;
using System.Collections;

public class PhysicsHelper  {


    public static float GetVelocityToReachHeight(float heightToReach)
    {       
        return Mathf.Sqrt(-2*Physics2D.gravity.y*heightToReach);
    }

    public static void dampVector(ref Vector2 vectorToBeDamped, float dampMag)
    {
        float vecMag = vectorToBeDamped.magnitude; //get the vector magnitude
        vecMag -= dampMag * Time.deltaTime; //damp the magnitude by dampMag * dt
        if (vecMag < 0) //if dampMag went below zero
        {
            vecMag = 0; //set dampMag to zero
        }
        vectorToBeDamped = vectorToBeDamped.normalized * vecMag; //apply the new damped magnitude to the vector
    }

    public static bool CollidingWithSomethingOnRight(Collider2D collider, LayerMask collidableLayersMask)
    {
        Vector3 rightEdgePos = collider.bounds.min;
        rightEdgePos.x = collider.bounds.max.x ;
        return Physics2D.Raycast(rightEdgePos, Vector2.right, int.MaxValue, collidableLayersMask).distance <= .02f;
    }
    public static bool CollidingWithSomethingOnLeft(Collider2D collider, LayerMask collidableLayersMask)
    {        
        return Physics2D.Raycast(collider.bounds.min, Vector2.left, int.MaxValue, collidableLayersMask).distance <= .02f;
    }

    public static bool CollidingWithSomethingOnEitherSide(Collider2D collider, LayerMask collidableLayersMask)
    {
        return CollidingWithSomethingOnRight(collider, collidableLayersMask)
            || CollidingWithSomethingOnLeft(collider, collidableLayersMask);
    }

    public static float GetObjectToGroundDistance(Collider2D collider, LayerMask collidableLayersMask )
    {
        Vector3 leftEdgePos = collider.bounds.min;
        leftEdgePos.x -= 1f;
        RaycastHit2D rayHitLeftEdge = Physics2D.Raycast(leftEdgePos, Vector2.down, int.MaxValue, collidableLayersMask);
        Vector3 rightEdgePos = collider.bounds.min;
        rightEdgePos.x = collider.bounds.max.x + 1f;
        RaycastHit2D rayHitRightEdge = Physics2D.Raycast(rightEdgePos, Vector2.down, int.MaxValue, collidableLayersMask);        
        return Mathf.Min(rayHitLeftEdge.distance, rayHitRightEdge.distance);
    }

    /// <summary>
    /// Damps a value (positive or negative) towards zero.
    /// </summary>
    /// <param name="valueToBeDamped"></param>
    /// <param name="dampMag"></param>
    /// <returns>True if the value has reached zero</returns>
    public static bool dampFloat(ref float valueToBeDamped, float dampMag)
    {
        if(valueToBeDamped<0)
        {
                
            valueToBeDamped += dampMag * Time.deltaTime;
            if(valueToBeDamped>=0)
            {
                valueToBeDamped = 0;
                return true;
            }
        }
        else
        {
            valueToBeDamped -= dampMag * Time.deltaTime;
            if (valueToBeDamped <=0)
            {
                valueToBeDamped = 0;
                return true;
            }

        }
        return false;
             
    }
}
