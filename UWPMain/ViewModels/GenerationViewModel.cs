using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Algorithms;
using DataStructures;

namespace UWPMain.ViewModels
{
    public class GenerationViewModel : BindableBase
    {
        public PathGraph FittestPath { get; set; }

        public int GenerationNumber { get; set; }
    }
}
