using Landis.Core;
using System;
using System.Collections.Generic;

namespace Landis.Library.PnETCohorts
{
    public class EstablishmentProbability  : IEstablishmentProbability
    {
        private LocalOutput establishment_siteoutput;
        private List<ISpeciesPnET> _hasEstablished;
        private Dictionary<ISpeciesPnET, float> _pest;
        private Dictionary<ISpeciesPnET, float> _fwater;
        private Dictionary<ISpeciesPnET, float> _frad;

        //private static int Timestep;

       
        public bool HasEstablished(ISpeciesPnET species)
        {
            return _hasEstablished.Contains(species);
        }
       
        public Landis.Library.Parameters.Species.AuxParm<float> Probability
        {
            get
            {
                Landis.Library.Parameters.Species.AuxParm<float> probability = new Library.Parameters.Species.AuxParm<float>(Globals.ModelCore.Species);
                foreach (ISpecies spc in Globals.ModelCore.Species)
                {
                    ISpeciesPnET speciespnet = SpeciesParameters.SpeciesPnET[spc];
                    probability[spc] = _pest[speciespnet];
                }
                return probability; //0.0-1.0 index
            }
        }
        public float Get_FWater(ISpeciesPnET species)
        {
            {
                return _fwater[species];
            }
        }
        public float Get_FRad(ISpeciesPnET species)
        {
            {
                return _frad[species];
            }
        }

        public string Header
        {
            get
            {
                return "Year" + "," + "Species" + "," + "Pest" + "," + "FWater_Avg" +"," + "FRad_Avg" +","+"ActiveMonths"+"," + "Est";
            }
        }
        
       
        /*public static void Initialize(int timestep)
        {
            Timestep = timestep;

             
        }*/
     
        public Dictionary<ISpeciesPnET,float> Calculate_Establishment_Month(IEcoregionPnETVariables pnetvars, IEcoregionPnET ecoregion, float PAR, IHydrology hydrology,float minHalfSat, float maxHalfSat, bool invertPest, float propRootAboveFrost)
        {
            Dictionary<ISpeciesPnET, float> estabDict = new Dictionary<ISpeciesPnET, float>();

            float halfSatRange = maxHalfSat - minHalfSat;

            foreach (ISpeciesPnET spc in SpeciesParameters.SpeciesPnET.AllSpecies)
            {
                if (pnetvars.Tmin > spc.PsnTMin && pnetvars.Tmax < spc.PsnTMax && propRootAboveFrost > 0)
                {
                    // Adjust HalfSat for CO2 effect
                    float halfSatIntercept = spc.HalfSat - 350 * spc.CO2HalfSatEff;
                    float adjHalfSat = spc.CO2HalfSatEff * pnetvars.CO2 + halfSatIntercept;
                    float frad = (float)(Math.Min(1.0,(Math.Pow(Cohort.ComputeFrad(PAR, adjHalfSat),2) * (1/(Math.Pow(spc.EstRad,2))))));
                    float adjFrad = frad;
                    // Optional adjustment to invert Pest based on relative halfSat
                    if (invertPest && halfSatRange > 0)
                    {
                        float frad_adj_int = (spc.HalfSat - minHalfSat) / halfSatRange;
                        float frad_slope = (frad_adj_int * 2) - 1;
                        adjFrad = 1 - frad_adj_int + frad * frad_slope;
                    }
                    
                    float PressureHead = hydrology.PressureHeadTable.CalculateWaterContent(hydrology.Water, ecoregion.SoilType);

                    float fwater = (float)(Math.Min(1.0,(Math.Pow(Cohort.ComputeFWater(spc.H1,spc.H2, spc.H3, spc.H4, PressureHead), 2) * (1/(Math.Pow(spc.EstMoist,2))))));

                    float pest = (float) Math.Min(1.0,adjFrad * fwater);
                    estabDict[spc] = pest;
                    _fwater[spc] = fwater;
                    _frad[spc] = adjFrad;
                }
                
            }
            return estabDict;
        }
        public void ResetPerTimeStep()
        {
         
            _pest = new Dictionary<ISpeciesPnET, float>();
            _fwater = new Dictionary<ISpeciesPnET, float>();
            _frad = new Dictionary<ISpeciesPnET, float>();
            _hasEstablished = new List<ISpeciesPnET>();

            foreach (ISpeciesPnET spc in SpeciesParameters.SpeciesPnET.AllSpecies)
            {
                _pest.Add(spc, 0);
                _fwater.Add(spc, 0);
                _frad.Add(spc, 0);
            }
        }
        public EstablishmentProbability(string SiteOutputName, string FileName)
        {
            ResetPerTimeStep();
             
            if(SiteOutputName!=null && FileName!=null)
            {
                establishment_siteoutput = new LocalOutput(SiteOutputName, "Establishment.csv", Header );
            }
            
        }

        public void EstablishmentTrue(ISpeciesPnET spc)
        {
            _hasEstablished.Add(spc);
        }
        
        public void RecordPest(int year, ISpeciesPnET spc, float annualPest, float annualfWater, float annualfRad, bool estab, int monthCount)
        {
            if (estab)
            {
                if (HasEstablished(spc) == false)
                {
                    _hasEstablished.Add(spc);
                }
            }
            if (establishment_siteoutput != null)
            {
                if (monthCount == 0)
                {
                    establishment_siteoutput.Add(year.ToString() + "," + spc.Name + "," + annualPest + "," + 0 + "," + 0 + ","+0+"," + HasEstablished(spc));
                }
                else
                {
                    establishment_siteoutput.Add(year.ToString() + "," + spc.Name + "," + annualPest + "," + annualfWater + "," + annualfRad + ","+monthCount+"," + HasEstablished(spc));
                }
                // TODO: win time by reducing calls to write
                establishment_siteoutput.Write();
            }
            // Record annualPest to be accessed as Probability
            _pest[spc] = annualPest;
        }
    }
}
