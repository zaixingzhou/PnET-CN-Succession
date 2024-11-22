using System;
using System.Collections.Generic;
using System.Text;

using Landis.Core;
using Landis.Library.AgeOnlyCohorts;
using Landis.Library.Biomass;
using Landis.SpatialModeling;

namespace Landis.Library.PnETCohorts
{

    public static class SiteVars
    {
        public static ISiteVar<Pool> WoodyDebris;
        public static ISiteVar<Pool> Litter;
        public static ISiteVar<Double> FineFuels;
        public static ISiteVar<float> PressureHead;
        public static ISiteVar<float> ExtremeMinTemp;
        public static ISiteVar<double> AnnualPE;  //Annual Potential Evaporation
        public static ISiteVar<double> ClimaticWaterDeficit;
        public static ISiteVar<double> SmolderConsumption;
        public static ISiteVar<double> FlamingConsumption;
        public static ISiteVar<Landis.Library.PnETCohorts.SiteCohorts> SiteCohorts;
        
        // soil som
        public static ISiteVar<float> HOMSite;
        public static ISiteVar<float> HONSite;
        public static ISiteVar<float> NO3Site;
        public static ISiteVar<float> NH4Site;

        public static ISiteVar<float> NRatioSite;
        public static ISiteVar<float> NRatioNit;

        public static void Initialize()
        {
            WoodyDebris = Globals.ModelCore.Landscape.NewSiteVar<Pool>();
            Litter = Globals.ModelCore.Landscape.NewSiteVar<Pool>();
            FineFuels = Globals.ModelCore.Landscape.NewSiteVar<Double>();
            SiteCohorts = Globals.ModelCore.Landscape.NewSiteVar<SiteCohorts>();
            PressureHead = Globals.ModelCore.Landscape.NewSiteVar<float>();
            ExtremeMinTemp = Globals.ModelCore.Landscape.NewSiteVar<float>();
            AnnualPE = Globals.ModelCore.Landscape.NewSiteVar<Double>();
            ClimaticWaterDeficit = Globals.ModelCore.Landscape.NewSiteVar<Double>();
            SmolderConsumption = Globals.ModelCore.Landscape.NewSiteVar<Double>();
            FlamingConsumption = Globals.ModelCore.Landscape.NewSiteVar<Double>();

            HOMSite = Globals.ModelCore.Landscape.NewSiteVar<float>();
            HONSite = Globals.ModelCore.Landscape.NewSiteVar<float>();
            NO3Site = Globals.ModelCore.Landscape.NewSiteVar<float>();
            NH4Site = Globals.ModelCore.Landscape.NewSiteVar<float>();

            NRatioSite = Globals.ModelCore.Landscape.NewSiteVar<float>();
            NRatioNit = Globals.ModelCore.Landscape.NewSiteVar<float>();

            Globals.ModelCore.RegisterSiteVar(WoodyDebris, "Succession.WoodyDebris");
            Globals.ModelCore.RegisterSiteVar(Litter, "Succession.Litter");
            Globals.ModelCore.RegisterSiteVar(FineFuels, "Succession.FineFuels");
            Globals.ModelCore.RegisterSiteVar(PressureHead, "Succession.PressureHead");
            Globals.ModelCore.RegisterSiteVar(ExtremeMinTemp, "Succession.ExtremeMinTemp");
            Globals.ModelCore.RegisterSiteVar(AnnualPE, "Succession.PET"); //FIXME
            Globals.ModelCore.RegisterSiteVar(ClimaticWaterDeficit, "Succession.CWD");
            Globals.ModelCore.RegisterSiteVar(SmolderConsumption, "Succession.SmolderConsumption");
            Globals.ModelCore.RegisterSiteVar(FlamingConsumption, "Succession.FlamingConsumption");
           
            Globals.ModelCore.RegisterSiteVar(HOMSite, "Succession.HOMSite");
            Globals.ModelCore.RegisterSiteVar(HONSite, "Succession.HONSite");
            Globals.ModelCore.RegisterSiteVar(NO3Site, "Succession.NO3Site");
            Globals.ModelCore.RegisterSiteVar(NH4Site, "Succession.NH4Site");

            Globals.ModelCore.RegisterSiteVar(NRatioSite, "Succession.NRatioSite");
            Globals.ModelCore.RegisterSiteVar(NRatioNit, "Succession.NRatioNit");

        }

    }
}
