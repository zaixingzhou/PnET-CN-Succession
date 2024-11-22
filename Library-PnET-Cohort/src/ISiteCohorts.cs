using System.Collections.Generic;
using Landis.Core;


namespace Landis.Library.PnETCohorts 
{
    public interface ISiteCohorts : Landis.Library.BiomassCohorts.ISiteCohorts, Landis.Library.AgeOnlyCohorts.ISiteCohorts
    {

        float[] NetPsn { get; }

        float[] MaintResp{ get; }

        float[] GrossPsn{ get; }

        float[] FolResp { get; }

        float[] AverageAlbedo { get; }

        float[] ActiveLayerDepth { get; }

        float[] FrostDepth { get; }
        float[] MonthlyAvgSnowPack { get; }
        float[] MonthlyAvgWater { get; }
        float[] MonthlyAvgLAI { get; }
        float CanopyLAImax{get;}

        float SiteMossDepth { get; }

        int AverageAge { get; }

        Landis.Library.Parameters.Species.AuxParm<int> CohortCountPerSpecies { get; }

        Landis.Library.Parameters.Species.AuxParm<bool> SpeciesPresent { get; }

        IEstablishmentProbability EstablishmentProbability { get; }

        Landis.Library.Parameters.Species.AuxParm<int> MaxFoliageYearPerSpecies { get; }

        Landis.Library.Parameters.Species.AuxParm<int> BiomassPerSpecies { get; }

        Landis.Library.Parameters.Species.AuxParm<int> AbovegroundBiomassPerSpecies { get; }

        Landis.Library.Parameters.Species.AuxParm<int> WoodBiomassPerSpecies { get; }

        Landis.Library.Parameters.Species.AuxParm<int> BelowGroundBiomassPerSpecies { get; }

        Landis.Library.Parameters.Species.AuxParm<int> FoliageBiomassPerSpecies { get; }

        Landis.Library.Parameters.Species.AuxParm<int> NSCPerSpecies { get; }

        Landis.Library.Parameters.Species.AuxParm<float> LAIPerSpecies { get; }

        Landis.Library.Parameters.Species.AuxParm<int> WoodySenescencePerSpecies { get; }

        Landis.Library.Parameters.Species.AuxParm<int> FoliageSenescencePerSpecies { get; }

        Landis.Library.Parameters.Species.AuxParm<List<ushort>> CohortAges { get; }

        float BiomassSum { get; }

        float AbovegroundBiomassSum { get; }
        float WoodBiomassSum { get; }

        float WoodySenescenceSum { get; }

        float FoliageSenescenceSum { get; }

        int CohortCount { get; }

        float JulySubCanopyPar { get; }

        float SubCanopyParMAX { get; }

        double Litter{ get; }

        double WoodyDebris { get; }

        int AgeMax { get; }

        float WaterAvg { get; }

        float BelowGroundBiomassSum { get; }

        float FoliageSum { get; }

        float NSCSum { get; }

        float AETSum { get; } //mm

        float NetPsnSum { get; }

        float PET { get; }

        List<ISpecies> SpeciesByPlant { get; set; }
        List<ISpecies> SpeciesBySerotiny { get; set; }
        List<ISpecies> SpeciesByResprout { get; set; }
        List<ISpecies> SpeciesBySeed { get; set; }

        List<int> CohortsBySuccession { get; set; }
        List<int> CohortsByCold { get; set; }
        List<int> CohortsByHarvest { get; set; }
        List<int> CohortsByFire { get; set; }
        List<int> CohortsByWind { get; set; }
        List<int> CohortsByOther { get; set; }

    }
}
