﻿using Landis.Core;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Landis.Extension.Succession.BiomassPnET
{
    /*public class WaterStressTable
    {
        Landis.Library.Parameters.Species.AuxParm<float[]> table = new Library.Parameters.Species.AuxParm<float[]>(PlugIn.ModelCore.Species);
        float[][] table2 = new float[PlugIn.ModelCore.Species.Count][];

        private static Landis.Library.Parameters.Species.AuxParm<float> H2;
        private static Landis.Library.Parameters.Species.AuxParm<float> H3;
        private static Landis.Library.Parameters.Species.AuxParm<float> H4;
        //---------------------------------------------------------------------
        public float this[int SpeciesIndex, int pressurehead]
        {
            get
            {
                if (pressurehead < table2[SpeciesIndex].Length) return table2[SpeciesIndex][pressurehead];
                return 0;
            }
        }
        //---------------------------------------------------------------------
        public float this[ISpecies species, int pressurehead]
        {
            get
            {
                if(pressurehead < table[species].Length) return table[species][pressurehead];
                return 0;
            }
        }
        //--------------------------------------------------------------------- 
        float GetFWater(ISpecies species, float pressurehead)
        {
            if (pressurehead < 0 || pressurehead > H4[species]) return 0;
            else if (pressurehead > H3[species]) return 1 - ((pressurehead - H3[species]) / (H4[species] - H3[species]));
            else if (pressurehead < H2[species]) return pressurehead / H2[species];
            else return 1;
        }
    }*/
}
