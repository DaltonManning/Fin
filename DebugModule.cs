using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CONTROLBUILDERLib;
namespace Fin
{
    public class DebugMod
    {
        public void CM()
        {
            CBOpenIF cb = null;
            try
            {
                Console.WriteLine("wow");
                string contmod = cb.GetDiagramInstance("CopDisperserLib.UnitLSAHldTkTr3");
                Console.WriteLine(contmod);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
