using Landis.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Landis.Library.PnETCohorts
{
    public static class Globals
    {
        public static ICore ModelCore;
        public static ushort IMAX { get; private set; }
        public static readonly object CWDThreadLock = new object();
        public static readonly object litterThreadLock = new object();
        public static readonly object distributionThreadLock = new object();
        public static readonly object ecoregionDataThreadLock = new object();
        public static readonly object initialSitesThreadLock = new object();
        public const float bulkIntercept = 165.0f; //kg/m3
        public const float bulkSlope = 1.3f; //kg/m3
        public const float Pwater = 1000.0f;  // Density of water (kg/m3)
        public const float lambAir = 0.023f; // W/m K (CLM5 documentation, Table 2.7)
        public const float lambIce = 2.29f; // W/m K (CLM5 documentation, Table 2.7)
        public const float snowHeatCapacity = 2090f; // J/kg K (https://www.engineeringtoolbox.com/specific-heat-capacity-d_391.html)
        public const float snowReflectanceThreshold = 0.100f;  // minimum depth of snow (m) that counts as full snow for albedo calculations

        public static void InitializeCore(ICore mCore, ushort IMAX)
        {
            ModelCore = mCore;
            Globals.IMAX = IMAX;
        }
    }
}
