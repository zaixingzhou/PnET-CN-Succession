using System.Collections.Generic;

namespace Landis.Library.PnETCohorts 
{
    public interface IObservedClimate: IEnumerable<ObservedClimate>  
    {
        float O3 { get; }// Atmospheric O3 concentration, acumulated during growing season (AOT40) (ppb h)
        float CO2 { get; } // Atmospheric CO2 concentration (ppm)
        float PAR0 { get; }// Photosynthetically active radiation, average daily during the month (W/m2)
        float Prec { get; }// Precipitation (mm/mo)
        float Tmax { get; }// Maximum daily temperature 
        float Tmin { get; }// Minimum daily temperature
        float SPEI { get; } // SPEI

        // Add Ndep, Zhou
        float NH4 { get; } // NH4 deposition gNm-2 
        float NO3 { get; } // NH4 deposition gNm-2 


    }
}
