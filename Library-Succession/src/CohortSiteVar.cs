
using Landis.Core;
using Landis.SpatialModeling;

namespace Landis.Library.Succession
{
    /// <summary>
    /// A site variable for a particular type of site cohorts.
    /// </summary>
    /// <typeparam name="TSiteCohortsInterface">The interface for the site cohorts</typeparam>
    public static class CohortSiteVar<TSiteCohortsInterface>
    {
        /// <summary>
        /// A wrapper around a site variable of cohorts so it can accessed as a site
        /// variable of a "simpler" cohort type.  For example, a wrapper around a site
        /// variable of biomass cohorts so the cohorts can be acccessed through their
        /// age-cohort interfaces.
        /// </summary>
        /// <typeparam name="TSiteCohorts">The class for site cohorts</typeparam>
        public class Wrapper<TSiteCohorts>
            : ISiteVar<TSiteCohortsInterface>
            where TSiteCohorts : class, TSiteCohortsInterface
        {
            private ISiteVar<TSiteCohorts> wrappedSiteVar;

            /// <summary>
            /// Construct a wrapper around a site variable of cohorts.
            /// </summary>
            public Wrapper(ISiteVar<TSiteCohorts> siteVar)
            {
                wrappedSiteVar = siteVar;
            }

            #region ISiteVariable members
            System.Type ISiteVariable.DataType
            {
                get
                {
                    return typeof(TSiteCohortsInterface);
                }
            }

            InactiveSiteMode ISiteVariable.Mode
            {
                get
                {
                    return wrappedSiteVar.Mode;
                }
            }

            ILandscape ISiteVariable.Landscape
            {
                get
                {
                    return wrappedSiteVar.Landscape;
                }
            }
            #endregion

            #region ISiteVar<TSiteCohortsInterface> members
            // Extensions other than succession have no need to assign the whole
            // site-cohorts object at any site.

            TSiteCohortsInterface ISiteVar<TSiteCohortsInterface>.this[Site site]
            {
                get
                {
                    return wrappedSiteVar[site];
                }
                set
                {
                    throw new System.InvalidOperationException("Operation restricted to succession extension");
                }
            }

            TSiteCohortsInterface ISiteVar<TSiteCohortsInterface>.ActiveSiteValues
            {
                set
                {
                    throw new System.InvalidOperationException("Operation restricted to succession extension");
                }
            }

            TSiteCohortsInterface ISiteVar<TSiteCohortsInterface>.InactiveSiteValues
            {
                set
                {
                    throw new System.InvalidOperationException("Operation restricted to succession extension");
                }
            }

            TSiteCohortsInterface ISiteVar<TSiteCohortsInterface>.SiteValues
            {
                set
                {
                    throw new System.InvalidOperationException("Operation restricted to succession extension");
                }
            }
            #endregion
        }

        /// <summary>
        /// Wraps a site variable of site cohorts into a new site variable of an
        /// interface that the cohorts implement.
        /// </summary>
        /// <typeparam name="TSiteCohorts">The class of the variable's site cohorts</typeparam>
        /// <example>
        /// Example of how the SiteVars class in Biomass Succession would define its Cohorts
        /// property, and then register a couple of site-variable wrappers.
        /// <code>
        /// public static class SiteVars
        /// {
        ///     // Notice the type argument below is the SiteCohorts, not its interface used by
        ///     // non-succession extensions.  All the code within the succession extension will
        ///     // need to access all the methods of SiteCohorts (e.g., growth-related methods).
        ///     public static ISiteVar&lt;BiomassCohorts.SiteCohorts> Cohorts { get; private set; }
        ///
        ///     public static void Initialize()
        ///     {
        ///         // See https://code.google.com/p/landis-extensions/wiki/ModelClass
        ///         Cohorts = Model.Core.Landscape.NewSiteVar&lt;BiomassCohorts.SiteCohorts>();
        ///
        ///         // Create a site variable for biomass (non-succession) extensions to access the cohorts.
        ///         ISiteVar&lt;BiomassCohorts.ISiteCohorts> biomassCohortSiteVar = CohortSiteVar&lt;BiomassCohorts.ISiteCohorts>.Wrap(Cohorts);
        ///         Model.Core.RegisterSiteVar(biomassCohortSiteVar, "Succession.BiomassCohorts");
        ///
        ///         // Create a site variable for base extensions to access the cohorts as age cohorts.
        ///         ISiteVar&lt;AgeOnlyCohorts.ISiteCohorts> ageCohortSiteVar = CohortSiteVar&lt;AgeOnlyCohorts.ISiteCohorts>.Wrap(Cohorts);
        ///         Model.Core.RegisterSiteVar(ageCohortSiteVar, "Succession.AgeOnlyCohorts");
        ///     }
        /// }
        /// </code>
        /// </example>
        public static ISiteVar<TSiteCohortsInterface> Wrap<TSiteCohorts>(ISiteVar<TSiteCohorts> siteVar)
            where TSiteCohorts : class, TSiteCohortsInterface
        {
            return new Wrapper<TSiteCohorts>(siteVar);
        }
    }
}
