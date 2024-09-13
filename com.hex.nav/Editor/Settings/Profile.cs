using System;
using System.Collections.Generic;

namespace SpaceNavigatorDriver
{
    [Serializable]
    public class Profile
    {
        public string Name = "Profile";
        public OperationMode Mode;
        public CoordinateSystem System;

        public int GearIndex;
        public List<Gear> Gears;
        
        public LockSet Locks;

        public static Profile DefaultProfile0 => new()
        {
            Mode = OperationMode.Fly,
            Name = "Scene Fly",
            Gears = Gear.DefaultGears()
        };
    }
}