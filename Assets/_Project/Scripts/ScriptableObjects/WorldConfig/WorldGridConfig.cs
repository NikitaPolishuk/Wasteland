using System;
using UnityEngine;

namespace Assets.ScriptableObjects
{
    [CreateAssetMenu(fileName = "WorldGridConfig", menuName = "Configs/WorldGridConfig")]
    public class WorldGridConfig : ScriptableObject
    {
        [Header("Tiles")] public RuleTile GroundRuleTile;

        [Header("World Settings")] 
        public int Width = 50;
        public int GroundHeight = 4;
        public int GroundY = 0;
    }
    
    
}
