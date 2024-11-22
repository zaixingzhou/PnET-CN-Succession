﻿//  Copyright ...
//  Authors:  Arjan de Bruijn

using Landis.Utilities;
using Landis.Core;
using Landis.Library.Climate;
using Landis.Library.InitialCommunities;
using Landis.SpatialModeling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Landis.Library.PnETCohorts
{
    public class SiteCohorts : ISiteCohorts, Landis.Library.BiomassCohorts.ISiteCohorts, Landis.Library.AgeOnlyCohorts.ISiteCohorts
    {
        private float canopylaimax;
        private float wateravg;
        private float snowPack;
        private float[] CanopyLAI;
        private float subcanopypar;
        private float julysubcanopypar;
        private float subcanopyparmax;
        private float propRootAboveFrost;
        private float soilDiffusivity;
        private float leakageFrac;
        //private float runoffCapture;
        private float[] netpsn = null;
        //private float netpsnsum;
        private float[] grosspsn = null;
        private float[] folresp = null;
        private float[] maintresp = null;
        private float[] averageAlbedo = null;
        private float[] activeLayerDepth = null;
        private float[] frostDepth = null;
        private float[] monthCount = null;
        private float[] monthlySnowPack = null;
        private float[] monthlyWater = null;
        private float[] monthlyLAI = null;
        private float[] monthlyLAICumulative = null;
        private float transpiration;
        private float potentialTranspiration;
        private double HeterotrophicRespiration;
        private Hydrology hydrology = null;
        IEstablishmentProbability establishmentProbability = null;
        
        public ActiveSite Site;
        public Dictionary<ISpecies, List<Cohort>> cohorts = null;
        public List<ISpecies> SpeciesEstablishedByPlant = null;
        public List<ISpecies> SpeciesEstablishedBySerotiny = null;
        public List<ISpecies> SpeciesEstablishedByResprout = null;
        public List<ISpecies> SpeciesEstablishedBySeed = null;
        public List<int> CohortsKilledBySuccession = null;
        public List<int> CohortsKilledByCold = null;
        public List<int> CohortsKilledByHarvest = null;
        public List<int> CohortsKilledByFire = null;
        public List<int> CohortsKilledByWind = null;
        public List<int> CohortsKilledByOther = null;
        public List<ExtensionType> DisturbanceTypesReduced = null;
        public IEcoregionPnET Ecoregion;
        public LocalOutput siteoutput;

        private float[] AET = new float[12]; // mm/mo
        private static IDictionary<uint, SiteCohorts> initialSites;
        private static byte MaxCanopyLayers;
        //private static ushort MaxDevLyrAv;
        private static float LayerThreshRatio;
        private float interception;
        private float precLoss;
        private static byte Timestep;
        private static int CohortBinSize;
        private static bool PrecipEventsWithReplacement;
        private int nlayers;
        private static int MaxLayer;
        private static bool soilIceDepth;
        private static bool permafrost;
        private static bool invertPest;
        //private static string parUnits;
        Dictionary<float, float> depthTempDict = new Dictionary<float, float>();  //for permafrost
        //float lastTempBelowSnow = float.MaxValue;
        private static float maxHalfSat;
        private static float minHalfSat;
        private static bool CohortStacking;
        Dictionary<double, bool> ratioAbove10 = new Dictionary<double, bool>();

        private static float CanopySumScale;

        //private static float CanopySumScale;
        private DateTime StartDate;

        /// <summary>
        /// Occurs when a site is disturbed by an age-only disturbance.
        /// </summary>
        //public static event Landis.Library.BiomassCohorts.DisturbanceEventHandler AgeOnlyDisturbanceEvent;

        //---------------------------------------------------------------------
        public List<ISpecies> SpeciesByPlant
        {
            get
            {
                return SpeciesEstablishedByPlant;
            }
            set
            {
                SpeciesEstablishedByPlant = value;
            }
        }
        //---------------------------------------------------------------------
        public List<ISpecies> SpeciesBySerotiny
        {
            get
            {
                return SpeciesEstablishedBySerotiny;
            }
            set
            {
                SpeciesEstablishedBySerotiny = value;
            }
        }
        //---------------------------------------------------------------------
        public List<ISpecies> SpeciesByResprout
        {
            get
            {
                return SpeciesEstablishedByResprout;
            }
            set
            {
                SpeciesEstablishedByResprout = value;
            }
        }
        //---------------------------------------------------------------------
        public List<ISpecies> SpeciesBySeed
        {
            get
            {
                return SpeciesEstablishedBySeed;
            }
            set
            {
                SpeciesEstablishedBySeed = value;
            }
        }
        //---------------------------------------------------------------------
        public List<int> CohortsBySuccession
        {
            get
            {
                return CohortsKilledBySuccession;
            }
            set
            {
                CohortsKilledBySuccession = value;
            }
        }
        //---------------------------------------------------------------------
        public List<int> CohortsByCold
        {
            get
            {
                return CohortsKilledByCold;
            }
            set
            {
                CohortsKilledByCold = value;
            }
        }
        //---------------------------------------------------------------------
        public List<int> CohortsByHarvest
        {
            get
            {
                return CohortsKilledByHarvest;
            }
            set
            {
                CohortsKilledByHarvest = value;
            }
        }
        //---------------------------------------------------------------------
        public List<int> CohortsByFire
        {
            get
            {
                return CohortsKilledByFire;
            }
            set
            {
                CohortsKilledByFire = value;
            }
        }
        //---------------------------------------------------------------------
        public List<int> CohortsByWind
        {
            get
            {
                return CohortsKilledByWind;
            }
            set
            {
                CohortsKilledByWind = value;
            }
        }
        //---------------------------------------------------------------------
        public List<int> CohortsByOther
        {
            get
            {
                return CohortsKilledByOther;
            }
            set
            {
                CohortsKilledByOther = value;
            }
        }
        //---------------------------------------------------------------------

        public float Transpiration
        {
            get
            {
                return transpiration;
            }
        }        
        //---------------------------------------------------------------------

        public float PotentialTranspiration
        {
            get
            {
                return potentialTranspiration;
            }
        }
        //---------------------------------------------------------------------
        public float JulySubCanopyPar
        {
            get
            {
                return julysubcanopypar;
            }
        }
        //---------------------------------------------------------------------
        public float SubcanopyPAR
        {
            get
            {
                return subcanopypar;
            }
        }
        //---------------------------------------------------------------------
        public IEstablishmentProbability EstablishmentProbability 
        {
            get
            {
                return establishmentProbability;
            }
        }
        //---------------------------------------------------------------------
        public float SubCanopyParMAX
        {
            get
            {
                return subcanopyparmax;
            }
        }
        //---------------------------------------------------------------------
        public float WaterAvg
        {
            get
            {
                return wateravg;
            }
        }
        //---------------------------------------------------------------------
        public float[] NetPsn
        {
            get
            {
                if (netpsn == null)
                {
                    float[] netpsn_array = new float[12];
                    for (int i = 0; i < netpsn_array.Length; i++)
                    {
                        netpsn_array[i] = 0;
                    }
                    return netpsn_array;
                }
                else
                {
                    //return netpsn.Select(psn => (int)psn).ToArray();
                    return netpsn.ToArray();
                }
            }
        }

        public static bool InitialSitesContainsKey(uint key)
        {
            if (initialSites != null && initialSites.ContainsKey(key))
            {
                return true;
            }
            return false;
        }

        public static void Initialize()
        {
            initialSites = new Dictionary<uint, SiteCohorts>();
            Timestep = ((Parameter<byte>)Names.GetParameter(Names.Timestep)).Value;
            //MaxDevLyrAv = ((Parameter<ushort>)PlugIn.GetParameter(Names.MaxDevLyrAv, 0, ushort.MaxValue)).Value;
            LayerThreshRatio = ((Parameter<float>)Names.GetParameter(Names.LayerThreshRatio, 0, float.MaxValue)).Value;
            MaxCanopyLayers = ((Parameter<byte>)Names.GetParameter(Names.MaxCanopyLayers, 0, 20)).Value;
            soilIceDepth = ((Parameter<bool>)Names.GetParameter(Names.SoilIceDepth)).Value;
            invertPest = ((Parameter<bool>)Names.GetParameter(Names.InvertPest)).Value;
            CohortStacking = ((Parameter<bool>)Names.GetParameter(Names.CohortStacking)).Value;
            permafrost = false;
            //parUnits = ((Parameter<string>)Names.GetParameter(Names.PARunits)).Value;

            CanopySumScale = ((Parameter<float>)Names.GetParameter(Names.CanopySumScale, 0f, 1f)).Value;

            Parameter<string> CohortBinSizeParm = null;
            if (Names.TryGetParameter(Names.CohortBinSize, out CohortBinSizeParm))
            {
                if (!Int32.TryParse(CohortBinSizeParm.Value, out CohortBinSize))
                {
                    throw new System.Exception("CohortBinSize is not an integer value.");
                }
            }
            else
                CohortBinSize = Timestep;

            string precipEventsWithReplacement = ((Parameter<string>)Names.GetParameter(Names.PrecipEventsWithReplacement)).Value;
            PrecipEventsWithReplacement = true;
            if (precipEventsWithReplacement == "false" || precipEventsWithReplacement == "no")
                PrecipEventsWithReplacement = false;

            maxHalfSat = 0;
            minHalfSat = float.MaxValue;
            foreach(ISpeciesPnET spc in SpeciesParameters.SpeciesPnET.AllSpecies)
            {
                if (spc.HalfSat > maxHalfSat)
                    maxHalfSat = spc.HalfSat;
                if (spc.HalfSat < minHalfSat)
                    minHalfSat = spc.HalfSat;
            }
        }

        // Constructor for initialization of SiteCohorts with no initialSite entry yet        
        public SiteCohorts(DateTime StartDate, ActiveSite site, ICommunity initialCommunity, bool usingClimateLibrary, string initialCommunitiesSpinup, float minFolRatioFactor, string SiteOutputName = null)
        {
            this.Ecoregion = EcoregionData.GetPnETEcoregion(Globals.ModelCore.Ecoregion[site]);
            this.Site = site;
            cohorts = new Dictionary<ISpecies, List<Cohort>>();
            SpeciesEstablishedByPlant = new List<ISpecies>();
            SpeciesEstablishedBySerotiny = new List<ISpecies>();
            SpeciesEstablishedByResprout = new List<ISpecies>();
            SpeciesEstablishedBySeed = new List<ISpecies>();
            CohortsKilledBySuccession = new List<int>(new int[Globals.ModelCore.Species.Count()]);
            CohortsKilledByCold = new List<int>(new int[Globals.ModelCore.Species.Count()]);
            CohortsKilledByHarvest = new List<int>(new int[Globals.ModelCore.Species.Count()]);
            CohortsKilledByFire = new List<int>(new int[Globals.ModelCore.Species.Count()]);
            CohortsKilledByWind = new List<int>(new int[Globals.ModelCore.Species.Count()]);
            CohortsKilledByOther = new List<int>(new int[Globals.ModelCore.Species.Count()]);
            DisturbanceTypesReduced = new List<ExtensionType>();
            uint key = ComputeKey((ushort)initialCommunity.MapCode, Globals.ModelCore.Ecoregion[site].MapCode);
            int tempMaxCanopyLayers = MaxCanopyLayers;
            if (CohortStacking)
                tempMaxCanopyLayers = initialCommunity.Cohorts.Count();

            lock (Globals.initialSitesThreadLock)
            {
                if (initialSites.ContainsKey(key) == false)
                {

                    initialSites.Add(key, this);

                }
            }

            List<IEcoregionPnETVariables> ecoregionInitializer = usingClimateLibrary ? EcoregionData.GetClimateRegionData(Ecoregion, StartDate, StartDate.AddMonths(1), Climate.Climate.Phase.SpinUp_Climate) : EcoregionData.GetData(Ecoregion, StartDate, StartDate.AddMonths(1));

            //List<IEcoregionPnETVariables> ecoregionInitializer = EcoregionData.GetData(Ecoregion, StartDate, StartDate.AddMonths(1));
            hydrology = new Hydrology(Ecoregion.FieldCap);
            wateravg = hydrology.Water;
            subcanopypar = ecoregionInitializer[0].PAR0;
            subcanopyparmax = subcanopypar;

            SiteVars.WoodyDebris[Site] = new Library.Biomass.Pool();
            SiteVars.Litter[Site] = new Library.Biomass.Pool();
            SiteVars.FineFuels[Site] = SiteVars.Litter[Site].Mass;
            //PlugIn.PressureHead[Site] = hydrology.GetPressureHead(Ecoregion);
            List<float> cohortBiomassLayerProp = new List<float>();
            List<float> cohortCanopyLayerProp = new List<float>();
            //List<float> cohortCanopyGrowingSpace = new List<float>();

            if (SiteOutputName != null)
            {
                this.siteoutput = new LocalOutput(SiteOutputName, "Site.csv", Header(site));

                establishmentProbability = new EstablishmentProbability(SiteOutputName, "Establishment.csv");
            }
            else
            {
                establishmentProbability = new EstablishmentProbability(null, null);
            }

            bool biomassProvided = false;
            foreach (Landis.Library.BiomassCohorts.ISpeciesCohorts speciesCohorts in initialCommunity.Cohorts)
            {
                foreach (Landis.Library.BiomassCohorts.ICohort cohort in speciesCohorts)
                {
                    if (cohort.Biomass > 0)  // 0 biomass indicates biomass value was not read in
                    {
                        biomassProvided = true;
                        break;
                    }
                }
            }


            this.StartDate = StartDate; //Zhou

            // Initialize CN site variables
            Initialize_CN_Site(); //ZHOU

            if (biomassProvided && !(initialCommunitiesSpinup.ToLower() == "spinup"))
            {
                List<double> CohortBiomassList = new List<double>();
                List<double> CohortMaxBiomassList = new List<double>();
                if (initialCommunitiesSpinup.ToLower() == "nospinup")
                {
                    foreach (Landis.Library.BiomassCohorts.ISpeciesCohorts speciesCohorts in initialCommunity.Cohorts)
                    {
                        foreach (Landis.Library.BiomassCohorts.ICohort cohort in speciesCohorts)
                        {
                            // TODO: Add warning if biomass is 0
                            bool addCohort = AddNewCohort(new Cohort(SpeciesParameters.SpeciesPnET[cohort.Species], cohort.Age, cohort.Biomass, SiteOutputName, (ushort)(StartDate.Year - cohort.Age), CohortStacking));
                            CohortBiomassList.Add(AllCohorts.Last().AGBiomass);
                            CohortMaxBiomassList.Add(AllCohorts.Last().BiomassMax);
                        }
                    }
                }


                else
                {
                    if ((initialCommunitiesSpinup.ToLower() != "spinuplayers") && (initialCommunitiesSpinup.ToLower() != "spinuplayersrescale"))
                        Globals.ModelCore.UI.WriteLine("Warning:  InitialCommunitiesSpinup parameter is not 'Spinup', 'SpinupLayers','SpinupLayersRescale' or 'NoSpinup'.  Biomass is provided so using 'SpinupLayers' by default.");
                    SpinUp(StartDate, site, initialCommunity, usingClimateLibrary, SiteOutputName, false);
                    // species-age key to store maxbiomass values
                    Dictionary<ISpecies, Dictionary<int, float[]>> cohortDictionary = new Dictionary<ISpecies, Dictionary<int, float[]>>();
                    foreach (Cohort cohort in AllCohorts)
                    {
                        ISpecies spp = cohort.Species;
                        int age = cohort.Age;
                        if (cohortDictionary.ContainsKey(spp))
                        {
                            if (cohortDictionary[spp].ContainsKey(age))
                            {
                                // message duplicate species and age
                            }
                            else
                            {
                                float[] values = new float[] { (int)cohort.BiomassMax, cohort.Biomass };
                                cohortDictionary[spp].Add(age, values);
                            }
                        }
                        else
                        {
                            Dictionary<int, float[]> ageDictionary = new Dictionary<int, float[]>();
                            float[] values = new float[] { (int)cohort.BiomassMax, cohort.Biomass };
                            ageDictionary.Add(age, values);
                            cohortDictionary.Add(spp, ageDictionary);
                        }

                    }
                    ClearAllCohorts();
                    foreach (Landis.Library.BiomassCohorts.ISpeciesCohorts speciesCohorts in initialCommunity.Cohorts)
                    {
                        foreach (Landis.Library.BiomassCohorts.ICohort cohort in speciesCohorts)
                        {
                            // TODO: Add warning if biomass is 0
                            int age = cohort.Age;
                            ISpecies spp = cohort.Species;
                            float[] values = cohortDictionary[spp][age];
                            int cohortMaxBiomass = (int)values[0];
                            float cohortSpinupBiomass = values[1];
                            float inputMaxBiomass = Math.Max(cohortMaxBiomass, cohort.Biomass);
                            if (initialCommunitiesSpinup.ToLower() == "spinuplayersrescale")
                            {
                                inputMaxBiomass = cohortMaxBiomass * (cohort.Biomass / cohortSpinupBiomass);
                            }
                            float cohortCanopyGrowingSpace = 1f;


                            bool addCohort = AddNewCohort(new Cohort(SpeciesParameters.SpeciesPnET[cohort.Species], cohort.Age, cohort.Biomass, (int)inputMaxBiomass, cohortCanopyGrowingSpace, SiteOutputName, (ushort)(StartDate.Year - cohort.Age), CohortStacking));
                            CohortBiomassList.Add(AllCohorts.Last().AGBiomass);
                            CohortMaxBiomassList.Add(AllCohorts.Last().BiomassMax);
                        }
                    }
                }
                bool runAgain = true;
                int attempts = 0;
                while (runAgain)
                {
                    attempts++;
                    bool badSpinup = false;
                    // Sort cohorts into layers                    
                    List<List<double>> cohortBins = GetBinsByCohort(CohortMaxBiomassList);

                    float[] CanopyLAISum = new float[tempMaxCanopyLayers];
                    float[] LayerBiomass = new float[tempMaxCanopyLayers];
                    List<float>[] LayerBiomassValues = new List<float>[tempMaxCanopyLayers];
                    float[] LayerFoliagePotential = new float[tempMaxCanopyLayers];
                    List<float>[] LayerFoliagePotentialValues = new List<float>[tempMaxCanopyLayers];
                    Dictionary<Cohort, float> canopyProportions = new Dictionary<Cohort, float>();
                    CanopyLAI = new float[tempMaxCanopyLayers];
                    List<double> NewCohortMaxBiomassList = new List<double>();
                    foreach (Cohort cohort in AllCohorts)
                    {
                        int layerIndex = 0;
                        foreach (List<double> layerBiomassList in cohortBins)
                        {
                            if (layerBiomassList.Contains(cohort.BiomassMax))
                            {
                                cohort.Layer = (byte)layerIndex;
                                // if "ground" then ensure cohort.Layer = 0
                                if (cohort.SpeciesPnET.Lifeform.ToLower().Contains("ground"))
                                {
                                    cohort.Layer = 0;
                                }
                                break;
                            }
                            layerIndex++;
                        }
                        int layer = cohort.Layer;
                        int layerCount = cohortBins[layer].Count();

                        // Estimate new wood biomass from FracBelowG and FrActWs (see Estimate_WoodBio.xlsx)
                        float estSlope = -8.236285f + 27.768424f * cohort.SpeciesPnET.FracBelowG + 191053.281571f * cohort.SpeciesPnET.FrActWd + 312.812679f * cohort.SpeciesPnET.FracFol + -594492.216284f * cohort.SpeciesPnET.FracBelowG * cohort.SpeciesPnET.FrActWd + -941.447695f * cohort.SpeciesPnET.FracBelowG * cohort.SpeciesPnET.FracFol + -6490254.134756f * cohort.SpeciesPnET.FrActWd * cohort.SpeciesPnET.FracFol + 19879995.810771f * cohort.SpeciesPnET.FracBelowG * cohort.SpeciesPnET.FrActWd * cohort.SpeciesPnET.FracFol;
                        float estInt = 1735.179f + 2994.393f * cohort.SpeciesPnET.FracBelowG + 10167232.544f * cohort.SpeciesPnET.FrActWd + 53598.871f * cohort.SpeciesPnET.FracFol + -92028081.987f * cohort.SpeciesPnET.FracBelowG * cohort.SpeciesPnET.FrActWd + -168141.498f * cohort.SpeciesPnET.FracBelowG * cohort.SpeciesPnET.FracFol + -1104139533.563f * cohort.SpeciesPnET.FrActWd * cohort.SpeciesPnET.FracFol + 3507005746.011f * cohort.SpeciesPnET.FracBelowG * cohort.SpeciesPnET.FrActWd * cohort.SpeciesPnET.FracFol;
                        float newWoodBiomass = estInt + estSlope * cohort.AGBiomass * layerCount; // Inflate AGBiomass by # of cohorts in layer, assuming equal space among them
                        float newTotalBiomass = newWoodBiomass / (1 - cohort.SpeciesPnET.FracBelowG);

                        //if(newTotalBiomass > 0)
                        //    cohort.ChangeBiomass((int)Math.Round(newTotalBiomass - cohort.TotalBiomass));
                        cohort.CanopyLayerProp = 1f / layerCount;
                        if (CohortStacking)
                            cohort.CanopyLayerProp = 1.0f;
                        float cohortFol = cohort.adjFracFol * cohort.FActiveBiom * cohort.TotalBiomass;
                        float cohortLAI = 0;

                        for (int i = 0; i < Globals.IMAX; i++)
                            cohortLAI += cohort.CalculateLAI(cohort.SpeciesPnET, cohortFol, i, cohortLAI);
                        cohortLAI = Math.Min(cohortLAI, cohort.SpeciesPnET.MaxLAI);
                        cohort.LastLAI = cohortLAI;

                        //float rescaleFactor = (1f / layerCount)/(cohort.LastLAI / cohort.SpeciesPnET.MaxLAI);
                        //newTotalBiomass = newTotalBiomass * rescaleFactor;
                        //cohort.ChangeBiomass((int)Math.Round(newTotalBiomass - cohort.TotalBiomass));

                        cohort.CanopyGrowingSpace = Math.Min(cohort.CanopyGrowingSpace, 1.0f);
                        float cohortLAIRatio = Math.Min(cohortLAI / cohort.SpeciesPnET.MaxLAI, cohort.CanopyGrowingSpace);
                        canopyProportions.Add(cohort, cohortLAIRatio);
                        //cohort.CanopyLayerProp = cohortLAIRatio;
                        cohort.NSC = cohort.SpeciesPnET.DNSC * cohort.FActiveBiom * (cohort.TotalBiomass + cohort.Fol) * cohort.SpeciesPnET.CFracBiomass;
                        cohort.Fol = cohortFol * (1 - cohort.SpeciesPnET.TOfol);
                        if (LayerFoliagePotentialValues[layer] == null)
                        {
                            LayerFoliagePotentialValues[layer] = new List<float>();
                        }
                        LayerFoliagePotentialValues[layer].Add(cohortLAIRatio);
                        LayerFoliagePotential[layer] += (cohortLAIRatio);
                    }

                    // Adjust cohort biomass values so that site-values equal input biomass
                    float[] LayerFoliagePotentialAdj = new float[tempMaxCanopyLayers];
                    int index = 0;
                    foreach (Cohort cohort in AllCohorts)
                    {
                        int layer = cohort.Layer;
                        int layerCount = cohortBins[layer].Count();
                        float denomSum = 0f;
                        float canopyLayerProp = Math.Min(canopyProportions[cohort], cohort.CanopyGrowingSpace);

                        canopyLayerProp = Math.Min(canopyProportions[cohort], 1f / layerCount);
                        if (LayerFoliagePotential[layer] > 1)  // Zhou, needed revisiting
                        {
                            float canopyLayerPropAdj = canopyProportions[cohort] / LayerFoliagePotential[layer];
                            canopyLayerProp = canopyProportions[cohort] / LayerFoliagePotential[layer];
                            canopyLayerProp = (canopyLayerPropAdj - canopyProportions[cohort]) * CanopySumScale + canopyProportions[cohort];
                            //canopyLayerProp = cohort.Biomass/BiomassSite; // Zhou
                            cohort.CanopyGrowingSpace = canopyLayerProp;
                        }
                        else
                            cohort.CanopyGrowingSpace = 1f;
                        cohort.CanopyLayerProp = Math.Min(canopyProportions[cohort], canopyLayerProp);
                        if(CohortStacking)
                        {
                            canopyLayerProp = 1.0f;
                            cohort.CanopyLayerProp = 1.0f;
                            cohort.CanopyGrowingSpace = 1.0f;
                        }

                        float targetBiomass = (float)CohortBiomassList[index];
                        float newWoodBiomass = targetBiomass / cohort.CanopyLayerProp;
                        float newTotalBiomass = (newWoodBiomass) / (1 - cohort.SpeciesPnET.FracBelowG);
                        cohort.ChangeBiomass((int)Math.Round((newTotalBiomass - cohort.TotalBiomass) / 2f));
                        float cohortFoliage = cohort.adjFracFol * cohort.FActiveBiom * cohort.TotalBiomass;
                        float cohortLAI = 0;
                        for (int i = 0; i < Globals.IMAX; i++)
                            cohortLAI += cohort.CalculateLAI(cohort.SpeciesPnET, cohortFoliage, i, cohortLAI);
                        cohortLAI = Math.Min(cohortLAI, cohort.SpeciesPnET.MaxLAI);
                        cohort.LastLAI = cohortLAI;
                        cohort.CanopyGrowingSpace = Math.Min(cohort.CanopyGrowingSpace, 1.0f);
                        cohort.CanopyLayerProp = Math.Min(cohort.LastLAI / cohort.SpeciesPnET.MaxLAI, cohort.CanopyGrowingSpace);
                        if (CohortStacking)
                        {
                            canopyLayerProp = 1.0f;
                            cohort.CanopyLayerProp = 1.0f;
                            cohort.CanopyGrowingSpace = 1.0f;
                        }
                        float cohortFol = cohort.adjFracFol * cohort.FActiveBiom * cohort.TotalBiomass;
                        cohort.Fol = cohortFol * (1 - cohort.SpeciesPnET.TOfol);
                        cohort.NSC = cohort.SpeciesPnET.DNSC * cohort.FActiveBiom * (cohort.TotalBiomass + cohort.Fol) * cohort.SpeciesPnET.CFracBiomass;

                        // Check cohort.Biomass
                        LayerFoliagePotentialAdj[layer] += (cohort.CanopyLayerProp);
                        //CanopyLAISum[layer] += (cohort.LAI.Sum() * ((1 - cohort.SpeciesPnET.FracBelowG) * cohort.TotalBiomass));
                        CanopyLAISum[layer] += (cohort.LAI.Sum() * cohort.CanopyLayerProp);
                        //LayerBiomass[layer] += ((1 - cohort.SpeciesPnET.FracBelowG) * cohort.TotalBiomass);
                        LayerBiomass[layer] += (cohort.CanopyLayerProp * cohort.TotalBiomass);
                        index++;
                        NewCohortMaxBiomassList.Add(cohort.BiomassMax);
                    }
                    //Re-sort layers
                    cohortBins = GetBinsByCohort(NewCohortMaxBiomassList);
                    float[] CanopyLayerSum = new float[tempMaxCanopyLayers];
                    List<double> FinalCohortMaxBiomassList = new List<double>();
                    // Assign new layers
                    foreach (Cohort cohort in AllCohorts)
                    {
                        int layerIndex = 0;
                        foreach (List<double> layerBiomassList in cohortBins)
                        {
                            if (layerBiomassList.Contains(cohort.BiomassMax))
                            {
                                cohort.Layer = (byte)layerIndex;
                                // if "ground" then ensure cohort.Layer = 0
                                if (cohort.SpeciesPnET.Lifeform.ToLower().Contains("ground"))
                                {
                                    cohort.Layer = 0;
                                }
                                break;
                            }
                            layerIndex++;
                        }
                    }
                    // Calculate new layer prop
                    float[] MainLayerCanopyProp = new float[tempMaxCanopyLayers];
                    foreach (Cohort c in AllCohorts)
                    {
                        int layerIndex = c.Layer;
                        float LAISum = c.LAI.Sum();
                        if (c.Leaf_On)
                        {
                            if (LAISum > c.LastLAI)
                                c.LastLAI = LAISum;
                        }
                        if(CohortStacking)
                            MainLayerCanopyProp[layerIndex] += 1.0f;
                        else
                            MainLayerCanopyProp[layerIndex] += Math.Min(c.LastLAI / c.SpeciesPnET.MaxLAI, c.CanopyGrowingSpace);
                    }
                    int cohortIndex = 0;

                    float canopySumScale = CanopySumScale;

                    foreach (Cohort cohort in AllCohorts)
                    {
                        int layer = cohort.Layer;
                        int layerCount = cohortBins[layer].Count();

                        float targetBiomass = (float)CohortBiomassList[cohortIndex];

                        float canopyLayerProp = Math.Min(cohort.LastLAI / cohort.SpeciesPnET.MaxLAI, cohort.CanopyGrowingSpace);
                        if (MainLayerCanopyProp[layer] > 1)
                        {
                            float canopyLayerPropAdj = cohort.CanopyLayerProp / MainLayerCanopyProp[layer];
                            canopyLayerProp = (canopyLayerPropAdj - cohort.CanopyLayerProp) * canopySumScale + cohort.CanopyLayerProp;
                            canopyLayerProp = cohort.CanopyLayerProp / MainLayerCanopyProp[layer];
                            //canopyLayerProp = cohort.Biomass / BiomassSite; // Zhou
                            cohort.CanopyGrowingSpace = Math.Min(cohort.CanopyGrowingSpace, canopyLayerProp);
                        }
                        else
                        {
                            cohort.CanopyGrowingSpace = 1f;
                        }
                        if (CohortStacking)
                        {
                            canopyLayerProp = 1.0f;
                            cohort.CanopyLayerProp = 1.0f;
                            cohort.CanopyGrowingSpace = 1.0f;
                        }
                        float newWoodBiomass = targetBiomass / canopyLayerProp;
                        float newTotalBiomass = (newWoodBiomass) / (1 - cohort.SpeciesPnET.FracBelowG);
                        float changeAmount = newTotalBiomass - cohort.TotalBiomass;
                        float tempFActiveBiom = (float)Math.Exp(-cohort.SpeciesPnET.FrActWd * newTotalBiomass);
                        float cohortTempFoliage = cohort.adjFracFol * tempFActiveBiom * newTotalBiomass;
                        float cohortTempLAI = 0;
                        for (int i = 0; i < Globals.IMAX; i++)
                            cohortTempLAI += cohort.CalculateLAI(cohort.SpeciesPnET, cohortTempFoliage, i, cohortTempLAI);
                        cohortTempLAI = Math.Min(cohortTempLAI, cohort.SpeciesPnET.MaxLAI);

                        float tempBiomass = newTotalBiomass * (1 - cohort.SpeciesPnET.FracBelowG) * Math.Min(cohortTempLAI / cohort.SpeciesPnET.MaxLAI, canopyLayerProp);
                        //bool match = ((int)tempBiomass == (int)targetBiomass);
                        float diff = tempBiomass - targetBiomass;
                        float lastDiff = diff;
                        bool match = (Math.Abs(tempBiomass - targetBiomass) < 2);
                        float multiplierRoot = 1f;
                        while (!match)
                        {
                            float multiplier = multiplierRoot;
                            if ((Math.Abs(tempBiomass - targetBiomass)) > 1000)
                                multiplier = multiplierRoot * 200f;
                            else if ((Math.Abs(tempBiomass - targetBiomass)) > 500)
                                multiplier = multiplierRoot * 100f;
                            else if ((Math.Abs(tempBiomass - targetBiomass)) > 100)
                                multiplier = multiplierRoot * 20f;
                            else if ((Math.Abs(tempBiomass - targetBiomass)) > 50)
                                multiplier = multiplierRoot * 10f;
                            else if ((Math.Abs(tempBiomass - targetBiomass)) > 10)
                                multiplier = multiplierRoot * 2f;
                            lastDiff = diff;
                            if (tempBiomass > targetBiomass)
                            {
                                newTotalBiomass = Math.Max(newTotalBiomass - multiplier, 1);
                            }
                            else
                            {
                                newTotalBiomass = Math.Max(newTotalBiomass + multiplier, 1);
                            }
                            changeAmount = newTotalBiomass - cohort.TotalBiomass;
                            tempFActiveBiom = (float)Math.Exp(-cohort.SpeciesPnET.FrActWd * newTotalBiomass);
                            cohortTempFoliage = cohort.adjFracFol * tempFActiveBiom * newTotalBiomass;
                            cohortTempLAI = 0;
                            for (int i = 0; i < Globals.IMAX; i++)
                                cohortTempLAI += cohort.CalculateLAI(cohort.SpeciesPnET, cohortTempFoliage, i, cohortTempLAI);
                            cohortTempLAI = Math.Min(cohortTempLAI, cohort.SpeciesPnET.MaxLAI);
                            tempBiomass = newTotalBiomass * (1 - cohort.SpeciesPnET.FracBelowG) * Math.Min(cohortTempLAI / cohort.SpeciesPnET.MaxLAI, canopyLayerProp);
                            diff = tempBiomass - targetBiomass;
                            if ((Math.Abs(diff) > Math.Abs(lastDiff)))
                            {
                                break;
                            }
                            if ((attempts < 3) && ((tempBiomass <= 0) || (float.IsNaN(tempBiomass))))
                            {
                                badSpinup = true;
                                break;
                            }
                            //match = ((int)tempBiomass == (int)targetBiomass);

                            match = (Math.Abs(tempBiomass - targetBiomass) < 2);
                        }

                        cohort.ChangeBiomass((int)Math.Round((newTotalBiomass - cohort.TotalBiomass) * 1f / (1f)));
                        float cohortFoliage = cohort.adjFracFol * cohort.FActiveBiom * cohort.TotalBiomass;
                        float cohortLAI = 0;
                        for (int i = 0; i < Globals.IMAX; i++)
                            cohortLAI += cohort.CalculateLAI(cohort.SpeciesPnET, cohortFoliage, i, cohortLAI);
                        cohortLAI = Math.Min(cohortLAI, cohort.SpeciesPnET.MaxLAI);
                        cohort.LastLAI = cohortLAI;
                        cohort.CanopyLayerProp = Math.Min(cohort.LastLAI / cohort.SpeciesPnET.MaxLAI, cohort.CanopyGrowingSpace);
                        if (CohortStacking)
                            cohort.CanopyLayerProp = 1.0f;
                        CanopyLayerSum[layer] += (cohort.CanopyLayerProp);
                        //float cohortFol = cohort.adjFracFol * cohort.FActiveBiom * cohort.TotalBiomass;
                        cohort.Fol = cohortFoliage * (1 - cohort.SpeciesPnET.TOfol);
                        cohort.NSC = cohort.SpeciesPnET.DNSC * cohort.FActiveBiom * (cohort.TotalBiomass + cohort.Fol) * cohort.SpeciesPnET.CFracBiomass;

                        cohortIndex++;
                        FinalCohortMaxBiomassList.Add(cohort.BiomassMax);
                    }

                    //Re-sort layers
                    cohortBins = GetBinsByCohort(FinalCohortMaxBiomassList);
                    // Assign new layers
                    foreach (Cohort cohort in AllCohorts)
                    {
                        int layerIndex = 0;
                        foreach (List<double> layerBiomassList in cohortBins)
                        {
                            if (layerBiomassList.Contains(cohort.BiomassMax))
                            {
                                cohort.Layer = (byte)layerIndex;
                                // if "ground" then ensure cohort.Layer = 0
                                if (cohort.SpeciesPnET.Lifeform.ToLower().Contains("ground"))
                                {
                                    cohort.Layer = 0;
                                }
                                break;
                            }
                            layerIndex++;
                        }
                    }
                    // Calculate new layer prop
                    MainLayerCanopyProp = new float[tempMaxCanopyLayers];
                    foreach (Cohort c in AllCohorts)
                    {
                        int layerIndex = c.Layer;
                        float LAISum = c.LAI.Sum();
                        if (c.Leaf_On)
                        {
                            if (LAISum > c.LastLAI)
                                c.LastLAI = LAISum;
                        }
                        if(CohortStacking)
                            MainLayerCanopyProp[layerIndex] += 1.0f;
                        else
                            MainLayerCanopyProp[layerIndex] += Math.Min(c.LastLAI / c.SpeciesPnET.MaxLAI, c.CanopyGrowingSpace);
                    }

                    CanopyLayerSum = new float[tempMaxCanopyLayers];

                    cohortIndex = 0;
                    foreach (Cohort cohort in AllCohorts)
                    {
                        int layer = cohort.Layer;
                        int layerCount = cohortBins[layer].Count();

                        float targetBiomass = (float)CohortBiomassList[cohortIndex];

                        float canopyLayerProp = Math.Min(cohort.LastLAI / cohort.SpeciesPnET.MaxLAI, cohort.CanopyGrowingSpace);
                        if (MainLayerCanopyProp[layer] > 1)
                        {
                            float canopyLayerPropAdj = cohort.CanopyLayerProp / MainLayerCanopyProp[layer];
                            canopyLayerProp = (canopyLayerPropAdj - cohort.CanopyLayerProp) * canopySumScale + cohort.CanopyLayerProp;
                            //canopyLayerProp = cohort.CanopyLayerProp / MainLayerCanopyProp[layer];
                            cohort.CanopyGrowingSpace = Math.Min(cohort.CanopyGrowingSpace, canopyLayerProp);
                        }
                        else
                        {
                            cohort.CanopyGrowingSpace = 1f;
                        }
                        if (CohortStacking)
                        {
                            canopyLayerProp = 1.0f;
                            cohort.CanopyLayerProp = 1.0f;
                            cohort.CanopyGrowingSpace = 1.0f;
                        }
                        float newWoodBiomass = targetBiomass / canopyLayerProp;
                        float newTotalBiomass = (newWoodBiomass) / (1 - cohort.SpeciesPnET.FracBelowG);
                        float changeAmount = newTotalBiomass - cohort.TotalBiomass;
                        float tempMaxBio = Math.Max(cohort.BiomassMax, newTotalBiomass);
                        float tempFActiveBiom = (float)Math.Exp(-cohort.SpeciesPnET.FrActWd * tempMaxBio);
                        float cohortTempFoliage = cohort.adjFracFol * tempFActiveBiom * newTotalBiomass;
                        float cohortTempLAI = 0;
                        for (int i = 0; i < Globals.IMAX; i++)
                            cohortTempLAI += cohort.CalculateLAI(cohort.SpeciesPnET, cohortTempFoliage, i, cohortTempLAI);
                        cohortTempLAI = Math.Min(cohortTempLAI, cohort.SpeciesPnET.MaxLAI);

                        float tempBiomass = (newTotalBiomass * (1 - cohort.SpeciesPnET.FracBelowG) + cohortTempFoliage) * Math.Min(cohortTempLAI / cohort.SpeciesPnET.MaxLAI, cohort.CanopyGrowingSpace);

                        if ((attempts < 3) && ((tempBiomass <= 0) || (float.IsNaN(tempBiomass))))
                        {
                            badSpinup = true;
                            break;

                        }
                        //bool match = ((int)tempBiomass == (int)targetBiomass);
                        float diff = tempBiomass - targetBiomass;
                        float lastDiff = diff;
                        bool match = (Math.Abs(tempBiomass - targetBiomass) < 2);
                        int loopCount = 0;
                        while (!match)
                        {
                            float multiplier = 1f;
                            if ((Math.Abs(tempBiomass - targetBiomass)) > 1000)
                                multiplier = 200f;
                            else if ((Math.Abs(tempBiomass - targetBiomass)) > 500)
                                multiplier = 100f;
                            else if ((Math.Abs(tempBiomass - targetBiomass)) > 100)
                                multiplier = 20f;
                            else if ((Math.Abs(tempBiomass - targetBiomass)) > 50)
                                multiplier = 10f;
                            else if ((Math.Abs(tempBiomass - targetBiomass)) > 10)
                                multiplier = 2f;
                            if (tempBiomass > targetBiomass)
                            {
                                newTotalBiomass = Math.Max(newTotalBiomass - multiplier, 1);
                            }
                            else
                            {
                                newTotalBiomass = Math.Max(newTotalBiomass + multiplier, 1);
                            }
                            changeAmount = newTotalBiomass - cohort.TotalBiomass;
                            tempMaxBio = Math.Max(cohort.BiomassMax, newTotalBiomass);
                            tempFActiveBiom = (float)Math.Exp(-cohort.SpeciesPnET.FrActWd * tempMaxBio);
                            cohortTempFoliage = cohort.adjFracFol * tempFActiveBiom * newTotalBiomass;
                            cohortTempLAI = 0;
                            for (int i = 0; i < Globals.IMAX; i++)
                                cohortTempLAI += cohort.CalculateLAI(cohort.SpeciesPnET, cohortTempFoliage, i, cohortTempLAI);
                            cohortTempLAI = Math.Min(cohortTempLAI, cohort.SpeciesPnET.MaxLAI);
                            tempBiomass = (newTotalBiomass * (1 - cohort.SpeciesPnET.FracBelowG) + cohortTempFoliage) * Math.Min(cohortTempLAI / cohort.SpeciesPnET.MaxLAI, cohort.CanopyGrowingSpace);
                            diff = tempBiomass - targetBiomass;
                            if ((Math.Abs(diff) > Math.Abs(lastDiff)))
                            {
                                if ((Math.Abs(diff) / targetBiomass > 0.10) && (attempts < 3))
                                    badSpinup = true;
                                break;
                            }
                            if ((attempts < 3) && ((tempBiomass <= 0) || (float.IsNaN(tempBiomass))))
                            {
                                badSpinup = true;
                                break;
                            }
                            //match = ((int)tempBiomass == (int)targetBiomass);
                            match = (Math.Abs(tempBiomass - targetBiomass) < 2);
                            loopCount++;
                            if (loopCount > 1000)
                                break;
                        }
                        if (badSpinup)
                            break;
                        if (loopCount <= 1000)
                        {
                            float cohortFoliage = cohort.adjFracFol * tempFActiveBiom * newTotalBiomass;
                            cohort.Fol = cohortFoliage;
                            cohort.ChangeBiomass((int)Math.Round((newTotalBiomass - cohort.TotalBiomass) * 1f / (1f)));
                        }else
                        {
                            cohort.Fol = cohort.adjFracFol * cohort.FActiveBiom * cohort.TotalBiomass;
                        }

                            // Calculate limit of maximum biomass based on minimum foliage/total biomass ratios from Jenkins (reduced by half to be not so strict)
                            //float maxBiomassLimit = (float)(Math.Log((ratioLimit * (1 - SpeciesParameters.SpeciesPnET[cohort.Species].FracBelowG)) / SpeciesParameters.SpeciesPnET[cohort.Species].FracFol) / (-1f * SpeciesParameters.SpeciesPnET[cohort.Species].FrActWd));


                            float cohortLAI = 0;
                            for (int i = 0; i < Globals.IMAX; i++)
                                cohortLAI += cohort.CalculateLAI(cohort.SpeciesPnET, cohort.Fol, i, cohortLAI);
                            cohortLAI = Math.Min(cohortLAI, cohort.SpeciesPnET.MaxLAI);
                            cohort.LastLAI = cohortLAI;
                            cohort.CanopyLayerProp = Math.Min(cohort.LastLAI / cohort.SpeciesPnET.MaxLAI, cohort.CanopyGrowingSpace);
                            if (CohortStacking)
                                cohort.CanopyLayerProp = 1.0f;
                            CanopyLayerSum[layer] += (cohort.CanopyLayerProp);
                            //float cohortFol = cohort.adjFracFol * cohort.FActiveBiom * cohort.TotalBiomass;
                            cohort.Fol = cohort.Fol * (1 - cohort.SpeciesPnET.TOfol);
                            cohort.NSC = cohort.SpeciesPnET.DNSC * cohort.FActiveBiom * (cohort.TotalBiomass + cohort.Fol) * cohort.SpeciesPnET.CFracBiomass;
                        
                        float fol_total_ratio = cohort.Fol / (cohort.Fol + cohort.Wood);
                        // Calculate minimum foliage/total biomass ratios from Jenkins (reduced by MinFolRatioFactor to be not so strict)
                        float ratioLimit = 0;
                        if (SpeciesParameters.SpeciesPnET[cohort.Species].SLWDel == 0) //Conifer
                        {
                            ratioLimit = 0.057f * minFolRatioFactor;
                        }
                        else
                        {
                            ratioLimit = 0.019f * minFolRatioFactor;
                        }
                        if ((attempts < 3) && (fol_total_ratio < ratioLimit))
                        {
                            badSpinup = true;
                            break;
                            //Globals.ModelCore.UI.WriteLine("Warning:");
                        }
                        cohortIndex++;
                    }
                    if (badSpinup)
                    {
                        if ((initialCommunitiesSpinup.ToLower() == "spinuplayers") && (attempts < 2))
                        {
                            Globals.ModelCore.UI.WriteLine("");
                            Globals.ModelCore.UI.WriteLine("Warning: initial community " + initialCommunity.MapCode + " could not initialize properly using SpinupLayers.  Processing with SpinupLayersRescale option instead.");
                            ClearAllCohorts();
                            SpinUp(StartDate, site, initialCommunity, usingClimateLibrary, null, false);
                            // species-age key to store maxbiomass values and canopy growing space
                            Dictionary<ISpecies, Dictionary<int, float[]>> cohortDictionary = new Dictionary<ISpecies, Dictionary<int, float[]>>();
                            foreach (Cohort cohort in AllCohorts)
                            {
                                ISpecies spp = cohort.Species;
                                int age = cohort.Age;
                                if (cohortDictionary.ContainsKey(spp))
                                {
                                    if (cohortDictionary[spp].ContainsKey(age))
                                    {
                                        // FIXME - message duplicate species and age
                                    }
                                    else
                                    {
                                        float[] values = new float[] { (int)cohort.BiomassMax, cohort.Biomass };
                                        cohortDictionary[spp].Add(age, values);
                                    }
                                }
                                else
                                {
                                    Dictionary<int, float[]> ageDictionary = new Dictionary<int, float[]>();
                                    float[] values = new float[] { (int)cohort.BiomassMax, cohort.Biomass };
                                    ageDictionary.Add(age, values);
                                    cohortDictionary.Add(spp, ageDictionary);
                                }
                            }

                            ClearAllCohorts();
                            CohortBiomassList = new List<double>();
                            CohortMaxBiomassList = new List<double>();
                            foreach (Landis.Library.BiomassCohorts.ISpeciesCohorts speciesCohorts in initialCommunity.Cohorts)
                            {
                                foreach (Landis.Library.BiomassCohorts.ICohort cohort in speciesCohorts)
                                {
                                    int age = cohort.Age;
                                    ISpecies spp = cohort.Species;
                                    float[] values = cohortDictionary[spp][age];
                                    int cohortMaxBiomass = (int)values[0];
                                    float cohortSpinupBiomass = values[1];
                                    float lastSeasonAvgFrad = values[2];
                                    float inputMaxBiomass = Math.Max(cohortMaxBiomass, cohort.Biomass);
                                    inputMaxBiomass = cohortMaxBiomass * (cohort.Biomass / cohortSpinupBiomass);
                                    float cohortCanopyGrowingSpace = 1f;
                                    bool addCohort = AddNewCohort(new Cohort(SpeciesParameters.SpeciesPnET[cohort.Species], cohort.Age, cohort.Biomass, (int)inputMaxBiomass, cohortCanopyGrowingSpace, SiteOutputName, (ushort)(StartDate.Year - cohort.Age),CohortStacking));
                                    CohortBiomassList.Add(AllCohorts.Last().AGBiomass);
                                    CohortMaxBiomassList.Add(AllCohorts.Last().BiomassMax);
                                    AllCohorts.Last().SetAvgFRad(lastSeasonAvgFrad);
                                }
                            }
                            badSpinup = false;
                        }
                        else if ((initialCommunitiesSpinup.ToLower() == "spinuplayersrescale") && (attempts < 2))
                        {
                            Globals.ModelCore.UI.WriteLine("");
                            Globals.ModelCore.UI.WriteLine("Warning: initial community " + initialCommunity.MapCode + " could not initialize properly using SpinupLayersRescale.  Processing with SpinupLayers option instead.");
                            ClearAllCohorts();
                            SpinUp(StartDate, site, initialCommunity, usingClimateLibrary, null, false);
                            // species-age key to store maxbiomass values and canopy growing space
                            Dictionary<ISpecies, Dictionary<int, float[]>> cohortDictionary = new Dictionary<ISpecies, Dictionary<int, float[]>>();
                            foreach (Cohort cohort in AllCohorts)
                            {
                                ISpecies spp = cohort.Species;
                                int age = cohort.Age;
                                if (cohortDictionary.ContainsKey(spp))
                                {
                                    if (cohortDictionary[spp].ContainsKey(age))
                                    {
                                        // FIXME - message duplicate species and age
                                    }
                                    else
                                    {
                                        float[] values = new float[] { (int)cohort.BiomassMax, cohort.Biomass };
                                        cohortDictionary[spp].Add(age, values);
                                    }
                                }
                                else
                                {
                                    Dictionary<int, float[]> ageDictionary = new Dictionary<int, float[]>();
                                    float[] values = new float[] { (int)cohort.BiomassMax, cohort.Biomass };
                                    ageDictionary.Add(age, values);
                                    cohortDictionary.Add(spp, ageDictionary);
                                }

                            }
                            ClearAllCohorts();
                            CohortBiomassList = new List<double>();
                            CohortMaxBiomassList = new List<double>();
                            foreach (Landis.Library.BiomassCohorts.ISpeciesCohorts speciesCohorts in initialCommunity.Cohorts)
                            {
                                foreach (Landis.Library.BiomassCohorts.ICohort cohort in speciesCohorts)
                                {
                                    int age = cohort.Age;
                                    ISpecies spp = cohort.Species;
                                    float[] values = cohortDictionary[spp][age];
                                    int cohortMaxBiomass = (int)values[0];
                                    float cohortSpinupBiomass = values[1];
                                    float lastSeasonAvgFrad = values[2];
                                    float inputMaxBiomass = Math.Max(cohortMaxBiomass, cohort.Biomass);
                                    float cohortCanopyGrowingSpace = 1f;
                                    bool addCohort = AddNewCohort(new Cohort(SpeciesParameters.SpeciesPnET[cohort.Species], cohort.Age, cohort.Biomass, (int)inputMaxBiomass, cohortCanopyGrowingSpace, SiteOutputName, (ushort)(StartDate.Year - cohort.Age), CohortStacking));
                                    CohortBiomassList.Add(AllCohorts.Last().AGBiomass);
                                    CohortMaxBiomassList.Add(AllCohorts.Last().BiomassMax);
                                    AllCohorts.Last().SetAvgFRad(lastSeasonAvgFrad);
                                }
                            }
                            badSpinup = false;
                        }
                        else // NoSpinup or secondAttempt
                        {
                            Globals.ModelCore.UI.WriteLine("");
                            if (initialCommunitiesSpinup.ToLower() == "nospinup")
                            {
                                Globals.ModelCore.UI.WriteLine("Warning: initial community " + initialCommunity.MapCode + " could not initialize properly on first attempt using NoSpinup. Reprocessing.");
                            }
                            else
                            {
                                Globals.ModelCore.UI.WriteLine("Warning: initial community " + initialCommunity.MapCode + " could not initialize properly using SpinupLayers or SpinupLayersRescale.  Processing with NoSpinup option instead.");
                            }
                            ClearAllCohorts();
                            CohortBiomassList = new List<double>();
                            CohortMaxBiomassList = new List<double>();
                            foreach (Landis.Library.BiomassCohorts.ISpeciesCohorts speciesCohorts in initialCommunity.Cohorts)
                            {
                                foreach (Landis.Library.BiomassCohorts.ICohort cohort in speciesCohorts)
                                {
                                    // TODO: Add warning if biomass is 0
                                    bool addCohort = AddNewCohort(new Cohort(SpeciesParameters.SpeciesPnET[cohort.Species], cohort.Age, cohort.Biomass, SiteOutputName, (ushort)(StartDate.Year - cohort.Age), CohortStacking));
                                    CohortBiomassList.Add(AllCohorts.Last().AGBiomass);
                                    CohortMaxBiomassList.Add(AllCohorts.Last().BiomassMax);
                                }
                            }
                        }

                    }
                    else
                    {
                        for (int layer = 0; layer < tempMaxCanopyLayers; layer++)
                        {
                            /*if (LayerBiomass[layer] > 0)
                                CanopyLAI[layer] = CanopyLAISum[layer] / LayerBiomass[layer];
                            else
                                CanopyLAI[layer] = 0;*/


                        }
                        //this.canopylaimax = CanopyLAI.Sum();
                        this.canopylaimax = CanopyLAISum.Sum();

                        //CalculateInitialWater(StartDate);
                        runAgain = false;
                    }

                }
            }
            else
            {
                SpinUp(StartDate, site, initialCommunity, usingClimateLibrary, SiteOutputName);
            }
        }

        // Constructor for SiteCohorts that have an initial site already set up
        public SiteCohorts(DateTime StartDate, ActiveSite site, ICommunity initialCommunity, string SiteOutputName = null)
        {
            this.Ecoregion = EcoregionData.GetPnETEcoregion(Globals.ModelCore.Ecoregion[site]);
            this.Site = site;
            cohorts = new Dictionary<ISpecies, List<Cohort>>();
            SpeciesEstablishedByPlant = new List<ISpecies>();
            SpeciesEstablishedBySerotiny = new List<ISpecies>();
            SpeciesEstablishedByResprout = new List<ISpecies>();
            SpeciesEstablishedBySeed = new List<ISpecies>();
            CohortsKilledBySuccession = new List<int>(new int[Globals.ModelCore.Species.Count()]);
            CohortsKilledByCold = new List<int>(new int[Globals.ModelCore.Species.Count()]);
            CohortsKilledByHarvest = new List<int>(new int[Globals.ModelCore.Species.Count()]);
            CohortsKilledByFire = new List<int>(new int[Globals.ModelCore.Species.Count()]);
            CohortsKilledByWind = new List<int>(new int[Globals.ModelCore.Species.Count()]);
            CohortsKilledByOther = new List<int>(new int[Globals.ModelCore.Species.Count()]);
            DisturbanceTypesReduced = new List<ExtensionType>();

            uint key = ComputeKey((ushort)initialCommunity.MapCode, Globals.ModelCore.Ecoregion[site].MapCode);

            this.StartDate = StartDate; //Zhou
            //Initialize_CN_Site(); //ZHOU

            if (initialSites.ContainsKey(key))
            {
                if (SiteOutputName != null)
                {
                    this.siteoutput = new LocalOutput(SiteOutputName, "Site.csv", Header(site));

                    establishmentProbability = new EstablishmentProbability(SiteOutputName, "Establishment.csv");
                }
                else
                {

                    establishmentProbability = new EstablishmentProbability(null, null);
                }

                subcanopypar = initialSites[key].subcanopypar;
                subcanopyparmax = initialSites[key].SubCanopyParMAX;
                wateravg = initialSites[key].wateravg;

                hydrology = new Hydrology(initialSites[key].hydrology.Water);

                SiteVars.WoodyDebris[Site] = SiteVars.WoodyDebris[initialSites[key].Site].Clone();
                SiteVars.Litter[Site] = SiteVars.Litter[initialSites[key].Site].Clone();
                SiteVars.FineFuels[Site] = SiteVars.Litter[Site].Mass;
                //PlugIn.PressureHead[Site] = hydrology.GetPressureHead(this.Ecoregion);

                // Soil N pools initialization //ZHOU
                SiteVars.HOMSite[Site] = SiteVars.SiteCohorts[initialSites[key].Site].HOM;
                SiteVars.HONSite[Site] = SiteVars.SiteCohorts[initialSites[key].Site].HON;
                SiteVars.NH4Site[Site] = SiteVars.SiteCohorts[initialSites[key].Site].NH4;
                SiteVars.NO3Site[Site] = SiteVars.SiteCohorts[initialSites[key].Site].NO3;

                SiteVars.NRatioSite[Site] = SiteVars.SiteCohorts[initialSites[key].Site].NRatioSite;
                SiteVars.NRatioNit[Site] = SiteVars.SiteCohorts[initialSites[key].Site].NRatioNit;

                /**/
                this.canopylaimax = initialSites[key].CanopyLAImax;
                List<float> cohortBiomassLayerProp = new List<float>();
                List<float> cohortCanopyLayerProp = new List<float>();
                List<float> cohortCanopyGrowingSpace = new List<float>();
                List<float> cohortLastLAI = new List<float>();
                List<float> cohortLastWoodySenescence = new List<float>();
                List<float> cohortLastFolSenescence = new List<float>();

                foreach (ISpecies spc in initialSites[key].cohorts.Keys)
                {
                    foreach (Cohort cohort in initialSites[key].cohorts[spc])
                    {
                        bool addCohort = false;

                        if (SiteOutputName != null)
                        {
                            addCohort = AddNewCohort(new Cohort(cohort, (ushort)(StartDate.Year - cohort.Age), SiteOutputName));
                        }
                        else
                        {
                            addCohort = AddNewCohort(new Cohort(cohort));
                        }
                        float biomassLayerProp = cohort.BiomassLayerProp;
                        cohortBiomassLayerProp.Add(biomassLayerProp);
                        float canopyLayerProp = cohort.CanopyLayerProp;
                        cohortCanopyLayerProp.Add(canopyLayerProp);
                        float canopyGrowingSpace = cohort.CanopyGrowingSpace;
                        cohortCanopyGrowingSpace.Add(canopyGrowingSpace);
                        float lastLAI = cohort.LastLAI;
                        cohortLastLAI.Add(lastLAI);
                        float lastWoodySenes = cohort.LastWoodySenescence;
                        cohortLastWoodySenescence.Add(lastWoodySenes);
                        float lastFolSenes = cohort.LastFoliageSenescence;
                        cohortLastFolSenescence.Add(lastFolSenes);

                    }
                }
                int index = 0;
                foreach (Cohort cohort in AllCohorts)
                {
                    cohort.BiomassLayerProp = cohortBiomassLayerProp[index];
                    cohort.CanopyLayerProp = cohortCanopyLayerProp[index];
                    cohort.CanopyGrowingSpace = cohortCanopyGrowingSpace[index];
                    cohort.LastLAI = cohortLastLAI[index];
                    cohort.LastWoodySenescence = cohortLastWoodySenescence[index];
                    cohort.LastFoliageSenescence = cohortLastFolSenescence[index];
                    index++;

                }

                this.netpsn = initialSites[key].NetPsn;
                this.folresp = initialSites[key].FolResp;
                this.grosspsn = initialSites[key].GrossPsn;
                this.maintresp = initialSites[key].MaintResp;
                this.averageAlbedo = initialSites[key].AverageAlbedo;
                this.CanopyLAI = initialSites[key].CanopyLAI;
                this.transpiration = initialSites[key].Transpiration;
                this.potentialTranspiration = initialSites[key].PotentialTranspiration;

                // Calculate AdjFolFrac
                AllCohorts.ForEach(x => x.CalcAdjFracFol());

                
            }
        }

        // Spins up sites if no biomass is provided
        private void SpinUp(DateTime StartDate, ActiveSite site, ICommunity initialCommunity, bool usingClimateLibrary, string SiteOutputName = null, bool AllowMortality = true)
        {
            List<Landis.Library.AgeOnlyCohorts.ICohort> sortedAgeCohorts = new List<Landis.Library.AgeOnlyCohorts.ICohort>();
            foreach (Landis.Library.AgeOnlyCohorts.ISpeciesCohorts speciesCohorts in initialCommunity.Cohorts)
            {
                foreach (Landis.Library.AgeOnlyCohorts.ICohort cohort in speciesCohorts)
                {
                    sortedAgeCohorts.Add(cohort);
                }
            }
            sortedAgeCohorts = new List<Library.AgeOnlyCohorts.ICohort>(sortedAgeCohorts.OrderByDescending(o => o.Age));

            if (sortedAgeCohorts.Count == 0) return;

            List<double> CohortMaxBiomassList = new List<double>();


            DateTime date = StartDate.AddYears(-(sortedAgeCohorts[0].Age - 1));

            Landis.Library.Parameters.Ecoregions.AuxParm<List<EcoregionPnETVariables>> mydata = new Library.Parameters.Ecoregions.AuxParm<List<EcoregionPnETVariables>>(Globals.ModelCore.Ecoregions);

            while (date.CompareTo(StartDate) <= 0)
            {
                //  Add those cohorts that were born at the current year
                while (sortedAgeCohorts.Count() > 0 && StartDate.Year - date.Year == (sortedAgeCohorts[0].Age - 1))
                {
                    Cohort cohort = new Cohort(sortedAgeCohorts[0].Species, SpeciesParameters.SpeciesPnET[sortedAgeCohorts[0].Species], (ushort)date.Year, SiteOutputName,1, CohortStacking);
                    if (CohortStacking)
                    {
                        cohort.CanopyLayerProp = 1.0f;
                        cohort.CanopyGrowingSpace = 1.0f;
                    }

                    bool addCohort = AddNewCohort(cohort);

                    sortedAgeCohorts.Remove(sortedAgeCohorts[0]);
                }

                // Simulation time runs untill the next cohort is added
                DateTime EndDate = (sortedAgeCohorts.Count == 0) ? StartDate : new DateTime((int)(StartDate.Year - (sortedAgeCohorts[0].Age - 1)), 1, 15);
                if (date.CompareTo(StartDate) == 0)
                    break;

                var climate_vars = usingClimateLibrary ? EcoregionData.GetClimateRegionData(Ecoregion, date, EndDate, Climate.Climate.Phase.SpinUp_Climate) : EcoregionData.GetData(Ecoregion, date, EndDate);

                Grow(climate_vars, AllowMortality,SiteOutputName != null);

                date = EndDate;

            }
            if (sortedAgeCohorts.Count > 0) throw new System.Exception("Not all cohorts in the initial communities file were initialized.");
        }

        List<List<int>> GetRandomRange(List<List<int>> bins)
        {
            List<List<int>> random_range = new List<List<int>>();
            if (bins != null) for (int b = 0; b < bins.Count(); b++)
                {
                    random_range.Add(new List<int>());

                    List<int> copy_range = new List<int>(bins[b]);

                    while (copy_range.Count() > 0)
                    {
                        int k = Statistics.DiscreteUniformRandom(0, copy_range.Count()-1);
                        random_range[b].Add(copy_range[k]);

                        copy_range.RemoveAt(k);
                    }
                }
            return random_range;
        }

        public void SetAet(float value, int Month)
        {
            AET[Month-1] = value;
        }

        public void SetPET(float value)
        {
            PET = value;
        }

        class DescendingComparer<T> : IComparer<T> where T : IComparable<T>
        {
            public int Compare(T x, T y)
            {
                return y.CompareTo(x);
            }
        }

        private static float ComputeMaxSnowMelt(float Tave, float DaySpan)
        {
            // Snowmelt rate can range between 1.6 to 6.0 mm/degree day, and default should be 2.74 according to NRCS Part 630 Hydrology National Engineering Handbook (Chapter 11: Snowmelt)
            return 2.74f * Math.Max(0, Tave) * DaySpan;
        }
        private static float CumputeSnowFraction(float Tave)
        {
            return (float)Math.Max(0.0, Math.Min(1.0, (Tave - 2) / -7));
        }

        public bool Grow(List<IEcoregionPnETVariables> data, bool AllowMortality = true, bool outputCohortData = true)
        {
            bool success = true;
            float sumPressureHead = 0;
            int countPressureHead = 0;

            establishmentProbability.ResetPerTimeStep();

            canopylaimax = float.MinValue;
            int tempMaxCanopyLayers = MaxCanopyLayers;
            if (CohortStacking)
                tempMaxCanopyLayers = AllCohorts.Count();

            SortedDictionary<double, Cohort> SubCanopyCohorts = new SortedDictionary<double, Cohort>();
            List<double> CohortBiomassList = new List<double>();
            List<double> CohortMaxBiomassList = new List<double>();
            int SiteAboveGroundBiomass = AllCohorts.Sum(a => a.AGBiomass);
            MaxLayer = 0;
            for (int cohort = 0; cohort < AllCohorts.Count(); cohort++)
            {
                if (Globals.ModelCore.CurrentTime > 0)
                {
                    AllCohorts[cohort].CalculateDefoliation(Site, SiteAboveGroundBiomass);
                }
                CohortBiomassList.Add(AllCohorts[cohort].TotalBiomass);
                CohortMaxBiomassList.Add(AllCohorts[cohort].BiomassMax);
            }

            //List<List<int>> rawBins = GetBins(new List<double>(SubCanopyCohorts.Keys));
            /*//Debug
            if (Globals.ModelCore.CurrentTime == 10 && this.Site.Location.Row == 188 && this.Site.Location.Column == 22 && CohortBiomassList.Count() == 9)
            {
                Globals.ModelCore.UI.WriteLine("AllCohorts = ");
                foreach (Cohort c in AllCohorts)
                {
                    Globals.ModelCore.UI.WriteLine("Species = "+ c.Species.Name+ "; Age = "+ c.Age.ToString()+"; Biomass = "+c.Biomass.ToString()+"; Layer = "+c.Layer.ToString());
                }

                Globals.ModelCore.UI.WriteLine("CohortBiomassList = ");
                foreach (double cohortBio in CohortBiomassList)
                {
                    Globals.ModelCore.UI.WriteLine(cohortBio.ToString());
                }
            }*/
            ratioAbove10.Clear();
            List<List<double>> cohortBins = GetBinsByCohort(CohortMaxBiomassList);

            List<int> cohortAges = new List<int>();
            List<List<int>> rawBins = new List<List<int>>();
            int subLayerIndex = 0;
            bool reducedLayer = false;
            for (int cohort = 0; cohort < AllCohorts.Count(); cohort++)
            {
                string lifeForm = AllCohorts[cohort].SpeciesPnET.Lifeform.ToLower();
                int cohortLayer = 0;
                // Lifeform "ground" always restricted to layer 0
                if (!lifeForm.Contains("ground"))
                {
                    for (int j = 0; j < cohortBins.Count(); j++)
                    {
                        if (cohortBins[j].Contains(AllCohorts[cohort].BiomassMax))
                            cohortLayer = j;
                    }

                    if (AllCohorts[cohort].Layer > MaxLayer)
                        MaxLayer = AllCohorts[cohort].Layer;
                }
                for (int i = 1; i <= Globals.IMAX; i++)
                {
                    SubCanopyCohorts.Add(subLayerIndex, AllCohorts[cohort]);
                    while (rawBins.Count() < (cohortLayer + 1))
                    {
                        List<int> subList = new List<int>();
                        //subList.Add(subLayerIndex);
                        rawBins.Add(subList);
                    }
                    //else
                    rawBins[cohortLayer].Add(subLayerIndex);
                    subLayerIndex++;
                }
                if (!cohortAges.Contains(AllCohorts[cohort].Age))
                {
                    cohortAges.Add(AllCohorts[cohort].Age);
                }
            }

            List<List<int>> LayeredBins = new List<List<int>>();
            
            LayeredBins = rawBins;

            nlayers = 0;
            foreach (List<int> layerList in LayeredBins)
            {
                if (layerList.Count > 0)
                {
                    nlayers++;
                }
            }
            MaxLayer = LayeredBins.Count - 1;
            //List<List<int>> bins = new List<List<int>>();
            //bins = LayeredBins;

            List<List<int>> random_range = GetRandomRange(LayeredBins);
             
            folresp = new float[13];
            netpsn = new float[13];
            grosspsn = new float[13];
            maintresp = new float[13];
            averageAlbedo = new float[13];
            activeLayerDepth = new float[13];
            frostDepth = new float[13];
            monthCount = new float[13];
            monthlySnowPack = new float[13];
            monthlyWater = new float[13];
            monthlyLAI = new float[13];
            monthlyLAICumulative = new float[13];

            //Dictionary<ISpeciesPnET, List<float>> annualEstab = new Dictionary<ISpeciesPnET, List<float>>();
            Dictionary<ISpeciesPnET, float> cumulativeEstab = new Dictionary<ISpeciesPnET, float>();
            Dictionary<ISpeciesPnET, List<float>> annualFwater = new Dictionary<ISpeciesPnET, List<float>>();
            Dictionary<ISpeciesPnET, float> cumulativeFwater = new Dictionary<ISpeciesPnET, float>();
            Dictionary<ISpeciesPnET, List<float>> annualFrad = new Dictionary<ISpeciesPnET, List<float>>();
            Dictionary<ISpeciesPnET, float> cumulativeFrad = new Dictionary<ISpeciesPnET, float>();
            Dictionary<ISpeciesPnET, float> monthlyEstab = new Dictionary<ISpeciesPnET, float>();
            Dictionary<ISpeciesPnET, int> monthlyCount = new Dictionary<ISpeciesPnET, int>();
            Dictionary<ISpeciesPnET, int> coldKillMonth = new Dictionary<ISpeciesPnET, int>(); // month in which cold kills each species

            foreach (ISpeciesPnET spc in SpeciesParameters.SpeciesPnET.AllSpecies)
            {
                //annualEstab[spc] = new List<float>();
                cumulativeEstab[spc] = 1;
                annualFwater[spc] = new List<float>();
                cumulativeFwater[spc] = 0;
                annualFrad[spc] = new List<float>();
                cumulativeFrad[spc] = 0;
                monthlyCount[spc] = 0;
                coldKillMonth[spc] = int.MaxValue;
            }

            float[] lastOzoneEffect = new float[SubCanopyCohorts.Count()];
            for (int i = 0; i < lastOzoneEffect.Length; i++)
            {
                lastOzoneEffect[i] = 0;
            }


            float lastPropBelowFrost = (hydrology.FrozenDepth/Ecoregion.RootingDepth);
            int daysOfWinter = 0;

            if (Globals.ModelCore.CurrentTime > 0) // cold can only kill after spinup
            {
                // Loop through months & species to determine if cold temp would kill any species
                float extremeMinTemp = float.MaxValue;
                int extremeMonth = 0;
                    for (int m = 0; m < data.Count(); m++)
                    {
                        float minTemp = data[m].Tave - (float)(3.0 * Ecoregion.WinterSTD);
                    if(minTemp < extremeMinTemp)
                        {
                            extremeMinTemp = minTemp;
                            extremeMonth = m;
                        }
                    }
                SiteVars.ExtremeMinTemp[Site] = extremeMinTemp;
                foreach (ISpeciesPnET spc in SpeciesParameters.SpeciesPnET.AllSpecies)
                {
                        // Check if low temp kills species
                    if (extremeMinTemp < spc.ColdTol)
                        {
                        coldKillMonth[spc] = extremeMonth;
                        }

                    }
                }
            //Clear pressurehead site values
            sumPressureHead = 0;
            countPressureHead = 0;

            // start monthly simulation, Zhou
            for (int m = 0; m < data.Count(); m++)
            {
                Ecoregion.Variables = data[m];
                transpiration = 0;
                potentialTranspiration = 0;
                subcanopypar = data[m].PAR0;
                interception = 0;

                //Initialize_CN_Cohort_monthly() //////////////////////////////////  

                AllCohorts.ForEach(x => x.Initialize_CN_Cohort_monthly()); //ZHOU

                /////////////////////////////////////////////////////////////////////
                // Reset monthly variables that get reported as single year snapshots
                if (data[m].Month == 1)
                {
                    folresp = new float[13];
                    netpsn = new float[13];
                    grosspsn = new float[13];
                    maintresp = new float[13];
                    averageAlbedo = new float[13];
                    activeLayerDepth = new float[13];
                    frostDepth = new float[13];
                    // Reset annual SiteVars
                    SiteVars.AnnualPE[Site] = 0;
                    SiteVars.ClimaticWaterDeficit[Site] = 0;
                    canopylaimax = float.MinValue;
                    monthlyLAI = new float[13];

                    // Reset max foliage in each cohort
                    foreach (ISpecies spc in cohorts.Keys)
                    {
                        foreach (Cohort cohort in cohorts[spc])
                        {
                            cohort.ResetFoliageMax();
                            cohort.LastAGBio = cohort.AGBiomass;
                            cohort.CalcAdjFracFol();
                            cohort.ClearFRad();
                        }
                    }
                }

                float ozoneD40 = 0;
                if (m > 0)
                    ozoneD40 = Math.Max(0, data[m].O3 - data[m - 1].O3);
                else
                    ozoneD40 = data[m].O3;
                float O3_D40_ppmh = ozoneD40 / 1000; // convert D40 units to ppm h


                // Melt snow
                float snowmelt = Math.Min(snowPack, ComputeMaxSnowMelt(data[m].Tave, data[m].DaySpan)); // mm
                if (snowmelt < 0) throw new System.Exception("Error, snowmelt = " + snowmelt + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);

                float newsnow = CumputeSnowFraction(data[m].Tave) * data[m].Prec;
                float newsnowpack = newsnow * (1 - Ecoregion.SnowSublimFrac); // (mm) Account for sublimation here
                if (newsnowpack < 0 || newsnowpack > data[m].Prec)
                {
                    throw new System.Exception("Error, newsnowpack = " + newsnowpack + " availablePrecipitation = " + data[m].Prec);
                }

                snowPack += newsnowpack - snowmelt;
                if (snowPack < 0) throw new System.Exception("Error, snowPack = " + snowPack + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);



                propRootAboveFrost = 1;
                leakageFrac = Ecoregion.LeakageFrac;
                float propThawed = 0;

                // snow calculations - from "Soil thawing worksheet with snow.xlsx"
                if (data[m].Tave <= 0)
                {
                    daysOfWinter += (int)data[m].DaySpan;
                }
                else if (snowPack > 0)
                {
                    daysOfWinter += (int)data[m].DaySpan;
                }
                else
                {
                    daysOfWinter = 0;
                }

                float Psno_kg_m3 = Globals.bulkIntercept + (Globals.bulkSlope * daysOfWinter); //kg/m3
                float Psno_g_cm3 = Psno_kg_m3 / 1000; //g/cm3

                float sno_dep = Globals.Pwater * (snowPack / 1000) / Psno_kg_m3; //m
                //if (data[m].Tave >= 0)  -- snowmelt has already been accounted for
                //{
                //    float fracAbove0 = data[m].Tmax / (data[m].Tmax - data[m].Tmin);
                //    sno_dep = sno_dep * fracAbove0;
                //}
                // from CLM model - https://escomp.github.io/ctsm-docs/doc/build/html/tech_note/Soil_Snow_Temperatures/CLM50_Tech_Note_Soil_Snow_Temperatures.html#soil-and-snow-thermal-properties
                // Eq. 85 - Jordan (1991)

                if (soilIceDepth)
                {
                    float lambda_Snow = (float) (Globals.lambAir+((0.0000775*Psno_kg_m3)+(0.000001105*Math.Pow(Psno_kg_m3,2)))*(Globals.lambIce-Globals.lambAir))*3.6F*24F; //(kJ/m/d/K) includes unit conversion from W to kJ
                    float vol_heat_capacity_snow = Globals.snowHeatCapacity * Psno_kg_m3 / 1000f; // kJ/m3/K
                                                                                          //float Ks_snow = data[m].DaySpan * lambda_Snow / vol_heat_capacity_snow; //thermal diffusivity (m2/month)
                    float Ks_snow = 1000000F / 86400F * (lambda_Snow / vol_heat_capacity_snow); //thermal diffusivity (mm2/s)
                    float damping = (float)Math.Sqrt((2.0F * Ks_snow) / Constants.omega);
                    float DRz_snow = 1F;
                    if (sno_dep > 0)
                        DRz_snow = (float)Math.Exp(-1.0F * sno_dep * damping); // Damping ratio for snow - adapted from Kang et al. (2000) and Liang et al. (2014)

                    float mossDepth = this.SiteMossDepth;

                    float cv = 2500; // heat capacity moss - kJ/m3/K (Sazonova and Romanovsky 2003)
                    float lambda_moss = 432; // kJ/m/d/K - converted from 0.2 W/mK (Sazonova and Romanovsky 2003)
                    float moss_diffusivity = lambda_moss / cv;
                    float damping_moss = (float)Math.Sqrt((2.0F * moss_diffusivity) / Constants.omega);
                    float DRz_moss = (float)Math.Exp(-1.0F * mossDepth * damping_moss); // Damping ratio for moss - adapted from Kang et al. (2000) and Liang et al. (2014)

                    //float waterContent = (float)Math.Min(1.0, hydrology.Water / Ecoregion.RootingDepth);  //m3/m3
                    //float waterContent = hydrology.Water/1000;
                    float waterContent = hydrology.Water;// volumetric m/m
                                                         // Permafrost calculations - from "Soil thawing worksheet.xlsx"
                                                         // 
                                                         //if (data[m].Tave < minMonthlyAvgTemp)
                                                         //    minMonthlyAvgTemp = data[m].Tave;
                                                         //Calculations of diffusivity from soil properties 
                                                         //float porosity = Ecoregion.Porosity / Ecoregion.RootingDepth;  //m3/m3                    
                                                         //float porosity = Ecoregion.Porosity/1000;  // m/m   
                    float porosity = Ecoregion.Porosity;  // volumetric m/m 
                    float ga = 0.035F + 0.298F * (waterContent / porosity);
                    float Fa = ((2.0F / 3.0F) / (1.0F + ga * ((Constants.lambda_a / Constants.lambda_w) - 1.0F))) + ((1.0F / 3.0F) / (1.0F + (1.0F - 2.0F * ga) * ((Constants.lambda_a / Constants.lambda_w) - 1.0F))); // ratio of air temp gradient
                    float Fs = PressureHeadSaxton_Rawls.GetFs(Ecoregion.SoilType);
                    float lambda_s = PressureHeadSaxton_Rawls.GetLambda_s(Ecoregion.SoilType);
                    float lambda_theta = (Fs * (1.0F - porosity) * lambda_s + Fa * (porosity - waterContent) * Constants.lambda_a + waterContent * Constants.lambda_w) / (Fs * (1.0F - porosity) + Fa * (porosity - waterContent) + waterContent); //soil thermal conductivity (kJ/m/d/K)
                    float D = lambda_theta / PressureHeadSaxton_Rawls.GetCTheta(Ecoregion.SoilType);  //m2/day
                    float Dmms = D * 1000000 / 86400; //mm2/s
                    soilDiffusivity = Dmms;
                    float Dmonth = D * data[m].DaySpan; // m2/month
                    float ks = Dmonth * 1000000F / (data[m].DaySpan * (Constants.SecondsPerHour * 24)); // mm2/s
                                                                                                        //float d = (float)Math.Pow((Constants.omega / (2.0F * Dmonth)), (0.5));
                    float d = (float)Math.Sqrt(2 * Dmms / Constants.omega);

                    float maxDepth = Ecoregion.RootingDepth + Ecoregion.LeakageFrostDepth;
                    float lastBelowZeroDepth = 0;
                    float bottomFreezeDepth = maxDepth / 1000;

                    //if (isSummer(data[m].Month))
                    //{
                        activeLayerDepth[data[m].Month - 1] = bottomFreezeDepth;
                    //}

                    // When there was permafrost at the end of summer, assume that the bottom of the ice lens is as deep as possible
                    //if (permafrost)
                    //{
                        frostDepth[data[m].Month - 1] = bottomFreezeDepth;
                    //}

                    float testDepth = 0;
                    float zTemp = 0;

                    float tSum = 0;
                    float pSum = 0;
                    float tMax = float.MinValue;
                    float tMin = float.MaxValue;
                    int maxMonth = 0;
                    int minMonth = 0;
                    int mCount = 0;
                    if (m < 12)
                    {
                        mCount = Math.Min(12, data.Count());
                        foreach (int z in Enumerable.Range(0, mCount))
                        {
                            tSum += data[z].Tave;
                            pSum += data[z].Prec;
                            if (data[z].Tave > tMax)
                            {
                                tMax = data[z].Tave;
                                maxMonth = data[z].Month;
                            }
                            if (data[z].Tave < tMin)
                            {
                                tMin = data[z].Tave;
                                minMonth = data[z].Month;
                            }
                        }
                    }
                    else
                    {
                        mCount = 12;
                        foreach (int z in Enumerable.Range(m - 11, 12))
                        {
                            tSum += data[z].Tave;
                            pSum += data[z].Prec;
                            if (data[z].Tave > tMax)
                            {
                                tMax = data[z].Tave;
                                maxMonth = data[z].Month;
                            }
                            if (data[z].Tave < tMin)
                            {
                                tMin = data[z].Tave;
                                minMonth = data[z].Month;
                            }
                        }
                    }
                    float annualTavg = tSum / mCount;
                    float annualPcpAvg = pSum / mCount;
                    float tAmplitude = (tMax - tMin) / 2;


                    // Regardless of permafrost, need to fill the tempDict with values
                    bool foundBottomIce = false;

                    // Calculate depth to bottom of ice lens with FrostDepth
                    while (testDepth <= bottomFreezeDepth)
                    {
                        float DRz = (float)Math.Exp(-1.0F * testDepth * d * Ecoregion.FrostFactor); // adapted from Kang et al. (2000) and Liang et al. (2014); added FrostFactor for calibration
                                                                                                    //float zTemp = annualTavg + (tempBelowSnow - annualTavg) * DRz;
                        // Calculate lag months from both max and min temperature months
                        int lagMax = (data[m].Month + (3 - maxMonth));
                        int lagMin = (data[m].Month + (minMonth - 5));
                        if(minMonth >= 9)
                            lagMin = (data[m].Month + (minMonth - 12 - 5));
                        float lagAvg = ((float)lagMax + (float)lagMin) / 2f;

                        zTemp = (float)(annualTavg + tAmplitude * DRz_snow * DRz_moss * DRz * Math.Sin(Constants.omega * lagAvg - testDepth / d));
                        depthTempDict[testDepth] = zTemp;

                        if (zTemp <= 0 && !permafrost)
                        {
                            lastBelowZeroDepth = testDepth;
                        }

                        if (zTemp > 0 && lastBelowZeroDepth > 0 && !foundBottomIce && !permafrost)
                        {
                            frostDepth[data[m].Month - 1] = lastBelowZeroDepth;
                            foundBottomIce = true;
                        }

                        if (zTemp <= 0)
                        {
                            if (testDepth < activeLayerDepth[data[m].Month - 1])
                            {
                                activeLayerDepth[data[m].Month - 1] = testDepth;
                            }
                        }
                        testDepth += 0.25F;
                    }

                    // The ice lens is deeper than the max depth
                    if (zTemp <= 0 && !foundBottomIce && !permafrost)
                    {
                        frostDepth[data[m].Month - 1] = bottomFreezeDepth;
                    }

                    propRootAboveFrost = Math.Min(1, (activeLayerDepth[data[m].Month - 1] * 1000) / Ecoregion.RootingDepth);
                    float propRootBelowFrost = 1 - propRootAboveFrost;
                    propThawed = Math.Max(0, propRootAboveFrost - (1 - lastPropBelowFrost));
                    float propNewFrozen = Math.Max(0, propRootBelowFrost - lastPropBelowFrost);
                    if (propRootAboveFrost < 1) // If part of the rooting zone is frozen
                    {
                        if (propNewFrozen > 0)  // freezing
                        {
                            float newFrozenSoil = propNewFrozen * Ecoregion.RootingDepth;
                            bool successpct = hydrology.SetFrozenWaterContent(((hydrology.FrozenDepth * hydrology.FrozenWaterContent) + (newFrozenSoil * hydrology.Water)) / (hydrology.FrozenDepth + newFrozenSoil));
                            bool successdepth = hydrology.SetFrozenDepth(Ecoregion.RootingDepth * propRootBelowFrost); // Volume of rooting soil that is frozen
                                                                                                                       // Water is a volumetric value (mm/m) so frozen water does not need to be removed, as the concentration stays the same
                        }
                    }
                    if (propThawed > 0) // thawing
                    {
                        // Thawing soil water added to existing water - redistributes evenly in active soil
                        float existingWater = (1 - lastPropBelowFrost) * hydrology.Water;
                        float thawedWater = propThawed * hydrology.FrozenWaterContent;
                        float newWaterContent = (existingWater + thawedWater) / propRootAboveFrost;
                        hydrology.AddWater(newWaterContent - hydrology.Water, Ecoregion.RootingDepth * propRootBelowFrost);
                        bool successdepth = hydrology.SetFrozenDepth(Ecoregion.RootingDepth * propRootBelowFrost);  // Volume of rooting soil that is frozen
                    }
                    float leakageFrostReduction = 1.0F;
                    if ((activeLayerDepth[data[m].Month - 1] * 1000) < (Ecoregion.RootingDepth + Ecoregion.LeakageFrostDepth))
                    {
                        if ((activeLayerDepth[data[m].Month - 1] * 1000) < Ecoregion.RootingDepth)
                        {
                            leakageFrostReduction = 0.0F;
                        }
                        else
                        {
                            leakageFrostReduction = (Math.Min((activeLayerDepth[data[m].Month - 1] * 1000), Ecoregion.LeakageFrostDepth) - Ecoregion.RootingDepth) / (Ecoregion.LeakageFrostDepth - Ecoregion.RootingDepth);
                        }
                    }
                    leakageFrac = Ecoregion.LeakageFrac * leakageFrostReduction;
                    lastPropBelowFrost = propRootBelowFrost;

                }
                else
                {
                    activeLayerDepth[data[m].Month - 1] = 999;
                }

                // permafrost
                //float frostFreeProp = Math.Min(1.0F, frostFreeSoilDepth / Ecoregion.RootingDepth);

                AllCohorts.ForEach(x => x.InitializeSubLayers());

                if (data[m].Prec < 0) throw new System.Exception("Error, this.data[m].Prec = " + data[m].Prec + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);

                // Calculate abovecanopy reference daily ET
                float RET = hydrology.Calculate_RET_Hamon(data[m].Tave, data[m].Daylength); //mm/day

                float newrain = data[m].Prec - newsnow;

                // Reduced by interception
                if (CanopyLAI == null)
                {
                    CanopyLAI = new float[tempMaxCanopyLayers];
                    //MaxLAI = new float[tempMaxCanopyLayers];
                }
                interception = newrain * (float)(1 - Math.Exp(-1 * Ecoregion.PrecIntConst * CanopyLAI.Sum()));
                float surfaceRain = newrain - interception;

                // Reduced by PrecLossFrac
                precLoss = surfaceRain * Ecoregion.PrecLossFrac;
                float precin = surfaceRain - precLoss;

                if (precin < 0) throw new System.Exception("Error, precin = " + precin + " newsnow = " + newsnow + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);

                int numEvents = Ecoregion.PrecipEvents;  // maximum number of precipitation events per month
                float PrecInByEvent = precin / numEvents;  // Divide precip into discreet events within the month
                if (PrecInByEvent < 0) throw new System.Exception("Error, PrecInByEvent = " + PrecInByEvent + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);

                if (propRootAboveFrost >= 1)
                {
                    bool successpct = hydrology.SetFrozenWaterContent(0F);
                    bool successdepth = hydrology.SetFrozenDepth(0F);
                }
                float MeltInWater = snowmelt;

                // Calculate ground PET
                float groundPET = hydrology.Calculate_PotentialGroundET_LAI(CanopyLAI.Sum(), data[m].Tave, data[m].Daylength, data[m].DaySpan, ((Parameter<float>)Names.GetParameter("ETExtCoeff")).Value);
                float  groundPETbyEvent = groundPET / numEvents;  // divide evaporation into discreet events to match precip

                // Randomly choose which layers will receive the precip events
                // If # of layers < precipEvents, some layers will show up multiple times in number list.  This ensures the same number of precip events regardless of the number of cohorts
                List<int> randomNumbers = new List<int>();
                if (PrecipEventsWithReplacement)// Sublayer selection with replacement
                {
                    while (randomNumbers.Count < numEvents)
                    {
                        int rand = Statistics.DiscreteUniformRandom(1, SubCanopyCohorts.Count());
                        randomNumbers.Add(rand);
                    }
                }
                else // Sublayer selection without replacement
                {
                    while (randomNumbers.Count < numEvents)
                    {
                        List<int> subCanopyList = Enumerable.Range(1, SubCanopyCohorts.Count()).ToList();
                        while ((randomNumbers.Count < numEvents) && (subCanopyList.Count() > 0))
                        {
                            int rand = Statistics.DiscreteUniformRandom(0, subCanopyList.Count() - 1);
                            randomNumbers.Add(subCanopyList[rand]);
                            subCanopyList.RemoveAt(rand);
                        }
                    }
                }
                var groupList = randomNumbers.GroupBy(i => i);

                // Reset Hydrology values
                hydrology.RunOff = 0;
                hydrology.Leakage = 0;
                hydrology.Evaporation = 0;
                hydrology.PE = groundPET;
                hydrology.PET = 0;
                float PETcumulative = 0;
                float TransCumulative = 0;
                float InterceptCumulative = 0;



                float O3_ppmh = data[m].O3 / 1000; // convert AOT40 units to ppm h
                float lastO3 = 0;
                if (m > 0)
                    lastO3 = (data[m - 1].O3 / 1000f);
                float O3_ppmh_month = Math.Max(0, O3_ppmh - lastO3);

                List<ISpeciesPnET> species = SpeciesParameters.SpeciesPnET.AllSpecies.ToList();
                Dictionary<string, float> DelAmax_spp = new Dictionary<string, float>();
                Dictionary<string, float> JCO2_spp = new Dictionary<string, float>();
                Dictionary<string, float> Amax_spp = new Dictionary<string, float>();
                Dictionary<string, float> FTempPSNRefNetPSN_spp = new Dictionary<string, float>();
                Dictionary<string, float> Ca_Ci_spp = new Dictionary<string, float>();

                float subCanopyPrecip = 0;
                float subCanopyPET = 0;;
                float subCanopyMelt = 0;
                int subCanopyIndex = 0;
                // set empty layer summaries to 0
                int layerCount = 0;
                if (LayeredBins != null)
                    layerCount = LayeredBins.Count();
                /*float[] layerWtTotalBio = new float[layerCount];
                float[] layerWtBio = new float[layerCount];
                float[] layerWtWoodBio = new float[layerCount];
                float[] layerWtRootBio = new float[layerCount];
                float[] layerWtFolBio = new float[layerCount];
                float[] layerWtNSC = new float[layerCount];                
                float[] layerWtNetPsn = new float[layerCount];
                float[] layerWtGrossPsn = new float[layerCount];
                float[] layerWtWoodSenescence = new float[layerCount];
                float[] layerWtFolSenescence = new float[layerCount];
                float[] layerWtMaintResp = new float[layerCount];
                float[] layerWtFolResp = new float[layerCount];
                float[] layerWtTranspiration = new float[layerCount];*/
                float[] layerWtLAI = new float[layerCount];
                float[] layerSumBio = new float[layerCount];
                float[] layerSumCanopyProp = new float[layerCount];

                if (LayeredBins != null && LayeredBins.Count() > 0)
                {
                    for (int b = LayeredBins.Count() - 1; b >= 0; b--) // main canopy layers
                    {
                        foreach (int r in random_range[b]) // sublayers within main canopy b
                        {
                            Cohort c = SubCanopyCohorts.Values.ToArray()[r];
                            // A cohort cannot be reduced to a lower layer once it reaches a higher layer
                            //c.Layer = (byte)Math.Max(b, c.Layer);
                            c.Layer = (byte)b;
                        }
                    }
                    for (int b = LayeredBins.Count() - 1; b >= 0; b--) // main canopy layers
                    {
                        float mainLayerPARweightedSum = 0;
                        float mainLayerLAIweightedSum = 0;
                        float mainLayerPAR = subcanopypar;
                        float mainLayerBioSum = 0;
                        float mainLayerCanopyProp = 0;

                        // Estimate layer SumCanopyProp
                        float sumCanopyProp = 0;
                        foreach (int r in random_range[b]) // sublayers within main canopy b
                        {
                            Cohort c = SubCanopyCohorts.Values.ToArray()[r];
                            sumCanopyProp += c.LastLAI / c.SpeciesPnET.MaxLAI;
                        }
                        sumCanopyProp = sumCanopyProp / Globals.IMAX;

                        foreach (int r in random_range[b]) // sublayers within main canopy b
                        {
                            subCanopyIndex++;
                            int precipCount = 0;
                            subCanopyPrecip = 0;
                            subCanopyPET = 0;
                            subCanopyMelt = MeltInWater / SubCanopyCohorts.Count();
                            PETcumulative = PETcumulative + RET * data[m].DaySpan / SubCanopyCohorts.Count();
                            bool coldKillBoolean = false;
                            foreach (var g in groupList)
                            {
                                if (g.Key == subCanopyIndex)
                                {
                                    precipCount = g.Count();
                                    subCanopyPrecip = PrecInByEvent; 
                                    InterceptCumulative += interception / groupList.Count();
                                    if (snowPack == 0)
                                        subCanopyPET = groundPETbyEvent;
                                }
                            }
                            Cohort c = SubCanopyCohorts.Values.ToArray()[r];
                            ISpeciesPnET spc = c.SpeciesPnET;

                            if (coldKillMonth[spc] == m)
                                coldKillBoolean = true;
                            float O3Effect = lastOzoneEffect[subCanopyIndex - 1];

                            float PETnonfor = PETcumulative - TransCumulative - InterceptCumulative - hydrology.Evaporation; // hydrology.Evaporation is cumulative

                            

                            
                            success = c.CalculatePhotosynthesis(subCanopyPrecip, precipCount, leakageFrac, ref hydrology, mainLayerPAR,
                                ref subcanopypar, O3_ppmh, O3_ppmh_month, subCanopyIndex, SubCanopyCohorts.Count(), ref O3Effect,
                                propRootAboveFrost, subCanopyMelt, coldKillBoolean, data[m], this, sumCanopyProp, subCanopyPET, AllowMortality);
                            if (success == false)
                            {
                                throw new System.Exception("Error CalculatePhotosynthesis");
                            }
                            //Globals.ModelCore.UI.WriteLine("DEBUG: Site = " + this.Site.ToString() + "; Cohort species = " + c.Species.Name + "; Cohort Age = " + c.Age + "; Year = " + data[m].Year + "; Month = " + data[m].Month + "; subCanopyIndex = " + subCanopyIndex);
                            TransCumulative = TransCumulative + c.Transpiration[c.index-1];
                            //InterceptCumulative = InterceptCumulative + c.Interception[c.index-1];
                            lastOzoneEffect[subCanopyIndex - 1] = O3Effect;

                            // Update for transpiration
                            //PETnonfor = PETcumulative - TransCumulative - InterceptCumulative - hydrology.Evaporation; // hydrology.Evaporation is cumulative

                            if (groundPET > 0)
                            {
                                float evaporationEvent = 0;
                                // If more than one precip event assigned to layer, repeat evaporation for all events prior to respiration
                                for (int p = 1; p <= precipCount; p++)
                                {
                                    PETnonfor = groundPETbyEvent;
                                    if (propRootAboveFrost > 0 && snowPack == 0)
                                    {
                                        evaporationEvent = hydrology.CalculateEvaporation(this, PETnonfor); //mm
                                    }

                                    success = hydrology.AddWater(-1 * evaporationEvent, Ecoregion.RootingDepth * propRootAboveFrost);
                                    if (success == false)
                                    {
                                        throw new System.Exception("Error adding water, evaporation = " + evaporationEvent + "; water = " + hydrology.Water + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);
                                    }
                                    hydrology.Evaporation += evaporationEvent;
                                }
                            }

                        } // end sublayer loop in canopy b
                        int cCount = AllCohorts.Count();
                        foreach (Cohort c in AllCohorts)
                        {
                            if (c.Layer == b)
                            {
                                float LAISum = c.LAI.Sum();
                                if (c.Leaf_On)
                                {
                                    if (LAISum > c.LastLAI)
                                        c.LastLAI = LAISum;
                                }
                                float PARFracUnderCohort = (float)Math.Exp(-c.SpeciesPnET.K * LAISum);
                                //mainLayerPARweightedSum += PARFracUnderCohort * ((1 - c.SpeciesPnET.FracBelowG) * c.TotalBiomass + c.Fol);
                                if (CohortStacking)
                                    mainLayerPARweightedSum += PARFracUnderCohort * 1.0f;
                                else
                                    mainLayerPARweightedSum += PARFracUnderCohort * Math.Min(c.LastLAI / c.SpeciesPnET.MaxLAI, c.CanopyGrowingSpace);
                                //mainLayerLAIweightedSum += c.LAI.Sum() * ((1 - c.SpeciesPnET.FracBelowG) * c.TotalBiomass + c.Fol);
                                mainLayerLAIweightedSum += LAISum * Math.Min(c.LastLAI / c.SpeciesPnET.MaxLAI, c.CanopyGrowingSpace);
                                mainLayerBioSum += c.AGBiomass;
                                
                                c.ANPP = (int)(c.AGBiomass - c.LastAGBio);

                                if (CohortStacking)
                                    mainLayerCanopyProp += 1.0f;
                                else
                                    mainLayerCanopyProp += Math.Min(c.LastLAI / c.SpeciesPnET.MaxLAI, c.CanopyGrowingSpace);
                            }
                        }
                        layerSumBio[b] = mainLayerBioSum;
                        layerSumCanopyProp[b] = mainLayerCanopyProp;
                        if (layerSumCanopyProp[b] > 1)
                        {
                            mainLayerPARweightedSum = 0;
                        }

                        List<float> Frac_list = new List<float>();
                        List<float> prop_List = new List<float>();
                        List<int> index_List = new List<int>();
                        int index = 0;
                        foreach (Cohort c in AllCohorts)
                        {
                            if (c.Layer == b)
                            {
                                index++;
                                index_List.Add(index);

                                c.BiomassLayerProp = c.AGBiomass / layerSumBio[b];
                                c.CanopyLayerProp = Math.Min(c.LastLAI / c.SpeciesPnET.MaxLAI, c.CanopyGrowingSpace);
                                if (layerSumCanopyProp[b] > 1)
                                {
                                    //c.CanopyLayerProp = c.CanopyLayerProp / layerSumCanopyProp[b];
                                    //c.CanopyGrowingSpace = Math.Min(c.CanopyGrowingSpace, c.CanopyLayerProp);
                                    if (c.growMonth == 1)
                                    {

                                        float canopyLayerPropAdj = c.CanopyLayerProp / layerSumCanopyProp[b];
                                        c.CanopyLayerProp = (canopyLayerPropAdj - c.CanopyLayerProp) * CanopySumScale + c.CanopyLayerProp;
                                        //c.CanopyLayerProp = c.Biomass / BiomassSite; // Zhou
                                        c.CanopyGrowingSpace = Math.Min(c.CanopyGrowingSpace, c.CanopyLayerProp);
                                    }
                                    float LAISum = c.LAI.Sum();
                                    float PARFracUnderCohort = (float)Math.Exp(-c.SpeciesPnET.K * LAISum);
                                    Frac_list.Add(PARFracUnderCohort);
                                    if (CohortStacking)
                                    {
                                        mainLayerPARweightedSum += PARFracUnderCohort * 1.0f;
                                    }
                                    else
                                    {
                                        mainLayerPARweightedSum += PARFracUnderCohort * c.CanopyLayerProp;
                                    }


                                }
                                if (CohortStacking)
                                {
                                    c.CanopyLayerProp = 1.0f;
                                    c.CanopyGrowingSpace = 1.0f;
                                }
                                prop_List.Add(c.CanopyLayerProp);
                                c.ANPP = (int)(c.ANPP * c.CanopyLayerProp);


                            }
                        }

                        if (mainLayerBioSum > 0)
                        {
                            //subcanopypar = mainLayerPAR * (mainLayerPARweightedSum + (1 - mainLayerCanopyProp));
                            //layerWtLAI[b] = mainLayerLAIweightedSum;

                            if (Frac_list.Count() > 0)
                            {

                                float cumulativeFracProp = 1;
                                /* 
                                float sumCumulativeProp = 0;
                                IEnumerable<IEnumerable<int>> index_set = GetPowerSet(index_List);
                                //IEnumerable<IEnumerable<float>> Frac_set = GetPowerSet(Frac_list);
                                //IEnumerable<IEnumerable<float>> Prop_set = GetPowerSet(prop_List);
                                //float cumulativeFrac = 0;
                                for (int i = 0; i < index_set.Count(); i++)
                                {
                                    IEnumerable<int> indices = index_set.ElementAt(i);
                                    float cumulativeProp = 1.0f;
                                    float cumulativeFrac = 1.0f;
                                    float frac = 1.0f;
                                    for (int j = 1; j <= index; j++)
                                    {
                                        float prop = 0; ;
                                        if (indices.Contains(j))
                                        {
                                            prop = prop_List[j - 1];
                                            frac = Frac_list[j - 1];
                                            cumulativeFrac = cumulativeFrac * frac;
                                        }
                                        else
                                        {
                                            prop = (1 - prop_List[j - 1]);
                                        }
                                        cumulativeProp = cumulativeProp * prop;

                                    }
                                    float fracProp = cumulativeProp * cumulativeFrac;
                                    cumulativeFracProp += fracProp;
                                    sumCumulativeProp += cumulativeProp;
                                    if (sumCumulativeProp > 0.75)
                                        break;
                                }
                                */
                                for (int i = 0; i < Frac_list.Count(); i++)
                                {
                                    float prop = prop_List[i];
                                    float frac = Frac_list[i];
                                    cumulativeFracProp = cumulativeFracProp * (float)Math.Pow(frac, prop);
                                }
                                subcanopypar = mainLayerPAR * cumulativeFracProp;
                            }
                            else
                            {
                                subcanopypar = mainLayerPAR * (mainLayerPARweightedSum + (1 - mainLayerCanopyProp));
                            }
                            layerWtLAI[b] = mainLayerLAIweightedSum;


                        }
                        else
                            subcanopypar = mainLayerPAR;
                    } // end main canopy layer loop
                    hydrology.PET += PETcumulative;
                    //hydrology.PE = (float)Math.Max(0,PETcumulative - TransCumulative - InterceptCumulative);
                }
                else // When no cohorts are present
                {
                    if (MeltInWater > 0)
                    {
                        // Add melted snow to soil moisture
                        // Instantaneous runoff (excess of porosity)
                        float waterCapacity = Ecoregion.Porosity * Ecoregion.RootingDepth * propRootAboveFrost; //mm
                        float meltrunoff = Math.Min(MeltInWater, Math.Max(hydrology.Water * Ecoregion.RootingDepth * propRootAboveFrost + MeltInWater - waterCapacity, 0));

                        //if ((hydrology.Water + meltrunoff) > (Ecoregion.Porosity + Ecoregion.RunoffCapture))
                        //    meltrunoff = (hydrology.Water + meltrunoff) - (Ecoregion.Porosity + Ecoregion.RunoffCapture);
                        hydrology.RunOff += meltrunoff;

                        success = hydrology.AddWater(MeltInWater - meltrunoff, Ecoregion.RootingDepth * propRootAboveFrost);
                        if (success == false) throw new System.Exception("Error adding water, MeltInWaterr = " + MeltInWater + "; water = " + hydrology.Water + "; meltrunoff = " + meltrunoff + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);
                        float capturedRunoff = 0;
                        if ((Ecoregion.RunoffCapture > 0) & (meltrunoff > 0))
                        {
                            capturedRunoff = Math.Max(0, Math.Min(meltrunoff, (Ecoregion.RunoffCapture - hydrology.SurfaceWater)));
                            hydrology.SurfaceWater += capturedRunoff;
                        }
                        hydrology.RunOff += (meltrunoff - capturedRunoff);
                    }
                    if (precin > 0)
                    {
                        for (int p = 0; p < numEvents; p++)
                        {
                            // Instantaneous runoff (excess of porosity)
                            float waterCapacity = Ecoregion.Porosity * Ecoregion.RootingDepth * propRootAboveFrost; //mm
                            float rainrunoff = Math.Min(precin, Math.Max(hydrology.Water * Ecoregion.RootingDepth * propRootAboveFrost + PrecInByEvent - waterCapacity, 0));
                            //if ((hydrology.Water + rainrunoff) > (ecoregion.Porosity + ecoregion.RunoffCapture))
                            //    rainrunoff = (hydrology.Water + rainrunoff) - (ecoregion.Porosity + ecoregion.RunoffCapture);
                            float capturedRunoff = 0;
                            if ((Ecoregion.RunoffCapture > 0) & (rainrunoff > 0))
                            {
                                capturedRunoff = Math.Max(0, Math.Min(rainrunoff, (Ecoregion.RunoffCapture - hydrology.SurfaceWater)));
                                hydrology.SurfaceWater += capturedRunoff;
                            }
                            hydrology.RunOff += (rainrunoff - capturedRunoff);

                            float precipIn = PrecInByEvent - rainrunoff; //mm

                            // Add incoming precipitation to soil moisture
                            success = hydrology.AddWater(precipIn, Ecoregion.RootingDepth * propRootAboveFrost);
                            if (success == false) throw new System.Exception("Error adding water, waterIn = " + precipIn + "; water = " + hydrology.Water + "; rainrunoff = " + rainrunoff + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);

                            // Fast Leakage
                            float leakage = Math.Max((float)leakageFrac * (hydrology.Water - Ecoregion.FieldCap), 0) * Ecoregion.RootingDepth * propRootAboveFrost; //mm
                            hydrology.Leakage += leakage;

                            // Remove fast leakage
                            success = hydrology.AddWater(-1 * leakage, Ecoregion.RootingDepth * propRootAboveFrost);
                            if (success == false) throw new System.Exception("Error adding water, Hydrology.Leakage = " + hydrology.Leakage + "; water = " + hydrology.Water + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);

                            // Evaporation
                            //PETcumulative = PETcumulative + RET * data[m].DaySpan / numEvents;
                            //float PETnonfor = PETcumulative - TransCumulative - InterceptCumulative - hydrology.Evaporation; // hydrology.Evaporation is cumulative
                            float PETnonfor = groundPET / numEvents;
                            //PETcumulative = PETcumulative + PETnonfor;
                            PETcumulative = PETcumulative + RET * data[m].DaySpan / numEvents;
                            float evaporationEvent = 0;
                            if (propRootAboveFrost > 0 && snowPack == 0)
                            {
                                evaporationEvent = hydrology.CalculateEvaporation(this, PETnonfor); //mm
                            }

                            success = hydrology.AddWater(-1 * evaporationEvent, Ecoregion.RootingDepth * propRootAboveFrost);
                            if (success == false)
                            {
                                throw new System.Exception("Error adding water, evaporation = " + evaporationEvent + "; water = " + hydrology.Water + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);
                            }
                            hydrology.Evaporation += evaporationEvent;

                            // Add surface water to soil
                            if (hydrology.SurfaceWater > 0)
                            {
                                float surfaceInput = Math.Min(hydrology.SurfaceWater, ((Ecoregion.Porosity - hydrology.Water) * Ecoregion.RootingDepth * propRootAboveFrost));
                                hydrology.SurfaceWater -= surfaceInput;
                                success = hydrology.AddWater(surfaceInput, Ecoregion.RootingDepth * propRootAboveFrost);
                                if (success == false) throw new System.Exception("Error adding water, Hydrology.SurfaceWater = " + hydrology.SurfaceWater + "; water = " + hydrology.Water + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);
                            }
                        }
                    }
                    else  //if (precin > 0)
                    {
                        if (MeltInWater > 0)
                        {
                            // Add surface water to soil
                            if (hydrology.SurfaceWater > 0)
                            {
                                float surfaceInput = Math.Min(hydrology.SurfaceWater, ((Ecoregion.Porosity - hydrology.Water) * Ecoregion.RootingDepth * propRootAboveFrost));
                                hydrology.SurfaceWater -= surfaceInput;
                                success = hydrology.AddWater(surfaceInput, Ecoregion.RootingDepth * propRootAboveFrost);
                                if (success == false) throw new System.Exception("Error adding water, Hydrology.SurfaceWater = " + hydrology.SurfaceWater + "; water = " + hydrology.Water + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);
                            }
                        }
                    }
                    hydrology.PET += PETcumulative;
                    //hydrology.PE = (float)Math.Max(0,PETcumulative - TransCumulative - InterceptCumulative);
                }


                // CN transformation/translocation ZHOU


                Check_NBalance(); // initialize balance check
                AllCohorts.ForEach(x => x.Allocate_CN(data[m].Month));  //ZHOU
                AllCohorts.ForEach(x => x.CNTrans());
                Decomp();  // soil decomposition and nitrification //ZHOU
                Competition_PlantNUptake(); // plant uptake based on biomass // ZHOU
                NLeach(); //  ZHOU






                SiteVars.AnnualPE[Site] = hydrology.PE;

                int cohortCount = AllCohorts.Count();
                CanopyLAI = new float[tempMaxCanopyLayers];
                float[] CanopyLAISum = new float[tempMaxCanopyLayers];
                float[] CanopyLAICount = new float[tempMaxCanopyLayers];
                float[] CanopyAlbedo = new float[tempMaxCanopyLayers];
                float[] LayerLAI = new float[tempMaxCanopyLayers];
                CumulativeLeafAreas leafAreas = new CumulativeLeafAreas();

                monthCount[data[m].Month - 1]++;
                monthlySnowPack[data[m].Month - 1] += snowPack;
                monthlyWater[data[m].Month - 1] += hydrology.Water;
                


                foreach (Cohort cohort in AllCohorts)
                {
                    folresp[data[m].Month - 1] += (cohort.FolResp.Sum() * cohort.CanopyLayerProp);
                    netpsn[data[m].Month - 1] += (cohort.NetPsn.Sum() * cohort.CanopyLayerProp);
                    grosspsn[data[m].Month - 1] += (cohort.GrossPsn.Sum() * cohort.CanopyLayerProp);
                    maintresp[data[m].Month - 1] += (cohort.MaintenanceRespiration.Sum() * cohort.CanopyLayerProp);
                    transpiration += cohort.Transpiration.Sum(); // Transpiration already scaled to CanopyLayerProp
                    potentialTranspiration += cohort.PotentialTranspiration.Sum(); // Transpiration already scaled to CanopyLayerProp
                    CalculateCumulativeLeafArea(ref leafAreas, cohort);
                    
                    int layer = cohort.Layer;
                    if (layer < CanopyLAISum.Length)
                    {
                        CanopyLAISum[layer] += (cohort.LAI.Sum() * ((1 - cohort.SpeciesPnET.FracBelowG) * cohort.TotalBiomass + cohort.Fol));
                        CanopyLAICount[layer] += ((1 - cohort.SpeciesPnET.FracBelowG) * cohort.TotalBiomass+cohort.Fol);
                        //MaxLAI[layer] = Math.Max(MaxLAI[layer], cohort.SpeciesPNET.MaxLAI);
                    }
                    //else
                    //{
                    //    Globals.ModelCore.UI.WriteLine("DEBUG: Cohort count = " + AllCohorts.Count() + "; CanopyLAISum count = " + CanopyLAISum.Count());
                    //}
                    else
                    {
                        Globals.ModelCore.UI.WriteLine("DEBUG: Cohort count = " + AllCohorts.Count() + "; CanopyLAISum count = " + CanopyLAISum.Count());
                    }

                    CanopyAlbedo[layer] += CalculateAlbedoWithSnow(cohort, cohort.Albedo, sno_dep) * cohort.CanopyLayerProp;
                    LayerLAI[layer] += cohort.SumLAI * cohort.CanopyLayerProp;
                    monthlyLAI[data[m].Month - 1] += (cohort.LAI.Sum() * cohort.CanopyLayerProp);
                    monthlyLAICumulative[data[m].Month - 1] += (cohort.LAI.Sum() * cohort.CanopyLayerProp);
                }
                if (AllCohorts.Count == 0)
                {
                    float albedo = 0.2F;
                    if (sno_dep > 0)
                    {
                        float snowMultiplier = sno_dep >= Globals.snowReflectanceThreshold ? 1 : sno_dep / Globals.snowReflectanceThreshold;
                        albedo = (float)(albedo + (albedo * (3.125 * snowMultiplier)));
                    }
                    averageAlbedo[data[m].Month - 1] = albedo;
                }
                else
                {
                    if (LayerLAI.Max() < 1)
                    {
                        var index = Array.FindLastIndex(CanopyAlbedo, value => value != 0);

                        // If a value not equal to zero was found
                        if (index != -1)
                        {
                            averageAlbedo[data[m].Month - 1] = CanopyAlbedo[index];
                        }
                    }
                    else
                    {
                        for (int layer = (tempMaxCanopyLayers - 1); layer >= 0; layer--)
                        {
                            if (LayerLAI[layer] > 1)
                            {
                                averageAlbedo[data[m].Month - 1] = CanopyAlbedo[layer];
                                break;
                            }
                        }
                    }
                }

                //folresp[data[m].Month - 1] = layerWtFolResp.Sum();
                //netpsn[data[m].Month - 1] = layerWtNetPsn.Sum();
                //grosspsn[data[m].Month - 1] = layerWtGrossPsn.Sum();
                //maintresp[data[m].Month - 1] = layerWtMaintResp.Sum();
                //transpiration = layerWtTranspiration.Sum();

                for (int layer = 0; layer < tempMaxCanopyLayers; layer++)
                {
                    if (layer < layerWtLAI.Length)
                        CanopyLAI[layer] = layerWtLAI[layer];
                    else
                        CanopyLAI[layer] = 0;
                }
                canopylaimax = (float)Math.Max(canopylaimax, LayerLAI.Sum());

                // Calculate establishment probability
                if (Globals.ModelCore.CurrentTime > 0)
                {
                    establishmentProbability.Calculate_Establishment_Month(data[m], Ecoregion, subcanopypar, hydrology, minHalfSat, maxHalfSat, invertPest, propRootAboveFrost);

                    foreach (ISpeciesPnET spc in SpeciesParameters.SpeciesPnET.AllSpecies)
                    {
                        if (annualFwater.ContainsKey(spc))
                        {
                            if (data[m].Tmin > spc.PsnTMin && data[m].Tmax < spc.PsnTMax && propRootAboveFrost > 0) // Active growing season
                            {
                                // Store monthly values for later averaging
                                //annualEstab[spc].Add(monthlyEstab[spc]);
                                annualFwater[spc].Add(establishmentProbability.Get_FWater(spc));
                                annualFrad[spc].Add(establishmentProbability.Get_FRad(spc));
                            }
                        }
                    }
                }

                float AET = hydrology.Evaporation + TransCumulative + InterceptCumulative;
                this.SetAet(AET, data[m].Month);
                this.SetPET(PETcumulative);
                SiteVars.ClimaticWaterDeficit[this.Site] += (PETcumulative - AET);

                /*// Soil water Evaporation
                // Surface PAR is effectively 0 when snowpack is present
                if (snowPack > 0)
                    subcanopypar = 0;

                //canopylaimax = (float)Math.Max(canopylaimax, CanopyLAI.Sum());
                canopylaimax = (float)Math.Max(canopylaimax, LayerLAI.Sum());
                subcanopyparmax = Math.Max(subcanopyparmax, subcanopypar);
                if (propRootAboveFrost > 0 && snowPack == 0)
                {
                    hydrology.Evaporation = hydrology.CalculateEvaporation(this, data[m]); //mm
                }
                else
                {
                    hydrology.Evaporation = 0;
                    hydrology.PE = 0;
                }
                success = hydrology.AddWater(-1 * hydrology.Evaporation, Ecoregion.RootingDepth * propRootAboveFrost);
                if (success == false)
                {
                    throw new System.Exception("Error adding water, evaporation = " + hydrology.Evaporation + "; water = " + hydrology.Water + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);
                }*/

                // Add surface water to soil
                if ((hydrology.SurfaceWater > 0) & (hydrology.Water < Ecoregion.Porosity))
                {
                    float surfaceInput = Math.Min(hydrology.SurfaceWater, ((Ecoregion.Porosity - hydrology.Water) * Ecoregion.RootingDepth * propRootAboveFrost));
                    hydrology.SurfaceWater -= surfaceInput;
                    success = hydrology.AddWater(surfaceInput, Ecoregion.RootingDepth * propRootAboveFrost);
                    if (success == false) throw new System.Exception("Error adding water, Hydrology.SurfaceWater = " + hydrology.SurfaceWater + "; water = " + hydrology.Water + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);
                }

                if (siteoutput != null && outputCohortData)
                {
                    AddSiteOutput(data[m]);
                    AllCohorts.ForEach(a => a.UpdateCohortData(data[m]));
                }
                if (data[m].Tave > 0)
                {
                    sumPressureHead += hydrology.PressureHeadTable.CalculateWaterPressure(hydrology.Water,Ecoregion.SoilType);
                    countPressureHead += 1;
                }

                if (data[m].Month == 7)
                {
                    julysubcanopypar = subcanopypar;
                }

                // Store growing season FRad values                
                AllCohorts.ForEach(x => x.StoreFRad());
                // Reset all cohort values
                AllCohorts.ForEach(x => x.NullSubLayers());

                //  Processes that happen only once per year
                if (data[m].Month == (int)Constants.Months.December)
                {
                    //  Decompose litter
                    HeterotrophicRespiration = (ushort)(SiteVars.Litter[Site].Decompose() + SiteVars.WoodyDebris[Site].Decompose());

                    // Calculate AdjFolFrac
                    AllCohorts.ForEach(x => x.CalcAdjFracFol());

                    ////   AllCohorts.ForEach(x => x.AllocateYr());    //zhou  
                    Competition_MaxNStore();  // allocation of MaxNstore
                    
                    //NH4 += AllCohorts.Sum(x => x.ExcessN * x.CanopyLayerProp); // balance N // ZHOU
                    AllCohorts.ForEach(x => x.AllocateYr()); //ZHOU
                    AllocateYrSite(); // Zhou

                    // Filter monthly pest values
                    // This assumes up to 3 months of growing season are relevant for establishment
                    // When > 3 months of growing season, exlcude 1st month, assuming trees focus on foliage growth in first month
                    // When > 4 months, ignore the 4th month and beyond as not primarily relevant for establishment
                    // When < 3 months, include all months
                    foreach (ISpeciesPnET spc in SpeciesParameters.SpeciesPnET.AllSpecies)
                    {
                        if (annualFwater[spc].Count > 3)
                        {
                            //cumulativeEstab[spc] = cumulativeEstab[spc] * (1 - annualEstab[spc][1]) * (1 - annualEstab[spc][2]) * (1 - annualEstab[spc][3]);
                            cumulativeFwater[spc] = cumulativeFwater[spc] + annualFwater[spc][1] + annualFwater[spc][2] + annualFwater[spc][3];
                            cumulativeFrad[spc] = cumulativeFrad[spc] + annualFrad[spc][1] + annualFrad[spc][2] + annualFrad[spc][3];
                            monthlyCount[spc] = monthlyCount[spc] + 3;
                        }
                        else if (annualFwater[spc].Count > 2)
                        {
                            //cumulativeEstab[spc] = cumulativeEstab[spc] * (1 - annualEstab[spc][0]) * (1 - annualEstab[spc][1]) * (1 - annualEstab[spc][2]) ;
                            cumulativeFwater[spc] = cumulativeFwater[spc] + annualFwater[spc][0] + annualFwater[spc][1] + annualFwater[spc][2];
                            cumulativeFrad[spc] = cumulativeFrad[spc] + annualFrad[spc][0] + annualFrad[spc][1] + annualFrad[spc][2];
                            monthlyCount[spc] = monthlyCount[spc] + 3;
                        }
                        else if (annualFwater[spc].Count > 1)
                        {
                            //cumulativeEstab[spc] = cumulativeEstab[spc] * (1 - annualEstab[spc][0]) * (1 - annualEstab[spc][1]);
                            cumulativeFwater[spc] = cumulativeFwater[spc] + annualFwater[spc][0] + annualFwater[spc][1];
                            cumulativeFrad[spc] = cumulativeFrad[spc] + annualFrad[spc][0] + annualFrad[spc][1];
                            monthlyCount[spc] = monthlyCount[spc] + 2;
                        }
                        else if (annualFwater[spc].Count == 1)
                        {
                            //cumulativeEstab[spc] = cumulativeEstab[spc] * (1 - annualEstab[spc][0]);
                            cumulativeFwater[spc] = cumulativeFwater[spc] + annualFwater[spc][0];
                            cumulativeFrad[spc] = cumulativeFrad[spc] + annualFrad[spc][0];
                            monthlyCount[spc] = monthlyCount[spc] + 1;
                        }

                        //Reset annual lists for next year
                        //annualEstab[spc].Clear();
                        annualFwater[spc].Clear();
                        annualFrad[spc].Clear();
                    } //foreach (ISpeciesPnET spc in SpeciesParameters.SpeciesPnET.AllSpecies)
                } //if (data[m].Month == (int)Constants.Months.December)

                wateravg += hydrology.Water;
            } //for (int m = 0; m < data.Count(); m++ )
            // Above is monthly loop                           
            // Below runs once per timestep
            wateravg = wateravg / data.Count(); // convert to average value
            if (Globals.ModelCore.CurrentTime > 0)
            {
                foreach (ISpeciesPnET spc in SpeciesParameters.SpeciesPnET.AllSpecies)
                {
                    bool estab = false;
                    float pest = 0;
                    if (monthlyCount[spc] > 0)
                    {
                        //annualEstab[spc] = annualEstab[spc] / monthlyCount[spc];
                        // Transform cumulative probability of no successful establishments to probability of at least one successful establishment
                        //cumulativeEstab[spc] = 1 - cumulativeEstab[spc] ;
                        cumulativeFwater[spc] = cumulativeFwater[spc] / monthlyCount[spc];
                        cumulativeFrad[spc] = cumulativeFrad[spc] / monthlyCount[spc];

                        // Modify Pest by maximum value
                        //pest = cumulativeEstab[spc] * spc.MaxPest;

                        // Calculate Pest from average Fwater, Frad and modified by MaxPest
                        pest = cumulativeFwater[spc] * cumulativeFrad[spc] * spc.MaxPest;
                    }
                    
                    if (!spc.PreventEstablishment)
                    {

                        if (pest > (float)Statistics.ContinuousUniformRandom())
                        {
                            establishmentProbability.EstablishmentTrue(spc);
                            estab = true;

                        }
                    }
                    EstablishmentProbability.RecordPest(Globals.ModelCore.CurrentTime, spc, pest, cumulativeFwater[spc], cumulativeFrad[spc], estab, monthlyCount[spc]);

                }
            }

            if (siteoutput != null && outputCohortData)
            {
                siteoutput.Write();
                AllCohorts.ForEach(cohort => { cohort.WriteCohortData(); });

            }
            float avgPH = sumPressureHead / countPressureHead;
            SiteVars.PressureHead[Site] = avgPH;
            
            if((Globals.ModelCore.CurrentTime > 0) || AllowMortality)
                RemoveMarkedCohorts();

            //HeterotrophicRespiration = (ushort)(PlugIn.Litter[Site].Decompose() + PlugIn.WoodyDebris[Site].Decompose());//Moved within m loop to trigger once per year

            return success;
        }

        // Finds the maximum value from an array of floats
        private float Max(float[] values)
        {
            float maximum = float.MinValue;

            for(int i = 0; i < values.Length; i++)
            {
                if (values[i] > maximum)
                {
                    maximum = values[i];
                }
            }

            return maximum;
        }

        private bool isSummer(byte month)
        {
            switch (month)
            {
                case 5:
                    return true;
                case 6:
                    return true;
                case 7:
                    return true;
                case 8:
                    return true;
                default:
                    return false;
            }
        }

        private bool isWinter(byte month)
        {
            switch (month)
            {
                case 1:
                    return true;
                case 2:
                    return true;
                case 3:
                    return true;
                case 12:
                    return true;
                default:
                    return false;
            }
        }

        /*private float CalculateAverageAlbedo(CumulativeLeafAreas leafAreas, float snowDepth)
        {

            if (!Globals.ModelCore.Ecoregion[this.Site].Active)
            {
                return -1;
            }

            float snowMultiplier = snowDepth >= Globals.snowReflectanceThreshold ? 1 : snowDepth / Globals.snowReflectanceThreshold;

            float darkConiferAlbedo = (float)((-0.067 * Math.Log(leafAreas.DarkConifer < 0.7 ? 0.7 : leafAreas.DarkConifer)) + 0.2095);
            darkConiferAlbedo = (float)(darkConiferAlbedo + (darkConiferAlbedo * (0.8 * snowMultiplier)));

            float lightConiferAlbedo = (float)((-0.054 * Math.Log(leafAreas.LightConifer < 0.7 ? 0.7 : leafAreas.LightConifer)) + 0.2082);
            lightConiferAlbedo = (float)(lightConiferAlbedo + (lightConiferAlbedo * (0.75 * snowMultiplier)));

            float deciduousAlbedo = (float)((-0.0073 * leafAreas.Deciduous) + 0.231);
            deciduousAlbedo = (float)(deciduousAlbedo + (deciduousAlbedo * (0.35 * snowMultiplier)));

            float grassMossOpenAlbedo = 0.2F;
            grassMossOpenAlbedo = (float)(grassMossOpenAlbedo + (grassMossOpenAlbedo * (3.75 * snowMultiplier)));

            // Set Albedo values to 0 if they are negative
            darkConiferAlbedo = darkConiferAlbedo >= 0 ? darkConiferAlbedo : 0;
            lightConiferAlbedo = lightConiferAlbedo >= 0 ? lightConiferAlbedo : 0;
            deciduousAlbedo = deciduousAlbedo >= 0 ? deciduousAlbedo : 0;
            grassMossOpenAlbedo = grassMossOpenAlbedo >= 0 ? grassMossOpenAlbedo : 0;

            if (leafAreas.DarkConiferProportion + leafAreas.LightConiferProportion + leafAreas.DeciduousProportion + leafAreas.GrassMossOpenProportion == 0)
            {
                return 0;
            }

            return ((darkConiferAlbedo * leafAreas.DarkConiferProportion) + (lightConiferAlbedo * leafAreas.LightConiferProportion)
                + (deciduousAlbedo * leafAreas.DeciduousProportion) + (grassMossOpenAlbedo * leafAreas.GrassMossOpenProportion))
                / (leafAreas.DarkConiferProportion + leafAreas.LightConiferProportion + leafAreas.DeciduousProportion + leafAreas.GrassMossOpenProportion);
        }*/

        // Does the final bits of Albedo calculation by adding snow consideration in
        private float CalculateAlbedoWithSnow(Cohort cohort, float albedo, float snowDepth)
        {
            // Inactive sites become large negative values on the map and are not considered in the averages
            if (!EcoregionData.GetPnETEcoregion(Globals.ModelCore.Ecoregion[this.Site]).Active)
            {
                return -1;
            }

            float snowMultiplier = snowDepth >= Globals.snowReflectanceThreshold ? 1 : snowDepth / Globals.snowReflectanceThreshold;

            if ((!string.IsNullOrEmpty(cohort.SpeciesPnET.Lifeform))
                    && (cohort.SpeciesPnET.Lifeform.ToLower().Contains("ground")
                        || cohort.SpeciesPnET.Lifeform.ToLower().Contains("open")
                        || cohort.SumLAI == 0))
            {
                return (float)(albedo + (albedo * (2.125 * snowMultiplier)));
            }
            else if ((!string.IsNullOrEmpty(cohort.SpeciesPnET.Lifeform))
                    && cohort.SpeciesPnET.Lifeform.ToLower().Contains("dark"))
            {
                return (float)(albedo + (albedo * (0.8 * snowMultiplier)));
            }
            else if ((!string.IsNullOrEmpty(cohort.SpeciesPnET.Lifeform))
                    && cohort.SpeciesPnET.Lifeform.ToLower().Contains("light"))
            {
                return (float)(albedo + (albedo * (0.75 * snowMultiplier)));
            }
            else if ((!string.IsNullOrEmpty(cohort.SpeciesPnET.Lifeform))
                    && cohort.SpeciesPnET.Lifeform.ToLower().Contains("decid"))
            {
                return (float)(albedo + (albedo * (0.35 * snowMultiplier)));
            }


            return 0;
        }

        private void CalculateCumulativeLeafArea(ref CumulativeLeafAreas leafAreas, Cohort cohort)
        {
            if ((!string.IsNullOrEmpty(cohort.SpeciesPnET.Lifeform))
                    && cohort.SpeciesPnET.Lifeform.ToLower().Contains("dark"))
            {
                leafAreas.DarkConifer += cohort.SumLAI;
            }
            else if ((!string.IsNullOrEmpty(cohort.SpeciesPnET.Lifeform))
                    && cohort.SpeciesPnET.Lifeform.ToLower().Contains("light"))
            {
                leafAreas.LightConifer += cohort.SumLAI;
            }
            else if ((!string.IsNullOrEmpty(cohort.SpeciesPnET.Lifeform))
                    && cohort.SpeciesPnET.Lifeform.ToLower().Contains("decid"))
            {
                leafAreas.Deciduous += cohort.SumLAI;
            }
            else if ((!string.IsNullOrEmpty(cohort.SpeciesPnET.Lifeform))
                    && (cohort.SpeciesPnET.Lifeform.ToLower().Contains("ground")
                        || cohort.SpeciesPnET.Lifeform.ToLower().Contains("open")))
            {
                leafAreas.GrassMossOpen += cohort.SumLAI;
            }
            else if ((!string.IsNullOrEmpty(cohort.SpeciesPnET.Lifeform))
                    && (cohort.SpeciesPnET.Lifeform.ToLower().Contains("tree")
                        || cohort.SpeciesPnET.Lifeform.ToLower().Contains("shrub")))
            {
                leafAreas.Deciduous += cohort.SumLAI;
            }
        }

        /*private void CalculateInitialWater(DateTime StartDate)
        {
            IEcoregionPnETVariables variables = null;
            canopylaimax = float.MinValue;

            SortedDictionary<double, Cohort> SubCanopyCohorts = new SortedDictionary<double, Cohort>();
            List<double> CohortBiomassList = new List<double>();
            List<double> CohortMaxBiomassList = new List<double>();
            int SiteAboveGroundBiomass = AllCohorts.Sum(a => a.AGBiomass);
            int MaxLayer = 0;
            for (int cohort = 0; cohort < AllCohorts.Count(); cohort++)
            {
                if (Globals.ModelCore.CurrentTime > 0)
                {
                    AllCohorts[cohort].CalculateDefoliation(Site, SiteAboveGroundBiomass);
                }

                CohortBiomassList.Add(AllCohorts[cohort].TotalBiomass);
                CohortMaxBiomassList.Add(AllCohorts[cohort].BiomassMax);
            }

            //List<List<int>> rawBins = GetBins(new List<double>(SubCanopyCohorts.Keys));
            // cut //Debug
            if (Globals.ModelCore.CurrentTime == 10 && this.Site.Location.Row == 188 && this.Site.Location.Column == 22 && CohortBiomassList.Count() == 9)
            {
                Globals.ModelCore.UI.WriteLine("AllCohorts = ");
                foreach (Cohort c in AllCohorts)
                {
                    Globals.ModelCore.UI.WriteLine("Species = ", c.Species.Name, "; Age = ", c.Age.ToString(), "; Biomass = ", c.Biomass.ToString(), "; Layer = ", c.Layer.ToString());
                }

                Globals.ModelCore.UI.WriteLine("CohortBiomassList = ");
                foreach (double cohortBio in CohortBiomassList)
                {
                    Globals.ModelCore.UI.WriteLine(cohortBio.ToString());
                }
            } // end cut
            ratioAbove10.Clear();
            List<List<double>> cohortBins = GetBinsByCohort(CohortMaxBiomassList);

            List<int> cohortAges = new List<int>();
            List<List<int>> rawBins = new List<List<int>>();
            int subLayerIndex = 0;
            bool reducedLayer = false;
            for (int cohort = 0; cohort < AllCohorts.Count(); cohort++)
            {
                string lifeForm = AllCohorts[cohort].SpeciesPnET.Lifeform.ToLower();
                int cohortLayer = 0;
                // Lifeform "ground" always restricted to layer 0
                if (!lifeForm.Contains("ground"))
                {
                    for (int j = 0; j < cohortBins.Count(); j++)
                    {
                        if (cohortBins[j].Contains(AllCohorts[cohort].BiomassMax))
                            cohortLayer = j;
                    }
                    //if ((AllCohorts[cohort].Layer > cohortLayer) && (!string.IsNullOrEmpty(lifeForm))
                    //    && (lifeForm.Contains("tree") || lifeForm.Contains("shrub"))) 
                    // {
                    //     reducedLayer = true;
                    // }

                    if (AllCohorts[cohort].Layer > MaxLayer)
                        MaxLayer = AllCohorts[cohort].Layer;
                }
                for (int i = 1; i <= Globals.IMAX; i++)
                {
                    //double CumCohortBiomass = ((float)i / (float)PlugIn.IMAX) * AllCohorts[cohort].TotalBiomass;
                    // cut double CumCohortBiomass = (1f / (float)PlugIn.IMAX) * AllCohorts[cohort].TotalBiomass;
                    while (SubCanopyCohorts.ContainsKey(CumCohortBiomass))
                    {
                        // Add a negligable value [-1e-10; + 1e-10] to CumCohortBiomass in order to prevent duplicate keys
                        double k = 1e-10 * 2.0 * (PlugIn.ContinuousUniformRandom() - 0.5);
                        CumCohortBiomass += k;
                    }
                    SubCanopyCohorts.Add(CumCohortBiomass, AllCohorts[cohort]); // - end cut
                    SubCanopyCohorts.Add(subLayerIndex, AllCohorts[cohort]);
                    while (rawBins.Count() < (cohortLayer + 1))
                    {
                        List<int> subList = new List<int>();
                        //subList.Add(subLayerIndex);
                        rawBins.Add(subList);
                    }
                    //else
                    rawBins[cohortLayer].Add(subLayerIndex);
                    subLayerIndex++;
                }
                if (!cohortAges.Contains(AllCohorts[cohort].Age))
                {
                    cohortAges.Add(AllCohorts[cohort].Age);
                }
            }

            List<List<int>> LayeredBins = new List<List<int>>();
            // cut //if ((rawBins.Count > 0) && (reducedLayer)) // cohort(s) were previously in a higher layer
            {
                double maxCohortBiomass = CohortBiomassList.Max();
                for (int i = 0; i < rawBins.Count(); i++)
                {
                    List<int> binLayers = rawBins[i];
                    for (int b = 0; b < binLayers.Count(); b++)
                    {
                        int layerKey = binLayers[b];
                        int canopyIndex = i;
                        Cohort layerCohort = SubCanopyCohorts.Values.ToArray()[layerKey];
                        double cohortBio = layerCohort.TotalBiomass;
                        //bool highRatio = ((maxCohortBiomass / cohortBio) > 10.0);
                        //if(layerCohort.Layer > i && !highRatio)
                        if (layerCohort.Layer > i)
                        {
                            canopyIndex = layerCohort.Layer;
                        }
                        if (LayeredBins.ElementAtOrDefault(canopyIndex) == null)
                        {
                            while (LayeredBins.ElementAtOrDefault(canopyIndex) == null)
                            {
                                LayeredBins.Add(new List<int>());
                            }
                        }
                        LayeredBins[canopyIndex].Add(layerKey);
                    }
                }
            }
            else
            { // end cut
                LayeredBins = rawBins;
            //}
            nlayers = 0;
            foreach (List<int> layerList in LayeredBins)
            {
                if (layerList.Count > 0)
                    nlayers++;
            }
            MaxLayer = LayeredBins.Count - 1;

            List<List<int>> random_range = GetRandomRange(LayeredBins);

            List<IEcoregionPnETVariables> climate_vars = EcoregionData.GetData(Ecoregion, StartDate, StartDate.AddMonths(1));

            if (climate_vars != null && climate_vars.Count > 0)
            {
                Ecoregion.Variables = climate_vars.First();
                variables = climate_vars.First();
            }
            else
            {
                return;
            }
            transpiration = 0;
            potentialTranspiration = 0;
            subcanopypar = variables.PAR0;
            interception = 0;

            AllCohorts.ForEach(x => x.InitializeSubLayers());

            if (variables.Prec < 0) throw new System.Exception("Error, Ecoregion.Variables.Prec = " + variables.Prec);

            float snowmelt = Math.Min(snowPack, ComputeMaxSnowMelt(variables.Tave, variables.DaySpan)); // mm
            if (snowmelt < 0) throw new System.Exception("Error, snowmelt = " + snowmelt);

            float newsnow = CumputeSnowFraction(variables.Tave) * variables.Prec;
            float newsnowpack = newsnow * (1 - Ecoregion.SnowSublimFrac); // (mm) Account for sublimation here
            if (newsnowpack < 0 || newsnowpack > variables.Prec)
            {
                throw new System.Exception("Error, newsnowpack = " + newsnowpack + " availablePrecipitation = " + variables.Prec);
            }

            snowPack += newsnowpack - snowmelt;
            if (snowPack < 0) throw new System.Exception("Error, snowPack = " + snowPack);

            float newrain = variables.Prec - newsnow;

            // Reduced by interception
            interception = newrain * (float)(1 - Math.Exp(-1 * Ecoregion.PrecIntConst * CanopyLAI.Sum()));
            float surfaceRain = newrain - interception;

            // Reduced by PrecLossFrac
            precLoss = surfaceRain * Ecoregion.PrecLossFrac;
            float availableRain = surfaceRain - precLoss;

            float precin = availableRain + snowmelt;
            if (precin < 0) throw new System.Exception("Error, precin = " + precin + " newsnow = " + newsnow + " snowmelt = " + snowmelt);

            int numEvents = Ecoregion.PrecipEvents;  // maximum number of precipitation events per month
            float PrecInByEvent = precin / numEvents;  // Divide precip into discreet events within the month
            if (PrecInByEvent < 0) throw new System.Exception("Error, PrecInByEvent = " + PrecInByEvent);

            // Randomly choose which layers will receive the precip events
            // If # of layers < precipEvents, some layers will show up multiple times in number list.  This ensures the same number of precip events regardless of the number of cohorts
            List<int> randomNumbers = new List<int>();
            if (PrecipEventsWithReplacement)// Sublayer selection with replacement
            {
                while (randomNumbers.Count < numEvents)
                {
                    int rand = Statistics.DiscreteUniformRandom(1, SubCanopyCohorts.Count());
                    randomNumbers.Add(rand);
                }
            }
            else // Sublayer selection without replacement
            {
                while (randomNumbers.Count < numEvents)
                {
                    List<int> subCanopyList = Enumerable.Range(1, SubCanopyCohorts.Count()).ToList();
                    while ((randomNumbers.Count < numEvents) && (subCanopyList.Count() > 0))
                    {
                        int rand = Statistics.DiscreteUniformRandom(1, subCanopyList.Count());
                        randomNumbers.Add(subCanopyList[rand]);
                        subCanopyList.RemoveAt(rand);
                    }
                }
            }
            var groupList = randomNumbers.GroupBy(i => i);

            // Reset Hydrology values
            hydrology.RunOff = 0;
            hydrology.Leakage = 0;
            hydrology.Evaporation = 0;

            float subCanopyPrecip = 0;
            int subCanopyIndex = 0;
            if (LayeredBins != null)
            {
                for (int b = LayeredBins.Count() - 1; b >= 0; b--)
                {
                    foreach (int r in random_range[b])
                    {
                        subCanopyIndex++;
                        int precipCount = 0;
                        subCanopyPrecip = 0;
                        foreach (var g in groupList)
                        {
                            if (g.Key == subCanopyIndex)
                            {
                                precipCount = g.Count();
                                subCanopyPrecip = PrecInByEvent;
                            }
                        }
                        Cohort c = SubCanopyCohorts.Values.ToArray()[r];
                        ISpeciesPnET spc = c.SpeciesPnET;

                        // A cohort cannot be reduced to a lower layer once it reaches a higher layer
                        //if (c.Layer > bins.Count())
                        //    c.Layer = (byte)bins.Count();
                        //c.Layer = (byte)Math.Max(b, c.Layer);
                        c.Layer = (byte)b;
                    }
                }
            }
            else // When no cohorts are present
            {
                return;
            }

            // Surface PAR is effectively 0 when snowpack is present
            if (snowPack > 0)
                subcanopypar = 0;

            canopylaimax = (float)Math.Max(canopylaimax, CanopyLAI.Sum());
            wateravg = hydrology.Water;
            subcanopyparmax = Math.Max(subcanopyparmax, subcanopypar);

            if (propRootAboveFrost > 0 && snowPack == 0)
            {
                hydrology.Evaporation = hydrology.CalculateEvaporation(this, variables); //mm
            }
            else
            {
                hydrology.Evaporation = 0;
                hydrology.PE = 0;
            }
            bool success = hydrology.AddWater(-1 * hydrology.Evaporation, Ecoregion.RootingDepth * propRootAboveFrost);
            if (success == false)
            {
                throw new System.Exception("Error adding water, evaporation = " + hydrology.Evaporation + "; water = " + hydrology.Water + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);
            }
            if (hydrology.SurfaceWater > 0)
            {
                float surfaceInput = Math.Min(hydrology.SurfaceWater, (Ecoregion.Porosity - hydrology.Water));
                hydrology.SurfaceWater -= surfaceInput;
                success = hydrology.AddWater(surfaceInput, Ecoregion.RootingDepth * propRootAboveFrost);
                if (success == false) throw new System.Exception("Error adding water, Hydrology.SurfaceWater = " + hydrology.SurfaceWater + "; water = " + hydrology.Water + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);
            }
        }*/

        
        public float[] MaintResp
        {
            get
            {
                if (maintresp == null)
                {
                    float[] maintresp_array = new float[12];
                    for (int i = 0; i < maintresp_array.Length; i++)
                    {
                        maintresp_array[i] = 0;
                    }
                    return maintresp_array;
                }
                else
                {
                    return maintresp.Select(r => (float)r).ToArray();
                }
            }
        }

        public float[] FolResp
        {
            get
            {
                if (folresp == null)
                {
                    float[] folresp_array = new float[12];
                    for (int i = 0; i < folresp_array.Length; i++)
                    {
                        folresp_array[i] = 0;
                    }
                    return folresp_array;
                }
                else
                {
                    return folresp.Select(psn => (float)psn).ToArray();
                }
            }
        }
        public float[] GrossPsn
        {
            get
            {
                if (grosspsn == null)
                {
                    float[] grosspsn_array = new float[12];
                    for (int i = 0; i < grosspsn_array.Length; i++)
                    {
                        grosspsn_array[i] = 0;
                    }
                    return grosspsn_array;
                }
                else
                {
                    return grosspsn.Select(psn => (float)psn).ToArray();
                }
            }
        }
        public float[] MonthlyAvgSnowPack
        {
            get
            {
                if (monthlySnowPack == null)
                {
                    float[] snowPack_array = new float[12];
                    for (int i = 0; i < snowPack_array.Length; i++)
                    {
                        snowPack_array[i] = 0;
                    }
                    return snowPack_array;
                }
                else
                {
                     float[] snowSum = monthlySnowPack.Select(snowPack => (float)snowPack).ToArray();
                    float[] monthSum = monthCount.Select(months => (float)months).ToArray();
                    float[] snowPack_array = new float[12];
                    for (int i = 0; i < snowPack_array.Length; i++)
                    {
                        snowPack_array[i] = snowSum[i] / monthSum[i];
                    }
                    return snowPack_array;
                }
            }
        }
        public float[] MonthlyAvgWater
        {
            get
            {
                if (monthlyWater == null)
                {
                    float[] water_array = new float[12];
                    for (int i = 0; i < water_array.Length; i++)
                    {
                        water_array[i] = 0;
                    }
                    return water_array;
                }
                else
                {
                    float[] waterSum = monthlyWater.Select(water => (float)water).ToArray();
                    float[] monthSum = monthCount.Select(months => (float)months).ToArray();
                    float[] water_array = new float[12];
                    for (int i = 0; i < water_array.Length; i++)
                    {
                        water_array[i] = waterSum[i] / monthSum[i];
                    }
                    return water_array;
                }
            }
        }
        public float[] MonthlyAvgLAI
        {
            get
            {
                if (monthlyLAICumulative == null)
                {
                    float[] lai_array = new float[12];
                    for (int i = 0; i < lai_array.Length; i++)
                    {
                        lai_array[i] = 0;
                    }
                    return lai_array;
                }
                else
                {
                    float[] laiSum = monthlyLAICumulative.Select(lai => (float)lai).ToArray();
                    float[] monthSum = monthCount.Select(months => (float)months).ToArray();
                    float[] lai_array = new float[12];
                    for (int i = 0; i < lai_array.Length; i++)
                    {
                        lai_array[i] = laiSum[i] / monthSum[i];
                    }
                    return lai_array;
                }
            }
        }
        public float[] AverageAlbedo
        {
            get
            {
                if (averageAlbedo == null)
                {
                    float[] averageAlbedo_array = new float[12];
                    for (int i = 0; i < averageAlbedo_array.Length; i++)
                    {
                        averageAlbedo_array[i] = 0.24f;
                    }
                    return averageAlbedo_array;
                }
                else
                {
                    return averageAlbedo.Select(r => (float)r).ToArray();
                }
            }
        }

        public float[] ActiveLayerDepth
        {
            get
            {
                if (activeLayerDepth == null)
                {
                    float[] activeLayerDepth_array = new float[12];
                    for (int i = 0; i < activeLayerDepth_array.Length; i++)
                    {
                        activeLayerDepth_array[i] = 0;
                    }
                    return activeLayerDepth_array;
                }
                else
                {
                    return activeLayerDepth.Select(r => (float)r).ToArray();
                }
            }
        }

        public float[] FrostDepth
        {
            get
            {
                if (frostDepth == null)
                {
                    float[] frostDepth_array = new float[12];
                    for (int i = 0; i < frostDepth_array.Length; i++)
                    {
                        frostDepth_array[i] = 0;
                    }
                    return frostDepth_array;
                }
                else
                {
                    return frostDepth.Select(r => (float)r).ToArray();
                }
            }
        }

        public float NetPsnSum
        {
            get
            {
                if (netpsn == null)
                {
                    float[] netpsn_array = new float[12];
                    for (int i = 0; i < netpsn_array.Length; i++)
                    {
                        netpsn_array[i] = 0;
                    }
                    return netpsn_array.Sum();
                }
                else
                {
                    return netpsn.Select(psn => (float)psn).ToArray().Sum();
                }
            }
        }
        public float CanopyLAImax
        {
            get
            {
                return canopylaimax;
            }
        }
        public float[] MonthlyLAI
        {
            get
            {
                return monthlyLAI;
            }
        }
        public float SiteMossDepth
        {
            get
            {
                float mossDepth = Ecoregion.MossDepth; //m

                foreach (ISpecies spc in cohorts.Keys)
                {
                    // Add each species' mossDepth to the total
                    foreach (Cohort cohort in cohorts[spc])
                    {
                        mossDepth += (cohort.MossDepth * cohort.CanopyLayerProp);
                    }
                }
                return mossDepth;
            }
        }

        public double WoodyDebris 
        {
            get
            {
                return SiteVars.WoodyDebris[Site].Mass;
            }
        }

        public double Litter 
        {
            get
            {
                return SiteVars.Litter[Site].Mass;
            }
        }
       
        public  Landis.Library.Parameters.Species.AuxParm<bool> SpeciesPresent
        {
            get
            {
                Landis.Library.Parameters.Species.AuxParm<bool> SpeciesPresent = new Library.Parameters.Species.AuxParm<bool>(Globals.ModelCore.Species);

                foreach (ISpecies spc in cohorts.Keys)
                {
                    SpeciesPresent[spc] = true;
                }
                return SpeciesPresent;
            }
        }

        public Landis.Library.Parameters.Species.AuxParm<int> BiomassPerSpecies 
        { 
            get
            {
                Landis.Library.Parameters.Species.AuxParm<int> BiomassPerSpecies = new Library.Parameters.Species.AuxParm<int>(Globals.ModelCore.Species);

                foreach (ISpecies spc in cohorts.Keys)
                {
                    BiomassPerSpecies[spc] = cohorts[spc].Sum(o => (int)(o.TotalBiomass * o.CanopyLayerProp));
                }
                return BiomassPerSpecies;
            }
        }
        public Landis.Library.Parameters.Species.AuxParm<int> AbovegroundBiomassPerSpecies
        {
            get
            {
                Landis.Library.Parameters.Species.AuxParm<int> AbovegroundBiomassPerSpecies = new Library.Parameters.Species.AuxParm<int>(Globals.ModelCore.Species);

                foreach (ISpecies spc in cohorts.Keys)
                {
                    AbovegroundBiomassPerSpecies[spc] = cohorts[spc].Sum(o => (int)(o.AGBiomass * o.CanopyLayerProp));
                }
                return AbovegroundBiomassPerSpecies;
            }
        }
        public Landis.Library.Parameters.Species.AuxParm<int> WoodBiomassPerSpecies
        {
            get
            {
                Landis.Library.Parameters.Species.AuxParm<int> WoodBiomassPerSpecies = new Library.Parameters.Species.AuxParm<int>(Globals.ModelCore.Species);

                foreach (ISpecies spc in cohorts.Keys)
                {
                    WoodBiomassPerSpecies[spc] = cohorts[spc].Sum(o => (int)(o.Wood * o.CanopyLayerProp));
                }
                return WoodBiomassPerSpecies;
            }
        }
        public Landis.Library.Parameters.Species.AuxParm<int> BelowGroundBiomassPerSpecies
        {
            get
            {
                Landis.Library.Parameters.Species.AuxParm<int> BelowGroundBiomassPerSpecies = new Library.Parameters.Species.AuxParm<int>(Globals.ModelCore.Species);

                foreach (ISpecies spc in cohorts.Keys)
                {
                    BelowGroundBiomassPerSpecies[spc] = cohorts[spc].Sum(o => (int)(o.Root * o.CanopyLayerProp));
                }
                return BelowGroundBiomassPerSpecies;
            }
        }
        public Landis.Library.Parameters.Species.AuxParm<int> FoliageBiomassPerSpecies
        {
            get
            {
                Landis.Library.Parameters.Species.AuxParm<int> FoliageBiomassPerSpecies = new Library.Parameters.Species.AuxParm<int>(Globals.ModelCore.Species);

                foreach (ISpecies spc in cohorts.Keys)
                {
                    FoliageBiomassPerSpecies[spc] = cohorts[spc].Sum(o => (int)(o.Fol * o.CanopyLayerProp));
                }
                return FoliageBiomassPerSpecies;
            }
        }
        public Landis.Library.Parameters.Species.AuxParm<int> MaxFoliageYearPerSpecies
        {
            get
            {
                Landis.Library.Parameters.Species.AuxParm<int> MaxFoliageYearPerSpecies = new Library.Parameters.Species.AuxParm<int>(Globals.ModelCore.Species);

                foreach (ISpecies spc in cohorts.Keys)
                {
                    MaxFoliageYearPerSpecies[spc] = cohorts[spc].Max(o => (int)o.MaxFolYear);
                }
                return MaxFoliageYearPerSpecies;
            }
        }
        public Landis.Library.Parameters.Species.AuxParm<int> NSCPerSpecies
        {
            get
            {
                Landis.Library.Parameters.Species.AuxParm<int> NSCPerSpecies = new Library.Parameters.Species.AuxParm<int>(Globals.ModelCore.Species);

                foreach (ISpecies spc in cohorts.Keys)
                {
                    NSCPerSpecies[spc] = cohorts[spc].Sum(o => (int)(o.NSC * o.CanopyLayerProp));
                }
                return NSCPerSpecies;
            }
        }
        public Landis.Library.Parameters.Species.AuxParm<float> LAIPerSpecies
        {
            get
            {
                Landis.Library.Parameters.Species.AuxParm<float> LAIPerSpecies = new Library.Parameters.Species.AuxParm<float>(Globals.ModelCore.Species);

                foreach (ISpecies spc in cohorts.Keys)
                {
                    //LAIPerSpecies[spc] = cohorts[spc].Sum(o => (int)(o.LAI != null ? o.LAI.Sum() * o.CanopyLayerProp : 0));
                    LAIPerSpecies[spc] = cohorts[spc].Sum(o => (o.LastLAI * o.CanopyLayerProp));
                }
                return LAIPerSpecies;
            }
        }
        public Landis.Library.Parameters.Species.AuxParm<int> WoodySenescencePerSpecies
        {
            get
            {
                Landis.Library.Parameters.Species.AuxParm<int> WoodySenescencePerSpecies = new Library.Parameters.Species.AuxParm<int>(Globals.ModelCore.Species);

                foreach (ISpecies spc in cohorts.Keys)
                {
                    WoodySenescencePerSpecies[spc] = cohorts[spc].Sum(o => (int)(o.LastWoodySenescence * o.CanopyLayerProp));
                }
                return WoodySenescencePerSpecies;
            }
        }
        public Landis.Library.Parameters.Species.AuxParm<int> FoliageSenescencePerSpecies
        {
            get
            {
                Landis.Library.Parameters.Species.AuxParm<int> FoliageSenescencePerSpecies = new Library.Parameters.Species.AuxParm<int>(Globals.ModelCore.Species);

                foreach (ISpecies spc in cohorts.Keys)
                {
                    FoliageSenescencePerSpecies[spc] = cohorts[spc].Sum(o => (int)(o.LastFoliageSenescence * o.CanopyLayerProp));
                }
                return FoliageSenescencePerSpecies;
            }
        }
        public Landis.Library.Parameters.Species.AuxParm<int> CohortCountPerSpecies 
        { 
            get
            {
                Landis.Library.Parameters.Species.AuxParm<int> CohortCountPerSpecies = new Library.Parameters.Species.AuxParm<int>(Globals.ModelCore.Species);

                foreach (ISpecies spc in cohorts.Keys)
                {
                    CohortCountPerSpecies[spc] = cohorts[spc].Count();
                }
                return CohortCountPerSpecies;
            }
        }

        public Landis.Library.Parameters.Species.AuxParm<List<ushort>> CohortAges 
        { 
            get
            {
                Landis.Library.Parameters.Species.AuxParm<List<ushort>> CohortAges = new Library.Parameters.Species.AuxParm<List<ushort>>(Globals.ModelCore.Species);

                foreach (ISpecies spc in cohorts.Keys)
                {
                    CohortAges[spc] = new List<ushort>(cohorts[spc].Select(o => o.Age));
                }
                return CohortAges;
            }
        }

        public float BiomassSum
        {
            get
            {
                return AllCohorts.Sum(o => (o.TotalBiomass * o.CanopyLayerProp));
            }

        }
        public float AbovegroundBiomassSum
        {
            get
            {
                return AllCohorts.Sum(o => (o.AGBiomass * o.CanopyLayerProp));
            }

        }
        public float WoodBiomassSum
        {
            get
            {
                return AllCohorts.Sum(o => (o.Wood * o.CanopyLayerProp));
            }

        }
        public float WoodySenescenceSum
        {
            get
            {
                return AllCohorts.Sum(o => (o.LastWoodySenescence * o.CanopyLayerProp));
            }

        }
        public float FoliageSenescenceSum
        {
            get
            {
                return AllCohorts.Sum(o => (o.LastFoliageSenescence * o.CanopyLayerProp));
            }

        }
        public float BelowGroundBiomassSum 
        {
            get
            {
                return AllCohorts.Sum(o =>(o.Root * o.CanopyLayerProp));
                //return (uint)cohorts.Values.Sum(o => o.Sum(x => x.Root));
            }
        }

        public float FoliageSum
        {
            get
            {
                return AllCohorts.Sum(o => (o.Fol * o.CanopyLayerProp));
            }

        }

        public float NSCSum
        {
            get
            {
                return AllCohorts.Sum(o => (o.NSC * o.CanopyLayerProp));
            }
        }
        public float PET
        {
            get;
            set;
        }
        public int CohortCount
        {
            get
            {
                return cohorts.Values.Sum(o => o.Count());
            }
        }
        

        public int AverageAge 
        {
            get
            {
                return (int) cohorts.Values.Average(o => o.Average(x=>x.Age));
            }
        }

        public float AETSum
        {
            get
            {
                return AET.Sum();
            }
        }
        class SubCanopyComparer : IComparer<int[]>
        {
            // Compare second int (cumulative cohort biomass)
            public int Compare(int[] x, int[] y)
            {
                return (x[0] > y[0])? 1:-1;
            }
        }

        private SortedDictionary<int[], Cohort> GetSubcanopyLayers()
        {
            SortedDictionary<int[], Cohort> subcanopylayers = new SortedDictionary<int[], Cohort>(new SubCanopyComparer());

            foreach (Cohort cohort in AllCohorts)
            {
                for (int i = 0; i < Globals.IMAX; i++)
                {
                    int[] subcanopylayer = new int[] { (ushort)((i + 1) / (float)Globals.IMAX * cohort.BiomassMax) };
                    subcanopylayers.Add(subcanopylayer, cohort);
                }
            }
            return subcanopylayers;
        }

        private static int[] GetNextBinPositions(int[] index_in, int numcohorts)
        {
            
            for (int index = index_in.Length - 1; index >= 0; index--)
            {
                int maxvalue = numcohorts - index_in.Length + index - 1;
                if (index_in[index] < maxvalue)
                {
                    {
                        index_in[index]++;

                        for (int i = index+1; i < index_in.Length; i++)
                        {
                            index_in[i] = index_in[i - 1] + 1;
                        }
                        return index_in;
                    }

                }
                /*
                else 
                {
                    if (index == 0) return null;

                    index_in[index - 1]++;
 
                    for (int i = index; i < index_in.Length; i++)
                    {
                        index_in[i] = index_in[index - 1] + i;
                    }
                     
                }
                 */
        }
            return null;
        }
      
        private int[] GetFirstBinPositions(int nlayers, int ncohorts)
        {
            int[] Bin = new int[nlayers - 1];

            for (int ly = 0; ly < Bin.Length; ly++)
            {
                Bin[ly] = ly+1;
            }
            return Bin;
        }

        public static List<T> Shuffle<T>(List<T> array)
        {
            int n = array.Count;
            while (n > 1)
            {
                int k = Statistics.DiscreteUniformRandom(0, n);
                n--;

                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
            return array;
        }

        uint CalculateLayerMaxDev(List<double> f)
        {
            return (uint)Math.Max(Math.Abs(f.Max() - f.Average()), Math.Abs(f.Min() - f.Average()));
        }

        int[] MinMaxCohortNr(int[] Bin, int i, int Count)
        {
            int min = (i > 0) ? Bin[i - 1] : 0;
            int max = (i < Bin.Count()) ? Bin[i] : Count - 1;

            return new int[] { min, max };
        }

        //private List<uint> layermaxdev = new List<uint>();

        /*private List<List<int>> GetBins(List<double> CumSublayerBiomass)
        {
            nlayers = 0;
            layermaxdev.Clear();
            if (CumSublayerBiomass.Count() == 0)
            {                
                return null;
            }

            // Bin and BestBin are lists of indexes that determine what cohort belongs to what canopy layer, 
            // i.e. when Bin[1] contains 45 then SubCanopyCohorts[45] is in layer 1
            int[] BestBin = null;
            int[] Bin = null;
                       

            float LayerMaxDev = float.MaxValue;

            //=====================OPTIMIZATION LOOP====================================
            do
            {
                nlayers++;
                

                Bin = GetFirstBinPositions(nlayers, CumSublayerBiomass.Count());

                while (Bin != null)
                {
                    layermaxdev.Clear();

                    if (Bin.Count() == 0)
                    {
                        layermaxdev.Add(CalculateLayerMaxDev(CumSublayerBiomass));
                    }
                    else for (int i = 0; i <= Bin.Count(); i++)
                    {
                        int[] MinMax = MinMaxCohortNr(Bin, i, CumSublayerBiomass.Count());

                        // Get the within-layer variance in biomass
                        layermaxdev.Add(CalculateLayerMaxDev(CumSublayerBiomass.GetRange(MinMax[0], MinMax[1] - MinMax[0])));
                    }

                    // Keep the optimal (min within-layer variance) layer setting
                    if (layermaxdev.Max() < LayerMaxDev)
                    {
                        BestBin = new List<int>(Bin).ToArray();
                        LayerMaxDev = layermaxdev.Max();
                    }
                    Bin = GetNextBinPositions(Bin, CumSublayerBiomass.Count());

                }
            }
            while (layermaxdev.Max() >= MaxDevLyrAv && nlayers < MaxCanopyLayers && nlayers < (CumSublayerBiomass.Count()/PlugIn.IMAX));
            //=====================END OPTIMIZATION LOOP====================================


            // Actual layer configuration
            List<List<int>> Bins = new List<List<int>>();
            if (BestBin.Count() == 0)
            {
                // One canopy layer
                Bins.Add(new List<int>());
                for (int i = 0; i < CumSublayerBiomass.Count(); i++)
                {
                    Bins[0].Add(i);
                }
            }
            else for (int i = 0; i <= BestBin.Count(); i++)
            {
                // Multiple canopy layers
                Bins.Add(new List<int>());

                int[] minmax = MinMaxCohortNr(BestBin, i, CumSublayerBiomass.Count());

                // Add index numbers to the Bins array
                for (int a = minmax[0]; a < ((i == BestBin.Count()) ? minmax[1]+1 : minmax[1]); a++)
                {
                    Bins[i].Add(a);
                }
            }
            return Bins;
        }*/
        private List<double> layerThreshRatio = new List<double>();
        private List<List<double>> GetBinsByCohort(List<double> CohortBiomassList)
        {
            if (CohortBiomassList.Count() == 0)
            {
                return null;
            }

            nlayers = 1;
            layerThreshRatio.Clear();
            float diffProp = LayerThreshRatio;
            // sort by ascending biomass
            CohortBiomassList.Sort();
            // reverse to sort by descending biomass
            CohortBiomassList.Reverse();

            int tempMaxCanopyLayers = MaxCanopyLayers;

            if(CohortStacking)
            {
                tempMaxCanopyLayers = CohortBiomassList.Count();
                diffProp = 1;
            }    

            List<List<double>> CohortBins = new List<List<double>>();
            int topLayerIndex = 0;
            CohortBins.Add(new List<double>());
            CohortBins[0].Add(CohortBiomassList[0]);
            foreach (double cohortBio in CohortBiomassList)
            {
                double smallestThisLayer = CohortBins[0][0];
                //if (layerIndex > 0)
                //{
                //    smallestThisLayer = CohortBins[layerIndex][0];
                //}
                double ratio = (cohortBio / smallestThisLayer);

                if (ratio < (diffProp))
                {
                    if (topLayerIndex < (tempMaxCanopyLayers - 1))
                    {
                        topLayerIndex++;
                        nlayers++;
                        CohortBins.Add(new List<double>());
                        foreach (int i in Enumerable.Range(1, topLayerIndex).Reverse())
                        {
                            CohortBins[i] = new List<double>(CohortBins[i - 1]);
                        }
                        CohortBins[0].Clear();
                    }
                }

                // Add a negligable value [-1e-10; + 1e-10] to ratio in order to prevent duplicate keys
                double k = 1e-10 * 2.0 * (Statistics.ContinuousUniformRandom() - 0.5);
                layerThreshRatio.Add(ratio + k);
                if (!(CohortBins[0].Contains(cohortBio)))
                    CohortBins[0].Add(cohortBio);
                //bool largeRatio = (ratio > 10.0);
                //if (!(ratioAbove10.ContainsKey(cohortBio)))
                //{
                //    ratioAbove10.Add(cohortBio, largeRatio);
                //}
            }
            bool tooManyLayers = false;
            if (CohortBins.Count() > tempMaxCanopyLayers)
            {
                tooManyLayers = true;
            }
            if (tooManyLayers)
            {
                List<double> sortedRatios = layerThreshRatio.ToList();
                sortedRatios.Sort();
                //sortedRatios.Reverse();
                List<double> smallestRatios = new List<double>();
                for (int r = 0; r < (tempMaxCanopyLayers - 1); r++)
                {
                    smallestRatios.Add(sortedRatios[r]);
                }

                CohortBins.Clear();
                topLayerIndex = tempMaxCanopyLayers - 1;
                nlayers = 1;
                for (int r = 0; r < tempMaxCanopyLayers; r++)
                {
                    CohortBins.Add(new List<double>());
                }
                CohortBins[topLayerIndex].Add(CohortBiomassList[0]);
                int cohortInd = 0;
                foreach (double cohortRatio in layerThreshRatio)
                {
                    if (smallestRatios.Contains(cohortRatio))
                    {
                        //if (nlayers < (tempMaxCanopyLayers))
                        //{
                        topLayerIndex--;
                        nlayers++;

                    }
                    if (!(CohortBins[topLayerIndex].Contains(CohortBiomassList[cohortInd])))
                        CohortBins[topLayerIndex].Add(CohortBiomassList[cohortInd]);
                    cohortInd++;
                }

            }

            return CohortBins;
        }
        public static uint ComputeKey(uint a, ushort b)
        {
            uint value = (uint)((a << 16) | b);
            return value;
        }

        public List<Cohort> AllCohorts
        {
            get
            {
                List<Cohort> all = new List<Cohort>();
                foreach (ISpecies spc in cohorts.Keys)
                {
                    all.AddRange(cohorts[spc]);
                }
                return all;
            }
        }

        public void ClearAllCohorts()
        {
            cohorts.Clear();
        }
        public int ReduceOrKillBiomassCohorts(Landis.Library.BiomassCohorts.IDisturbance disturbance)
        {
            List<int> reduction = new List<int>();

            List<Cohort> ToRemove = new List<Cohort>();

            foreach (List<Cohort> species_cohort in cohorts.Values)
            {
                Landis.Library.PnETCohorts.SpeciesCohorts species_cohorts = GetSpeciesCohort(cohorts[species_cohort[0].Species]);

                for (int c = 0; c < species_cohort.Count(); c++)
                {
                    Landis.Library.PnETCohorts.ICohort cohort = species_cohort[c];

                    // Disturbances return reduction in aboveground biomass
                    int _reduction = disturbance.ReduceOrKillMarkedCohort(cohort);

                    reduction.Add(_reduction);
                    if (reduction[reduction.Count() - 1] >= species_cohort[c].Biomass)  //Compare to aboveground biomass at site scale
                    {
                        ToRemove.Add(species_cohort[c]);
                        // Edited by BRM - 090115
                    }
                    else
                    {
                        double reductionProp = (double)reduction[reduction.Count() - 1] / (double)species_cohort[c].Biomass;  //Proportion of aboveground biomass at site scale
                        species_cohort[c].ReduceBiomass(this, reductionProp, disturbance.Type);  // Reduction applies to all biomass
                    }
                    //
                }

            }

            foreach (Cohort cohort in ToRemove)
            {
                RemoveCohort(cohort, disturbance.Type);
            }

            return reduction.Sum();
        }

        public int ReduceOrKillBiomassCohorts(IDisturbance disturbance)
        {
            List<int> reduction = new List<int>();

            List<Cohort> ToRemove = new List<Cohort>();

            foreach (List<Cohort> species_cohort in cohorts.Values)
            {
                Landis.Library.PnETCohorts.SpeciesCohorts species_cohorts = GetSpeciesCohort(cohorts[species_cohort[0].Species]);

                for (int c = 0; c < species_cohort.Count(); c++)
                {
                    Landis.Library.PnETCohorts.ICohort cohort = species_cohort[c];

                    // Disturbances return reduction in aboveground biomass
                    int _reduction = disturbance.ReduceOrKillMarkedCohort(cohort);

                    reduction.Add(_reduction);
                    if (reduction[reduction.Count() - 1] >= species_cohort[c].Biomass)  //Compare to aboveground biomass at site scale
                    {
                        ToRemove.Add(species_cohort[c]);
                        // Edited by BRM - 090115
                    }
                    else
                    {
                        double reductionProp = (double)reduction[reduction.Count() - 1] / (double)species_cohort[c].Biomass;  //Proportion of aboveground biomass at site scale
                        species_cohort[c].ReduceBiomass(this, reductionProp, disturbance.Type);  // Reduction applies to all biomass
                    }
                    //
                }

            }

            foreach (Cohort cohort in ToRemove)
            {
                RemoveCohort(cohort, disturbance.Type);
            }

            return reduction.Sum();
        }

        public int AgeMax 
        {
            get
            {
                return (cohorts.Values.Count() > 0) ? cohorts.Values.Max(o => o.Max(x => x.Age)) : -1;
            }
        }
       
        Landis.Library.AgeOnlyCohorts.ISpeciesCohorts Landis.Library.Cohorts.ISiteCohorts<Landis.Library.AgeOnlyCohorts.ISpeciesCohorts>.this[ISpecies species]
        {
            get
            {
                if (cohorts.ContainsKey(species))
                {
                    return (Landis.Library.AgeOnlyCohorts.ISpeciesCohorts)GetSpeciesCohort(cohorts[species]);
                }
                return null;
            }
        }

        public Landis.Library.BiomassCohorts.ISpeciesCohorts this[ISpecies species]
        {
            get
            {
                if (cohorts.ContainsKey(species))
                {
                    return GetSpeciesCohort(cohorts[species]);
                }
                return null;
                
            }
        }

        void Landis.Library.AgeOnlyCohorts.ISiteCohorts.RemoveMarkedCohorts(Landis.Library.AgeOnlyCohorts.ICohortDisturbance disturbance)
        {
            /*
            if (AgeOnlyDisturbanceEvent != null)
            {
                AgeOnlyDisturbanceEvent(this, new Landis.Library.BiomassCohorts.DisturbanceEventArgs(disturbance.CurrentSite, disturbance.Type));
            }
             */
            ReduceOrKillBiomassCohorts(new Landis.Library.BiomassCohorts.WrappedDisturbance(disturbance));
        }

        void Landis.Library.AgeOnlyCohorts.ISiteCohorts.RemoveMarkedCohorts(Landis.Library.AgeOnlyCohorts.ISpeciesCohortsDisturbance disturbance)
        {
            /*
            if (AgeOnlyDisturbanceEvent != null)
            {
                AgeOnlyDisturbanceEvent(this, new Landis.Library.BiomassCohorts.DisturbanceEventArgs(disturbance.CurrentSite, disturbance.Type));
            }
            */

            // Does this only occur when a site is disturbed?
            //Allocation.ReduceDeadPools(this, disturbance.Type); 

            //  Go through list of species cohorts from back to front so that
            //  a removal does not mess up the loop.
            int totalReduction = 0;

            List<Cohort> ToRemove = new List<Cohort>();

            Landis.Library.AgeOnlyCohorts.SpeciesCohortBoolArray isSpeciesCohortDamaged = new Landis.Library.AgeOnlyCohorts.SpeciesCohortBoolArray();

            foreach (ISpecies spc in cohorts.Keys)
            {
                Landis.Library.PnETCohorts.SpeciesCohorts speciescohort = GetSpeciesCohort(cohorts[spc]);
               
                isSpeciesCohortDamaged.SetAllFalse(speciescohort.Count);

                disturbance.MarkCohortsForDeath((Landis.Library.AgeOnlyCohorts.ISpeciesCohorts)speciescohort, isSpeciesCohortDamaged);

                for (int c = 0; c < isSpeciesCohortDamaged.Count; c++)
                {
                    if (isSpeciesCohortDamaged[c])
                    {
                        totalReduction += speciescohort[c].Biomass;

                        ToRemove.Add(cohorts[spc][c]);
//                        ToRemove.AddRange(cohorts[spc].Where(o => o.Age == speciescohort[c].Age));
                    }
                }

            }
            foreach (Cohort cohort in ToRemove)
            {
                Landis.Library.BiomassCohorts.Cohort.KilledByAgeOnlyDisturbance(disturbance, cohort, disturbance.CurrentSite, disturbance.Type);
                RemoveCohort(cohort, disturbance.Type);
            }
        }
        private void RemoveMarkedCohorts()
        {
            for (int c = cohorts.Values.Count - 1; c >= 0; c--)
            {
                List<Cohort> species_cohort = cohorts.Values.ElementAt(c);

                for (int cc = species_cohort.Count - 1; cc >= 0; cc--)
        {
                    if (species_cohort[cc].IsAlive == false)
                    {
                        bool coldKill = species_cohort[cc].ColdKill < int.MaxValue;
                        if(coldKill)
                            RemoveCohort(species_cohort[cc], new ExtensionType(Names.ExtensionName+":Cold"));
                        else
                            RemoveCohort(species_cohort[cc], new ExtensionType(Names.ExtensionName));
                    }
                }
            }
        }

        // with cold temp killing cohorts - now moved to within CalculatePhotosynthesis function
        /*private void RemoveMarkedCohorts(float minMonthlyAvgTemp, float winterSTD)
        {

            for (int c = cohorts.Values.Count - 1; c >= 0; c--)
            {
                List<Cohort> species_cohort = cohorts.Values.ElementAt(c);

                for (int cc = species_cohort.Count - 1; cc >= 0; cc--)
                {
                    if (species_cohort[cc].IsAlive == false)
                    {
                      
                        RemoveCohort(species_cohort[cc], new ExtensionType(Names.ExtensionName));

                    }
                    else
                    {
                        if(permafrost)
                        {
                            // Check if low temp kills cohorts
                            if((minMonthlyAvgTemp - (3.0 * winterSTD)) < species_cohort[cc].SpeciesPnET.ColdTol)
                            {
                                RemoveCohort(species_cohort[cc], new ExtensionType(Names.ExtensionName));
                            }
                        }
                    }
                
                }
            }

        }*/

        public void RemoveCohort(Cohort cohort, ExtensionType disturbanceType)
        {

            if(disturbanceType.Name == Names.ExtensionName)
            {
                CohortsKilledBySuccession[cohort.Species.Index] += 1;
            }
            else if(disturbanceType.Name == (Names.ExtensionName+":Cold"))
            { 
                CohortsKilledByCold[cohort.Species.Index] += 1;
            }
            else if(disturbanceType.Name == "disturbance:harvest")
            {
                CohortsKilledByHarvest[cohort.Species.Index] += 1;
            }
            else if(disturbanceType.Name == "disturbance:fire")
            {
                CohortsKilledByFire[cohort.Species.Index] += 1;
            }
            else if (disturbanceType.Name == "disturbance:wind")
            {
                CohortsKilledByWind[cohort.Species.Index] += 1;
            }
            else
            {
                CohortsKilledByOther[cohort.Species.Index] += 1;
            }

            if (disturbanceType.Name != Names.ExtensionName)
            {
                Cohort.RaiseDeathEvent(this, cohort, Site, disturbanceType);
            }

            cohorts[cohort.Species].Remove(cohort);

            if (cohorts[cohort.Species].Count == 0)
            {
                cohorts.Remove(cohort.Species);
            }

            if (!DisturbanceTypesReduced.Contains(disturbanceType))
            {
                Allocation.ReduceDeadPools(this, disturbanceType); // Reduce dead pools before adding through Allocation
                DisturbanceTypesReduced.Add(disturbanceType);
            }
            Allocation.Allocate(this, cohort, disturbanceType, 1.0);  // Allocation fraction is 1.0 for complete removals


        }

        public bool IsMaturePresent(ISpecies species)
        {
            //ISpeciesPnET pnetSpecies = SpeciesParameters.SpeciesPnET[species];

            bool speciesPresent = cohorts.ContainsKey(species);

            bool IsMaturePresent = (speciesPresent && (cohorts[species].Max(o => o.Age) >= species.Maturity)) ? true : false;

            return IsMaturePresent;
        }

        public bool AddNewCohort(Cohort newCohort)
        {
            bool addCohort = false;
            if (cohorts.ContainsKey(newCohort.Species))
            {
                // This should deliver only one KeyValuePair
                KeyValuePair<ISpecies, List<Cohort>> i = new List<KeyValuePair<ISpecies, List<Cohort>>>(cohorts.Where(o => o.Key == newCohort.Species))[0];

                List<Cohort> Cohorts = new List<Cohort>(i.Value.Where(o => o.Age < CohortBinSize));
                //List<Cohort> Cohorts = new List<Cohort>(i.Value.Where(o => o.Age < Timestep));

                if(Cohorts.Count > 1)
                {
                    foreach(Cohort Cohort in Cohorts.Skip(1))
                    {
                        newCohort.Accumulate(Cohort);
                    }
                }                

                if (Cohorts.Count() > 0)
                {
                    Cohorts[0].Accumulate(newCohort);
                    return addCohort;
                }
                else
                {

                    cohorts[newCohort.Species].Add(newCohort);
                    addCohort = true;
                    return addCohort;
                }

            }
            cohorts.Add(newCohort.Species, new List<Cohort>(new Cohort[] { newCohort }));
            addCohort = true;
            return addCohort;
        }

        Landis.Library.PnETCohorts.SpeciesCohorts GetSpeciesCohort(List<Cohort> cohorts)
        {
            Landis.Library.PnETCohorts.SpeciesCohorts spc = new Library.PnETCohorts.SpeciesCohorts(cohorts[0]);

            for (int c = 1; c < cohorts.Count; c++)
            {
                spc.AddNewCohort(cohorts[c]);
            }
            

            return spc;
        }

        public void AddWoodyDebris(float Litter, float KWdLit)
        {
            lock (Globals.CWDThreadLock)
            {
                SiteVars.WoodyDebris[Site].AddMass(Litter, KWdLit);
            }
        }
        public void RemoveWoodyDebris(double percentReduction)
        {
            lock (Globals.CWDThreadLock)
            {
                SiteVars.WoodyDebris[Site].ReduceMass(percentReduction);
            }
        }
        public void AddLitter(float AddLitter, ISpeciesPnET spc)
        {
            lock (Globals.litterThreadLock)
            {
                double KNwdLitter = Math.Max(0.3, (-0.5365 + (0.00241 * AET.Sum())) - (((-0.01586 + (0.000056 * AET.Sum())) * spc.FolLignin * 100)));

                SiteVars.Litter[Site].AddMass(AddLitter, KNwdLitter);
            }
        }
        public void RemoveLitter(double percentReduction)
        {
            lock (Globals.litterThreadLock)
            {
                SiteVars.Litter[Site].ReduceMass(percentReduction);
            }
        }
        
        string Header(Landis.SpatialModeling.ActiveSite site)
        {
            
            string s = OutputHeaders.Time +  "," +
                       OutputHeaders.Year + "," +
                       OutputHeaders.Month + "," +
                       OutputHeaders.Ecoregion + "," + 
                       OutputHeaders.SoilType +"," +
                       OutputHeaders.NrOfCohorts + "," +
                       OutputHeaders.MaxLayerRatio + "," +
                       OutputHeaders.Layers + "," +
                       OutputHeaders.SumCanopyProp + "," +
                       OutputHeaders.PAR0 + "," +
                       OutputHeaders.Tmin + "," +
                       OutputHeaders.Tave + "," +
                       OutputHeaders.Tday + "," +
                       OutputHeaders.Tmax + "," +
                       OutputHeaders.Precip + "," +
                       OutputHeaders.CO2 + "," +
                       OutputHeaders.O3 + "," +
                       OutputHeaders.RunOff + "," + 
                       OutputHeaders.Leakage + "," + 
                       OutputHeaders.PET + "," +
                       OutputHeaders.PE + "," +
                       OutputHeaders.Evaporation + "," +
                       OutputHeaders.PotentialTranspiration + "," +
                       OutputHeaders.Transpiration + "," + 
                       OutputHeaders.Interception + "," +
                       OutputHeaders.SurfaceRunOff + "," +
                       OutputHeaders.water + "," +
                       OutputHeaders.PressureHead + "," + 
                       OutputHeaders.SurfaceWater + "," +
                       OutputHeaders.availableWater + "," +
                       OutputHeaders.SnowPack + "," +
                        OutputHeaders.LAI + "," + 
                        OutputHeaders.VPD + "," + 
                        OutputHeaders.GrossPsn + "," + 
                        OutputHeaders.NetPsn + "," +
                        OutputHeaders.MaintResp + "," +
                        OutputHeaders.Wood + "," + 
                        OutputHeaders.Root + "," + 
                        OutputHeaders.Fol + "," + 
                        OutputHeaders.NSC + "," + 
                        OutputHeaders.HeteroResp + "," +
                        OutputHeaders.Litter + "," + 
                        OutputHeaders.CWD + "," +
                        OutputHeaders.WoodySenescence + "," + 
                        OutputHeaders.FoliageSenescence + "," +
                        OutputHeaders.SubCanopyPAR + ","+
                        OutputHeaders.SoilDiffusivity + "," +
                        OutputHeaders.ActiveLayerDepth+","+
                        OutputHeaders.LeakageFrac + "," +

                        
                        OutputHeaders.AverageAlbedo + "," +
                        OutputHeaders.FrostDepth + "," +
                        OutputHeaders.SPEI
                        + "," + "WoodMass"
                        + "," + "TotalBiomass"
                        + "," + "HOM"
                        + "," + "HON"
                        + "," + "SoilDecResp"
                        + "," + "NEP"
                        + "," + "GrossNMin"
                        + "," + "NetNMin"
                        + "," + "NRatioNit"
                        + "," + "NetNitr"
                        + "," + "NDrain"
                        + "," + "NdepTot"
                        + "," + "NPoolTot"
                        + "," + "NetN"
                        + "," + "RootNSink"
                        + "," + "PlantNSite"
                        + "," + "LitterMSite"
                        + "," + "LitterNSite"
                        + "," + "Canopy%N"
                        + "," + "NRatioSite"
                        + "," + "LitFolNSite"
                        + "," + "LitRootNSite"
                        + "," + "LitWoodNSite"
                        + "," + "Litter%NSite"
                        + "," + "OpenSpace"

                        + "," + "CanopyNSite"
                        + "," + "WoodNSite"
                        + "," + "RootNSite"
                        + "," + "DWoodNSite"

                        + "," + "Wood%NSite"
                        + "," + "DWood%NSite"
                        
                        + "," + "NH4dep"
                        + "," + "NO3dep"

                        + "," + "NetPsnSite"
                        + "," + "SoilDecResp"
                        + "," + "WoodDecRespSite"
                        + "," + "WoodMRespSite"
                        + "," + "WoodGRespSite"
                        + "," + "FolGRespSite"
                        + "," + "RootMRespSite"
                        + "," + "RootGRespSite"

            ; 


            return s;
        }

        private void AddSiteOutput(IEcoregionPnETVariables monthdata)
        {
            //uint maxLayerDev = 0;
            //if (layermaxdev.Count() > 0)
            //    maxLayerDev = layermaxdev.Max();
            double maxLayerRatio = 0;
            if (layerThreshRatio.Count() > 0)
                maxLayerRatio = layerThreshRatio.Max();

            string s = monthdata.Time + "," +
                monthdata.Year + "," +
                monthdata.Month + "," +
                Ecoregion.Name + "," +
                Ecoregion.SoilType + "," +
                cohorts.Values.Sum(o => o.Count) + "," +
                //maxLayerDev + "," +
                maxLayerRatio + "," +
                nlayers + "," +
                cohorts.Values.Sum(o => o.Sum(x => (x.CanopyLayerProp))) + "," +
                monthdata.PAR0 + "," +
                monthdata.Tmin + "," +
                monthdata.Tave + "," +
                monthdata.Tday + "," +
                monthdata.Tmax + "," +
                monthdata.Prec + "," +
                monthdata.CO2 + "," +
                monthdata.O3 + "," +
                hydrology.RunOff + "," +
                hydrology.Leakage + "," +
                hydrology.PET + "," +
                hydrology.PE + "," +
                hydrology.Evaporation + "," +
                cohorts.Values.Sum(o => o.Sum(x => (x.PotentialTranspiration.Sum()))) + "," +
                cohorts.Values.Sum(o => o.Sum(x => (x.Transpiration.Sum()))) + "," +
                interception + "," +
                precLoss + "," +
                hydrology.Water + "," +
                hydrology.PressureHeadTable.CalculateWaterPressure(hydrology.Water,Ecoregion.SoilType)+ "," +
                hydrology.SurfaceWater + "," +
                ((hydrology.Water - Ecoregion.WiltPnt) * Ecoregion.RootingDepth * propRootAboveFrost + hydrology.SurfaceWater) + "," +  // mm of avialable water
                snowPack + "," +
                cohorts.Values.Sum(o => o.Sum(x => (x.LAI.Sum() * x.CanopyLayerProp))) + "," +
                //this.CanopyLAI.Sum() + "," +
                monthdata.VPD + "," +
                cohorts.Values.Sum(o => o.Sum(x => (x.GrossPsn.Sum() * x.CanopyLayerProp))) + "," +
                cohorts.Values.Sum(o => o.Sum(x => (x.NetPsn.Sum() * x.CanopyLayerProp))) + "," +
                cohorts.Values.Sum(o => o.Sum(x => (x.MaintenanceRespiration.Sum() * x.CanopyLayerProp))) + "," +
                cohorts.Values.Sum(o => o.Sum(x => (x.Wood * x.CanopyLayerProp))) + "," +
                cohorts.Values.Sum(o => o.Sum(x => (x.Root * x.CanopyLayerProp))) + "," +
                cohorts.Values.Sum(o => o.Sum(x => (x.Fol * x.CanopyLayerProp))) + "," +
                cohorts.Values.Sum(o => o.Sum(x => (x.NSC * x.CanopyLayerProp))) + "," +
                HeterotrophicRespiration + "," +
                SiteVars.Litter[Site].Mass + "," +
                SiteVars.WoodyDebris[Site].Mass + "," +
                cohorts.Values.Sum(o => o.Sum(x => (x.LastWoodySenescence * x.CanopyLayerProp))) + "," +
                cohorts.Values.Sum(o => o.Sum(x => (x.LastFoliageSenescence * x.CanopyLayerProp))) + "," +
                subcanopypar + "," +
                soilDiffusivity + "," +
                activeLayerDepth[monthdata.Month - 1] * 1000 + "," +
                leakageFrac + "," +

                
                averageAlbedo[monthdata.Month - 1] + "," +
                frostDepth[monthdata.Month - 1] * 1000 + "," +
                monthdata.SPEI
           //    + "," + cohorts.Values.Sum(o => o.Sum(x => x.WoodMassCo))
           //     + "," + cohorts.Sum(x => x.WoodMass)
                + "," + WoodMassSite
                + "," + cohorts.Values.Sum(o => o.Sum(x => x.TotalBiomass * x.CanopyLayerProp))
                + "," + HOM
                + "," + HON
                + "," + SoilDecResp
                + "," + NEPSite
                + "," + GrossNMin
                + "," + NetNMin
                + "," + NRatioNit
                + "," + NetNitr
                + "," + NDrain
                + "," + NdepTot
                + "," + NPools_Site
                + "," + NPools_MonNet
                + "," + RootNSink
                + "," + PlantNSite
                + "," + LitterMSite
                + "," + LitterNSite
                + "," + CanopyNCon * 100.0f
                + "," + NRatioSite
                + "," + LitFolNSite
                + "," + LitRootNSite
                + "," + LitWoodNSite
                + "," + LitterNSite / LitterMSite
                + "," + OpenSpaceSite

                + "," + CanopyNSite
                + "," + WoodNSite
                + "," + RootNSite                
                + "," + DWoodNSite
                
                + "," +   WoodNSite / WoodMassSite
                + "," +  DWoodNSite/ DeadWoodMSite
 
                + "," + monthdata.NH4
                + "," + monthdata.NO3

                + "," + NetPsnSite
                + "," + SoilDecResp
                + "," + WoodDecRespSite
                + "," + WoodMRespSite
                + "," + WoodGRespSite
                + "," + FolGRespSite
                + "," + RootMRespSite
                + "," + RootGRespSite



            ;

            this.siteoutput.Add(s);
        }
 
        public IEnumerator<Landis.Library.BiomassCohorts.ISpeciesCohorts> GetEnumerator()
        {
            foreach (ISpecies species in cohorts.Keys)
            {
                yield return this[species];
            }
        }
       
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<Landis.Library.BiomassCohorts.ISpeciesCohorts> IEnumerable<Landis.Library.BiomassCohorts.ISpeciesCohorts>.GetEnumerator()
        {
            foreach (ISpecies species in cohorts.Keys)
            {
                Landis.Library.BiomassCohorts.ISpeciesCohorts isp = this[species];
                yield return isp;
            }
             
        }

        IEnumerator<Landis.Library.AgeOnlyCohorts.ISpeciesCohorts> IEnumerable<Landis.Library.AgeOnlyCohorts.ISpeciesCohorts>.GetEnumerator()
        {
            foreach (ISpecies species in cohorts.Keys)
            {
                yield return (Landis.Library.AgeOnlyCohorts.ISpeciesCohorts)this[species];
            }
        }

        public struct CumulativeLeafAreas
        {
            public float DarkConifer;
            public float LightConifer;
            public float Deciduous;
            public float GrassMossOpen;

            public float Total
            {
                get
                {
                    return DarkConifer + LightConifer + Deciduous + GrassMossOpen;
                }
            }

            public float DarkConiferProportion
            {
                get
                {
                    return Total == 0 ? 0 : DarkConifer / Total;
                }
            }

            public float LightConiferProportion
            {
                get
                {
                    return Total == 0 ? 0 : LightConifer / Total;
                }
            }

            public float DeciduousProportion
            {
                get
                {
                    return Total == 0 ? 0 : Deciduous / Total;
                }
            }

            public float GrassMossOpenProportion
            {
                get
                {
                    return Total == 0 ? 0 : GrassMossOpen / Total;
                }
            }

            public void Reset()
            {
                DarkConifer = 0;
                LightConifer = 0;
                Deciduous = 0;
                GrassMossOpen = 0;
            }
        }


        /// <summary>
        /// CN functions at site level   //ZHOU
        /// </summary>
       
        
        
        
        
        public void Competition_MaxNStore()
        {

            //Distribution based on the biomass, get the weighted MaxNStore, which is a site level attribute. need to revisit for multi-layers canopy. ZZX

            AllCohorts.ForEach(x =>
            {
                x.BiomassWeight = x.Biomass / BiomassSite;
                x.MaxNStoreWeighted = x.MaxNStore * x.BiomassWeight;

            });
            MaxNStoreSite = AllCohorts.Sum(x => x.MaxNStoreWeighted);
        }
        public float MaxNStoreSite
        { get; set; }


        public float PlantNUptakeSite
        { get; set; }

        public float PlantNSite
        {
            get
            {
                return AllCohorts.Sum(o => o.PlantN * o.CanopyLayerProp);
            }
            set {; }
        }


        public float ExcessNSite
        { get; set; }

        public float OpenSpaceSite
        {
            get
            {
                float[] CanopyPropSum = new float[MaxCanopyLayers];

                float[] GroundProp = new float[MaxCanopyLayers];

                float OpenSpace = 1.0f;
                foreach (Cohort cohort in AllCohorts)

                {

                    int layer = cohort.Layer;

                    CanopyPropSum[layer] += cohort.CanopyLayerProp;


                }



                for (int layer = 0; layer < MaxCanopyLayers; layer++)

                {

                    if (CanopyPropSum[layer] < 1.0)

                    {

                        float propGround = 1.0f - CanopyPropSum[layer];

                        GroundProp[layer] = propGround;

                    }

                    else

                    {

                        GroundProp[layer] = 0.0f;
                        for (int i = 0; i < AllCohorts.Count(); i++)
                        { 
                           // AllCohorts[i].CanopyLayerProp = AllCohorts[i].Biomass / BiomassSite;
                            
                        }
                        //float CanopySumScaler = CanopySumScale;

                    }
                    OpenSpace = OpenSpace * GroundProp[layer];

                }
                return OpenSpace;
            }
            set {; }
        }


        public void Competition_PlantNUptake()
        {
            float NH4Up, NO3Up;
            float PlantNuptemp; 

            
            // MaxNStoreSite = 21.0f;

            ////Plant Uptake Distribution, based on the biomass

            // float BiomassSite = AllCohorts.Sum(o => o.Biomass);
            float TMult = (float)Math.Exp(0.1f * (Ecoregion.Variables.Tave - 7.1f)) * 0.68f;

            for (int i = 0; i < AllCohorts.Count(); i++)
            {
                
                AllCohorts[i].RootNSinkStr = (float)(Math.Min((AllCohorts[i].RootNSinkEff * TMult), 0.98));

                AllCohorts[i].BiomassWeight = AllCohorts[i].Biomass / BiomassSite;
                AllCohorts[i].RootNSinkStrWeighted = AllCohorts[i].RootNSinkStr * AllCohorts[i].BiomassWeight;


            }


            //AllCohorts.ForEach(x =>
            //{
            //    x.PlantNUptakeWeight = x.Biomass / TotBiomass;
            //    x.RootNSinkStrWeighted = x.RootNSinkStr * x.PlantNUptakeWeight;

            //});

            RootNSink = AllCohorts.Sum(o => o.RootNSinkStrWeighted);
            if (RootNSink < 0) RootNSink = 0;
            if (RootNSink > 1) RootNSink = 1.0f;

            AllCohorts.ForEach(x =>
            {
                x.PlantNUptakeWeight = x.RootNSinkStrWeighted / RootNSink;

            });


            //PlantNUptakeSite = (NH4 + NO3) * (1.0f - OpenSpaceSite()) * RootNSink ;  //
            PlantNUptakeSite = (NH4 + NO3) * RootNSink;  // potential plant N update, will adjust based on the plant N pool

            //   float PlantNSite = AllCohorts.Sum(o => o.PlantN);


            if (PlantNUptakeSite < 0)
            {
                PlantNUptakeSite = 0;
                //RootNSink = 0;
            }



            // Adjust PlantNUptake based on the Plant N pool size not more than the MaxNStore

            for (int i = 0; i < AllCohorts.Count(); i++)
            {
                //PlantNUptakeSite * AllCohorts[i].PlantNUptakeWeight is the individual cohorts uptake at site level,
                // then back scaled to cohort level with its proportion .CanopyLayerProp
                AllCohorts[i].PlantNUptakeCo = PlantNUptakeSite * AllCohorts[i].PlantNUptakeWeight;  // cohort at site level
                //AllCohorts[i].PlantNUptake = AllCohorts[i].PlantNUptakeCo / AllCohorts[i].CanopyLayerProp;  // back scale to the full unit level
                AllCohorts[i].PlantNUptake = AllCohorts[i].PlantNUptakeCo / AllCohorts[i].BiomassWeight;  // back scale to the full unit level
                /*
                if ((AllCohorts[i].PlantN * AllCohorts[i].CanopyLayerProp + AllCohorts[i].PlantNUptakeCo) > AllCohorts[i].MaxNStore * AllCohorts[i].CanopyLayerProp)
                {
                    AllCohorts[i].PlantNUptakeCo = (AllCohorts[i].MaxNStore - AllCohorts[i].PlantN) * AllCohorts[i].CanopyLayerProp;
                    if (AllCohorts[i].PlantNUptakeCo < 0.0f) AllCohorts[i].PlantNUptakeCo = 0.0f;

                }
                AllCohorts[i].PlantNUptake = AllCohorts[i].PlantNUptakeCo / AllCohorts[i].CanopyLayerProp;
                */

                if ((AllCohorts[i].PlantN + AllCohorts[i].PlantNUptake )> AllCohorts[i].MaxNStore)  // 
                {
                    AllCohorts[i].PlantNUptake = AllCohorts[i].MaxNStore - AllCohorts[i].PlantN;
                    if (AllCohorts[i].PlantNUptake < 0.0f) AllCohorts[i].PlantNUptake = 0.0f;
                    AllCohorts[i].PlantNUptakeCo = AllCohorts[i].PlantNUptake* AllCohorts[i].BiomassWeight;  // downscale the cohort uptake at site level

                }

                AllCohorts[i].PlantN += AllCohorts[i].PlantNUptake;
                

            }
            
            PlantNUptakeSite = AllCohorts.Sum(o => o.PlantNUptakeCo);  // actual Plant N uptake
            RootNSink = PlantNUptakeSite / (NO3 + NH4);  // actual root N sink
            
            NH4Up = NH4 * RootNSink;
            NH4 = NH4 - NH4Up;
            NO3Up = NO3 * RootNSink;
            NO3 = NO3 - NO3Up;

            //PlantNUptakeYr = PlantNUptakeYr + PlantNUptake;

        }

        public float RootNSink
        { get; set; }
/*
        public float NO3
        { get; set; }

        public float NH4
        { get; set; }
*/

        public float NO3
        {
            get
            { return SiteVars.NO3Site[Site]; }
            set
            { SiteVars.NO3Site[Site] = value; }
        }

        public float NH4
        {
            get
            { return SiteVars.NH4Site[Site]; }
            set
            { SiteVars.NH4Site[Site] = value; }
        }


        public float HOM
        { get 
            { return SiteVars.HOMSite[Site]; } 
            set 
            { SiteVars.HOMSite[Site] = value; } 
        }

        public float HON
        {
            get
            { return SiteVars.HONSite[Site]; }
            set
            { SiteVars.HONSite[Site] = value; }
        }

        public float GrossNMin
        { get; set; }

        public float SoilDecResp
        { get; set; }

        public float GrossNMinYr
        { get; set; }
        public float GrossNImmobYr
        { get; set; }

        public float NetNMinYr
        { get; set; }
        public float NetNitrYr
        { get; set; }
        public float NetNMin
        { get; set; }
        public float NetNitr
        { get; set; }


        public float LitterMSite
        {
            get
            {

                return LitFolSite + LitRootSite  + LitWoodSite;
            }

        }
        public float LitFolSite
        {
            get
            {
                return AllCohorts.Sum(o => o.FolLitM * o.CanopyLayerProp);

            }

        }
        public float LitWoodSite
        {
            get
            {
                return AllCohorts.Sum(o => o.WoodTransM * o.CanopyLayerProp);

            }

        }
        public float LitRootSite
        {
            get
            {
                return AllCohorts.Sum(o => o.RootLitM * o.CanopyLayerProp);

            }

        }
        public float LitterNSite
        {
            get
            {
                float LitFolN = AllCohorts.Sum(o => o.FolLitN * o.CanopyLayerProp);
                float LitRootN = AllCohorts.Sum(o => o.RootLitN * o.CanopyLayerProp);
                float LitWoodN = AllCohorts.Sum(o => o.WoodTransN * o.CanopyLayerProp);
                return LitFolN  + LitRootN  + LitWoodN;
            }

        }

        public float LitFolNSite
        {
            get
            {
                float dummy;
                dummy = FolLitMSite;
                return AllCohorts.Sum(o => o.FolLitN * o.CanopyLayerProp);

            }

        }

        public float FolLitMSite
        {
            get
            {
                return AllCohorts.Sum(o => o.FolLitM * o.CanopyLayerProp);

            }

        }
        public float LitRootNSite
        {
            get
            {

                return AllCohorts.Sum(o => o.RootLitN * o.CanopyLayerProp);

            }

        }
        public float LitWoodNSite
        {
            get
            {

                return AllCohorts.Sum(o => o.WoodTransN * o.CanopyLayerProp);

            }

        }

        public float BiomassSite
        {
            get
            {
                return AllCohorts.Sum(o => o.Biomass);
            }

        }
        public float WoodMassSite
        {
            get
            {
                return AllCohorts.Sum(o => o.WoodMassCo);
            }

        }
        public float DeadWoodMSite
        {
            get
            {
                return AllCohorts.Sum(o => o.DeadWoodM * o.CanopyLayerProp);
            }

        }
        
        public float NRatioNit
        {
            get
            { return SiteVars.NRatioNit[Site]; }
            set
            { SiteVars.NRatioNit[Site] = value; }
        }





        public float DHO
        { get; set; }
        public float NEPSite
        { 
            get
            {
                float NEPtemp;
                NEPtemp = NetPsnSite - SoilDecResp - WoodDecRespSite - WoodMRespSite - WoodGRespSite - FolGRespSite - RootMRespSite - RootGRespSite;
                
                return NEPtemp;
            }


                set {; } }



        public float TotGPPSite  // this is GPP
        {
            get
            {
                // AllCohorts.Sum(o => o.GrossPsn * o.CanopyLayerProp);
                //return AllCohorts.Sum(o => o.CanopyGrossPsnActMo * o.CanopyLayerProp);
                return cohorts.Values.Sum(o => o.Sum(x => (x.GrossPsn.Sum() * x.CanopyLayerProp)));
            }

        }
        public float NetPsnSite
        {
            get
            {

                 //return cohorts.Values.Sum(o => o.Sum(x => (x.NetPsn.Sum() * x.CanopyLayerProp)));
                return AllCohorts.Sum(o => o.NetPsnMo * o.CanopyLayerProp); 
            }

        }

        public float WoodDecRespSite
        {
            get
            {

                return AllCohorts.Sum(o => o.WoodDecResp * o.CanopyLayerProp); 
            }

        }

        public float WoodMRespSite
        {
            get
            {

                return AllCohorts.Sum(o => o.WoodMRespMo * o.CanopyLayerProp);
            }

        }


        public float WoodGRespSite
        {
            get
            {

                return AllCohorts.Sum(o => o.WoodGRespMo * o.CanopyLayerProp);
            }

        }

        public float FolGRespSite
        {
            get
            {

                return AllCohorts.Sum(o => o.FolGRespMo * o.CanopyLayerProp);
            }

        }

        public float RootMRespSite
        {
            get
            {

                return AllCohorts.Sum(o => o.RootMRespMo * o.CanopyLayerProp);
            }

        }

        public float RootGRespSite
        {
            get
            {

                return AllCohorts.Sum(o => o.RootGRespMo * o.CanopyLayerProp);
            }

        }

        //public static int ReadNFile { get; set; }
        public void Ndep_input()
        {
            float ndepbl; // n deposition in 1860 for base line
            float ndep2; // n deposition in present year with record for base line
            float nh4p; // nh4 percentage,
            float no3r; // NO3/NH4 ratio,
            float totn, year, year_now;
            float totnmon;

           /* ReadNFile = 1;   // Zhou need to move these code to the climate functions

            if (1 == ReadNFile)
            {
                // read input N deposition file
                ReadNFile = 0;
            }
            */
            float ndepfactor = 1.0f;  //ZZX_scenario
            //ndepfactor = -0.25f;

            ndepbl = 0.15f; // HF gNm-2
            nh4p = 0.36f; // HF
                          //  no3r = (1 - nh4p) / nh4p; // HF
            ndep2 = 0.718f * ndepfactor;  //   //ZZX_scenario
            //year_now = 1964.0f;
            year_now = 1981.0f;
            year = (float)Ecoregion.Variables.Year;

            totn = (ndep2 - ndepbl) / (year_now - 1860f) * (year - 1860f) + ndepbl;
            if (year < 1860f) totn = ndepbl;
            if (year > year_now) totn = ndep2;
            totnmon = totn / 12.0f;
            NH4dep = totnmon * nh4p;
            NO3dep = totnmon - NH4dep;

        }

        public float NRatio_site()
        {
            float PlantNTot, MaxNStoreTot, NRatioSiteTemp;

            PlantNTot = AllCohorts.Sum(o => ((o.NRatio - 1.0f) / 0.6f * o.MaxNStoreWeighted));  // back calculate plantN need to revisit ZZX FolNConRange
            MaxNStoreTot = AllCohorts.Sum(x => x.MaxNStoreWeighted);
            if (MaxNStoreTot < 0.000001) MaxNStoreTot = 21.0f; // zero cohorts
            NRatioSiteTemp = 1.0f + (PlantNTot / MaxNStoreTot) * 0.6f;
            //if (NRatioSiteTemp < 0.000001) NRatioSiteTemp = 1.3f; // zero cohorts
            return NRatioSiteTemp;
        }

        public float PlantN_Reallocate()
        {
            float PlantNTot, MaxNStoreTot, NRatioSiteTemp;

            PlantNTot = AllCohorts.Sum(o => ((o.NRatio - 1.0f) / 0.6f * o.MaxNStoreWeighted));  // back calculate plantN
            MaxNStoreTot = AllCohorts.Sum(x => x.MaxNStoreWeighted);
            NRatioSiteTemp = 1.0f + (PlantNTot / MaxNStoreTot) * 0.6f;
            return NRatioSiteTemp;
        }

        public void AllocateYrSite()
        {
                
            NRatioSite = NRatio_site();
            NRatioNit = NRatioNitSite();

        }


       
        public float NRatioNitSite()
        {
            float NRatioNitTemp;
            float FolNConRange = 0.6f;   // need to revisit ZZX
            float nr = NRatioSite - 1.0f - (FolNConRange / 3.0f);
            if (nr < 0.0f) nr = 0.0f;

            NRatioNitTemp = (nr / (0.6667f * FolNConRange)) * (nr / (0.6667f * FolNConRange));
            if (NRatioNitTemp > 1) NRatioNitTemp = 1;                   

            return NRatioNitTemp;
        }

         // soil decomposition
        public void Decomp()
        {
            //
            //PnET-CN decomposition routine
            //

            float TMult, KhoAct, SoilPctN, NReten, GrossNImmob;
            float WMult;


            //            NO3dep = 0.385f; // gN/m2
            //            NH4dep = 0.192f; // gN/m2
            ////Add atmospheric N deposition
            ///


            // atmospheric N deposition from the input climate file

            NH4dep = Ecoregion.Variables.NH4;
            NO3dep = Ecoregion.Variables.NO3;

            // atmospheric N deposition from a seperate file
            //Ndep_input(); //ZZX_scenario

            NO3 = NO3 + NO3dep;
            NH4 = NH4 + NH4dep;




            ////Temperature effect on all soil processes


            TMult = (float)Math.Exp(0.1f * (Ecoregion.Variables.Tave - 7.1f)) * 0.68f;
            //WMult = share->MeanSoilMoistEff;
            WMult = 1.0f;

            ////Add litter to Humus pool

            HOM = HOM + LitterMSite;
            HON = HON + LitterNSite;

            ////Humus dynamics
            ///
            float Kho = 0.075f * 1.0f; //ZZX_scenario
            float CFracBiomass = 0.45f;

            KhoAct = Kho / 12.0f;
            DHO = HOM * (float)(1.0 - Math.Exp(-KhoAct * TMult * WMult));
            GrossNMin = DHO * (HON / HOM);
            SoilDecResp = DHO * CFracBiomass;

            GrossNMinYr = GrossNMinYr + GrossNMin;
            HON = HON - GrossNMin;
            HOM = HOM - DHO;

            float NImmobA = 151;
            float NImmobB = -35;

            ////Immobilization and net mineralization
            SoilPctN = (HON / HOM) * 100f;
            NReten = (NImmobA + NImmobB * SoilPctN) / 100f;
            if (NReten < 0) NReten = 0.0f;
            if (NReten >1) NReten = 1.0f;
            GrossNImmob = NReten * GrossNMin;
            HON = HON + GrossNImmob;
            

            //GrossNImmobYr =GrossNImmobYr + GrossNImmob;
            NetNMin = GrossNMin - GrossNImmob;
            NH4 = NH4 + NetNMin;


            int zzx;
            zzx = 1;
            if (Ecoregion.Variables.Year == 2020 && Ecoregion.Variables.Month ==11)
            {
                zzx = 10;
            }

            /*
            for (int i = 0; i < AllCohorts.Count(); i++)
            {
                AllCohorts[i].NitWeight = AllCohorts[i].Biomass / BiomassSite;
            }

            float SoilNit = AllCohorts.Sum(o => (o.NRatioNit * o.NitWeight));
            if (SoilNit < 0) SoilNit = 0;
            if (SoilNit > 1) SoilNit = 1.0f;

            NRatioNit = SoilNit;
            */
            

            NetNitr = (NH4 * NRatioNit);

            // prevent excessive N from leaching out during spinup
           /* if (NH4 > 2.0) HON = HON + NH4 - 2.0f;
            NH4 = 2.0f;
            int year;
            year = 1970;
            year = StartDate.Year - 10;
            if (year > (Ecoregion.Variables.Year)) NetNitr = 0.0f;  // prevent N from leaching out during spinup
           */

            

            NO3 = NO3 + NetNitr;
            NH4 = NH4 - NetNitr;

            NetNMinYr = NetNMinYr + NetNMin;
            NetNitrYr = NetNitrYr + NetNitr;




        }
        public float NDrain
        { get; set; }

        public float FracDrain
        { get; set; }
        public float Drainage
        { get { return hydrology.Leakage; } set {; } }
        public float NDrainMgL
        { get; set; }

        public void NLeach()
        {
            //
            //PnET-CN leaching routine
            //
            NDrain = 0.0f;
            FracDrain = (float)(Drainage / (hydrology.Water + Ecoregion.Variables.Prec));
            if (FracDrain > 1.0f) FracDrain = 1.0f;
            NDrain = FracDrain * NO3;

            //  NDrain = 0.5f * (NO3dep + NH4dep);  // Zhou

            int year;
            year = 1970;
            year = StartDate.Year;// + 10;
            if (year > (Ecoregion.Variables.Year)) NDrain = 0;  // Zhou;  // prevent N from leaching out during spinup


            if (NDrain > NO3) NDrain = NO3;
            //           share->NDrainYr = share->NDrainYr + share->NDrain;
            // 
            NDrainMgL = (NDrain * 1000f) / (Drainage * 10f + 1.0E-6f);  //to convert to mg/l
            NO3 = NO3 - NDrain;

        }

        public void Initialize_CN_Site()
        {
            //
            HOM = 13502.9f*0.5f;
            HON = 385.0f*0.5f;
            NH4 = 0.5f;
            NO3 = 0.1f;
            
            NRatioNit = 0.0f;
            NRatioSite = 1.3f;


        }
        public float NetNMinLastYr
        { get; set; }
        public void Initialize_Yr_Site()
        {
            //
            NetNMinLastYr = NetNMinYr;
            NetNMinYr = 0.0f;

        }
        public float CanopyNCon // decimal of foliar N concentration
        {
            get
            {
                
                float CanopyBiomass, CanopyTotalN,CanopyNtemp;
                CanopyBiomass = AllCohorts.Sum(o => o.CanopyTotMass * o.CanopyLayerProp);
                if (CanopyBiomass > 0.0f)
                {
                    CanopyTotalN = AllCohorts.Sum(o => o.CanopyTotN * o.CanopyLayerProp);
                    CanopyNtemp = CanopyTotalN / CanopyBiomass;
                }
                else CanopyNtemp = 0.0f;
                return CanopyNtemp;

            }
            set {; }
        }
        public float NRatioSite
        {
            get
            { return SiteVars.NRatioSite[Site]; }
            set
            { SiteVars.NRatioSite[Site] = value; }
        }
        /*
    {
        get
        {
            float PlantNTot, MaxNStoreTot;


            PlantNTot = AllCohorts.Sum(o => ((o.NRatio - 1.0f) / 0.6f * o.MaxNStoreWeighted));  // back calculate plantN
            MaxNStoreTot = AllCohorts.Sum(x => x.MaxNStoreWeighted);
            return 1.0f + (PlantNTot / MaxNStoreTot) * 0.6f;
        }
        set {; }
    }
    */
        // // check ecosystem N balance

        public float CanopyNSite  // includes BudN
        {
            get
            {
                return AllCohorts.Sum(o => o.CanopyTotN * o.CanopyLayerProp);
            }
            set {; }
        }

        public float BudNSite
        {
            get
            {
                return AllCohorts.Sum(o => o.BudN * o.CanopyLayerProp);
            }
            set {; }
        }
        public float WoodNSite
        {
            get
            {
                return AllCohorts.Sum(o => o.WoodMassN * o.CanopyLayerProp);
            }
            set {; }
        }

        public float DWoodNSite
        {
            get
            {
                return AllCohorts.Sum(o => o.DeadWoodN * o.CanopyLayerProp);
            }
            set {; }
        }

        public float RootNSite
        {
            get
            {
                return AllCohorts.Sum(o => o.RootMassN * o.CanopyLayerProp);
            }
            set {; }
        }

        public float NdepTot  // input N
        {
            get
            {
                return NO3dep + NH4dep;
            }
            set {; }
        }

        public float NO3dep  // input N
        { get; set; }

        public float NH4dep  // input N
        { get; set; }


        public float NPools_YrStart  // initial N pools
        { get; set; }



        public float NPools_Site  // initial N pools
        {
            get
            {
                return HON + NH4 + NO3 + CanopyNSite + WoodNSite + RootNSite + PlantNSite + DWoodNSite;
            }
            set {; }
        }

        public float NPools_YrEnd  // N pools at the end of year
        { get; set; }

        public float NPools_YrNet  // yearly balance
        {
            get
            {
                return NPools_YrEnd - NPools_YrStart;
            }
            set {; }
        }
        public float NPools_MonNet // Monthly balance
        {
            get
            {
                return NPools_Site - NPools_YrStart - NdepTot + NDrain;
            }
            set {; }
        }

        public void Check_NBalance()
        {
            //
            NPools_YrStart = NPools_Site;  // get current values before N cycling zz
            // to run openspace
            float openspace = OpenSpaceSite;
        }




    }



}