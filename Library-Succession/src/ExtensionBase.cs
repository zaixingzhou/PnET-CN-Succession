using Landis.Utilities;
using Landis.Core;
using Landis.SpatialModeling;

using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Landis.Library.Succession
{
    /// <summary>
    /// Base component upon which a succession plug-in can built.
    /// </summary>
    public abstract class ExtensionBase
        : SuccessionMain
    {
        private DisturbedSiteEnumerator disturbedSites;
        public bool ShowProgress;
        private uint? prevSiteDataIndex;
        private static readonly object threadLock = new object();

        //---------------------------------------------------------------------

        private static ILog logger;

        //---------------------------------------------------------------------

        static ExtensionBase()
        {
            logger = LogManager.GetLogger(typeof(ExtensionBase));
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ExtensionBase(string name)
            : base(name)
        {
            this.ThreadCount = 1;
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
        /// Number of threads that an extension has been optimized to use to split up work.
        /// This number will be set by the inheriting succession extension if it has been
        /// optimized
        /// </summary>
        protected int ThreadCount
        {
            get;
            set;
        }

        //---------------------------------------------------------------------


        /// <summary>
        /// Initializes the instance and its associated site variables.
        /// </summary>
        protected void Initialize(ICore modelCore,
                                  SeedingAlgorithms seedAlg)
        {
            Model.Core = modelCore;
            SiteVars.Initialize();
            Seeding.InitializeMaxSeedNeighborhood();

            disturbedSites = new DisturbedSiteEnumerator(Model.Core.Landscape,
                                                         SiteVars.Disturbed);

            SeedingAlgorithm algorithm = SeedingAlgorithmsUtil.GetAlgorithm(seedAlg,
                                                                            Timestep);
            Reproduction.Initialize(algorithm);
        }

        //---------------------------------------------------------------------

        //public override void InitializeSites(string initialCommunities,
        //                                     string initialCommunitiesMap,
        //                                     ICore modelCore)
        //{
        //    
        //}

        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes an active site's cohorts using a specific initial
        /// community.
        /// </summary>
        protected abstract void InitializeSite(ActiveSite site);

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

        public void RunReproductionFirst()
        {
            bool isSuccessionTimestep = (Model.Core.CurrentTime % Timestep == 0);
            IEnumerable<ActiveSite> sites;
            if (isSuccessionTimestep)
                sites = Model.Core.Landscape.ActiveSites;
            else
                sites = disturbedSites;

            ComputeShade(sites);
            ReproduceCohorts(sites);
            AgeCohorts(sites, isSuccessionTimestep);

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
            Stopwatch watch = new Stopwatch();
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
                progressBar = Model.Core.UI.CreateProgressMeter(Model.Core.Landscape.ActiveSiteCount); // NewProgressBar();
            }
            watch.Start();

            if (this.ThreadCount != 1)
            {
                var sitesArray = sites.ToArray(); 

                // Parallelize the calculations involved in ageing/growing cohorts to decrease process time
                Parallel.For(0, sitesArray.Count(), new ParallelOptions
                {
                    MaxDegreeOfParallelism = this.ThreadCount
                },
                    i =>
                    {
                        ushort deltaTime = (ushort)(Model.Core.CurrentTime - SiteVars.TimeOfLast[sitesArray[i]]);
                        AgeCohorts(sitesArray[i], deltaTime, succTimestep);
                        SiteVars.TimeOfLast[sitesArray[i]] = Model.Core.CurrentTime;

                        lock (threadLock)
                        {
                            if (ShowProgress)
                                Update(progressBar, sitesArray[i].DataIndex, true);
                        }
                    });
                
            }
            else
            {
                foreach (ActiveSite site in sites)
                {
                    ushort deltaTime = (ushort)(Model.Core.CurrentTime - SiteVars.TimeOfLast[site]);
                    AgeCohorts(site, deltaTime, succTimestep);
                    SiteVars.TimeOfLast[site] = Model.Core.CurrentTime;

                    if (ShowProgress)
                        Update(progressBar, site.DataIndex);
                }
            }

            watch.Stop();
            //this.totalTime += watch.Elapsed.TotalSeconds;
            //this.runs += 1;
            watch.Reset();

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
                progressBar = Model.Core.UI.CreateProgressMeter(Model.Core.Landscape.ActiveSiteCount); //NewProgressBar();
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
                progressBar = Model.Core.UI.CreateProgressMeter(Model.Core.Landscape.ActiveSiteCount); // NewProgressBar();
            }

            if (this.ThreadCount != 1)
            {
                var sitesArray = sites.ToArray();

                // Parallelize the calculations involved in reproduction to decrease process time
                Parallel.For(0, sitesArray.Count(), new ParallelOptions
                {
                    MaxDegreeOfParallelism = this.ThreadCount
                },
                    i =>
                    {
                        ThreadSafeRandom randomGen = new ThreadSafeRandom();
                        ushort deltaTime = (ushort)(Model.Core.CurrentTime - SiteVars.TimeOfLast[sitesArray[i]]);
                        Reproduction.Reproduce(sitesArray[i], randomGen);
                        SiteVars.TimeOfLast[sitesArray[i]] = Model.Core.CurrentTime;

                        lock (threadLock)
                        {
                            if (ShowProgress)
                                Update(progressBar, sitesArray[i].DataIndex, true);
                        }
                    });

            }
            else
            {
                foreach (ActiveSite site in sites)
                {
                    Reproduction.Reproduce(site);

                    if (ShowProgress)
                        Update(progressBar, site.DataIndex);
                }
                if (ShowProgress)
                    CleanUp(progressBar);
            }

            logger.Debug(string.Format("{0:G}", DateTime.Now));
            for (int generation = 0; generation <= maxGeneration; generation++)
                logger.Debug(string.Format("  gen {0}: {1}", generation,
                                          GC.CollectionCount(generation)));
        }

        //---------------------------------------------------------------------

        private void Update(ProgressBar progressBar,
                            uint currentSiteDataIndex,
                            bool parallel = false)
        {
            uint increment = 0;
            if (!parallel)
            {
                increment = (uint)(prevSiteDataIndex.HasValue
                                        ? (currentSiteDataIndex - prevSiteDataIndex.Value)
                                        : currentSiteDataIndex);
            }
            else
            {
                increment = 1;
            }
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
