using Edu.Wisc.Forest.Flel.Util;
//using  Landis.Library.BaseCohorts;
using Landis.Library.InitialCommunities;
using Landis.Species;
//using Landis.Util;
using log4net;
using System;
using System.Collections.Generic;
using Wisc.Flel.GeospatialModeling.Landscapes;
using Wisc.Flel.GeospatialModeling.RasterIO;

namespace Landis.Library.Succession
{
    /// <summary>
    /// Base component upon which a succession plug-in can built.
    /// </summary>
    public abstract class PlugIn
        : PlugIns.SuccessionPlugIn
    {
        private DisturbedSiteEnumerator disturbedSites;
        public bool ShowProgress;
        private uint? prevSiteDataIndex;

        //---------------------------------------------------------------------

        private static ILog logger;

        //---------------------------------------------------------------------

        static PlugIn()
        {
            logger = LogManager.GetLogger(typeof(PlugIn));
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public PlugIn(string name)
            : base(name)
        {
            this.ShowProgress = true;
        }


        //---------------------------------------------------------------------

        /// <summary>
        /// Site variable that indicates if a site has been disturbed.
        /// </summary>
        protected ISiteVar<bool> Disturbed
        {
            get
            {
                return SiteVars.Disturbed;
            }
        }

        //---------------------------------------------------------------------


        /// <summary>
        /// Initializes the instance and its associated site variables.
        /// </summary>
        protected void Initialize(PlugIns.ICore modelCore,
                                  double[,] establishProbabilities,
                                  SeedingAlgorithms seedAlg,
                                  Reproduction.Delegates.AddNewCohort addNewCohort)
        {
            Model.Core = modelCore;
            SiteVars.Initialize();
            Seeding.InitializeMaxSeedNeighborhood();

            disturbedSites = new DisturbedSiteEnumerator(Model.Core.Landscape,
                                                         SiteVars.Disturbed);

            SeedingAlgorithm algorithm;
            switch (seedAlg)
            {
                case SeedingAlgorithms.NoDispersal:
                    algorithm = NoDispersal.Algorithm;
                    break;

                case SeedingAlgorithms.UniversalDispersal:
                    algorithm = UniversalDispersal.Algorithm;
                    break;

                case SeedingAlgorithms.WardSeedDispersal:
                    algorithm = WardSeedDispersal.Algorithm;
                    break;

                default:
                    throw new ArgumentException(string.Format("Unknown seeding algorithm: {0}", seedAlg));
            }

            Reproduction.Initialize(establishProbabilities, algorithm,
                                    addNewCohort == null ? null : new Reproduction.Delegates.AddNewCohort(addNewCohort));
        }

        //---------------------------------------------------------------------

        public override void InitializeSites(string initialCommunities,
                                             string initialCommunitiesMap,
                                             PlugIns.ICore modelCore)
        {
            Landis.Model.iui.WriteLine("Loading initial communities from file \"{0}\" ...", initialCommunities);
            InitialCommunities.DatasetParser parser = new InitialCommunities.DatasetParser(Timestep, Model.Core.Species);
            InitialCommunities.IDataset communities = modelCore.Load<InitialCommunities.IDataset>(initialCommunities, parser);

            Landis.Model.iui.WriteLine("Reading initial communities map \"{0}\" ...", initialCommunitiesMap);
            IInputRaster<InitialCommunities.Pixel> map;
            map = Model.Core.OpenRaster<InitialCommunities.Pixel>(initialCommunitiesMap);
            using (map)
            {
                foreach (Site site in Model.Core.Landscape.AllSites)
                {
                    InitialCommunities.Pixel pixel = map.ReadPixel();
                    if (!site.IsActive)
                        continue;
                    ActiveSite activeSite = (ActiveSite)site;
                    ushort mapCode = pixel.Band0;
                    ICommunity community = communities.Find(mapCode);
                    if (community == null)
                    {
                        throw new PixelException(activeSite.Location,
                                                 "Unknown map code for initial community: {0}",
                                                 mapCode);
                    }
                    InitializeSite(activeSite, community);
                }
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes an active site's cohorts using a specific initial
        /// community.
        /// </summary>
        protected abstract void InitializeSite(ActiveSite site,
                                               ICommunity initialCommunity);

        //---------------------------------------------------------------------

        public override void Run()
        {
            bool isSuccessionTimestep = (Model.Core.CurrentTime % Timestep == 0);
            IEnumerable<ActiveSite> sites;
            if (isSuccessionTimestep)
                sites = Model.Core.Landscape.ActiveSites;
            else
                sites = disturbedSites;

            AgeCohorts(sites, isSuccessionTimestep);
            ComputeShade(sites);
            ReproduceCohorts(sites);

            if (!isSuccessionTimestep)
                SiteVars.Disturbed.ActiveSiteValues = false;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Advances the age of all the cohorts at certain specified sites.
        /// </summary>
        /// <param name="sites">
        /// The sites whose cohorts are to be aged.
        /// </param>
        public void AgeCohorts(IEnumerable<ActiveSite> sites,
                               bool isSuccessionTimestep)
        {
            int? succTimestep = null;
            if (isSuccessionTimestep)
            {
                succTimestep = Timestep;
                ShowProgress = true;
            }
            else
            {
                ShowProgress = false;
            }

            ProgressBar progressBar = null;
            if (ShowProgress)
            {
                System.Console.WriteLine("growing cohorts ...");
                prevSiteDataIndex = null;
                progressBar = Landis.Model.iui.CreateProgressMeter(Model.Core.Landscape.ActiveSiteCount); // NewProgressBar();
            }

            foreach (ActiveSite site in sites)
            {
                ushort deltaTime = (ushort)(Model.Core.CurrentTime - SiteVars.TimeOfLast[site]);
                AgeCohorts(site, deltaTime, succTimestep);
                SiteVars.TimeOfLast[site] = Model.Core.CurrentTime;

                if (ShowProgress)
                    Update(progressBar, site.DataIndex);
            }
            if (ShowProgress)
                CleanUp(progressBar);
        }


        //---------------------------------------------------------------------

        /// <summary>
        /// Advances the age of all the cohorts at a site.
        /// </summary>
        /// <param name="site">
        /// The site whose cohorts are to be aged.
        /// </param>
        /// <param name="years">
        /// The number of years to advance the cohorts' ages by.
        /// </param>
        /// <param name="successionTimestep">
        /// The succession timestep (years).  If this parameter has a value,
        /// then the ageing is part of a succession timestep; therefore, all
        /// young cohorts (whose age is less than succession timestep) are
        /// combined into a single cohort whose age is the succession timestep.
        /// </param>
        protected abstract void AgeCohorts(ActiveSite site,
                                           ushort years,
                                           int? successionTimestep);

        //---------------------------------------------------------------------

        /// <summary>
        /// Computes the shade at certain specified sites.
        /// </summary>
        /// <param name="sites">
        /// The sites where shade is to be computed.
        /// </param>
        public void ComputeShade(IEnumerable<ActiveSite> sites)
        {
            ProgressBar progressBar = null;
            if (ShowProgress)
            {
                System.Console.WriteLine("Computing shade ...");
                prevSiteDataIndex = null;
                progressBar = Landis.Model.iui.CreateProgressMeter(Model.Core.Landscape.ActiveSiteCount); //NewProgressBar();
            }

            foreach (ActiveSite site in sites)
            {
                SiteVars.Shade[site] = ComputeShade(site);

                if (ShowProgress)
                    Update(progressBar, site.DataIndex);
            }
            if (ShowProgress)
                CleanUp(progressBar);
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Computes the shade at a site.
        /// </summary>
        public abstract byte ComputeShade(ActiveSite site);

        //---------------------------------------------------------------------

        /// <summary>
        /// Does cohort reproduction at certain specified sites.
        /// </summary>
        /// <param name="sites">
        /// The sites where cohort reproduction should be done.
        /// </param>
        /// <remarks>
        /// Because this is the last stage of succession during a timestep,
        /// the NextTimeToRun is updated after all the sites are processed.
        /// </remarks>
        public void ReproduceCohorts(IEnumerable<ActiveSite> sites)
        {
            logger.Debug(string.Format("{0:G}", DateTime.Now));
            int maxGeneration = GC.MaxGeneration;
            for (int generation = 0; generation <= maxGeneration; generation++)
                logger.Debug(string.Format("  gen {0}: {1}", generation,
                                           GC.CollectionCount(generation)));

            ProgressBar progressBar = null;
            if (ShowProgress)
            {
                System.Console.WriteLine("Cohort reproduction ...");
                prevSiteDataIndex = null;
                progressBar = Landis.Model.iui.CreateProgressMeter(Model.Core.Landscape.ActiveSiteCount); // NewProgressBar();
            }

            foreach (ActiveSite site in sites)
            {
                Reproduction.Reproduce(site);

                /*#if WATCH_CHANGING_GC
                                bool collectionCountChanged = false;
                                for (int generation = 0; generation <= maxGeneration; generation++) {
                                    int currentCount = GC.CollectionCount(generation);
                                    if (collectionCounts[generation] != currentCount) {
                                        collectionCountChanged = true;
                                        collectionCounts[generation] = currentCount;
                                    }
                                }
                                if (collectionCountChanged) {
                                    logger.Info(string.Format("Site {0}, index = {1}", site.Location, site.DataIndex));
                                    for (int generation = 0; generation <= maxGeneration; generation++)
                                        logger.Info(string.Format("  gen {0}: {1}", generation, collectionCounts[generation]));
                                }
                #endif*/
                if (ShowProgress)
                    Update(progressBar, site.DataIndex);
            }
            if (ShowProgress)
                CleanUp(progressBar);

            logger.Debug(string.Format("{0:G}", DateTime.Now));
            for (int generation = 0; generation <= maxGeneration; generation++)
                logger.Debug(string.Format("  gen {0}: {1}", generation,
                                          GC.CollectionCount(generation)));
        }
        // }
        // }

        //---------------------------------------------------------------------

        private void Update(ProgressBar progressBar,
                            uint currentSiteDataIndex)
        {
            uint increment = (uint)(prevSiteDataIndex.HasValue
                                        ? (currentSiteDataIndex - prevSiteDataIndex.Value)
                                        : currentSiteDataIndex);
            progressBar.IncrementWorkDone(increment);
            prevSiteDataIndex = currentSiteDataIndex;
        }


        //---------------------------------------------------------------------

        private void CleanUp(ProgressBar progressBar)
        {
            if (!prevSiteDataIndex.HasValue)
            {
                //    Then no sites were processed; the site iterator was a
                //    disturbed-sites iterator, and there were no disturbed
                //    sites.  So increment the progress bar to 100%.
                progressBar.IncrementWorkDone((uint)Model.Core.Landscape.ActiveSiteCount);
            }
        }

    }

}
