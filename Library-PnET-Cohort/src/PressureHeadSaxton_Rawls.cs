﻿using Landis.Core;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Landis.Library.PnETCohorts
{
    public class PressureHeadSaxton_Rawls  
    {
        public const string SaxtonAndRawlsParameters = "SaxtonAndRawlsParameters";

        public static List<string> ParameterNames
        {
            get
            {
                return typeof(PressureHeadSaxton_Rawls).GetFields().Select(x => x.Name).ToList();
            }
        }
        //---------------------------------------------------------------------
        public static Parameter<string> Sand;
        public static Parameter<string> Clay;
        public static Parameter<string> PctOM;
        public static Parameter<string> DensFactor;
        public static Parameter<string> Gravel;

        static Dictionary<string, float> tensionA = new Dictionary<string, float>();
        static Dictionary<string, float> tensionB = new Dictionary<string, float>();
        static Dictionary<string, float> porosity_OM_comp = new Dictionary<string, float>();
        static Dictionary<string, float> clayProp = new Dictionary<string, float>();
        static Dictionary<string, float> cTheta = new Dictionary<string, float>();
        static Dictionary<string, float> lambda_s = new Dictionary<string, float>();
        static Dictionary<string, float> Fs = new Dictionary<string, float>();

        Landis.Library.Parameters.Ecoregions.AuxParm<float[]> table = new Library.Parameters.Ecoregions.AuxParm<float[]>(Globals.ModelCore.Ecoregions);
        //---------------------------------------------------------------------
        // mm/m of active soil
        public float Porosity(string SoilType)
        {
            return porosity_OM_comp[SoilType];
        }
        //---------------------------------------------------------------------
        public float this[IEcoregion ecoregion, int water]
        {
            get
            {
                try
                {
                    if (water >= table[ecoregion].Length) return 0;
                    return table[ecoregion][water];
                }
                catch (System.Exception e)
                {
                    throw new System.Exception("Cannot get pressure head for water content " + water);
                }
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// </summary>
        /// <param name="watercontent": fraction  ></param>
        /// <param name="soiltype"></param>
        /// <returns></returns>
        public float CalculateWaterPressure(double watercontent, string soiltype)
        { 
            double tension = 0.0;

            if (watercontent <= (porosity_OM_comp[soiltype]))
            {
                tension = tensionA[soiltype] * Math.Pow((watercontent), (-tensionB[soiltype]));
            }

            float pressureHead;
            if (double.IsInfinity(tension))
                pressureHead = float.MaxValue;
            else
            {
                pressureHead = (float)(tension * 0.1019977334);
                if (pressureHead > float.MaxValue)
                    pressureHead = float.MaxValue;
                else
                    pressureHead = (float)Math.Round(pressureHead,2);
            }
            return pressureHead;
        }
        //---------------------------------------------------------------------
        // tension =  pressurehead (kPA) 
        // Calculates volumetric water content (m3H2O/m3 SOIL)
        public float CalculateWaterContent(float tension /* kPA*/, string soiltype)
        {
            // mH2O value =  kPa value x 0.101972
            //float tension = (float) (WaterPressure / 0.1019977334);

            float watercontent = (float) Math.Pow(tension / tensionA[soiltype], 1.0/-tensionB[soiltype]);

            return watercontent; 
        }
        //---------------------------------------------------------------------
        public PressureHeadSaxton_Rawls()
        {
            Landis.Library.Parameters.Ecoregions.AuxParm<string> SoilType = (Landis.Library.Parameters.Ecoregions.AuxParm<string>)Names.GetParameter(Names.SoilType);
            Landis.Library.Parameters.Ecoregions.AuxParm<float> RootingDepth = (Landis.Library.Parameters.Ecoregions.AuxParm<float>)(Parameter<float>)Names.GetParameter(Names.RootingDepth, 0, float.MaxValue);

            table = new Library.Parameters.Ecoregions.AuxParm<float[]>(Globals.ModelCore.Ecoregions);

            Sand = Names.GetParameter("sand");
            Clay = Names.GetParameter("clay");
            PctOM = Names.GetParameter("pctOM");
            DensFactor = Names.GetParameter("densFactor");
            Gravel = Names.GetParameter("gravel");

            foreach (IEcoregion ecoregion in Globals.ModelCore.Ecoregions)
            {
                if (ecoregion.Active)
                {
                    List<float> PressureHead = new List<float>();

                    if (tensionB.ContainsKey(SoilType[ecoregion]) == false)
                    {

                        double sand = double.Parse(Sand[SoilType[ecoregion]]);
                        double clay = double.Parse(Clay[SoilType[ecoregion]]);
                        double pctOM = double.Parse(PctOM[SoilType[ecoregion]]);
                        double densFactor = double.Parse(DensFactor[SoilType[ecoregion]]);
                        double gravel = double.Parse(Gravel[SoilType[ecoregion]]);

                        // Moisture at wilting point 
                        double predMoist1500 = -0.024 * sand + 0.487 * clay + 0.006 * pctOM + 0.005 * sand * pctOM - 0.013 * clay * pctOM + 0.068 * sand * clay + 0.031;
                        double predMoist1500adj = predMoist1500 + 0.14 * predMoist1500 - 0.02;

                        // Moisture at field capacity
                        double predMoist33 = -0.251 * sand + 0.195 * clay + 0.011 * pctOM + 0.006 * sand * pctOM - 0.027 * clay * pctOM + 0.452 * sand * clay + 0.299;
                        double predMoist33Adj = predMoist33 + (1.283 * predMoist33 * predMoist33 - 0.374 * predMoist33 - 0.015);
                        double porosMoist33 = 0.278 * sand + 0.034 * clay + 0.022 * pctOM - 0.018 * sand * pctOM - 0.027 * clay * pctOM - 0.584 * sand * clay + 0.078;
                        double porosMoist33Adj = porosMoist33 + (0.636 * porosMoist33 - 0.107);
                        double satPor33 = porosMoist33Adj + predMoist33Adj;
                        double satSandAdj = -0.097 * sand + 0.043;
                        double sandAdjSat = satPor33 + satSandAdj;
                        double density_OM = (1.0 - sandAdjSat) * 2.65;
                        double density_comp = density_OM * (densFactor);
                        porosity_OM_comp.Add(SoilType[ecoregion], (float)(1.0 - (density_comp / 2.65)));
                        double porosity_change_comp = (1.0 - density_comp / 2.65) - (1.0 - density_OM / 2.65);
                        double moist33_comp = predMoist33Adj + 0.2 * porosity_change_comp;
                        double porosity_moist33_comp = porosity_OM_comp[SoilType[ecoregion]] - moist33_comp;
                        double lambda = (Math.Log(moist33_comp) - Math.Log(predMoist1500adj)) / (Math.Log(1500) - Math.Log(33));
                        double gravel_red_sat_cond = (1.0 - gravel) / (1.0 - gravel * (1.0 - 1.5 * (density_comp / 2.65)));
                        double satcond_mmhr = 1930 * Math.Pow((porosity_moist33_comp), (3.0 - lambda)) * gravel_red_sat_cond;
                        double gravels_vol = ((density_comp / 2.65) * gravel) / (1 - gravel * (1 - density_comp / 2.65));
                        double bulk_density = gravels_vol * 2.65 + (1 - gravels_vol) * density_comp; // g/cm3                      

                        tensionB.Add(SoilType[ecoregion], (float)((Math.Log(1500) - Math.Log(33.0)) / (Math.Log(moist33_comp) - Math.Log(predMoist1500adj))));
                        tensionA.Add(SoilType[ecoregion], (float)Math.Exp(Math.Log(33.0) + (tensionB[SoilType[ecoregion]] * Math.Log(moist33_comp))));

                        // For Permafrost
                        clayProp.Add(SoilType[ecoregion], (float)clay);
                        double cTheta_temp = Constants.cs * (1.0 - porosity_OM_comp[SoilType[ecoregion]]) + Constants.cw * porosity_OM_comp[SoilType[ecoregion]];  //specific heat of soil	kJ/m3/K
                        cTheta.Add(SoilType[ecoregion], (float)cTheta_temp);
                        double lambda_s_temp = (1.0 - clay) * Constants.lambda_0 + clay * Constants.lambda_clay;   //thermal conductivity soil	kJ/m/d/K
                        lambda_s.Add(SoilType[ecoregion], (float)lambda_s_temp);
                        double Fs_temp = ((2.0 / 3.0) / (1.0 + Constants.gs * ((lambda_s_temp / Constants.lambda_w) - 1.0))) + ((1.0 / 3.0) / (1.0 + (1.0 - 2.0 * Constants.gs) * ((lambda_s_temp / Constants.lambda_w) - 1.0)));  //ratio of solid temp gradient
                        Fs.Add(SoilType[ecoregion], (float)Fs_temp);
                    }
                    double watercontent = 0.0;

                    float pressureHead = float.MaxValue;
                    while (pressureHead > 0.01)
                    {
                        pressureHead = CalculateWaterPressure(watercontent, SoilType[ecoregion]);

                        PressureHead.Add(pressureHead);
                        watercontent += 0.01;
                    }
                    table[ecoregion] = PressureHead.ToArray();
                }
            }
        }
        //---------------------------------------------------------------------
        public static float GetClay(string SoilType)
        {
            return clayProp[SoilType];
        }
        //---------------------------------------------------------------------
        public static float GetFs(string SoilType)
        {
            return Fs[SoilType];
        }
        //---------------------------------------------------------------------
        public static float GetLambda_s(string SoilType)
        {
            return lambda_s[SoilType];
        }
        //---------------------------------------------------------------------
        public static float GetCTheta(string SoilType)
        {
            return cTheta[SoilType];
        }
        //---------------------------------------------------------------------
    }
}
