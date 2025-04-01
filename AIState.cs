using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "AIState")]
public class AIState : SerializedScriptableObject
{
    public enum MovementType
    {
        Charge,
        StopWhenClose, //AKA don't back up if too close
        Orbit,
        KeepDistance, //DO back up if too close
        Stay
    }
    public enum AngleType
    {
        TowardsPlayer,
        Maintain,
        MoveDirection
    }

    public enum AttackPattern
    {
        WhileInRange,
        NoAttack
    }

    public enum Transition
    {
        InSight,
        OutOfSight
    }

    public enum TargetPlayer
    {
        Nearest,
        Furthest,
        Random
    }

    public Transition transition;
    public TargetPlayer targetPlayer = TargetPlayer.Nearest;
    [Tooltip("Need line of sight to aggro onto an enemy")]
    public bool needLineOfSight = true;
    public float retargetTime = 5f;
    public float maxTargetDistance = 10f;
    [Tooltip("Set for starting states. This means they won't leave state if outside maxTargetDistance")]
    public bool dontTransitionWithoutTarget = false;

    [Space(20)]
    public MovementType movementType;
    public float distance = 2;

    [Space(20)]
    public AngleType angleType;
    public float angleOffset;

    [Space(20)]
    public AttackPattern pattern;
    [HideIf("@pattern==AttackPattern.NoAttack")]
    public float range = 5;
    [HideIf("@pattern==AttackPattern.NoAttack")]
    public int attack = 1;
    [HideIf("@pattern==AttackPattern.NoAttack")]
    public float minAttackAngle = 20f;
    [HideIf("@pattern==AttackPattern.NoAttack")]
    public bool allowReverseAngle = false;
    [HideIf("@pattern==AttackPattern.NoAttack")]
    public float attackDelay = 2;
    [Tooltip("Use this if enemy can be easily circle camped. Around 1f should be enough.")]
    public float oversteer = 1;

    [Space(20)]
    public float angularSpeed = 10f;
    public float velocityMult = 1f;
    public bool freezeTurnDuringAttack = true;
    public bool freezeMoveDuringAttack = true;
    [Space(20)]
    public float stayOnStateTime = 5f;
    

    public Vector3 GetPoint(Ship ship, Ship playerShip)
    {
        if (playerShip == null)
            return ship.transform.position;

        switch (movementType)
        {
            case MovementType.Charge:
                return playerShip.transform.position;
            case MovementType.StopWhenClose:
                if (Vector2.Distance(ship.transform.position, playerShip.transform.position) > distance)
                {
                    return playerShip.transform.position;
                }
                return ship.transform.position;
            case MovementType.KeepDistance:
                float kdDist = Vector2.Distance(ship.transform.position, playerShip.transform.position);
                if ( kdDist> distance)
                {
                    return playerShip.transform.position;
                }
                else if ( kdDist > distance * .92f)
                {
                    return ship.transform.position;
                }
                Vector3 direction = (ship.transform.position - playerShip.transform.position).normalized;

                return playerShip.transform.position + direction * distance;

            case MovementType.Orbit:
                float distO = Vector3.Distance(ship.transform.position, playerShip.transform.position);
                if (distO > distance)
                {
                    return playerShip.transform.position;
                }
                if (distO < distance / 2f)
                {
                    Vector2 directionOD = (ship.transform.position - playerShip.transform.position).normalized;

                    return (Vector2) playerShip.transform.position + directionOD * distance;
                }
                Vector2 directionOrbit = (ship.transform.position - playerShip.transform.position).normalized;
                Vector2 tangent = new(-directionOrbit.y, directionOrbit.x);

                return (Vector2) ship.transform.position + tangent;

            case MovementType.Stay:
                return ship.transform.position;
            default:
                Debug.Log("Unaccounted state: " + movementType.ToString());
                return ship.transform.position;
        }

    }

    public Ship GetTarget(Ship ship, EnemyAI ai)
    {
        if (Ship.PlayerShips.Count == 0)
            return null;

        switch (targetPlayer)
        {
            case TargetPlayer.Furthest:
                Ship f = null;
                float fdist = 0f;
                foreach (Ship s in Ship.PlayerShips)
                {
                    if (s.playerDisabled)
                        continue;
                    if (needLineOfSight && !Node.IsInLOS(s.transform.position, ship.transform.position))
                        continue;
                    if (s.isResetting)
                        continue;

                    float d = Vector2.Distance(ship.transform.position, s.transform.position);

                    if (d > maxTargetDistance)
                        continue;

                    if (d > fdist)
                    {
                        fdist = d;
                        f = s;
                    }
                }
                if (f == null)
                    return ai.target;
                return f;
            case TargetPlayer.Nearest:
                return GetNearestPlayer(needLineOfSight, ship, maxTargetDistance, ai);
            case TargetPlayer.Random:
            default:
                return Ship.PlayerShips[Random.Range(0, Ship.PlayerShips.Count)];
        }
    }
    public static Ship GetNearestPlayer(bool needLineOfSight, Ship ship, float maxTargetDistance, EnemyAI ai)
    {
        Ship n = null;
        float ndist = float.PositiveInfinity;
        foreach (Ship s in Ship.PlayerShips)
        {
            if (s.playerDisabled)
                continue;
            if (needLineOfSight && !Node.IsInLOS(s.transform.position, ship.transform.position))
                continue;

            float d = Vector2.Distance(ship.transform.position, s.transform.position);

            if (d > maxTargetDistance)
                continue;

            if (d < ndist)
            {
                ndist = d;
                n = s;
            }
        }
        if (n == null)
            return ai.target;
        return n;
    }

    public bool ShouldGetNewTarget(EnemyAI ai, bool assignFloats = true)
    {
        if (ai.target == null)
        {
            if (assignFloats)
                ai.tempFloats["TARGET_CD"] = Time.time + retargetTime;
            return true;
        }
            

        if (ai.tempFloats.ContainsKey("TARGET_CD"))
        {
            if (ai.tempFloats["TARGET_CD"] < Time.time)
            {
                if (assignFloats)
                    ai.tempFloats["TARGET_CD"] = Time.time + retargetTime;
                return true;
            }
            return false;
        }
        if (assignFloats)
            ai.tempFloats["TARGET_CD"] = Time.time + retargetTime;
        return true;
    }

    public float GetAngle(Ship ship, Ship playerShip, AngleType? overrideType = null)
    {
        if (playerShip == null)
            return ship.transform.eulerAngles.z + angleOffset;
        AngleType type = angleType;
        if (overrideType.HasValue)
        {
            type = overrideType.Value;
        }
        float angle;
        switch (type)
        {
            case AngleType.Maintain:
                angle = ship.transform.eulerAngles.z + angleOffset;
                break;
            case AngleType.TowardsPlayer:
                Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, (Vector2)playerShip.transform.position - (Vector2)ship.transform.position);
                targetRotation = Quaternion.Euler(0f, 0f, targetRotation.eulerAngles.z + angleOffset);
                bool reverse = false;
                if (allowReverseAngle)
                {
                    Quaternion targetRotation2 = Quaternion.LookRotation(Vector3.forward, (Vector2)ship.transform.position - (Vector2)playerShip.transform.position);
                    targetRotation2 = Quaternion.Euler(0f, 0f, targetRotation2.eulerAngles.z + angleOffset);
                    if (Quaternion.Angle(targetRotation, ship.transform.rotation) > Quaternion.Angle(targetRotation2, ship.transform.rotation))
                    {
                        targetRotation = targetRotation2;
                        reverse = true;
                    }
                }

                angle = targetRotation.eulerAngles.z;

                if (oversteer > 0f)
                {
                    float current = playerShip.transform.rotation.z;
                    if (reverse)
                    {
                        current += 180;

                    }
                    
                    current %= 360;
                    if ((angle - current) % 360 > (current - angle) % 360)
                        angle -= oversteer;
                    else
                        angle += oversteer;

                }


                
                break;
            case AngleType.MoveDirection:
                if (ship.rb.velocity.sqrMagnitude > 0f)
                    angle = angleOffset + Vector2.SignedAngle(Vector2.up, ship.rb.velocity);
                else
                    angle = ship.transform.eulerAngles.z + angleOffset;
                break;
            default:
                Debug.Log("Unaccounted state: " + angleType.ToString());
                angle = ship.transform.eulerAngles.z + angleOffset;
                break;

        }


        return angle;
    }

    public bool ShouldAttack(Ship ship, Ship playerShip, EnemyAI ai)
    {
        if (ai.tempFloats.ContainsKey("CD") && ai.tempFloats["CD"] > Time.time)
        {
            return false;
        }
        if (playerShip == null)
            return false;
        if (!Node.IsInLOS(ship.transform.position, playerShip.transform.position))
            return false;
        float offAngle = GetAngle(ship, playerShip, AngleType.TowardsPlayer);


        float angle = Vector2.Angle(Quaternion.Euler(0f,0f, offAngle) * Vector3.right, ship.transform.right);
        
        if ( angle  > minAttackAngle && (Mathf.Abs(angle - (minAttackAngle + 180)) > minAttackAngle && allowReverseAngle) )
        {
            return false;
        }

        switch (pattern)
        {
            case AttackPattern.WhileInRange:
                return Vector2.Distance(ship.transform.position, playerShip.transform.position) < range;
            case AttackPattern.NoAttack:
                return false;
            default:
                return false;
        }
    }

    public void ActuallyAttack(EnemyAI ai)
    {
        ai.tempFloats["CD"] = Time.time + attackDelay;
    }

    public bool ShouldLeaveState(EnemyAI ai)
    {
        if (dontTransitionWithoutTarget && ai.target == null)
            return false;
        return true;
    }
}
