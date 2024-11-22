//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.Core;
using System.Collections.Generic;

namespace Landis.Library.PnETCohorts
{
    /// The biomass cohorts for a particular species at a site.
    /// </summary>
    public interface ISpeciesCohorts
         : Landis.Library.Cohorts.ISpeciesCohorts<ICohort>
    //Landis.Library.Cohorts.ISpeciesCohorts<Landis.Library.PnETCohorts.ICohort>, Landis.Library.BiomassCohorts.ISpeciesCohorts, Landis.Library.Cohorts.ISpeciesCohorts<Landis.Library.BiomassCohorts.ICohort>    
    {

    }
    
}
