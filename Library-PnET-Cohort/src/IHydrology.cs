﻿


namespace Landis.Library.PnETCohorts
{
    public interface IHydrology
    {
        float Water { get; } // volumetric water (mm/m)
        float GetPressureHead(IEcoregionPnET ecoregion); // Get the pressurehead (mmH2O) for the current water content
        bool AddWater(float water, float activeSoilDepth); // Add mm water to volumetric water content (mm/m) (considering activeSoilDepth - frozen soil cannot accept water)
        float CalculateEvaporation(SiteCohorts sitecohorts, float PET);
        float FrozenWaterContent { get; } // volumetric water content (mm/m) of the frozen soil
        float FrozenDepth { get; } // Depth at which soil is frozen (mm); Rooting zone soil below this depth is frozen
        bool SetFrozenWaterContent(float water);  // Change FrozenWaterContent
        bool SetFrozenDepth(float depth); // Change FrozenDepth
        float Calculate_RET_Hamon(float T, float dayLength); // Calculate reference ET
        PressureHeadSaxton_Rawls PressureHeadTable { get; } // Get the PressureHeadTable object
    }
}
