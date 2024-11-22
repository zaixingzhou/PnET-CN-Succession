using System.Collections.Generic;

namespace Landis.Library.PnETCohorts
{
    public interface IEstablishmentProbability
    {
        Landis.Library.Parameters.Species.AuxParm<float> Probability { get; }

        //void Calculate_Establishment(IEcoregionPnETVariables pnetvars, IEcoregionPnET ecoregion, float PAR, IHydrology hydrology);

        Dictionary<ISpeciesPnET,float> Calculate_Establishment_Month(IEcoregionPnETVariables pnetvars, IEcoregionPnET ecoregion, float PAR, IHydrology hydrology, float minHalfSat, float maxHalfSat, bool invertPest, float propRootAboveFrost);

        void ResetPerTimeStep();
         
        bool HasEstablished(ISpeciesPnET species);

        void EstablishmentTrue(ISpeciesPnET spc);

        void RecordPest(int year, ISpeciesPnET spc, float pest,float fwater,float frad, bool estab, int monthCount);

        float Get_FWater(ISpeciesPnET species);
        float Get_FRad(ISpeciesPnET species);
    }
}
