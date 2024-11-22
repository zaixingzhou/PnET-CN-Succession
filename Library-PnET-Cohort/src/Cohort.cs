// uses dominance to allocate psn and subtract transpiration from soil water, average cohort vars over layer

using Landis.Core;
using Landis.SpatialModeling;
using Landis.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Landis.Library.PnETCohorts
{
    //public class Cohort : Landis.Library.AgeOnlyCohorts.ICohort, Landis.Library.BiomassCohorts.ICohort, Landis.Library.PnETCohorts.ICohort
    public class Cohort : Landis.Library.PnETCohorts.ICohort
    { 
        public delegate void SubtractTranspiration(float transpiration, ISpeciesPnET Species);
        public ushort index;

        private ISpecies species;
        private ISpeciesPnET speciesPnET;
        private CohortData data;
        private bool firstYear;
        private LocalOutput cohortoutput;

        //---------------------------------------------------------------------
        /// <summary>
        /// The cohort's data
        /// </summary>
        public CohortData Data
        {
            get
            {
                return data;
            }
        }
        //---------------------------------------------------------------------
        // Age (years)
        public ushort Age
        {
            get
            {
                return data.Age;
            }
        }
        //---------------------------------------------------------------------
        // Non soluble carbons
        public float NSC
        {
            get
            {
                return data.NSC;
            }
            set
            {
                data.NSC = value;
            }
        }
        //---------------------------------------------------------------------
        // Maximum Foliage Value For Current Year
        public float MaxFolYear
        {
            get
            {
                return data.MaxFolYear;
            }
            set
            {
                data.MaxFolYear = value;
            }
        }
        //---------------------------------------------------------------------
        // Measure of cohort's diffuse reflection of solar radiation out of total solar radiation without snow reflectance
        public float Albedo
        {
            get
            {
                float albedo = 0;
                if ((!string.IsNullOrEmpty(this.SpeciesPnET.Lifeform))
                        && (this.SpeciesPnET.Lifeform.ToLower().Contains("ground")
                            || this.SpeciesPnET.Lifeform.ToLower().Contains("open")
                            || this.SumLAI == 0))
                {
                    albedo = 0.24F;
                }
                else if ((!string.IsNullOrEmpty(this.SpeciesPnET.Lifeform))
                    && this.SpeciesPnET.Lifeform.ToLower().Contains("dark"))
                {
                    albedo = (float)((-0.067 * Math.Log(this.SumLAI < 0.7 ? 0.7 : this.SumLAI)) + 0.2095);
                }
                else if ((!string.IsNullOrEmpty(this.SpeciesPnET.Lifeform))
                        && this.SpeciesPnET.Lifeform.ToLower().Contains("light"))
                {
                    albedo = (float)((-0.054 * Math.Log(this.SumLAI < 0.7 ? 0.7 : this.SumLAI)) + 0.2082);
                }
                else if ((!string.IsNullOrEmpty(this.SpeciesPnET.Lifeform))
                        && this.SpeciesPnET.Lifeform.ToLower().Contains("decid"))
                {
                    albedo = (float)((-0.0073 * this.SumLAI) + 0.231);
                }

                // Do not allow albedo to be negative
                return albedo > 0 ? albedo : 0;
            }
        }
        //---------------------------------------------------------------------
        // Foliage (g/m2)
        public float Fol
        {
            get
            {
                return data.Fol;
            }
            set
            {
                data.Fol = value;
            }
        }
        //---------------------------------------------------------------------
        // Aboveground Biomass (g/m2) scaled to the site
        public int Biomass
        {
            get
            {
                return (int)(data.AGBiomass * data.CanopyLayerProp);
            }
        }
        //---------------------------------------------------------------------
        // Species Moss Depth (m)
        public float MossDepth
        {
            get
            {
                return data.Biomass * speciesPnET.MossScalar;
            }
        }
        //---------------------------------------------------------------------
        // Aboveground Biomass (g/m2)
        public int AGBiomass
        {
            get
            {
                return (int)((Math.Round((1 - speciesPnET.FracBelowG) * data.TotalBiomass) + data.Fol));
            }
        }
        //---------------------------------------------------------------------
        // Total Biomass (root + wood) (g/m2)
        public int TotalBiomass
        {
            get
            {
                return (int)Math.Round(data.TotalBiomass);
            }
        }
        //---------------------------------------------------------------------
        // Wood (g/m2)
        public uint Wood
        {
            get
            {
                return (uint)Math.Round((1 - speciesPnET.FracBelowG) * data.TotalBiomass);
            }
        }
        //---------------------------------------------------------------------
        // Root (g/m2)
        public uint Root
        {
            get
            {
                return (uint)Math.Round(speciesPnET.FracBelowG * data.TotalBiomass);
            }
        }
        //---------------------------------------------------------------------
        // Max biomass achived in the cohorts' life time. 
        // This value remains high after the cohort has reached its 
        // peak biomass. It is used to determine canopy layers where
        // it prevents that a cohort could descent in the canopy when 
        // it declines (g/m2)
        public float BiomassMax
        {
            get
            {
                return data.BiomassMax;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Boolean whether cohort has been killed by cold temp relative to cold tolerance
        /// </summary>
        public int ColdKill
        {
            get
            {
                return data.ColdKill;
            }
        }
        //---------------------------------------------------------------------
        // Add dead wood to last senescence
        public void AccumulateWoodySenescence(int senescence)
        {
            data.LastWoodySenescence += senescence;
        }
        //---------------------------------------------------------------------
        // Add dead foliage to last senescence
        public void AccumulateFoliageSenescence(int senescence)
        {
            data.LastFoliageSenescence += (float)senescence;
        }
        //---------------------------------------------------------------------
        // Growth reduction factor for age
        float Fage
        {
            get
            {
                return Math.Max(0, 1 - (float)Math.Pow((Age / (float)speciesPnET.Longevity), speciesPnET.PsnAgeRed));
            }
        }
        //---------------------------------------------------------------------
        // NSC fraction: measure for resources
        public float NSCfrac
        {
            get
            {
                return NSC / ((FActiveBiom * (data.TotalBiomass + Fol))*SpeciesPnET.CFracBiomass);
            }
        }
        //---------------------------------------------------------------------
        // Species with PnET parameter additions
        public ISpeciesPnET SpeciesPnET
        {
            get
            {
                return speciesPnET;
            }
        }
        //---------------------------------------------------------------------
        // LANDIS species (without PnET parameter additions)
        public Landis.Core.ISpecies Species
        {
            get
            {
                //return species;
                int sppIndex = species.Index;
                return (Landis.Core.ISpecies)Globals.ModelCore.Species[sppIndex];
            }
        }
        //---------------------------------------------------------------------
        // Defoliation proportion - BRM
        public float DefolProp
        {
            get
            {
                return data.DeFolProp;
            }
           // private set
           set
            {
                data.DeFolProp = value;
            }
        }
        //---------------------------------------------------------------------
        // Annual Woody Senescence (g/m2)
        public float LastWoodySenescence
        {
            get
            {
                return data.LastWoodySenescence;
            }
            set
            {
                data.LastWoodySenescence = value;
            }
        }
        //---------------------------------------------------------------------
        // Annual Foliage Senescence (g/m2)
        public float LastFoliageSenescence
        {
            get
            {
                return data.LastFoliageSenescence;
            }
            set
            {
                data.LastFoliageSenescence = value;
            }
        }
        //---------------------------------------------------------------------
        // Last average FRad
        public float LastFRad
        {
            get
            {
                return data.LastFRad;
            }
        }
        //---------------------------------------------------------------------
        public float adjFolN
        {
            get
            {
                return data.adjFolN;
            }
        }
        //---------------------------------------------------------------------
        public float[] AdjFolN
        {
            get
            {
                return data.AdjFolN;
            }
        }
        //---------------------------------------------------------------------
        public float adjFracFol
        {
            get
            {
                return data.adjFracFol;
            }
        }
        //---------------------------------------------------------------------
        public float[] AdjFracFol
        {
            get
            {
                return data.AdjFracFol;
            }
        }
        //---------------------------------------------------------------------
        public float AdjHalfSat
        {
            get
            {
                return data.AdjHalfSat;
            }
        }
        //---------------------------------------------------------------------
        public float[] CiModifier
        {
            get
            {
                return data.CiModifier;
            }
        }
        //---------------------------------------------------------------------
        public float[] DelAmax
        {
            get
            {
                return data.DelAmax;
            }
        }
        //---------------------------------------------------------------------
        public float[] FolResp
        {
            get
            {
                return data.FolResp;
            }
        }
        //---------------------------------------------------------------------
        public float[] FOzone
        {
            get
            {
                return data.FOzone;
            }
        }
        //---------------------------------------------------------------------
        public float[] FRad
        {
            get
            {
                return data.FRad;
            }
        }
        //---------------------------------------------------------------------
        public float[] FWater
        {
            get
            {
                return data.FWater;
            }
        }
        //---------------------------------------------------------------------
        public float[] GrossPsn
        {
            get
            {
                return data.GrossPsn;
            }
        }
        //---------------------------------------------------------------------
        public float[] Interception
        {
            get
            {
                return data.Interception;
            }
        }
        //---------------------------------------------------------------------
        public float[] LAI
        {
            get
            {
                return data.LAI;
            }
        }        
        //---------------------------------------------------------------------
        public float LastLAI
        {
            get
            {
                return data.LastLAI;
            }
            set
            {
                data.LastLAI = value;
            }
        }
        //---------------------------------------------------------------------
        public float LastAGBio
        {
            get
            {
                return data.LastAGBio;
            }
            set
            {
                data.LastAGBio = value;
            }
        }
        //---------------------------------------------------------------------
        public List<float> LastSeasonFRad
        {
            get
            {
                return data.LastSeasonFRad;
            }
        }
        //---------------------------------------------------------------------
        public byte Layer
        {
            get
            {
                return data.Layer;
            }
            set
            {
                data.Layer = value;
            }
        }
        //---------------------------------------------------------------------
        public bool Leaf_On
        {
            get
            {
                return data.Leaf_On;
            }
        }
        //---------------------------------------------------------------------
        public float[] MaintenanceRespiration
        {
            get
            {
                return data.MaintenanceRespiration;
            }
        }
        //---------------------------------------------------------------------
        public float[] NetPsn
        {
            get
            {
                return data.NetPsn;
            }
        }
        //---------------------------------------------------------------------
        public float[] PressHead
        {
            get
            {
                return data.PressHead;
            }
        }
        //---------------------------------------------------------------------
        public float[] Transpiration
        {
            get
            {
                return data.Transpiration;
            }
        }
        //---------------------------------------------------------------------
        public float[] PotentialTranspiration
        {
            get
            {
                return data.PotentialTranspiration;
            }
        }
        //---------------------------------------------------------------------
        public float[] Water
        {
            get
            {
                return data.Water;
            }
        }
        //---------------------------------------------------------------------
        public int[] NumEvents
        {
            get
            {
                return data.NumEvents;
            }
        }
        //---------------------------------------------------------------------
        public float FActiveBiom
        {
            get
            {
                return (float)Math.Exp(-speciesPnET.FrActWd * data.BiomassMax);
            }
        }
        //---------------------------------------------------------------------
        public bool IsAlive
        {
            // Determine if cohort is alive. It is assumed that a cohort is dead when 
            // NSC decline below 1% of biomass
            get
            {
                return NSCfrac > 0.01F;
            }
        }
        //---------------------------------------------------------------------
        public float SumLAI
        {
            get
            {
                if (data.LAI == null)
                {
                    return 0;
                }
                return data.LAI.Sum();
            }
        }
        //---------------------------------------------------------------------
        public float BiomassLayerProp
        {
            get
            {
                return data.BiomassLayerProp;
            }
            set
            {
                data.BiomassLayerProp = value;
            }
        }
        //---------------------------------------------------------------------
        public float CanopyLayerProp
        {
            get
            {
                return data.CanopyLayerProp;
            }
            set
            {
                data.CanopyLayerProp = value;
            }
        }
        //---------------------------------------------------------------------
        public float CanopyGrowingSpace
        {
            get
            {
                return data.CanopyGrowingSpace;
            }
            set
            {
                data.CanopyGrowingSpace = value;
            }
        }
        public int ANPP
        {
            get
            {
                return data.ANPP;
            }
            set
            {
                data.ANPP = value;
            }
        }

        //---------------------------------------------------------------------
        // List of DisturbanceTypes that have had ReduceDeadPools applied
        public List<ExtensionType> ReducedTypes = null;
        //---------------------------------------------------------------------
        // Index of growing season month
        public int growMonth = -1;
        //---------------------------------------------------------------------
        public void InitializeSubLayers()
        {
            // Initialize subcanopy layers
            index = 0;
            data.LAI = new float[Globals.IMAX];
            data.GrossPsn = new float[Globals.IMAX];
            data.FolResp = new float[Globals.IMAX];
            data.NetPsn = new float[Globals.IMAX];
            data.Transpiration = new float[Globals.IMAX];
            data.PotentialTranspiration = new float[Globals.IMAX];
            data.FRad = new float[Globals.IMAX];
            data.FWater = new float[Globals.IMAX];
            data.Water = new float[Globals.IMAX];
            data.PressHead = new float[Globals.IMAX];
            data.NumEvents = new int[Globals.IMAX];
            data.FOzone = new float[Globals.IMAX];
            data.MaintenanceRespiration = new float[Globals.IMAX];
            data.Interception = new float[Globals.IMAX];
            data.AdjFolN = new float[Globals.IMAX];
            data.AdjFracFol = new float[Globals.IMAX];
            data.CiModifier = new float[Globals.IMAX];
            data.DelAmax = new float[Globals.IMAX];

            Initialize_CN_Cohort_monthly();//Zhou
        }
        //---------------------------------------------------------------------
        public void StoreFRad()
        {
            // Filter for growing season months only
            if (data.Leaf_On)
            {
                data.LastFRad = data.FRad.Average();
                data.LastSeasonFRad.Add(LastFRad);
            }
        }
        //---------------------------------------------------------------------
        public void SetAvgFRad(float lastAvgFrad)
        {
            data.LastSeasonFRad.Add(lastAvgFrad);
        }
        //---------------------------------------------------------------------
        public void ClearFRad()
        {
            data.LastSeasonFRad = new List<float>();
        }
        //---------------------------------------------------------------------
        public void CalcAdjFracFol()
        {
            if (data.LastSeasonFRad.Count() > 0)
            {
                float lastSeasonAvgFRad = data.LastSeasonFRad.ToArray().Average();
                float fracFol_slope = speciesPnET.FracFolShape;
                float fracFol_int = speciesPnET.MaxFracFol;
                // linear version
                //data.adjFracFol = (lastSeasonAvgFRad * fracFol_slope + fracFol_int) * speciesPnET.FracFol;
                //exponential version
                //data.adjFracFol = (float)Math.Pow((lastSeasonAvgFRad + 0.2), fracFol_slope) * speciesPnET.FracFol + speciesPnET.FracFol * fracFol_int;
                //modified exponential version - controls lower and upper limit of function
                data.adjFracFol = speciesPnET.FracFol + ((fracFol_int - speciesPnET.FracFol) * (float)Math.Pow(lastSeasonAvgFRad, fracFol_slope)); //slope is shape parm; fracFol is minFracFol; int is maxFracFol. EJG-7-24-18

                firstYear = false;
            }
            else
                //data.adjFracFol = speciesPnET.MaxFracFol;
                data.adjFracFol = speciesPnET.FracFol;
            data.LastSeasonFRad = new List<float>();

        }
        //---------------------------------------------------------------------
        public void NullSubLayers()
        {
            // Reset values for subcanopy layers
            data.LAI = null;
            data.GrossPsn = null;
            data.FolResp = null;
            data.NetPsn = null;
            data.Transpiration = null;
            data.PotentialTranspiration = null;
            data.FRad = null;
            data.FWater = null;
            data.PressHead = null;
            data.NumEvents = null;
            data.Water = null;
            data.FOzone = null;
            data.MaintenanceRespiration = null;
            data.Interception = null;
            data.AdjFolN = null;
            data.AdjFracFol = null;
            data.CiModifier = null;
            data.DelAmax = null;
        }
        //---------------------------------------------------------------------
        // Get totals for combined cohorts
        public void Accumulate(Cohort c)
        {
            data.TotalBiomass += c.TotalBiomass;
            data.BiomassMax = Math.Max(BiomassMax, data.TotalBiomass);
            data.Fol += c.Fol;
            data.MaxFolYear = Math.Max(MaxFolYear, data.Fol);
            data.AGBiomass = (1 - c.SpeciesPnET.FracBelowG) * data.TotalBiomass + data.Fol;
            data.Biomass = data.AGBiomass * data.CanopyLayerProp;
            data.ANPP += c.ANPP;
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Increments the cohort's age by one year.
        /// </summary>
        public void IncrementAge()
        {
            data.Age += 1;
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Changes the cohort's biomass.
        /// </summary>
        public void ChangeBiomass(int delta)
        {
            float newTotalBiomass = data.TotalBiomass + delta;
            data.TotalBiomass = System.Math.Max(0, newTotalBiomass);
            data.AGBiomass = (1 - this.SpeciesPnET.FracBelowG) * data.TotalBiomass + data.Fol;
            data.Biomass = data.AGBiomass * data.CanopyLayerProp;
            data.BiomassMax = Math.Max(data.BiomassMax, data.TotalBiomass);
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Changes the cohort's ANPP.
        /// </summary>
        public void ChangeANPP(int delta)
        {
            data.ANPP = data.ANPP + delta;
        }


        //---------------------------------------------------------------------
        // Constructor
        public Cohort(ISpecies species, ISpeciesPnET speciesPnET, ushort year_of_birth, string SiteName, double propBiomass, bool cohortStacking) // : base(species, 0, (int)(1F / species.DNSC * (ushort)species.InitialNSC))
        {
            this.species = species;
            this.speciesPnET = speciesPnET;
            data.Age = 1;
            data.ColdKill = int.MaxValue;

            this.data.NSC = (ushort)speciesPnET.InitialNSC;

            // Initialize biomass assuming fixed concentration of NSC, convert gC to gDW
            this.data.TotalBiomass = (uint)Math.Max(1.0,(this.NSC / (speciesPnET.DNSC * speciesPnET.CFracBiomass) * propBiomass)) ;
            this.data.AGBiomass = (1 - speciesPnET.FracBelowG) * this.data.TotalBiomass + this.data.Fol;
            data.BiomassMax = this.data.TotalBiomass;


            Initialize_CN_Cohort(); // ZHOU 1

            float cohortLAI = 0;
            float cohortIdealFol = (speciesPnET.FracFol * this.FActiveBiom * this.data.TotalBiomass);

            //cohortIdealFol = Calculate_FolMassMax(); // Zhou

            for (int i = 0; i < Globals.IMAX; i++)
                cohortLAI += CalculateLAI(this.SpeciesPnET, cohortIdealFol, i, cohortLAI);
            this.data.LastLAI = cohortLAI;
            this.data.LastAGBio = this.data.AGBiomass;
            this.data.CanopyLayerProp = this.data.LastLAI / speciesPnET.MaxLAI;
            if(cohortStacking)
                this.data.CanopyLayerProp = 1.0f;
            this.data.CanopyGrowingSpace = 1.0f;
            this.data.Biomass = this.data.AGBiomass * this.data.CanopyLayerProp;

            this.data.ANPP = (int)this.data.Biomass;   // Zhou, not get it, need revisit

            // Then overwrite them if you need stuff for outputs
            if (SiteName != null)
            {
                InitializeOutput(SiteName, year_of_birth);
            }

            data.LastSeasonFRad = new List<float>();
            firstYear = true;
        }
        //---------------------------------------------------------------------
        public Cohort(ISpecies species,
                      CohortData cohortData)
        {
            this.species = species;
            this.speciesPnET = SpeciesParameters.SpeciesPnET.AllSpecies[species.Index];
            this.data = cohortData;

            Initialize_CN_Cohort(); // ZHOU 2  need to verify
        }
        //---------------------------------------------------------------------
        public Cohort(Cohort cohort) // : base(cohort.species, new Landis.Library.PnETCohorts.CohortData(cohort.age, cohort.Biomass))
        {
            this.species = cohort.Species;
            this.speciesPnET = cohort.speciesPnET;
            this.data.Age = cohort.Age;
            this.data.NSC = cohort.NSC;
            this.data.TotalBiomass = cohort.TotalBiomass;
            this.data.AGBiomass = (1 - cohort.SpeciesPnET.FracBelowG) * cohort.TotalBiomass + cohort.Fol;
            this.data.Biomass = this.data.AGBiomass * cohort.CanopyLayerProp;
            this.data.BiomassMax = cohort.BiomassMax;
            this.data.Fol = cohort.Fol;
            this.data.MaxFolYear = cohort.MaxFolYear;
            this.data.LastSeasonFRad = cohort.data.LastSeasonFRad;
            this.data.ColdKill = int.MaxValue;
            this.data.ANPP = cohort.ANPP;


            this.data.WoodMassN = cohort.WoodMassN;
            this.data.RootMass = cohort.RootMass;
            this.data.RootMassN = cohort.RootMassN;
            this.data.RootC = cohort.RootC;
            this.data.DeadWoodM = cohort.DeadWoodM;
            this.data.DeadWoodN = cohort.DeadWoodN;
            this.data.BudC = cohort.BudC;
            this.data.BudN = cohort.BudN;
            this.data.WoodC = cohort.WoodC;
            this.data.PlantN = cohort.PlantN;

            this.data.NRatio = cohort.NRatio;
            this.data.NRatioNit = cohort.NRatioNit;
            this.data.RootNSinkEff = cohort.RootNSinkEff;

            Initialize_CN_Cohort_v2(); // ZHOU 4 no need to initialization due to copying existing cohort
            
        }
        //---------------------------------------------------------------------
        public Cohort(Cohort cohort, ushort firstYear, string SiteName) // : base(cohort.species, new Landis.Library.PnETCohorts.CohortData(cohort.age, cohort.Biomass))
        {
            this.species = cohort.Species;
            this.speciesPnET = cohort.speciesPnET;
            this.data.Age = cohort.Age;
            this.data.NSC = cohort.NSC;
            this.data.TotalBiomass = cohort.TotalBiomass;
            this.data.AGBiomass = (1 - cohort.SpeciesPnET.FracBelowG) * cohort.TotalBiomass + cohort.Fol;
            this.data.Biomass = this.data.AGBiomass * cohort.CanopyLayerProp;
            this.data.BiomassMax = cohort.BiomassMax;
            this.data.Fol = cohort.Fol;
            this.data.MaxFolYear = cohort.MaxFolYear;
            this.data.LastSeasonFRad = cohort.data.LastSeasonFRad;


            

            this.data.ColdKill = int.MaxValue;
            this.data.ANPP = cohort.ANPP;

            if (SiteName != null)
            {
                InitializeOutput(SiteName, firstYear);
            }

            this.data.WoodMassN = cohort.WoodMassN;
            this.data.RootMass = cohort.RootMass;
            this.data.RootMassN = cohort.RootMassN;
            this.data.RootC = cohort.RootC;
            this.data.DeadWoodM = cohort.DeadWoodM;
            this.data.DeadWoodN = cohort.DeadWoodN;
            this.data.BudC = cohort.BudC;
            this.data.BudN = cohort.BudN;
            this.data.WoodC = cohort.WoodC;
            this.data.PlantN = cohort.PlantN;

            this.data.NRatio = cohort.NRatio;
            this.data.NRatioNit = cohort.NRatioNit;
            this.data.RootNSinkEff = cohort.RootNSinkEff;

            Initialize_CN_Cohort_v2(); // ZHOU 4 no need to initialization due to copying existing cohort
        }
        //---------------------------------------------------------------------
        public Cohort(ISpeciesPnET speciesPnET, ushort age, int woodBiomass, string SiteName, ushort firstYear, bool cohortStacking)
        {
            InitializeSubLayers();
            this.species = (ISpecies)speciesPnET;
            this.speciesPnET = speciesPnET;
            this.data.Age = age;
            //this.data.Biomass = woodBiomass;
            //incoming biomass is aboveground wood, calculate total biomass
            float biomass = (woodBiomass / (1 - speciesPnET.FracBelowG));
            this.data.TotalBiomass = biomass;
            this.data.BiomassMax = biomass;
            this.data.LastSeasonFRad = new List<float>();
            //this.data.adjFracFol = speciesPnET.FracFol;
            this.data.adjFracFol = speciesPnET.MaxFracFol;
            this.data.ColdKill = int.MaxValue;
            float cohortLAI = 0;
            //float cohortIdealFol = (speciesPnET.FracFol * this.FActiveBiom * this.data.TotalBiomass);
            float cohortIdealFol = (speciesPnET.MaxFracFol * this.FActiveBiom * this.data.TotalBiomass);
            
            //cohortIdealFol = Calculate_FolMassMax(); // Zhou

            for (int i = 0; i < Globals.IMAX; i++)
            {
                float subLayerLAI = CalculateLAI(this.SpeciesPnET, cohortIdealFol, i);
                cohortLAI += subLayerLAI;
                if (this.Leaf_On)
                {
                    LAI[index] = subLayerLAI;
                }
            }
            if (this.Leaf_On)
            {
                this.data.Fol = cohortIdealFol;
                this.data.MaxFolYear = cohortIdealFol;
            }
            this.data.LastLAI = cohortLAI;
            this.data.CanopyLayerProp = this.data.LastLAI / speciesPnET.MaxLAI;
            if (cohortStacking)
                this.data.CanopyLayerProp = 1.0f;
            this.data.CanopyGrowingSpace = 1.0f;
            this.data.AGBiomass = (1 - this.speciesPnET.FracBelowG) * this.data.TotalBiomass + this.data.Fol;
            this.data.LastAGBio = this.data.AGBiomass;
            this.data.Biomass = this.data.AGBiomass * this.data.CanopyLayerProp;
            this.data.NSC = this.speciesPnET.DNSC * this.FActiveBiom * (this.data.TotalBiomass + this.data.Fol) * speciesPnET.CFracBiomass;

            if (SiteName != null)
            {
                InitializeOutput(SiteName, firstYear);
            }

            Initialize_CN_Cohort(); // ZHOU 5

        }
        //---------------------------------------------------------------------
        public Cohort(ISpeciesPnET speciesPnET, ushort age, int woodBiomass, int maxBiomass, float canopyGrowingSpace, string SiteName, ushort firstYear, bool cohortStacking)
        {
            InitializeSubLayers();
            this.species = (ISpecies)speciesPnET;
            this.speciesPnET = speciesPnET;
            this.data.Age = age;
            //this.data.Biomass = woodBiomass;
            //incoming biomass is aboveground wood, calculate total biomass
            float biomass = (woodBiomass / (1 - speciesPnET.FracBelowG));
            this.data.TotalBiomass = biomass;
            this.data.BiomassMax = Math.Max(biomass,maxBiomass);
            this.data.LastSeasonFRad = new List<float>();
            //this.data.adjFracFol = speciesPnET.FracFol;
            this.data.adjFracFol = speciesPnET.MaxFracFol;
            this.data.ColdKill = int.MaxValue;
            float cohortLAI = 0;
            //float cohortIdealFol = (speciesPnET.FracFol * this.FActiveBiom * this.data.TotalBiomass);
            float cohortIdealFol = (speciesPnET.MaxFracFol * this.FActiveBiom * this.data.TotalBiomass);

            //cohortIdealFol = Calculate_FolMassMax(); // Zhou

            for (int i = 0; i < Globals.IMAX; i++)
            {
                float subLayerLAI = CalculateLAI(this.SpeciesPnET, cohortIdealFol, i);
                cohortLAI += subLayerLAI;
                if (this.Leaf_On)
                {
                    LAI[index] = subLayerLAI;
                }
            }
            if (this.Leaf_On)
            {
                this.data.Fol = cohortIdealFol;
                this.data.MaxFolYear = cohortIdealFol;
            }
            this.data.LastLAI = cohortLAI;
            this.data.CanopyLayerProp = this.data.LastLAI / speciesPnET.MaxLAI;
            if (cohortStacking)
                this.data.CanopyLayerProp = 1.0f;
            this.data.CanopyGrowingSpace = 1.0f;
            this.data.AGBiomass = (1 - this.speciesPnET.FracBelowG) * this.data.TotalBiomass + this.data.Fol;
            this.data.LastAGBio = this.data.AGBiomass;
            this.data.Biomass = this.data.AGBiomass * this.data.CanopyLayerProp;
            this.data.NSC = this.speciesPnET.DNSC * this.FActiveBiom * (this.data.TotalBiomass + data.Fol) * speciesPnET.CFracBiomass;

            if (SiteName != null)
            {
                InitializeOutput(SiteName, firstYear);
            }

            Initialize_CN_Cohort(); // ZHOU 6
        }
       /*     public Cohort(ISpeciesPnET speciesPnET, ushort age, int woodBiomass, int maxBiomass,float canopyGrowingSpace, string SiteName, ushort firstYear)
        {
            InitializeSubLayers();
            this.species = (ISpecies)speciesPnET;
            this.speciesPnET = speciesPnET;
            this.data.Age = age;
            //incoming biomass is aboveground wood, calculate total biomass
            float inputSiteTotalBiomass = (woodBiomass / (1 - speciesPnET.FracBelowG));
            //initially assume full canopyGrowingSpace
            canopyGrowingSpace = 1.0f;
            float inputTotal = inputSiteTotalBiomass / canopyGrowingSpace;
            float maxSiteTotal = maxBiomass * canopyGrowingSpace;
            float maxSiteWood = maxSiteTotal * (1 - speciesPnET.FracBelowG);
            float inputMaxRatio = woodBiomass / maxSiteWood;
            float rescaleMaxBiomass = maxBiomass;
            if (inputMaxRatio > 1)
                rescaleMaxBiomass = maxBiomass * inputMaxRatio;
            float rescaleProp = Math.Min(1.0f, inputMaxRatio);
            float rescaleTotalBiomass = inputTotal / rescaleProp;
            this.data.Biomass = woodBiomass;
            this.data.TotalBiomass = rescaleTotalBiomass;
            this.data.BiomassMax = rescaleMaxBiomass;
            this.data.LastSeasonFRad = new List<float>();
            this.data.adjFracFol = speciesPnET.FracFol;
            this.data.ColdKill = int.MaxValue;

            float cohortLAI = 0;
            float cohortIdealFol = (speciesPnET.FracFol * this.FActiveBiom * this.data.TotalBiomass);
            for (int i = 0; i < Globals.IMAX; i++)
            {
                float subLayerLAI = CalculateLAI(this.SpeciesPnET, cohortIdealFol, i);
                cohortLAI += subLayerLAI;
                if (this.Leaf_On)
                {
                    LAI[index] = subLayerLAI;
                }
            }
            if (this.Leaf_On)
            {
                this.data.Fol = cohortIdealFol;
            }
            this.data.LastLAI = Math.Min(cohortLAI, speciesPnET.MaxLAI);
            this.data.CanopyLayerProp = Math.Min(this.data.LastLAI / speciesPnET.MaxLAI, canopyGrowingSpace);
            this.data.CanopyGrowingSpace = canopyGrowingSpace;
            if (this.data.CanopyLayerProp != this.data.CanopyGrowingSpace)
            {
                float newTotalBiomass = inputSiteTotalBiomass / this.data.CanopyLayerProp;
                if (newTotalBiomass > maxBiomass)
                {
                    this.data.TotalBiomass = maxBiomass;
                    this.data.CanopyLayerProp = inputSiteTotalBiomass / maxBiomass;
                    if (this.data.CanopyLayerProp > 1)
                        this.data.BiomassMax = maxBiomass * this.data.CanopyLayerProp;
                }
                else {
                    this.data.TotalBiomass = newTotalBiomass;
                        }
                
            }
            this.data.AGBiomass = (1 - this.speciesPnET.FracBelowG) * this.data.TotalBiomass + this.data.Fol;
            this.data.Biomass = woodBiomass + this.data.Fol;
            this.data.NSC = this.speciesPnET.DNSC * this.FActiveBiom * (this.data.AGBiomass) * speciesPnET.CFracBiomass;

            if (SiteName != null)
            {
                InitializeOutput(SiteName, firstYear);
            }
        }*/

        //---------------------------------------------------------------------
        public void CalculateDefoliation(ActiveSite site, int SiteAboveGroundBiomass)
        {
            //int abovegroundBiomass = (int)((1 - speciesPnET.FracBelowG) * TotalBiomass) + (int)Fol;
            //defolProp = (float)Landis.Library.Biomass.CohortDefoliation.Compute(site, speciesPnET, abovegroundBiomass, SiteAboveGroundBiomass);
            lock (Globals.distributionThreadLock)
            {
                data.DeFolProp = (float)Landis.Library.BiomassCohorts.CohortDefoliation.Compute(this, site, SiteAboveGroundBiomass);
            }
        }
        //---------------------------------------------------------------------
        // Photosynthesis by canopy layer
        public bool CalculatePhotosynthesis(float PrecInByCanopyLayer,int precipCount, float leakageFrac, ref Hydrology hydrology, float mainLayerPAR, ref float SubCanopyPar, float o3_cum, float o3_month, int subCanopyIndex, int layerCount, ref float O3Effect, float frostFreeProp, float MeltInByCanopyLayer, bool coldKillBoolean, IEcoregionPnETVariables variables, SiteCohorts siteCohort, float sumCanopyProp,float groundPETbyEvent, bool allowMortality = true)
        {      
            bool success = true;
            float lastO3Effect = O3Effect;
            O3Effect = 0;

            // Leaf area index for the subcanopy layer by index. Function of specific leaf weight SLWMAX and the depth of the canopy
            // Depth of the canopy is expressed by the mass of foliage above this subcanopy layer (i.e. slwdel * index/imax *fol)
            data.LAI[index] = CalculateLAI(speciesPnET, data.Fol, index);

            // Precipitation interception has a max in the upper canopy and decreases exponentially through the canopy
            //Interception[index] = PrecInByCanopyLayer * (float)(1 - Math.Exp(-1 * ecoregion.PrecIntConst * LAI[index]));
            //if (Interception[index] > PrecInByCanopyLayer) throw new System.Exception("Error adding water, PrecInByCanopyLayer = " + PrecInByCanopyLayer + " Interception[index] = " + Interception[index]);

            if (MeltInByCanopyLayer > 0)
            {
                // Add melted snow water to soil moisture
                // Instantaneous runoff (excess of porosity + RunoffCapture)
                float waterCapacity = siteCohort.Ecoregion.Porosity * siteCohort.Ecoregion.RootingDepth * frostFreeProp; //mm
                float meltrunoff = Math.Min(MeltInByCanopyLayer, Math.Max(hydrology.Water * siteCohort.Ecoregion.RootingDepth * frostFreeProp + MeltInByCanopyLayer - waterCapacity, 0));
                //if ((hydrology.Water + meltrunoff) > (ecoregion.Porosity + ecoregion.RunoffCapture))
                //    meltrunoff = (hydrology.Water + meltrunoff) - (ecoregion.Porosity + ecoregion.RunoffCapture);
                float capturedRunoff = 0;
                if ((siteCohort.Ecoregion.RunoffCapture > 0) & (meltrunoff >0))
                {
                    capturedRunoff = Math.Max(0, Math.Min(meltrunoff, (siteCohort.Ecoregion.RunoffCapture - hydrology.SurfaceWater)));
                    hydrology.SurfaceWater += capturedRunoff;
                }
                hydrology.RunOff += (meltrunoff - capturedRunoff);

                success = hydrology.AddWater(MeltInByCanopyLayer - meltrunoff, siteCohort.Ecoregion.RootingDepth * frostFreeProp);
                if (success == false) throw new System.Exception("Error adding water, MeltInByCanopyLayer = " + MeltInByCanopyLayer + "; water = " + hydrology.Water + "; meltrunoff = " + meltrunoff + "; ecoregion = " + siteCohort.Ecoregion.Name + "; site = " + siteCohort.Site.Location);
            }
            float precipIn = 0;
            if (PrecInByCanopyLayer > 0)
            {// If more than one precip event assigned to layer, repeat precip, runoff, leakage for all events prior to respiration
                for (int p = 1; p <= precipCount; p++)
                {
                    // Incoming precipitation
                    // Instantaneous runoff (excess of porosity)
                    float waterCapacity = siteCohort.Ecoregion.Porosity * siteCohort.Ecoregion.RootingDepth * frostFreeProp; //mm
                    float rainrunoff = Math.Min(PrecInByCanopyLayer, Math.Max(hydrology.Water * siteCohort.Ecoregion.RootingDepth * frostFreeProp + PrecInByCanopyLayer - waterCapacity, 0));
                    //if ((hydrology.Water + rainrunoff) > (ecoregion.Porosity + ecoregion.RunoffCapture))
                    //    rainrunoff = (hydrology.Water + rainrunoff) - (ecoregion.Porosity + ecoregion.RunoffCapture);
                    float capturedRunoff = 0;
                    if ((siteCohort.Ecoregion.RunoffCapture > 0) & (rainrunoff > 0))
                    {
                        capturedRunoff = Math.Max(0, Math.Min(rainrunoff, (siteCohort.Ecoregion.RunoffCapture - hydrology.SurfaceWater)));
                        hydrology.SurfaceWater += capturedRunoff;
                    }
                    hydrology.RunOff += (rainrunoff - capturedRunoff);

                    precipIn = PrecInByCanopyLayer - rainrunoff; //mm

                    // Add incoming precipitation to soil moisture
                    success = hydrology.AddWater(precipIn, siteCohort.Ecoregion.RootingDepth * frostFreeProp);
                    if (success == false) throw new System.Exception("Error adding water, waterIn = " + precipIn + "; water = " + hydrology.Water + "; rainrunoff = " + rainrunoff + "; ecoregion = " + siteCohort.Ecoregion.Name + "; site = " + siteCohort.Site.Location);

                    float leakage = Math.Max((float)leakageFrac * (hydrology.Water - siteCohort.Ecoregion.FieldCap), 0) * siteCohort.Ecoregion.RootingDepth * frostFreeProp; //mm
                    hydrology.Leakage += leakage;


                    // Remove fast leakage
                    success = hydrology.AddWater(-1 * leakage, siteCohort.Ecoregion.RootingDepth * frostFreeProp);
                    if (success == false) throw new System.Exception("Error adding water, Hydrology.Leakage = " + hydrology.Leakage + "; water = " + hydrology.Water + "; ecoregion = " + siteCohort.Ecoregion.Name + "; site = " + siteCohort.Site.Location);

                    // Evaporation - moved to SiteCohorts.Grow after transpiration (psn)
                    /*float evaporationEvent = 0;
                    if (frostFreeProp > 0 && groundPETbyEvent > 0)
                    {
                        evaporationEvent = hydrology.CalculateEvaporation(siteCohort, groundPETbyEvent); //mm
                    }
                    success = hydrology.AddWater(-1 * evaporationEvent, siteCohort.Ecoregion.RootingDepth * frostFreeProp);
                    if (success == false)
                    {
                        throw new System.Exception("Error adding water, evaporation = " + evaporationEvent + "; water = " + hydrology.Water + "; ecoregion = " + siteCohort.Ecoregion.Name + "; site = " + siteCohort.Site.Location);
                    }
                    hydrology.Evaporation += evaporationEvent;*/

                    // Add surface water to soil
                    if (hydrology.SurfaceWater > 0)
                    {
                        float surfaceInput = Math.Min(hydrology.SurfaceWater, ((siteCohort.Ecoregion.Porosity - hydrology.Water) * siteCohort.Ecoregion.RootingDepth * frostFreeProp));
                        hydrology.SurfaceWater -= surfaceInput;
                        success = hydrology.AddWater(surfaceInput, siteCohort.Ecoregion.RootingDepth * frostFreeProp);
                        if (success == false) throw new System.Exception("Error adding water, Hydrology.SurfaceWater = " + hydrology.SurfaceWater + "; water = " + hydrology.Water + "; ecoregion = " + siteCohort.Ecoregion.Name + "; site = " + siteCohort.Site.Location);

                    }
                }
            }
            else
            {
                // Leakage only occurs following precipitation events or incoming melt water
                if (MeltInByCanopyLayer > 0)
                {

                    float leakage = Math.Max((float)leakageFrac * (hydrology.Water - siteCohort.Ecoregion.FieldCap), 0) * siteCohort.Ecoregion.RootingDepth * frostFreeProp; //mm
                    hydrology.Leakage += leakage;
                    // Remove fast leakage
                    success = hydrology.AddWater(-1 * leakage, siteCohort.Ecoregion.RootingDepth * frostFreeProp);
                    if (success == false) throw new System.Exception("Error adding water, Hydrology.Leakage = " + hydrology.Leakage + "; water = " + hydrology.Water + "; ecoregion = " + siteCohort.Ecoregion.Name + "; site = " + siteCohort.Site.Location);

                    // Add surface water to soil
                    if (hydrology.SurfaceWater > 0)
                    {
                        float surfaceInput = Math.Min(hydrology.SurfaceWater, ((siteCohort.Ecoregion.Porosity - hydrology.Water) * siteCohort.Ecoregion.RootingDepth * frostFreeProp));
                        hydrology.SurfaceWater -= surfaceInput;
                        success = hydrology.AddWater(surfaceInput, siteCohort.Ecoregion.RootingDepth * frostFreeProp);
                        if (success == false) throw new System.Exception("Error adding water, Hydrology.SurfaceWater = " + hydrology.SurfaceWater + "; water = " + hydrology.Water + "; ecoregion = " + siteCohort.Ecoregion.Name + "; site = " + siteCohort.Site.Location);
                    }
                }
            }
                
            
            //// Adjust soil water for freezing - Now done when calculating frozen depth
            //if (frostFreeProp < 1.0)
            //{
            //    // water in frozen soil is not accessible - treat it as if it leaked out
            //    float frozenLimit = ecoregion.FieldCap * frostFreeProp;
            //    float frozenWater = hydrology.Water - frozenLimit;
            //    // Remove frozen water
            //    success = hydrology.AddWater(-1 * frozenWater);
            //    if (success == false) throw new System.Exception("Error adding water, frozenWater = " + frozenWater + "; water = " + hydrology.Water + "; ecoregion = " + ecoregion.Name + "; site = " + site.Location);
            //}
            // Maintenance respiration depends on biomass,  non soluble carbon and temperature
            data.MaintenanceRespiration[index] = (1 / (float)Globals.IMAX) * (float)Math.Min(NSC, variables[Species.Name].MaintRespFTempResp * (data.TotalBiomass * speciesPnET.CFracBiomass));//gC //IMAXinverse
            // Subtract mainenance respiration (gC/mo)
            //data.NSC -= MaintenanceRespiration[index];
            if (data.NSC < 0)
                data.NSC = 0f;


            // Woody decomposition: do once per year to reduce unnescessary computation time so with the last subcanopy layer 
            if (index == Globals.IMAX - 1)
            {
                // In the last month
                if (variables.Month == (int)Constants.Months.December)
                {
                    if (allowMortality)
                    {

                        //Check if nscfrac is below threshold to determine if cohort is alive
                        if (!this.IsAlive)
                        {
                            data.NSC = 0.0F;  // if cohort is dead, nsc goes to zero and becomes functionally dead even though not removed until end of timestep
                        }
                        else if (Globals.ModelCore.CurrentTime > 0 && this.TotalBiomass < (uint)speciesPnET.InitBiomass)  //Check if biomass < Initial Biomass -> cohort dies
                        {
                            data.NSC = 0.0F;  // if cohort is dead, nsc goes to zero and becomes functionally dead even though not removed until end of timestep
                            data.Leaf_On = false;
                            data.NSC = 0.0F;
                            float foliageSenescence = FoliageSenescence();
                            data.LastFoliageSenescence = foliageSenescence;
                            siteCohort.AddLitter(foliageSenescence * data.CanopyLayerProp, SpeciesPnET);// Using Canopy proportioning
                            FolLitM = foliageSenescence;  // Zhou
                        }

                    }
                    float woodSenescence = Senescence();
                    data.LastWoodySenescence = woodSenescence;
                    siteCohort.AddWoodyDebris(woodSenescence * data.CanopyLayerProp, speciesPnET.KWdLit); // Using Canopy proportioning
                    


                    // Release of nsc, will be added to biomass components next year
                    // Assumed that NSC will have a minimum concentration, excess is allocated to biomass
                    float Allocation = Math.Max(NSC - (speciesPnET.DNSC * FActiveBiom * data.TotalBiomass * speciesPnET.CFracBiomass), 0);


                    //ZHOU for wood growth

/*                  //  data.TotalBiomass += Allocation / speciesPnET.CFracBiomass;  // convert gC to gDW
                 //   data.Biomass = (1 - speciesPnET.FracBelowG) * this.data.TotalBiomass + this.data.Fol;

                    data.TotalBiomass += Allocation / speciesPnET.CFracBiomass;  // convert gC to gDW
                    data.AGBiomass = (1 - speciesPnET.FracBelowG) * this.data.TotalBiomass + this.data.Fol;
                    data.Biomass = this.data.AGBiomass * this.data.CanopyLayerProp;

                    data.BiomassMax = Math.Max(BiomassMax, data.TotalBiomass);
                    data.NSC -= Allocation;
                    if (data.NSC < 0)
                        data.NSC = 0f;*/

                    data.Age++;

                    //firstDefol = true;
                    // firstAlloc = true;
                }
            }
            // Phenology: do once per cohort per month, using the first sublayer 
            if (index == 0)
            {
                if (coldKillBoolean)
                {
                    data.ColdKill = (int)Math.Floor(variables.Tave - (3.0 * siteCohort.Ecoregion.WinterSTD));
                    data.Leaf_On = false;
                    data.NSC = 0.0F;
                    float foliageSenescence = FoliageSenescence();
                    data.LastFoliageSenescence = foliageSenescence;

                    siteCohort.AddLitter(foliageSenescence * data.CanopyLayerProp, SpeciesPnET); // Using Canopy proportioning
                    FolLitM = foliageSenescence;
                    CNTrans_FolTrans();//Zhou

                }
                else
                {
                    // When LeafOn becomes false for the first time in a year
                    if (variables.Tmin <= this.SpeciesPnET.LeafOnMinT)
                    {
                        if (data.Leaf_On == true)
                        {
                            data.Leaf_On = false;
                            float foliageSenescence = FoliageSenescence();
                            data.LastFoliageSenescence = foliageSenescence;

                            siteCohort.AddLitter(foliageSenescence * data.CanopyLayerProp, SpeciesPnET); // Using Canopy proportioning
                            FolLitM = foliageSenescence;
                            CNTrans_FolTrans();//Zhou

                        }
                        growMonth = -1;
                    }
                    else
                    {
                        if (frostFreeProp > 0)
                        {
                            if (data.Leaf_On == false) // LeafOn becomes true for the first time in a year
                            {
                                growMonth = 1;
                            }
                            else
                            {
                                growMonth += 1;
                            }
                            data.Leaf_On = true;
                        }
                    }
                }
                /****************************** MGM's restructuring 10/25/2018 ***************************************/
                FolProdCMo = 0.0f;
                FolGRespMo = 0.0f;
                if (data.Leaf_On)
                {
                      //ZHOU
                    //float FolGRespMo;

                    if (growMonth == 2)  // Apply defoliation only in the second growing season month
                    {
                        ReduceFoliage(data.DeFolProp);
                    }
                    else
                    {
                        if (firstYear)
                            data.adjFracFol = speciesPnET.MaxFracFol;
                            //data.adjFracFol = speciesPnET.FracFol;
                        // Foliage linearly increases with active biomass
                        float IdealFol = (adjFracFol * FActiveBiom * data.TotalBiomass); // Using adjusted FracFol
                        float NSClimit = data.NSC;

                        //IdealFol = Calculate_FolMassMax(); // Zhou

                        if (mainLayerPAR < variables.PAR0) // indicates below the top layer
                        {

                            // Foliage allocation depends on availability of NSC (allows deficit at this time so no min nsc)
                            // carbon fraction of biomass to convert C to DW
                            float Folalloc = Math.Max(0, Math.Min(NSClimit, speciesPnET.CFracBiomass * (IdealFol - Fol)))*1.0f; // gC/mo
                            if (Folalloc < 0.0f) Folalloc = 0.0f;                                                                                             //Folalloc = 150.0f; //Zhou

                            /*
                             float maxfol = 150.0f;
                             if (Age <15 )
                             {
                                 Folalloc = 0.067f * Age * maxfol;
                             }
                             else 
                             {
                                 Folalloc = maxfol;
                             }
                              */

                            //if (Age > 20) Folalloc = 150.0f;
                            // Add foliage allocation to foliage
                            //Folalloc = BudC;
                            data.Fol += Folalloc / speciesPnET.CFracBiomass;// gDW

                            // Subtract from NSC
                            data.NSC -= Folalloc;  // carbon has been deducted from PlantC
                            if (data.NSC < 0)
                                data.NSC = 0f;

                            FolProdCMo = Folalloc; //Zhou
                           
                            FolGRespMo = FolProdCMo * speciesPnET.GRespFrac;
                            data.NSC -= FolGRespMo; //FolResp
                                                    
                            Allocate_Root(variables); // Zhou



                        // lower canopy layers can retain a reserve of NSC (NSCReserve) which limits NSC available for refoliation - default is no reserve (NSCReserve = 0)
                            NSClimit = data.NSC - (speciesPnET.NSCReserve * (FActiveBiom * (data.TotalBiomass + data.Fol) * speciesPnET.CFracBiomass));

                        }

                        float FolCost = 0;
                        float FolTentative = 0;
                        if (growMonth < 2)  // Growing season months before defoliation outbreaks - can add foliage in first growing season month
                        {
                            if (IdealFol > data.Fol)
                            {
                                // Foliage allocation depends on availability of NSC (allows deficit at this time so no min nsc)
                                // carbon fraction of biomass to convert C to DW
                                FolCost = Math.Max(0, Math.Min(NSClimit, speciesPnET.CFracBiomass * (IdealFol - Fol))); // gC/mo

                                // Add foliage allocation to foliage
                                FolTentative = FolCost / speciesPnET.CFracBiomass;// gDW
                                
                            }
                            data.LastLAI = 0;
                        }

                        else if (growMonth == 3) // Refoliation can occur in the 3rd growing season month
                        {
                            if (data.DeFolProp > 0)  // Only defoliated cohorts can add refoliate
                            {
                                //if (data.DeFolProp > 0.60 && speciesPnET.TOfol == 1)  // Refoliation at >60% reduction in foliage for deciduous trees - MGM
                                if (data.DeFolProp > speciesPnET.RefoliationMinimumTrigger)  // Refoliation threshold is variable
                                {
                                    // Foliage allocation depends on availability of NSC (allows deficit at this time so no min nsc)
                                    // carbon fraction of biomass to convert C to DW
                                    //float Folalloc = Math.Max(0f, Math.Min(NSClimit, speciesPnET.CFracBiomass * ((0.70f * IdealFol) - Fol)));  // 70% refoliation
                                    float Folalloc = Math.Max(0f, Math.Min(NSClimit, speciesPnET.CFracBiomass * ((speciesPnET.RefoliationMaximum * IdealFol) - Fol)));  // variable refoliation

                                    //float Folalloc2 = Math.Max(0f, Math.Min(NSClimit, speciesPnET.CFracBiomass * (0.95f * IdealFol - Fol)));  // cost of refol is the cost of getting to 95% IdealFol
                                    FolCost = Math.Max(0f, Math.Min(NSClimit, speciesPnET.CFracBiomass * (speciesPnET.RefoliationCost * IdealFol - Fol)));  // cost of refol is the cost of getting to variable propotion of IdealFol

                                    if (Folalloc < 0.0f) Folalloc = 0.0f;
                                    FolTentative = Folalloc / speciesPnET.CFracBiomass;// gDW

                                    // Subtract from NSC
                                    //data.NSC -= Folalloc2; // resource intensive to reflush in middle of growing season

                                }
                                else //No attempted refoliation but carbon loss after defoliation
                                {
                                    // Foliage allocation depends on availability of NSC (allows deficit at this time so no min nsc)
                                    // carbon fraction of biomass to convert C to DW
                                    //float Folalloc = Math.Max(0f, Math.Min(NSClimit, speciesPnET.CFracBiomass * (0.10f * IdealFol))); // gC/mo 10% of IdealFol to take out NSC 
                                    FolCost = Math.Max(0f, Math.Min(NSClimit, speciesPnET.CFracBiomass * (speciesPnET.NonRefoliationCost * IdealFol))); // gC/mo variable proportion of IdealFol to take out NSC 

                                    // Subtract from NSC do not add Fol
                                   // data.NSC -= Folalloc;
                                }
                            }
                            // Non-defoliated trees do not add to their foliage
                        }

                        if (FolTentative > 0.01)
                        {
                            // Leaf area index for the subcanopy layer by index. Function of specific leaf weight SLWMAX and the depth of the canopy
                            float tentativeLAI = 0;
                            for (int i = 0; i < Globals.IMAX; i++)
                                tentativeLAI += CalculateLAI(this.SpeciesPnET, Fol + FolTentative, i, tentativeLAI);
                            float tentativeCanopyProp = (tentativeLAI / this.speciesPnET.MaxLAI);
                            if (sumCanopyProp > 1)
                                tentativeCanopyProp = tentativeCanopyProp / sumCanopyProp;
                            //float relativeCanopyProp = this.CanopyLayerProp / tentativeCanopyProp;
                            // Downgrade foliage added if canopy is expanding 
                            //float actualFol = Math.Min(FolTentative, FolTentative * relativeCanopyProp);
                            float actualFol = FolTentative;
                            //if (FolTentative * relativeCanopyProp < FolTentative)
                            //    actualFol = FolTentative * relativeCanopyProp;
                            // Add Foliage
                            data.Fol += actualFol;

                            data.MaxFolYear = Math.Max(data.MaxFolYear, data.Fol);

                        }
                        // Subtract from NSC

                        //FolCost = BudC;
                        data.NSC -= FolCost;
                        if (data.NSC < 0)
                            data.NSC = 0f;


                    }
                    // if (FolProdCMo > 0.00001f) Allocate_Root(variables); // Zhou
                     //Allocate_Root(variables); // Zhou

                }
                /*^^^^^^^^^^^^^^^^^^^^^^^^^^^^ MGM's restructuring 10/25/2018 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^*/
            }

            // Leaf area index for the subcanopy layer by index. Function of specific leaf weight SLWMAX and the depth of the canopy
            data.LAI[index] = CalculateLAI(speciesPnET, Fol, index);

            // Adjust HalfSat for CO2 effect
            float halfSatIntercept = speciesPnET.HalfSat - 350 * speciesPnET.CO2HalfSatEff;
            data.AdjHalfSat = speciesPnET.CO2HalfSatEff * variables.CO2 + halfSatIntercept;
            // Reduction factor for radiation on photosynthesis
            float LayerPAR = (float) (mainLayerPAR * Math.Exp(-speciesPnET.K * (LAI.Sum() - LAI[index])));
            FRad[index] = ComputeFrad(LayerPAR, AdjHalfSat);

            // Below-canopy PAR if updated after each subcanopy layer
            //SubCanopyPar *= (float)Math.Exp(-speciesPnET.K * LAI[index]);

            // Get pressure head given ecoregion and soil water content (latter in hydrology)
            //float PressureHead = hydrology.GetPressureHead(siteCohort.Ecoregion);
            float PressureHead = hydrology.PressureHeadTable.CalculateWaterPressure(hydrology.Water, siteCohort.Ecoregion.SoilType);

            // Reduction water for sub or supra optimal soil water content

            float fWaterOzone = 1.0f;  //fWater for ozone functions; ignores H1 and H2 parameters because only impacts when drought-stressed
            if (Globals.ModelCore.CurrentTime > 0)
            {
                FWater[index] = ComputeFWater(speciesPnET.H1, speciesPnET.H2, speciesPnET.H3, speciesPnET.H4, PressureHead);
                Water[index] = hydrology.Water;
                PressHead[index] = PressureHead;
                NumEvents[index] = precipCount;
                fWaterOzone = ComputeFWater(-1, -1, speciesPnET.H3, speciesPnET.H4, PressureHead); // ignores H1 and H2 parameters because only impacts when drought-stressed
                if (frostFreeProp <= 0)
                {
                    FWater[index] = 0;
                    fWaterOzone = 0;
                }
            }
            else // Spinup
            {
                if (((Parameter<string>)Names.GetParameter(Names.SpinUpWaterStress)).Value == "true"
                    || ((Parameter<string>)Names.GetParameter(Names.SpinUpWaterStress)).Value == "yes")
                {
                    FWater[index] = ComputeFWater(speciesPnET.H1, speciesPnET.H2, speciesPnET.H3, speciesPnET.H4, PressureHead);
                    fWaterOzone = ComputeFWater(-1, -1, speciesPnET.H3, speciesPnET.H4, PressureHead); // ignores H1 and H2 parameters because only impacts when drought-stressed
                    if (frostFreeProp <= 0)
                    {
                        FWater[index] = 0;
                        fWaterOzone = 0;
                    }
                }
                else // Ignore H1 and H2 parameters during spinup
                {
                    FWater[index] = ComputeFWater(-1, -1, speciesPnET.H3, speciesPnET.H4, PressureHead);
                    fWaterOzone = FWater[index];
                    if (frostFreeProp <= 0)
                    {
                        FWater[index] = 0;
                        fWaterOzone = 0;
                    }
                }
                Water[index] = hydrology.Water;
                PressHead[index] = PressureHead;
                NumEvents[index] = precipCount;
            }
            
            // FoliarN adjusted based on canopy position (FRad)
            float folN_shape = speciesPnET.FolNShape; //Slope for linear FolN relationship
            float maxFolN = speciesPnET.MaxFolN; //Intercept for linear FolN relationship
            //adjFolN = (FRad[index] * folN_slope + folN_int) * species.FolN; // Linear reduction (with intercept) in FolN with canopy depth (FRad)
            //adjFolN = (float)Math.Pow((FRad[index]), folN_slope) * species.FolN + species.FolN * folN_int; // Expontential reduction
            // Non-Linear reduction in FolN with canopy depth (FRad)
         //   data.adjFolN = speciesPnET.FolN + ((maxFolN - speciesPnET.FolN) * (float)Math.Pow(FRad[index], folN_shape)); //slope is shape parm; FolN is minFolN; intcpt is max FolN. EJG-7-24-18
            
            data.adjFolN = speciesPnET.FolN;// zhou
            
            AdjFolN[index] = adjFolN;  // Stored for output
            AdjFracFol[index] = adjFracFol; //Stored for output


            float ciModifier = 1.0f; // if no ozone, ciModifier defaults to 1
            if (o3_cum > 0)
            {
                // Regression coefs estimated from New 3 algorithm for Ozone drought.xlsx
                // https://usfs.box.com/s/eksrr4d7fli8kr9r4knfr7byfy9r5z0i
                // Uses data provided by Yasutomo Hoshika and Elena Paoletti
                float ciMod_tol = (float)(fWaterOzone + (-0.021 * fWaterOzone + 0.0087) * o3_cum);
                ciMod_tol = Math.Min(ciMod_tol, 1.0f);
                float ciMod_int = (float)(fWaterOzone + (-0.0148 * fWaterOzone + 0.0062) * o3_cum);
                ciMod_int = Math.Min(ciMod_int, 1.0f);
                float ciMod_sens = (float)(fWaterOzone + (-0.0176 * fWaterOzone + 0.0118) * o3_cum);
                ciMod_sens = Math.Min(ciMod_sens, 1.0f);
                if ((speciesPnET.O3StomataSens == "Sensitive") || (speciesPnET.O3StomataSens == "Sens"))
                    ciModifier = ciMod_sens;
                else if ((speciesPnET.O3StomataSens == "Tolerant") || (speciesPnET.O3StomataSens == "Tol"))
                    ciModifier = ciMod_tol;
                else if ((speciesPnET.O3StomataSens == "Intermediate") || (speciesPnET.O3StomataSens == "Int"))
                    ciModifier = ciMod_int;
                else
                {
                    throw new System.Exception("Ozone data provided, but species O3StomataSens is not set to Sensitive, Tolerant or Intermediate");
                }
            }
            // FIXME temporary fix
            if (ciModifier <= 0)
                ciModifier = 0.00001f;

            CiModifier[index] = ciModifier;  // Stored for output

            // If trees are physiologically active
            if (Leaf_On)
            {
                // CO2 ratio internal to the leaf versus external
                float cicaRatio = (-0.075f * adjFolN) + 0.875f;
                float modCiCaRatio = cicaRatio * ciModifier; 
                //float modCiCaRatio = cicaRatio; //modified 12.6.22 - [mod3]
                // Reference co2 ratio
                float ci350 = 350 * modCiCaRatio;
                // Elevated leaf internal co2 concentration
                float ciElev = variables.CO2 * modCiCaRatio;
                float Ca_Ci = variables.CO2 - ciElev;

                // Franks method
                // (Franks,2013, New Phytologist, 197:1077-1094)
                float Gamma = 40; // 40; Gamma is the CO2 compensation point (the point at which photorespiration balances exactly with photosynthesis.  Assumed to be 40 based on leaf temp is assumed to be 25 C

                // Modified Gamma based on air temp
                // Tested here but removed for release v3.0
                // Bernacchi et al. 2002. Plant Physiology 130, 1992-1998
                // Gamma* = e^(13.49-24.46/RTk) [R is universal gas constant = 0.008314 kJ/J/mole, Tk is absolute temperature]
                //float Gamma_T = (float) Math.Exp(13.49 - 24.46 / (0.008314 * (variables.Tday + 273)));

                float Ca0 = 350;  // 350
                float Ca0_adj = Ca0 * cicaRatio;  // Calculated internal concentration given external 350


                // Franks method (Franks,2013, New Phytologist, 197:1077-1094)
                float delamax = (variables.CO2 - Gamma) / (variables.CO2 + 2 * Gamma) * (Ca0 + 2 * Gamma) / (Ca0 - Gamma);
                if (delamax < 0)
                {
                    delamax = 0;
                }

                // Franks method (Franks,2013, New Phytologist, 197:1077-1094)
                // Adj Ca0
                float delamax_adj = (variables.CO2 - Gamma) / (variables.CO2 + 2 * Gamma) * (Ca0_adj + 2 * Gamma) / (Ca0_adj - Gamma);
                if (delamax_adj < 0)
                {
                    delamax_adj = 0;
                }

                // Modified Franks method - by M. Kubiske
                // substitute ciElev for CO2
                float delamaxCi = (ciElev - Gamma) / (ciElev + 2 * Gamma) * (Ca0 + 2 * Gamma) / (Ca0 - Gamma);
                if (delamaxCi < 0)
                {
                    delamaxCi = 0;
                }

                // Modified Franks method - by M. Kubiske
                // substitute ciElev for CO2
                // adjusted Ca0
                float delamaxCi_adj = (ciElev - Gamma) / (ciElev + 2 * Gamma) * (Ca0_adj + 2 * Gamma) / (Ca0_adj - Gamma);
                if (delamaxCi_adj < 0)
                {
                    delamaxCi_adj = 0;
                }

                // Choose between delamax methods here:
                //DelAmax[index] = delamax;  // Franks
                //DelAmax[index] = delamax_adj;  // Franks with adjusted Ca0
                DelAmax[index] = delamaxCi;  // Modified Franks
                //DelAmax[index] = delamaxCi_adj;  // Modified Franks with adjusted Ca0

                // M. Kubiske method for wue calculation:  Improved methods for calculating WUE and Transpiration in PnET.
                float V = (float)(8314.47 * (variables.Tmin + 273) / 101.3);
                float JCO2 = (float)(0.139 * ((variables.CO2 - ciElev) / V) * 0.000001);  // Corrected conversion units 11/29/22
                float JH2O = variables[species.Name].JH2O / ciModifier;  // Modified from * to / 11.18.2022 [mod1]
                //float JH2O = variables[species.Name].JH2O;  // Modified 11.22.2022 [mod2, mod3]
                float wue = (JCO2 / JH2O) * (44 / 18);  //44=mol wt CO2; 18=mol wt H2O; constant =2.44444444444444
                float Amax = (float)(delamaxCi * (speciesPnET.AmaxA + variables[species.Name].AmaxB_CO2 * adjFolN)); //nmole CO2/g Fol/s
                float BaseFolResp = variables[species.Name].BaseFolRespFrac * Amax; //nmole CO2/g Fol/s
                float AmaxAdj = Amax * speciesPnET.AmaxFrac;  //Amax adjustment as applied in PnET
                float GrossAmax = AmaxAdj + BaseFolResp; //nmole CO2/g Fol/s

                //Reference gross Psn (lab conditions) in gC/g Fol/month
                float RefGrossPsn = variables.DaySpan * (GrossAmax * variables[species.Name].DVPD * variables.Daylength * Constants.MC) / Constants.billion;

                // Compute gross psn from stress factors and reference gross psn (gC/g Fol/month)
                // Reduction factors include temperature (FTempPSN), water (FWater), light (FRad), age (Fage)
                //GrossPsn[index] = (1 / (float)Globals.IMAX) * variables[species.Name].FTempPSN * FWater[index] * FRad[index] * Fage * RefGrossPsn * Fol;  // gC/m2 ground/mo
                // Remove FWater from psn reduction because it is accounted for in WUE through ciModifier [mod2, mod3]
                float GrossPsnPotential = (1 / (float)Globals.IMAX) * variables[species.Name].FTempPSN * FRad[index] * Fage * RefGrossPsn * Fol;  // gC/m2 ground/mo
                

                // M. Kubiske equation for transpiration: Improved methods for calculating WUE and Transpiration in PnET.
                // JH2O has been modified by CiModifier to reduce water use efficiency
                // Scale transpiration to proportion of site occupied (CanopyLayerProp)
                // Corrected conversion factor                
                PotentialTranspiration[index] = (float)(0.0015f * (GrossPsnPotential / (JCO2 / JH2O))) * CanopyLayerProp; //mm

                // Modified 11/4/22 to minimize transpiration reduction in approaching H4
                /*// Get pressure head given ecoregion and soil water content (latter in hydrology)
                float water_PostTrans = hydrology.AddWater(hydrology.Water, -1* Transpiration[index], siteCohort.Ecoregion.RootingDepth * frostFreeProp);
                float PressureHead_postTrans = hydrology.GetPressureHead(siteCohort.Ecoregion, water_PostTrans);
                float FWater_postTrans = ComputeFWater(speciesPnET.H1, speciesPnET.H2, speciesPnET.H3, speciesPnET.H4, PressureHead_postTrans);
                if (Globals.ModelCore.CurrentTime <= 0 && !(((Parameter<string>)Names.GetParameter(Names.SpinUpWaterStress)).Value == "true"
                    || ((Parameter<string>)Names.GetParameter(Names.SpinUpWaterStress)).Value == "yes"))
                    FWater_postTrans = ComputeFWater(-1, -1, speciesPnET.H3, speciesPnET.H4, PressureHead_postTrans);

                // Average fWater before and after transpiration - approximation of average FWater for continuous drawdown
                float modifiedFWater = (FWater[index] + FWater_postTrans) / 2.0f;

                if (modifiedFWater < FWater[index])
                {
                    //Adjust GrossPsn for modified FWater
                    GrossPsn[index] = GrossPsn[index] * (modifiedFWater / FWater[index]);
                    FWater[index] = modifiedFWater;
                    Transpiration[index] = (float)(0.0015f * (GrossPsn[index] / (JCO2 / JH2O))) * CanopyLayerProp; //mm
                }*/

                // It is possible for transpiration to calculate to exceed available water
                // In this case, we cap transpiration at available water, and back-calculate GrossPsn and NetPsn to downgrade those as well
                // Volumetric water content (mm/m) at species wilting point (h4) 
                // Convert kPA to mH2o (/9.804139432)
                float wiltPtWater = (float) hydrology.PressureHeadTable.CalculateWaterContent(speciesPnET.H4 * 9.804139432f, siteCohort.Ecoregion.SoilType);
                float availableWater = (hydrology.Water - wiltPtWater) * siteCohort.Ecoregion.RootingDepth * frostFreeProp;

                if (PotentialTranspiration[index] > availableWater)
                {
                    Transpiration[index] = (float)Math.Max(availableWater, 0f); //mm
                    GrossPsn[index] = (Transpiration[index] / 0.0015f) * (JCO2 / JH2O) / CanopyLayerProp;
                    FWater[index] = Transpiration[index] / PotentialTranspiration[index];
                }
                else
                {
                    GrossPsn[index] = GrossPsnPotential * FWater[index];  // gC/m2 ground/mo
                    Transpiration[index] = PotentialTranspiration[index] * FWater[index]; //mm
                }
                /*if(Transpiration[index] > PETmax)
                {
                    Transpiration[index] = PETmax;
                    GrossPsn[index] = (Transpiration[index] / 0.0015f) * (JCO2 / JH2O);
                    NetPsn[index] = GrossPsn[index] - FolResp[index];
                }*/

                // Subtract transpiration from hydrology
                success = hydrology.AddWater(-1 * Transpiration[index], siteCohort.Ecoregion.RootingDepth * frostFreeProp);
                if (success == false)
                    if (success == false) throw new System.Exception("Error adding water, Transpiration = " + Transpiration[index] + " water = " + hydrology.Water + "; ecoregion = " + siteCohort.Ecoregion.Name + "; site = " + siteCohort.Site.Location);
                if (hydrology.SurfaceWater > 0)
                {
                    float surfaceInput = Math.Min(hydrology.SurfaceWater, ((siteCohort.Ecoregion.Porosity - hydrology.Water) * siteCohort.Ecoregion.RootingDepth * frostFreeProp));
                    hydrology.SurfaceWater -= surfaceInput;
                    success = hydrology.AddWater(surfaceInput, siteCohort.Ecoregion.RootingDepth * frostFreeProp);
                    if (success == false) throw new System.Exception("Error adding water, Hydrology.SurfaceWater = " + hydrology.SurfaceWater + "; water = " + hydrology.Water + "; ecoregion = " + siteCohort.Ecoregion.Name + "; site = " + siteCohort.Site.Location);
                }


                // Net foliage respiration depends on reference psn (BaseFolResp)
                // Substitute 24 hours in place of DayLength because foliar respiration does occur at night.  BaseFolResp and Q10Factor use Tave temps reflecting both day and night temperatures.
                float RefFolResp = BaseFolResp * variables[species.Name].Q10Factor * variables.DaySpan * (Constants.SecondsPerHour * 24) * Constants.MC / Constants.billion; // gC/g Fol/month

                // Actual foliage respiration (growth respiration) 
                //FolResp[index] = FWater[index] * RefFolResp * Fol / (float)Globals.IMAX; // gC/m2 ground/mo  - uncertain why FWater limiter was included here, inconsistent with PnET, revised below
                FolResp[index] = RefFolResp * Fol / (float)Globals.IMAX; // gC/m2 ground/mo

                // NetPsn psn depends on gross psn and foliage respiration
                float nonOzoneNetPsn = GrossPsn[index] - FolResp[index];

                // Convert Psn gC/m2 ground/mo to umolCO2/m2 fol/s
                // netPsn_ground = LayerNestPsn*1000000umol*(1mol/12gC) * (1/(60s*60min*14hr*30day))
                float netPsn_ground = nonOzoneNetPsn * 1000000F * (1F / 12F) * (1F / (variables.Daylength * variables.DaySpan));
                float netPsn_leaf_s = 0;
                if (netPsn_ground > 0 && LAI[index] > 0)
                {
                    // nesPsn_leaf_s = NetPsn_ground*(1/LAI){m2 fol/m2 ground}
                    netPsn_leaf_s = netPsn_ground * (1F / LAI[index]);
                    if (float.IsInfinity(netPsn_leaf_s))
                    {
                        netPsn_leaf_s = 0;
                    }
                }

                //Calculate water vapor conductance (gwv) from Psn and Ci; Kubiske Conductance_5.xlsx
                //gwv_mol = NetPsn_leaf_s /(Ca-Ci) {umol/mol} * 1.6(molH20/molCO2)*1000 {mmol/mol}
                float gwv_mol = (float)(netPsn_leaf_s / (Ca_Ci) * 1.6 * 1000);
                //gwv = gwv_mol / (444.5 - 1.3667*Tc)*10    {denominator is from Koerner et al. 1979 (Sheet 3),  Tc = temp in degrees C, * 10 converts from cm to mm.  
                float gwv = (float) (gwv_mol / (444.5 - 1.3667 * variables.Tave) * 10);

                // Calculate gwv from Psn using Ollinger equation
                // g = -0.3133+0.8126*NetPsn_leaf_s
                //float g = (float) (-0.3133 + 0.8126 * netPsn_leaf_s);

                // Reduction factor for ozone on photosynthesis
                if (o3_month > 0)
                {
                    float o3Coeff = speciesPnET.O3GrowthSens;
                    O3Effect = ComputeO3Effect_PnET(o3_month, delamaxCi, netPsn_leaf_s, subCanopyIndex, layerCount, Fol, lastO3Effect, gwv, LAI[index], o3Coeff);
                }
                else
                { O3Effect = 0; }
                FOzone[index] = 1 - O3Effect;
                
                //Apply reduction factor for Ozone
                NetPsn[index] = nonOzoneNetPsn * FOzone[index];

                
                // Add net psn to non soluble carbons
                data.NSC += NetPsn[index]; //gC
                if (data.NSC < 0)
                    data.NSC = 0f;

            }
            else
            {
                // Reset subcanopy layer values
                NetPsn[index] = 0;
                FolResp[index] = 0;
                GrossPsn[index] = 0;
                Transpiration[index] = 0;
                PotentialTranspiration[index] = 0;
                FOzone[index] = 1;

            }

            //if (index < Globals.IMAX - 1) index++;
            index++;
            return success;
        }
        //---------------------------------------------------------------------
        // Based on Michaelis-Menten saturation curve
        // https://en.wikibooks.org/wiki/Structural_Biochemistry/Enzyme/Michaelis_and_Menten_Equation
        // Used in official releases 1.0 - 4.0
        /*public static float ComputeFrad(float Radiation, float HalfSat)
        {
            // Derived from Michaelis-Menton equation
            // https://en.wikibooks.org/wiki/Structural_Biochemistry/Enzyme/Michaelis_and_Menten_Equation

            return Radiation / (Radiation + HalfSat);
        }*/
        //---------------------------------------------------------------------
        // LightEffect equation from PnET
        // Used in official releases >= 5.0
        public static float ComputeFrad(float Radiation, float HalfSat)
        {
            float fRad = 0.0f;
            if (HalfSat > 0)
                fRad = (float)(1.0 - Math.Exp(-1.0 * Radiation * Math.Log(2.0) / HalfSat));
            else
                throw new System.Exception("HalfSat <= 0. Cannot calculate fRad.");
            return fRad;
        }
        //---------------------------------------------------------------------
        public static float ComputeFWater(float H1, float H2, float H3, float H4, float pressurehead)
        {
            float minThreshold = H1;
            if (H2 <= H1)
                minThreshold = H2;
            // Compute water stress
            if (pressurehead <= H1) return 0;
            else if (pressurehead < minThreshold || pressurehead >= H4) return 0;
            else if (pressurehead > H3) return 1 - ((pressurehead - H3) / (H4 - H3));
            else if (pressurehead < H2) return (1.0F/(H2-H1))*pressurehead - (H1/(H2-H1));
            else return 1;
        }
        //---------------------------------------------------------------------
        public static float ComputeO3Effect_PnET(float o3, float delAmax, float netPsn_leaf_s, int Layer, int nLayers, float FolMass, float lastO3Effect, float gwv, float layerLAI, float o3Coeff)
        {
            float currentO3Effect = 1.0F;
            float droughtO3Frac = 1.0F; // Not using droughtO3Frac from PnET code per M. Kubiske and A. Chappelka
            //float kO3Eff = 0.0026F;  // Generic coefficient from Ollinger
            float kO3Eff = 0.0026F * o3Coeff;  // Scaled by species using input parameters
            

            float O3Prof = (float)(0.6163 + (0.00105 * FolMass));
            float RelLayer = (float)Layer / (float)nLayers;
            float relO3 = Math.Min(1,1 - (RelLayer * O3Prof) * (RelLayer * O3Prof) * (RelLayer * O3Prof));
            // Kubiske method (using gwv in place of conductance
            currentO3Effect = (float)Math.Min(1, (lastO3Effect * droughtO3Frac) + (kO3Eff * gwv * o3 * relO3));

            // Ollinger method
            // Calculations for gsSlope and gsInt could be moved back to EcoregionPnETVariables since they only depend on delamax
            //float gsSlope=(float)((-1.1309*delAmax)+1.9762);
            //float gsInt = (float)((0.4656 * delAmax) - 0.9701);
            //float conductance = Math.Max(0, (gsInt + (gsSlope * netPsn_leaf_s)) * (1 - lastO3Effect));
            //float currentO3Effect_conductance = (float)Math.Min(1, (lastO3Effect * droughtO3Frac) + (kO3Eff * conductance * o3 * relO3));

            // Tested here but removed for release v3.0
            //string OzoneConductance = ((Parameter<string>)PlugIn.GetParameter(Names.OzoneConductance)).Value;
            //if (OzoneConductance == "Kubiske")
            //    return currentO3Effect;
            //else if (OzoneConductance == "Ollinger")
            //    return currentO3Effect_conductance;
            //else
            //{
            //    System.Console.WriteLine("OzoneConductance is not Kubiske or Ollinger.  Using Kubiske by default");
            //    return currentO3Effect;
            //}

            return currentO3Effect;
            
        }
        //---------------------------------------------------------------------
        public int ComputeNonWoodyBiomass(ActiveSite site)
        {
            return (int)(Fol);
        }
        //---------------------------------------------------------------------
        public static Percentage ComputeNonWoodyPercentage(Cohort cohort, ActiveSite site)
        {
            return new Percentage(cohort.Fol / (cohort.Wood + cohort.Fol));
        }
        //---------------------------------------------------------------------
        public void InitializeOutput(string SiteName, ushort YearOfBirth)
        {
            cohortoutput = new LocalOutput(SiteName, "Cohort_" + Species.Name + "_" + YearOfBirth + ".csv", OutputHeader);
       
        }
        //---------------------------------------------------------------------
        public void InitializeOutput(string SiteName)
        {
            cohortoutput = new LocalOutput(SiteName, "Cohort_" + Species.Name + ".csv", OutputHeader);
        }
        //---------------------------------------------------------------------
        public void InitializeOutput(LocalOutput localOutput)
        {
            cohortoutput = new LocalOutput(localOutput);
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Raises a Cohort.DeathEvent.
        /// </summary>
        public static void Died(object sender,
                                ICohort cohort,
                                ActiveSite site,
                                ExtensionType disturbanceType)
        {
            if (DeathEvent != null)
                DeathEvent(sender, new DeathEventArgs(cohort, site, disturbanceType));
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Raises a Cohort.DeathEvent if partial mortality.
        /// </summary>
        public static void PartialMortality(object sender,
                                ICohort cohort,
                                ActiveSite site,
                                ExtensionType disturbanceType,
                                float reduction)
        {
            if (PartialDeathEvent != null)
                PartialDeathEvent(sender, new PartialDeathEventArgs(cohort, site, disturbanceType, reduction));
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Occurs when a cohort is killed by an age-only disturbance.
        /// </summary>
        public static event DeathEventHandler<DeathEventArgs> AgeOnlyDeathEvent;
        //---------------------------------------------------------------------
        /// <summary>
        /// Occurs when a cohort dies either due to senescence or biomass
        /// disturbances.
        /// </summary>
        public static event DeathEventHandler<DeathEventArgs> DeathEvent;
        //---------------------------------------------------------------------
        public static event PartialDeathEventHandler<PartialDeathEventArgs> PartialDeathEvent;
        //---------------------------------------------------------------------
        /// <summary>
        /// Raises a Cohort.AgeOnlyDeathEvent.
        /// </summary>
        public static void KilledByAgeOnlyDisturbance(object sender,
                                                      ICohort cohort,
                                                      ActiveSite site,
                                                      ExtensionType disturbanceType)
        {
            if (AgeOnlyDeathEvent != null)
                AgeOnlyDeathEvent(sender, new DeathEventArgs(cohort, site, disturbanceType));
        }
        //---------------------------------------------------------------------
        public void UpdateCohortData(IEcoregionPnETVariables monthdata )
        {
            float netPsnSum = NetPsn.Sum();
            float grossPsnSum = GrossPsn.Sum();
            float transpirationSum = Transpiration.Sum();
            float JCO2_JH2O = 0;
            if(transpirationSum > 0)
                JCO2_JH2O = (float)((0.0015f * grossPsnSum * CanopyLayerProp) / transpirationSum);
            float WUE = JCO2_JH2O * ((float)44 / (float)18); //44=mol wt CO2; 18=mol wt H2O; constant =2.44444444444444

            // determine the limiting factor 
            float fWaterAvg = FWater.Average();
            float PressHeadAvg = PressHead.Average();
            float fRadAvg = FRad.Average();
            float fOzoneAvg = FOzone.Average();
            float fTemp = monthdata[Species.Name].FTempPSN;
            string limitingFactor = "NA";
            if(ColdKill < int.MaxValue)
            {
                limitingFactor = "ColdTol ("+ ColdKill.ToString()+ ")";
            }
            else
            {
               List<float> factorList = new List<float>(new float[]{fWaterAvg,fRadAvg,fOzoneAvg,Fage,fTemp});
               float minFactor = factorList.Min();
                if (minFactor == fTemp)
                    limitingFactor = "fTemp";
                else if (minFactor == Fage)
                    limitingFactor = "fAge";
                else if (minFactor == fWaterAvg)
                {
                    if(PressHeadAvg > this.SpeciesPnET.H3)
                    {
                        limitingFactor = "Too dry";
                    }
                    else if (PressHeadAvg < this.SpeciesPnET.H2)
                    {
                        limitingFactor = "Too wet";
                    }
                    else
                        limitingFactor = "fWater";
                }
                else if (minFactor == fRadAvg)
                    limitingFactor = "fRad";
                else if (minFactor == fOzoneAvg)
                    limitingFactor = "fOzone";
            }

            // Cohort output file
            string s = Math.Round(monthdata.Time,2) + "," +
                       monthdata.Year + "," +
                       monthdata.Month + "," +
                       Age + "," +
                       Layer + "," +
                       CanopyLayerProp + "," +
                       CanopyGrowingSpace + "," +
                       SumLAI + "," +
                       SumLAI* CanopyLayerProp + ","+
                       GrossPsn.Sum() + "," +
                       FolResp.Sum() + "," +
                       MaintenanceRespiration.Sum() + "," +
                       netPsnSum + "," +                  // Sum over canopy layers
                       transpirationSum + "," +
                       WUE.ToString() + "," +
                       Fol + "," +
                       Root + "," +
                       Wood + "," +
                       Fol * CanopyLayerProp + "," +
                       Root * CanopyLayerProp + "," +
                       Wood * CanopyLayerProp + "," +
                       NSC + "," +
                       NSCfrac + "," +
                       fWaterAvg + "," +
                       Water.Average() + ","+
                       PressHead.Average() + ","+
                       fRadAvg + "," +
                       fOzoneAvg + "," +
                       DelAmax.Average() + "," +
                       monthdata[Species.Name].FTempPSN + "," +
                       monthdata[Species.Name].FTempRespWeightedDayAndNight + "," +
                       Fage + "," +
                       Leaf_On + "," +
                       FActiveBiom + "," +
                       AdjFolN.Average() + "," +
                       AdjFracFol.Average() + "," +
                       CiModifier.Average() + ","+
                       AdjHalfSat + ","+
                       limitingFactor
                       +","+ BudC
                       + "," + PlantN
                       + "," + speciesPnET.MaxLAI
                       + "," + speciesPnET.FolN
                       + "," + CanopyTotMass
                       + "," + CanopyTotN
                       + "," + NRatio

                       //+ "," + "FolN"

                       ;
             

            cohortoutput.Add(s);

       
        }
        //---------------------------------------------------------------------
        public string OutputHeader
        {
            get
            { 
                // Cohort output file header
                string hdr = OutputHeaders.Time + "," +
                            OutputHeaders.Year + "," +
                            OutputHeaders.Month + "," +
                            OutputHeaders.Age + "," +
                            OutputHeaders.Layer + "," +
                            OutputHeaders.CanopyLayerProp + "," +
                            OutputHeaders.CanopyGrowingSpace + "," +
                            OutputHeaders.LAI + "," +
                            OutputHeaders.LAISite + "," +
                            OutputHeaders.GrossPsn + "," +
                            OutputHeaders.FolResp + "," +
                            OutputHeaders.MaintResp + "," +
                            OutputHeaders.NetPsn + "," +
                            OutputHeaders.Transpiration + "," +
                            OutputHeaders.WUE + "," +
                            OutputHeaders.Fol + "," +
                            OutputHeaders.Root + "," +
                            OutputHeaders.Wood + "," +
                            OutputHeaders.FolSite + "," +
                            OutputHeaders.RootSite + "," +
                            OutputHeaders.WoodSite + "," +
                            OutputHeaders.NSC + "," +
                            OutputHeaders.NSCfrac + "," +
                            OutputHeaders.fWater + "," +
                            OutputHeaders.water + "," +
                            OutputHeaders.PressureHead + ","+
                            OutputHeaders.fRad + "," +
                            OutputHeaders.FOzone + "," +
                            OutputHeaders.DelAMax + "," +
                            OutputHeaders.fTemp_psn + "," +
                            OutputHeaders.fTemp_resp + "," +
                            OutputHeaders.fage + "," +
                            OutputHeaders.LeafOn + "," +
                            OutputHeaders.FActiveBiom + "," +
                            OutputHeaders.AdjFolN + "," +
                            OutputHeaders.AdjFracFol + "," +
                            OutputHeaders.CiModifier + ","+
                            OutputHeaders.AdjHalfSat + ","+

                            OutputHeaders.LimitingFactor
                            + "," + "BudC"
                            + "," + "PlantN"
                            + "," + "MaxLAI"
                            + "," + "FolN"
                            + "," + "CanopyTotMass"
                            + "," + "CanopyTotN"
                            + "," + "NRatio"


                            ;


                return hdr;
            }
        }
        //---------------------------------------------------------------------
        public void WriteCohortData()
        {
            cohortoutput.Write();
         
        }
        //---------------------------------------------------------------------
        public float FoliageSenescence()
        {
            // If it is fall 
            float Litter = speciesPnET.TOfol * Fol;
            // If cohort is dead, then all foliage is lost
            if (NSCfrac <= 0.01F)
                Litter = Fol;
            Fol -= Litter;
            return Litter;
        }
        //---------------------------------------------------------------------
        public float Senescence()
        {
            float senescence = ((Root * speciesPnET.TOroot) + Wood * speciesPnET.TOwood);

           // data.TotalBiomass -= senescence;  // ZHOU
         //   data.Biomass = (1 - speciesPnET.FracBelowG) * data.TotalBiomass + data.Fol;

            // commented because the code has already been counted in Wood turnover
         /*   data.TotalBiomass -= senescence;
            data.AGBiomass = (1 - speciesPnET.FracBelowG) * data.TotalBiomass + data.Fol;
            data.Biomass = data.AGBiomass * data.CanopyLayerProp;

            data.BiomassMax = Math.Max(data.BiomassMax, data.TotalBiomass);*/
            return senescence;
        }
        //---------------------------------------------------------------------
        public void ReduceFoliage(double fraction)
        {
            Fol *= (float)(1.0 - fraction);
            data.MaxFolYear = Math.Max(data.MaxFolYear, Fol);
        }
        //---------------------------------------------------------------------
        public void ResetFoliageMax()
        {
            data.MaxFolYear = 0;
        }
        //---------------------------------------------------------------------
        public void ReduceBiomass(object sitecohorts, double fraction, ExtensionType disturbanceType)
        {
            if (!((SiteCohorts)sitecohorts).DisturbanceTypesReduced.Contains(disturbanceType))
            {
                Allocation.ReduceDeadPools(sitecohorts, disturbanceType);  // Reduce dead pools before adding through Allocation
                ((SiteCohorts)sitecohorts).DisturbanceTypesReduced.Add(disturbanceType);
            }
            Allocation.Allocate(sitecohorts, this, disturbanceType, fraction);


            data.TotalBiomass *= (float)(1.0 - fraction);
            data.AGBiomass = ((1 - speciesPnET.FracBelowG) * data.TotalBiomass + data.Fol);
            data.Biomass = data.AGBiomass * data.CanopyLayerProp;
            data.BiomassMax = Math.Max(data.BiomassMax, data.TotalBiomass);
            Fol *= (float)(1.0 - fraction);
            data.MaxFolYear = Math.Max(data.MaxFolYear, Fol);
        }
        //---------------------------------------------------------------------
        public float CalculateLAI(ISpeciesPnET species, float fol, int index)
        {
            // Leaf area index for the subcanopy layer by index. Function of specific leaf weight SLWMAX and the depth of the canopy
            // Depth of the canopy is expressed by the mass of foliage above this subcanopy layer (i.e. slwdel * index/imax *fol)
            float LAISum = 0;
            if (LAI != null)
            {
                for (int i = 0; i < index; i++)
                {
                    LAISum += LAI[i];
                }
            }
            float LAIlayerMax = (float)Math.Max(0.01, 25.0F - LAISum); // Cohort LAI is capped at 25; once LAI reaches 25 subsequent sublayers get LAI of 0.01
            float LAIlayer;
            LAIlayer = (1 / (float)Globals.IMAX) * fol / (species.SLWmax - species.SLWDel * index * (1 / (float)Globals.IMAX) * fol);

            /*
            // Zhou, analytical solution.
            if (species.SLWDel > 0.0f)
            {
                float folm_perlayer = (1 / (float)Globals.IMAX) * fol;
                float folm_index = index * folm_perlayer;
                float folm_pre = folm_index - folm_perlayer;
                float folm_diff;

                folm_diff = species.SLWmax - species.SLWDel * folm_index;
                float TotLAI_index;
                float TotLAI_pre;
                if (folm_diff > 0.0f)
                {

                    TotLAI_index = 1 / species.SLWDel * (float)Math.Log(species.SLWmax / (species.SLWmax - species.SLWDel * folm_index));


                }
                else
                {
                    TotLAI_index = species.MaxLAI;


                }// Zhou, analytical solution.
                if (TotLAI_index > species.MaxLAI) TotLAI_index = species.MaxLAI;
                TotLAI_pre = 1 / species.SLWDel * (float)Math.Log(species.SLWmax / (species.SLWmax - species.SLWDel * folm_pre));
                if (Double.IsNaN(TotLAI_pre)) TotLAI_pre = species.MaxLAI;
                LAIlayer = TotLAI_index - TotLAI_pre;
                if (LAIlayer < 0.05f) LAIlayer = 0.05f;
            }
            */

            if (fol > 0 && LAIlayer <= 0)
            {
                Globals.ModelCore.UI.WriteLine("\n Warning: LAI was calculated to be negative for " + species.Name + ". This could be caused by a low value for SLWmax.  LAI applied in this case is a max of 25 for each cohort.");
                LAIlayer = LAIlayerMax/(Globals.IMAX - index);
            }
            else
                LAIlayer = (float)Math.Min(LAIlayerMax, LAIlayer);

            return LAIlayer;
        }
        //---------------------------------------------------------------------
        public float CalculateLAI(ISpeciesPnET species, float fol, int index, float cumulativeLAI)
        {
            // Leaf area index for the subcanopy layer by index. Function of specific leaf weight SLWMAX and the depth of the canopy
            // Depth of the canopy is expressed by the mass of foliage above this subcanopy layer (i.e. slwdel * index/imax *fol)
            float LAISum = cumulativeLAI;
            
            float LAIlayerMax = (float)Math.Max(0.01, 25.0F - LAISum); // Cohort LAI is capped at 25; once LAI reaches 25 subsequent sublayers get LAI of 0.01
             
            float LAIlayer;
            LAIlayer = (1 / (float)Globals.IMAX) * fol / (species.SLWmax - species.SLWDel * index * (1 / (float)Globals.IMAX) * fol);// Brain's method, 

            /*
            // Zhou, analytical solution.
            if (species.SLWDel > 0.0f)
            {
                float folm_perlayer = (1 / (float)Globals.IMAX) * fol;
                float folm_index = index * folm_perlayer;
                float folm_pre = folm_index - folm_perlayer;
                float folm_diff;

                folm_diff = species.SLWmax - species.SLWDel * folm_index;
                float TotLAI_index;
                float TotLAI_pre;
                if (folm_diff > 0.0f)
                {

                    TotLAI_index = 1 / species.SLWDel * (float)Math.Log(species.SLWmax / (species.SLWmax - species.SLWDel * folm_index));


                }
                else
                {
                    TotLAI_index = species.MaxLAI;


                }// Zhou, analytical solution.
                if (TotLAI_index > species.MaxLAI) TotLAI_index = species.MaxLAI;
                TotLAI_pre = 1 / species.SLWDel * (float)Math.Log(species.SLWmax / (species.SLWmax - species.SLWDel * folm_pre));
                if (Double.IsNaN(TotLAI_pre)) TotLAI_pre = species.MaxLAI;
                LAIlayer = TotLAI_index - TotLAI_pre;
                if (LAIlayer < 0.05f) LAIlayer = 0.05f;
            }
            /**/

            if (fol > 0 && LAIlayer <= 0)
            {
                
                Globals.ModelCore.UI.WriteLine("\n Warning: LAI was calculated to be negative for " + species.Name + ". This could be caused by a low value for SLWmax.  LAI applied in this case is a max of 25 for each cohort.");
                LAIlayer = LAIlayerMax / (Globals.IMAX - index);
            }
            else
                LAIlayer = (float)Math.Min(LAIlayerMax, LAIlayer);

            return LAIlayer;
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Raises a Cohort.AgeOnlyDeathEvent.
        /// </summary>
        public static void RaiseDeathEvent(object sender,
                                Cohort cohort, 
                                ActiveSite site,
                                ExtensionType disturbanceType)
        {
            //if (AgeOnlyDeathEvent != null)
            //{
            //    AgeOnlyDeathEvent(sender, new Landis.Library.BiomassCohorts.DeathEventArgs(cohort, site, disturbanceType));
            //}
            if (DeathEvent != null)
            {
                DeathEvent(sender, new Landis.Library.PnETCohorts.DeathEventArgs(cohort, site, disturbanceType));
            }
           
        }
        //---------------------------------------------------------------------


        /// <summary>
        ///    CN functions at cohort level   //ZHOU
        /// </summary>


        public float PlantN
        {
            get
            {
                return data.PlantN;
            }
            set
            { data.PlantN = value; }
        }
        public float PlantNCo // PlantN for the real cohort at the site
        {
            get          
            {
                return PlantN * CanopyLayerProp;
            }
            set {; }
        }

        public float NRatio
        {
            get
            {
                return data.NRatio;
            }
            set
            { data.NRatio = value; }
        }

        public float BudN
        {
            get
            {
                return data.BudN;
            }
            set
            { data.BudN = value; }
        }

        public float BudC
        {
            get
            {
                return data.BudC;
            }
            set
            { data.BudC = value; }
        }


        public float NRatioNit
        {
            get
            {
                return data.NRatioNit;
            }
            set
            { data.NRatioNit = value; }
        }

        public float RootNSinkEff
        {
            get
            {
                return data.RootNSinkEff;
            }
            set
            { data.RootNSinkEff = value; }
        }


        public float RootMass
        {
            get
            {
                return data.RootMass;
            }
            set
            { data.RootMass = value; }
        }


        public float RootMassN
        {
            get
            {
                return data.RootMassN;
            }
            set
            { data.RootMassN = value; }
        }

        public float WoodMass
        {
            get
            {
                return data.TotalBiomass;
            }
            set
            { data.TotalBiomass = value; }
        }

        public float WoodMassCo  // real cohort biomass
        {
            get
            {
                return WoodMass * CanopyLayerProp;
            }
            set
            { ; }
        }

        public float WoodMassN
        {
            get
            { return data.WoodMassN; }
            set
            { data.WoodMassN = value; }
        }


        public float DeadWoodM
        {
            get
            { return data.DeadWoodM; }
            set
            { data.DeadWoodM = value; }
        }

        public float DeadWoodN
        {
            get
            { return data.DeadWoodN; }
            set
            { data.DeadWoodN = value; }
        }

        public float WoodDecResp
        { get; set; }

        public float FolLitM
        { get; set; }

        public float FolLitN
        { get; set; }
        public float RootC
        {
            get
            { return data.RootC; }
            set
            { data.RootC = value; }
        }

        public float PlantC
        { 
            get
            {return data.NSC;} 
            set
            { data.NSC = value;} 
        }

        public float ExcessN
        { get; set; }
      

        public void AllocateYr_Wood()
        {
            float PlantCReserveFrac = 0.75f;
         //   WoodC = Math.Max(data.NSC - (speciesPnET.DNSC * FActiveBiom * biomass * species.CFracBiomass), 0);  //Old
           // WoodC = Math.Max(NSC - (speciesPnET.DNSC * FActiveBiom * data.TotalBiomass * speciesPnET.CFracBiomass), 0);
         //   data.TotalBiomass += Allocation / speciesPnET.CFracBiomass;  // convert gC to gDW
            //    WoodC = (1.0f - PlantCReserveFrac) * data.NSC;
            //           data.NSC = data.NSC - WoodC;
            //   adjResp = 1.0f;  // ZZX_ADJ
            //data.NSC -= WoodC * adjResp;  // ZZX 

            if (data.NSC < 0) data.NSC = 0f;

        }

        public void AllocateYr_Bud()  //Annual C/N allocation for the PnET ecosystem model.
        {

            float FolMassMax; // get potential foliage mass in the canopy
            FolMassMax = Calculate_FolMassMax();

            BudC = (FolMassMax - Fol) * speciesPnET.CFracBiomass;

            if (BudC < 0.0f) BudC = 0.0f;

            //PlantC = PlantC - BudC;

            WoodC = (1.0f - PlantCReserveFrac) * PlantC;
            WoodC = Math.Max(NSC - (speciesPnET.DNSC * FActiveBiom * data.TotalBiomass * speciesPnET.CFracBiomass), 0);

            PlantC = PlantC - WoodC * adjResp;  //Zhou

            if (PlantC < 0.0f) PlantC = 0.0f;
/*
            if (WoodC < (MinWoodFolRatio * BudC))
            {
                float TotalC = WoodC + BudC;
                WoodC = TotalC * (MinWoodFolRatio / (1.0f + MinWoodFolRatio));
                BudC = TotalC - WoodC;
                FolMassMax = Fol + (BudC / speciesPnET.CFracBiomass);
                
            }
*/

           // if ()



            if (PlantN > MaxNStore)  // MaxNStoreWeighted replaced by MaxNStore to the cohort level, site level = x *CanopyLayerProp 
            {
               // ExcessN = (PlantN - MaxNStore);  //
               // PlantN = MaxNStore;
            }

            //   PlantN = MaxNStoreWeighted / 2.0f;  //// Test zhou
            //PlantN = 10.0f;// test zhou


              NRatio = 1.0f + (PlantN / MaxNStore) * speciesPnET.FolNConRange;

            if (NRatio < 1.0)
            {
                NRatio = 1.0f;
            }

            if (NRatio > (1.0 + speciesPnET.FolNConRange))
            {
                NRatio = 1.0f + speciesPnET.FolNConRange;
            }


            // adjFLPctN = 1.2f; // ZZX_ADJ
            //BudC = (adjFracFol * FActiveBiom * data.TotalBiomass) * speciesPnET.CFracBiomass * adjBudC - data.Fol; //  Could be replaced with PnET algorithm
            BudC = (adjFracFol * FActiveBiom * data.TotalBiomass) * speciesPnET.CFracBiomass * adjBudC; //  Could be replaced with PnET algorithm
            //if (Age > 20) BudC = 150.0f;
            BudN = (BudC / speciesPnET.CFracBiomass) * speciesPnET.FLPctN * (1 / (1 - speciesPnET.FolNRetrans)) * NRatio * adjFLPctN;
            if (BudN > PlantN)
            {
                
                if (PlantN < 0.01)
                {
                    BudC = BudC * 0.1f;
                    BudN = BudN * 0.1f;
                }

                else
                {
                    BudC = BudC * (PlantN / BudN);
                    BudN = BudN * (PlantN / BudN);
                }
            }


            //BudC = 100.0f; // ZZX_ADJ
            float tempCanopyTotN = data.Fol * (speciesPnET.FolN / 100) + BudN;
            float tempCanopyTotMass = data.Fol + (BudC / speciesPnET.CFracBiomass);
            //float  folnconnew = (fol * (species.FolN / 100) + BudN) / (fol + (BudC / species.CFracBiomass)) * 100;
            float folnconnew = tempCanopyTotN / tempCanopyTotMass * 100;
            speciesPnET.FolN = folnconnew;


            RPctN = speciesPnET.RLPctN * NRatio; // decimal
            WPctN = speciesPnET.WLPctN * NRatio; // decimal

            PlantN = PlantN - BudN;
            if (PlantN < 0.0f) PlantN = 0.0f;

            RootNSinkEff = (float)Math.Sqrt(1 - (PlantN / MaxNStore)); // Root update ability

        }

        public float CanopyTotN
        { get { return CanopyTotMass * speciesPnET.FolN/100.0f; } set {; } }
        public float CanopyTotMass
        { get { return data.Fol; } set {; } }

        public float CanopyTotNyr
        { get; set; }
        public float CanopyTotMassyr
        { get; set; }
        // Root update ability for N relative to the available soil N pools (NH4+NO3), 0-1.
        public void RootNSink(float Tave)
        {
            
            float TMult = (float)Math.Exp(0.1f * (Tave - 7.1f)) * 0.68f;
            RootNSinkStr = (float)(Math.Min((RootNSinkEff * TMult), 0.98));

        }


        public float BiomassWeight
        { get; set; }
        public float NitWeight
        { get; set; }
        public float RootNSinkStr
        { get; set; }

        public float RootNSinkStrWeighted
        { get; set; }

        public float MaxNStoreWeight
        { get; set; }
        public float MaxNStoreWeighted  // scaled to site level
        { get; set; }
        public float MaxNStore
        { get { return speciesPnET.MaxNStore; } set {; } }
        public float RootNSinkWeight
        { get; set; }
        public float PlantNUptakeWeight
        { get; set; }

        public float PlantNUptake
        { get; set; }

        public float PlantNUptakeCo
        { get; set; }

        // nitrification rate relative to the available soil N pools (NH4), 0-1.
        public void NRatioNitEff()  //
        {
            float nr = NRatio - 1 - (speciesPnET.FolNConRange / 3);
            if (nr < 0) nr = 0;

            NRatioNit = (nr / (0.6667f * speciesPnET.FolNConRange)) * (nr / (0.6667f * speciesPnET.FolNConRange));
            if (NRatioNit > 1) NRatioNit = 1;

        }
        public float WoodTransM  // wood litter
        { get; set; }
        public float WoodTransN
        { get; set; }

        public float WoodTurnover
        { get; set; }
        public void CNTrans_WoodTurnover()
        {
            //
            //Carbon and nitrogen translocation routine
            //

            float WoodMassLoss, WoodLitM, WoodLitN;

            //           float WoodTurnover = 0.03f;  // 0.025
            float WoodLitLossRate = 0.1f; //    
            float WoodLitCLoss = 0.8f; //  

            //WoodTurnover = species.TOwood;
            //   WoodMass = biomass;

            //  WoodTurnover = 0.03f;  // ZZX_ADJ
            WoodLitM = WoodMass * WoodTurnover / 12.0f; //Matlab to C
            WoodLitN = WoodMassN * WoodTurnover / 12.0f; //Matlab to C
            WoodMass = WoodMass - WoodLitM;
            WoodMassN = WoodMassN - WoodLitN;

            //   biomass = WoodMass;  // IMPROVE


            DeadWoodM = DeadWoodM + WoodLitM;
            DeadWoodN = DeadWoodN + WoodLitN;
            WoodMassLoss = DeadWoodM * WoodLitLossRate / 12.0f; //Matlab to C
            WoodTransM = WoodMassLoss * (1.0f - WoodLitCLoss);  // wood litter
            WoodDecResp = (WoodMassLoss - WoodTransM) * speciesPnET.CFracBiomass; // loss as CO2

            WoodTransN = (WoodMassLoss / DeadWoodM) * DeadWoodN;
            DeadWoodM = DeadWoodM - WoodMassLoss;
            DeadWoodN = DeadWoodN - WoodTransN;

            data.Biomass = ((1 - speciesPnET.FracBelowG) * data.TotalBiomass + data.Fol) * data.CanopyLayerProp;  // Zhou need to verify

            data.BiomassMax = Math.Max(BiomassMax, data.TotalBiomass);// ZZHOU

        }

        public void CNTrans_FolTrans()
        {
            //
            //Carbon and nitrogen translocation routine
            //

            float FolNLoss, Retrans;

            FolNLoss = FolLitM * (speciesPnET.FolN / 100.0f);
            Retrans = FolNLoss * speciesPnET.FolNRetrans;
            PlantN = PlantN + Retrans;
            FolLitN = FolNLoss - Retrans;

        }
        public float RootLitM
        { get; set; }
        public float RootLitN
        { get; set; }


        public void CNTrans_RootTurnover()
        {
            //
            //Carbon and nitrogen translocation routine
            //

            float RootTurnover;

            float RootTurnoverA = 0.789f;
            float RootTurnoverB = 0.191f;
            float RootTurnoverC = 0.021f;
            float NetNMinLastYr = 10.0f; //    gN/m2


            RootTurnover = RootTurnoverA - (RootTurnoverB * NetNMinLastYr) + (RootTurnoverC * (float)Math.Pow(NetNMinLastYr, 2)); //Matlab to C
            if (RootTurnover > 2.5)
            {
                RootTurnover = 2.5f;
            }
            if (RootTurnover < 0.1)
            {
                RootTurnover = 0.1f;
            }
            RootTurnover = RootTurnover / 12.0f; // Yearly to monthly

            RootLitM = RootMass * RootTurnover;
            RootLitN = RootLitM * (RootMassN / RootMass);
            RootMass = RootMass - RootLitM;
            RootMassN = RootMassN - RootLitN;


        }


        public float FolProdCMo
        { get; set; }
        public float RPctN
        { get; set; }
        public float RootMRespMo
        { get; set; }
        //RootMRespMo = RootProdCMo* veg->RootMRespFrac;
        public float RootGRespMo
        { get; set; }

        public float FolGRespMo
        { get; set; }

        public float WoodMRespMo
        { get; set; }
    
        public float CanopyGrossPsnActMo
        { 
            get {
                return GrossPsn.Sum();
                       
            } set {; } 
        }

        public float NetPsnMo
        {
            get
            {
                return NetPsn.Sum();

            }
            set {; }
        }
        public float FolMResp
        {
            get
            {
                return FolResp.Sum();

            }
            set {; }
        }

        
        public void Allocate_Root(IEcoregionPnETVariables variables)
        {
            //
            // Root allocation for the PnET ecosystem model.
            //

            float TMult, RootCAdd, RootAllocCMo, RootProdCMo;


            float RootAllocA = 0.0f;
            float RootAllocB = 2.0f;
            float RootMRespFrac = 1.0f;

            TMult = (float)Math.Exp(0.1 * (variables.Tave - 7.1)) * 0.68f; // annual rate

            RootCAdd = RootAllocA / 12.0f + RootAllocB * FolProdCMo;
            RootC = RootC + RootCAdd;
            RootAllocCMo = TMult / 12.0f;   //
            if (RootAllocCMo > 1.0) RootAllocCMo = 1.0f;

            RootAllocCMo = RootAllocCMo * RootC;

            RootC = RootC - RootAllocCMo;
            RootProdCMo = RootAllocCMo / (1.0f + RootMRespFrac + speciesPnET.GRespFrac);

            RootMRespMo = RootProdCMo * RootMRespFrac;
            RootGRespMo = RootProdCMo * speciesPnET.GRespFrac;


            // data.NSC -= RootCAdd + RootMRespMo + RootGRespMo; //ZZX
            data.NSC -= RootCAdd; //ZZX
            float RootProdMass = RootProdCMo / speciesPnET.CFracBiomass;
            RootMass = RootMass + RootProdMass;

            float RootProdMassN = RootProdMass * RPctN;

            if (PlantN < RootProdMassN)
            {
                RootProdMassN = PlantN;

            }
            RootMassN = RootMassN + RootProdMassN;
            PlantN = PlantN - RootProdMassN;   //ZZX


            ////  share->NetCBal = share->NetPsnMo - WoodMRespMo - WoodGRespMo - share->FolGRespMo - RootMRespMo - RootGRespMo;
            // needs -share->SoilDecResp - share->WoodDecResp, and will be updated in the respective routine.



        }

        public float WPctN
        { get; set; }

        public float WoodC
        {
            get
            {
                return data.WoodC;
            }
            set
            { data.WoodC = value; }
        }

        public float WoodGRespMo
        { get; set; }

        public void Allocate_Wood()
        {
            //
            // C allocation for the PnET ecosystem model.
            //

            float WoodProdCMo;


            //float WoodMRespMo, GDDWoodEff, delGDDWoodEff;
            //if (share->GDDTot >= veg->GDDWoodStart)
            //{
            //    GDDWoodEff = (share->GDDTot - veg->GDDWoodStart) / (veg->GDDWoodEnd - veg->GDDWoodStart);
            //    if (GDDWoodEff > 1.0) GDDWoodEff = 1;
            //    if (GDDWoodEff < 0) GDDWoodEff = 0;

            //    delGDDWoodEff = GDDWoodEff - share->OldGDDWoodEff;
            //    WoodProdCMo = share->WoodC * delGDDWoodEff;
            //    WoodGRespMo = WoodProdCMo * veg->GRespFrac;
            //    share->WoodProdCYr = share->WoodProdCYr + WoodProdCMo;
            //    share->WoodGRespYr = share->WoodGRespYr + WoodGRespMo;
            //    share->OldGDDWoodEff = GDDWoodEff;
            //}
            //else
            //{
            //    WoodProdCMo = 0;
            //    WoodGRespMo = 0;
            //}

            WoodProdCMo = WoodC;
            WoodGRespMo = WoodProdCMo * speciesPnET.GRespFrac;

            data.NSC -= WoodGRespMo;   // ZZX
            float WoodProdMass = WoodProdCMo / speciesPnET.CFracBiomass;
            WoodMass = WoodMass + WoodProdMass;

            float WoodProdMassN = WoodProdMass * WPctN;


            if (PlantN < WoodProdMassN)
            {
                WoodProdMassN = PlantN;

            }
            WoodMassN = WoodMassN + WoodProdMassN;
            PlantN = PlantN - WoodProdMassN;  //ZZX
            if (PlantN < 0.0f) PlantN = 0.0f;
            // data.Biomass = ((1 - speciesPnET.FracBelowG) * data.TotalBiomass + data.Fol) * data.CanopyLayerProp;  // Zhou need to verify

                // data.BiomassMax = Math.Max(BiomassMax, data.TotalBiomass);// ZZHOU



        }

        public float Calculate_FolMassMax()
        {
            float EnvMaxFol;
            float AvgDWater;  // water stress

            float avgPCBM;
            float FolMassMax; // maximum folmass retented
            float SppMaxFol;

            SppMaxFol = speciesPnET.SLWmax / speciesPnET.SLWDel * (1.0f - (float)Math.Exp(-speciesPnET.SLWDel * speciesPnET.MaxLAI));


            float LightEffMin; // light availability

            avgPCBM = SppMaxFol;
            LightEffMin = 0.05f;
            AvgDWater = 1.0f;


            if (Age > 15)
            {
                avgPCBM = SppMaxFol;
                //Folalloc = 0.067f * Age * maxfol;
            }
            else
            {
                avgPCBM = 0.067f * Age * SppMaxFol;
            }
            if (avgPCBM > SppMaxFol) avgPCBM = SppMaxFol;

            EnvMaxFol = avgPCBM * (1.0f + (FolRelGrowMax * LightEffMin)) * AvgDWater;
            FolMassMax = EnvMaxFol;

            return FolMassMax;
        }

        public void Allocate_WoodMResp()  // monthly
        {

            //if (Month == (int)Constants.Months.June) Allocate_Wood();// only occur once in June

           // Wood maintenance
            float WoodMRespA = 0.07f;
            
            WoodMRespMo = CanopyGrossPsnActMo * WoodMRespA;
            //WoodMRespMo = MaintenanceRespiration.Sum();
          

            data.NSC -= WoodMRespMo;

        }

        public void Allocate_CN(int Month)  // monthly
        {
            WoodGRespMo = 0.0f;
            WoodMRespMo = 0.0f;
            if (Month == (int)Constants.Months.June) Allocate_Wood();// only occur once in June
            Allocate_WoodMResp();  // wood maintenance


        }


        public void CNTrans()  // monthly
        {


            // CNTrans_FolTrans();
            CNTrans_RootTurnover();
            CNTrans_WoodTurnover();  // biomass and WoodMass connected

        }

        public void AllocateYr() // call at the end of year to estimate foliage and wood growth and N processes
        {
           
           

                // calculate wood growth for next year. one-year off. 
                AllocateYr_Wood();

            // calculate leaf growth and N stress and Root N update potential
            AllocateYr_Bud();

                // calculate Root N update potential
             //   RootNSink(variables);

                // calculate soil Nitrification potential
                NRatioNitEff();
            



        }

        public void Initialize_CN_Cohort()
        {

            WoodC = 0.01f * WoodMass;
            RootC = 0.005f * WoodMass;
            WoodMassN = speciesPnET.WLPctN * WoodMass;

            RootMass = 10.0f;
            RootMassN = RootMass * speciesPnET.RLPctN;

            DeadWoodM = 0.02f * WoodMass;
            DeadWoodN = speciesPnET.WLPctN * DeadWoodM;
            BudC = 0.005f * WoodMass;
            BudN = BudC * 0.04f;// 6.0f;//BudC *species.FolN *2.0f;  //
            PlantN = BudN * 1.6f;//10.0f; // BudN *1.6f; 


            MaxNStoreWeighted = PlantN * 2.0f;//20f;

            // PlantN is the pool before allocation of BudN
           // NRatio = 1.0f + ((PlantN + BudN) / MaxNStore) * speciesPnET.FolNConRange;
            // calculate NRatioNit
            //NRatioNitEff();

            RPctN = speciesPnET.RLPctN * NRatio; // decimal
            WPctN = speciesPnET.WLPctN * NRatio; // decimal

            // PlantN is the pool after allocation of BudN
            RootNSinkEff = (float)Math.Sqrt(1.0f - (PlantN / MaxNStore));
            FolLitM = 0.0f;
            FolLitN = 0.0f;



            Parameterization_CN(); // ZZX_ADJ

            
        }

        // Besides the pools in cohorts needed to be initialized, there are some intermidiate variables also need to have intitial values
        // in part, because some variables are only caculated at the end of year and then keep constant, e.g., NRatio, MaxNStoreWeighted, NRatioNit, RootNSinkEff,
        // but they have been used early in the year.
        public void Initialize_CN_Cohort_v2()
        {  
            
            MaxNStoreWeighted = PlantN * 2.0f;//20f;  // can be removed later Zhou

            // PlantN is the pool before allocation of BudN
          //  NRatio = 1.0f + ((PlantN+BudN) / MaxNStore) * speciesPnET.FolNConRange;
            // calculate NRatioNit
            //NRatioNitEff();

            RPctN = speciesPnET.RLPctN * NRatio; // decimal
            WPctN = speciesPnET.WLPctN * NRatio; // decimal
            
            // PlantN is the pool after allocation of BudN
           // RootNSinkEff = (float)Math.Sqrt(1.0f - (PlantN / MaxNStore));
            FolLitM = 0.0f;
            FolLitN = 0.0f;

            //CanopyTotMass = data.Fol;
            //CanopyTotN = CanopyTotMass * speciesPnET.FolN; 



            Parameterization_CN(); // ZZX_ADJ


        }






        public void Initialize_CN_Cohort_monthly()
        {
            FolLitM = 0.0f;
            FolLitN = 0.0f;

            WoodDecResp = 0.0f;
            WoodMRespMo = 0.0f;
            WoodGRespMo = 0.0f;
            FolGRespMo = 0.0f;
            RootMRespMo = 0.0f;
            RootGRespMo = 0.0f;

        }

        public float RespFactor
        { get; set; }
        public float adjFLPctN
        { get; set; }
        public float adjSLWmax
        { get; set; }
        public float adjResp
        { get; set; }
        public float adjfactor
        { get; set; }
        public float adjBudC
        { get; set; }

        public float FolRelGrowMax
        { get; set; }
        public float PlantCReserveFrac
        { get; set; }

        public float MinWoodFolRatio
        { get; set; }
        
        public void Parameterization_CN()  // ZZX_ADJ
        {
            int site;

            site = 0; // default
            site = 1; // EMS  hardwood
            site = 2;// HEM   conifer
            

            site = 1; // EMS
            site = 100;          //   site = 2;// HEM
            //site = 2;// HEM   conifer

            site = 0;  // use input TOwood


            adjFLPctN = 1.0f;
            adjResp = 1.0f;
            adjSLWmax = 1.0f;
            adjfactor = 1.0f;
            adjBudC = 1.0f;
            //WoodTurnover = 0.025f;
            WoodTurnover = speciesPnET.TOwood;

            FolRelGrowMax = 0.95f;
            PlantCReserveFrac = 0.75f; // 0.75
            MinWoodFolRatio = 1.25f;


            site = 1;
            if (site == 0)
            {

               WoodTurnover = speciesPnET.TOwood;

            }// default
            if (site == 1) // EMS
            {
                //adjSLWmax = 1.2f;
                //adjFLPctN = 1.10f;   // 1.2 //
                //adjResp = 1.20f;  // 1.15
                //WoodTurnover = 0.032f;  // 0.032
                                        // adjBudC = 1.3f;

                FolRelGrowMax = 0.95f;

            }

            if (site == 2) // HEM
            {


                //adjSLWmax = 1.5f;
                //adjFLPctN = 1.2f * 1.0f;   // 1.2 //;   // 1.2
                //adjResp = 1.15f;  // 1.15
                //WoodTurnover = 0.01f;  // 0.032
                
                FolRelGrowMax = 0.3f;
            }


            if (site == 3) // HBR
            {


                adjSLWmax = 1.5f;
                //adjFLPctN = 1.2f;   // 1.2
                // adjResp = 1.15f;  // 1.15
                // WoodTurnover = 0.032f;  // 0.032
            }



            if (site == 100) // EMS
            {
               // adjSLWmax = 1.2f;
              //  adjFLPctN = 1.10f;   // 1.2 //ZZX_scenario
                adjResp = 1.10f;  // 1.15
               // WoodTurnover = 0.025f;  // 0.032
                                       // adjBudC = 1.3f;
               // speciesPnET.FracBelowG = 0.5f;// 0.35 

            }


        }


    }
}
