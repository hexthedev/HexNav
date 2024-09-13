using System;
using System.Collections.Generic;

namespace SpaceNavigatorDriver
{
    [Serializable]
    public class Gear
    {
        public string Name = "Gear";
        public Sensitivity Translation;
        public Sensitivity Rotation;

        public static List<Gear> DefaultGears()
        {
            return new()
            {
                new Gear()
                {
                    Name = "Miniscule",
                    Translation = new Sensitivity() { Min = 0.01f, Value = 0.05f, Max = 1 },
                    Rotation = new Sensitivity() { Min = 0, Value = 1, Max = 5 },
                },
                new Gear()
                {
                    Name = "Human",
                    Translation = new Sensitivity() { Min = 0.1f, Value = 1, Max = 10 },
                    Rotation = new Sensitivity() { Min = 0, Value = 1, Max = 5 },
                },
                new Gear()
                {
                    Name = "Massive",
                    Translation = new Sensitivity() { Min = 1, Value = 50, Max = 100 },
                    Rotation = new Sensitivity() { Min = 0, Value = 1, Max = 5 },
                }
            };
        }
    }
}