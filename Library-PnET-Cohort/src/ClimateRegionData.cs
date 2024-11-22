//  Author: Robert Scheller, Melissa Lucash

using Landis.Core;
using Landis.Library.Climate;
using System.Linq;

namespace Landis.Library.PnETCohorts
{
    public class ClimateRegionData
    {
        public static Library.Parameters.Ecoregions.AuxParm<AnnualClimate_Monthly> AnnualWeather;

        //---------------------------------------------------------------------
        //public static void Initialize(IInputParameters parameters)
        public static void Initialize()
        {
            AnnualWeather = new Library.Parameters.Ecoregions.AuxParm<AnnualClimate_Monthly>(Globals.ModelCore.Ecoregions);

            foreach (IEcoregion ecoregion in Globals.ModelCore.Ecoregions)
            {
                if (ecoregion.Active)
                {
                    // Latitude is contained in the PnET Ecoregion
                    Climate.Climate.GenerateEcoregionClimateData(ecoregion, 0, EcoregionData.GetPnETEcoregion(ecoregion).Latitude);
                    SetSingleAnnualClimate(ecoregion, 0, Climate.Climate.Phase.SpinUp_Climate);  // Some placeholder data to get things started.
                }
            }
        }

        public static void SetSingleAnnualClimate(IEcoregion ecoregion, int year, Climate.Climate.Phase spinupOrfuture)
        {
            int actualYear = Climate.Climate.Future_MonthlyData.Keys.Min() + year;

            if (spinupOrfuture == Climate.Climate.Phase.Future_Climate)
            {
                if (Climate.Climate.Future_MonthlyData.ContainsKey(actualYear))
                {
                    AnnualWeather[ecoregion] = Climate.Climate.Future_MonthlyData[actualYear][ecoregion.Index];
                }
            }
            else
            {
                if (Climate.Climate.Spinup_MonthlyData.ContainsKey(actualYear))
                {
                    AnnualWeather[ecoregion] = Climate.Climate.Spinup_MonthlyData[actualYear][ecoregion.Index];
                }
            }
        }
    }
}
