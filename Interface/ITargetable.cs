using UnityEngine;

public interface ITargetable 
{
   Transform Transform { get; }
   Vector3 AimPoint { get; }
   
   bool IsAlive { get; }
}
