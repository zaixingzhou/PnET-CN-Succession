namespace Landis.Library.PnETCohorts
{
    using System.Collections.Generic;
    /// <summary>
    /// PnET's Cohort Interface
    /// </summary>
    public interface ICohort
        :Landis.Library.BiomassCohorts.ICohort, Landis.Library.AgeOnlyCohorts.ICohort
    {

        //---------------------------------------------------------------------

        //void IncrementAge();
        CohortData Data
        {
            get;
        }
        //Cohort Cohort
        //{
        //    get;
        // }
        int AGBiomass
        {
            get;
        }
        int TotalBiomass
        {
            get;
        }
        bool Leaf_On
        {
            get;
        }
        float BiomassMax
        {
            get;
        }
        float Fol
        {
            get;
        }
        float MaxFolYear
        {
            get;
        }
        float NSC
        {
            get;
        }
        /*float DeFolProp
        {
            get; set;
        }
        */
        float LastWoodySenescence
        {
            get;
        }
        float LastFoliageSenescence
        {
          get; set;
        }
        float LastFRad
        {
            get;
        }
        List<float> LastSeasonFRad
        {
            get;
        }
        float adjFracFol
        {
            get;
        }
        float AdjHalfSat
        {
            get;
        }
        float adjFolN
        {
            get;
        }
        int ColdKill
        {
            get;
        }
        byte Layer
        {
            get;
        }
        float[] LAI
        {
            get;
        }
        float LastLAI
        {
            get;
        }
        float LastAGBio
        {
            get;
        }
        float[] GrossPsn
        {
            get;
        }
        float[] FolResp
        {
            get;
        }
        float[] NetPsn
        {
            get;
        }
        float[] MaintenanceRespiration
        {
            get;
        }
        float[] Transpiration
        {
            get;
        }
        float[] PotentialTranspiration
        {
            get;
        }
        float[] FRad
        {
            get;
        }
        float[] FWater
        {
            get;
        }
        float[] Water
        {
            get;
        }
        float[] PressHead
        {
            get;
        }
        int[] NumEvents
        {
            get;
        }
        float[] FOzone
        {
            get;
        }
        float[] Interception
        {
            get;
        }
        float[] AdjFolN
        {
            get;
        }
        float[] AdjFracFol
        {
            get;
        }
        float[] CiModifier
        {
            get;
        }
        float[] DelAmax
        {
            get;
        }
        float BiomassLayerProp
        {
            get;
        }
        float CanopyLayerProp
        {
            get;
        }
        float CanopyGrowingSpace
        {
            get;
        }


    }
}
