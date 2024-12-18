using Landis.Core;
using Landis.SpatialModeling;

namespace Landis.Library.Succession
{
    /// <summary>
    /// Default implementations of some of the reproduction delegates.
    /// </summary>
    public static class ReproductionDefaults
    {
        /// <summary>
        /// The default method for determining if there is sufficient light at
        /// a site for a species to germinate/resprout.
        /// </summary>
        public static bool SufficientResources(ISpecies   species,
                                               ActiveSite site)
        {
            byte siteShade = SiteVars.Shade[site];
            bool sufficientLight;
            sufficientLight = (species.ShadeTolerance <= 4 && species.ShadeTolerance > siteShade) ||
                   (species.ShadeTolerance == 5 && siteShade > 1);
            //  pg 14, Model description, this ----------------^ may be 2?
            return sufficientLight;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Determines if a species can establish on a site.
        /// </summary>
        public static bool Establish(ISpecies species, ActiveSite site)
        {
            double establishProbability = 0;

            return Model.Core.GenerateUniform() < establishProbability;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Determines if a species can establish on a site.
        /// </summary>
        public static bool PlantingEstablish(ISpecies species, ActiveSite site)
        {
            double establishProbability = 1.0;

            return Model.Core.GenerateUniform() < establishProbability;
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Determines if a species can establish on a site.
        /// </summary>
        public static double EstablishmentProbability(ISpecies species, ActiveSite site)
        {
            return 1.0;

        }

        //---------------------------------------------------------------------
        /// <summary>
        /// The mature biomass on a site.
        /// </summary>
        public static double MatureBiomass(ISpecies species, ActiveSite site)
        {
            double matureBiomass = 0;

            return matureBiomass;
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// The active biomass on a site.
        /// </summary>
        public static double ActiveBiomass(ISpecies species, ActiveSite site)
        {
            double activeBiomass = 0;

            return activeBiomass;
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// The mature foliage mass on a site.
        /// </summary>
        public static double MatureFolMass(ISpecies species, ActiveSite site)
        {
            double folMass = 0;

            return folMass;
        }

        //---------------------------------------------------------------------
    }
}
