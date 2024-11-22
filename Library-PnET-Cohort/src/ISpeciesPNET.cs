 
namespace Landis.Library.PnETCohorts 
{
    /// <summary>
    /// The information for a tree species (its index and parameters).
    /// </summary>
    public interface ISpeciesPnET : Landis.Core.ISpecies
    {
        // Carbon fraction in biomass 
        float CFracBiomass { get; }

        // Fraction of non-soluble carbon to active biomass
        float DNSC { get; }

        // Fraction biomass below-ground
        float FracBelowG { get; }

        // Fraction foliage to active biomass
        float FracFol { get; }

        // Fraction active biomass to total biomass 
        float FrActWd { get; }

        // Water stress parameter for excess water: pressurehead below which growth halts
        float H1 { get; }

        // Water stress parameter for excess water: pressurehead below which growth declines
        float H2 { get; }

        // Water stress parameter for water shortage: pressurehead above which growth declines
        float H3 { get; }

        // Water stress parameter: pressurehead above growth halts (= wilting point)
        float H4 { get; }

        // Initial NSC for new cohort
        float InitialNSC { get; }

        // Half saturation value for radiation (W/m2)
        float HalfSat { get; }

        // Radiation extinction rate through the canopy (LAI-1)
        float K { get; }

        // Decompostiaion constant of woody litter (yr-1)
        float KWdLit { get; }

        //Growth reduction parameter with age
        float PsnAgeRed { get; }

        // Reduction of specific leaf weight throught the canopy (g/m2/g)
        float SLWDel { get; }

        // Max specific leaf weight (g/m2)
        float SLWmax { get; }

        // Foliage turnover (g/g/y)
        float TOfol { get; }

        // Root turnover (g/g/y)
        float TOroot { get; }

        // Wood turnover (g/g/y)
        float TOwood { get; }

        // Establishment factor related to light - fRad value that equates to optimal light for establishment
        float EstRad { get; }

        // Establishment factor related to moisture - fWater value that equates to optimal water for establishment
        float EstMoist { get; }

        // Mamximum total probability of establishment under optimal conditions
        float MaxPest { get; }

        // Lignin concentration in foliage
        float FolLignin { get; }

        // Prevent establishment 
        bool PreventEstablishment { get; }

        // Optimal temperature for photosynthesis
        float PsnTOpt { get; }

        // Temperature response factor for respiration
        float Q10 { get; }

        // Base foliar respiration (g respired / g photosynthesis)
        float BFolResp { get; }

        //Minimum temperature for photosynthesis
        float PsnTMin { get; }

        //Maximum temperature for photosynthesis
        float PsnTMax { get; }

        // Foliar nitrogen (gN/gC)
        float FolN { get; set; }

        // Vapor pressure deficit response parameter 
        float DVPD1 { get; }

        // Vapor pressure deficit response parameter 
        float DVPD2 { get; }

        // Reference photosynthesis (g)
        float AmaxA { get; }

        // Response parameter for photosynthesis to N
        float AmaxB { get; }

        // Modifier of AmaxA due to averaging non-linear Amax data
        float AmaxFrac { get; }

        // Referece maintenance respiration 
        float MaintResp { get; }

        // Effect of CO2 on AMaxB (change in AMaxB with increase of 200 ppm CO2)
        float CO2AMaxBEff { get; }

        // Effect of CO2 on HalfSat (change in HalfSat with increase of 1 ppm CO2 [slope])
        float CO2HalfSatEff { get; }

        // Ozone stomatal sensitivity class (Sensitive, Intermediate, Tolerant)
        string O3StomataSens { get; }

        // Slope for linear FolN relationship
        float FolNShape { get; }

        //Intercept for linear FolN relationship
        float MaxFolN { get; }

        // Slope for linear FracFol relationship
        float FracFolShape { get; }

        //Intercept for linear FracFol relationship
        float MaxFracFol { get; }

        // Slope coefficient for O3Effect
        float O3GrowthSens { get; }
        // Cold tolerance
        float ColdTol { get; }

        // Mininum Temp for leaf-on (optional)
        // If not provided LeafOnMinT = PsnTMin
        float LeafOnMinT { get; }

        // Initial Biomass
        int InitBiomass { get; }

        // Lower canopy NSC reserve 
        float NSCReserve { get; }

        // Lifeform
        string Lifeform { get; }




        //////////////////////////////////////////////////
        ///PnET-CN N cycling parameters  Zhou, 8/30/20
        ///////////////////////////////////////////////////

        float FolNConRange { get; }//max fractional increase in N concentration
        float MaxNStore { get; } ////max N content in PlantN pool g N m-2

        float Kho { get; } //// soil decomposition constant, yr-1

        float NImmobA { get; } // N immoblization rate parameter
        float NImmobB { get; }// linear coefficients for fraction of mineralized N reimmobilized as a function of SOM C:N

        float FolNRetrans { get; } //fraction of foliage N retransfer to plant N, remainder in litter 
        float FLPctN { get; }  //min % N concentration in foliar litter

        float WLPctN { get; } ////min N % cincentration in wood litter

        float RLPctN { get; } // min N % cincentration in root litter

        float GRespFrac { get; } // min N % cincentration in root litter



        // Minimum defoliation amount that triggers refoliation
        float RefoliationMinimumTrigger { get; }
        
        // Maximum amount of refoliation
        float RefoliationMaximum { get; }

        // Cost of refoliation
        float RefoliationCost { get; }

        // Cost to NSC without refoliation
        float NonRefoliationCost { get; }

        // Maximum LAI
        float MaxLAI { get; }


        // Scalar value for calculating species moss depth
        float MossScalar { get; }

    }
}
