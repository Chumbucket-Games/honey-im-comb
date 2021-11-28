using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants
{
    public const float HiveUnitOffset = -3.7f;
    public const float OverworldUnitOffset = 3f;
    public const int UnitScanLayerMask = Physics.DefaultRaycastLayers | (1 << Layers.Selectables);
    public struct Scenes
    {
        public const int MainMenu = 0;
        public const int TheHive = 1;
        public const int Credits = 2;
    }

    public struct Animations
    {
        public const string BeeFlying = "Flying";
        public const string BeeMoving = "Moving";
        public const string BeeAttacking = "Attack";
        public const string EnemyFlying = "Flying";
        public const string EnemyMoving = "Moving";
        public const string EnemyAttacking = "Attack";
        public const string BeeAttackSpeed = "Attack Speed";
    }

    public struct Tags
    {
        public const string Hive = "Hive";
        public const string Building = "Building";
        public const string Unit = "Unit";
        public const string ResourceNode = "ResourceNode";
    }

    public struct Layers
    {
        public const int Selectables = 6;
    }
}
