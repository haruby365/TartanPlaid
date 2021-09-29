// © 2021 Jong-il Hong

using System;
using Newtonsoft.Json;

namespace Haruby.TartanPlaid
{
    public class TartanSettings
    {
        public static readonly TartanSettings Default = new(10, 3);

        private TartanSettings()
        {

        }
        public TartanSettings(int unitWidth, int repeat)
        {
            UnitWidth = Math.Max(1, unitWidth);
            Repeat = Math.Max(1, repeat);
        }

        [JsonProperty]
        public int UnitWidth { get; private set; }
        [JsonProperty]
        public int Repeat { get; private set; }
    }
}
