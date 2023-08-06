using RyzenSmu;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Universal_x86_Tuning_Utility_Handheld.Scripts.Intel;

namespace Universal_x86_Tuning_Utility.Scripts.AMD_Backend
{

    internal class RyzenAdj_To_UXTU
    {
        static int i = 0;
        //Translate RyzenAdj like cli arguments to UXTU
        public static async void Translate(string _ryzenAdjString, bool isAutoReapply = false)
        {
            try
            {
                //Remove last space off cli arguments 
                _ryzenAdjString = _ryzenAdjString.Substring(0, _ryzenAdjString.Length - 1);
                //Split cli arguments into array
                string[] ryzenAdjCommands = _ryzenAdjString.Split(' ');
                ryzenAdjCommands = ryzenAdjCommands.Distinct().ToArray();

                //Run through array
                foreach (string ryzenAdjCommand in ryzenAdjCommands)
                {
                    await Task.Run(() =>
                    {
                        try
                        {
                            string command = ryzenAdjCommand;
                            if (!command.Contains("=")) command = ryzenAdjCommand + "=0";
                            // Extract the command string before the "=" sign
                            string ryzenAdjCommandString = command.Split('=')[0].Replace("=", null).Replace("--", null);
                            // Extract the command string after the "=" sign
                            string ryzenAdjCommandValueString = command.Substring(ryzenAdjCommand.IndexOf('=') + 1);

                            //Convert value of select cli argument to uint
                            uint ryzenAdjCommandValue = Convert.ToUInt32(ryzenAdjCommandValueString);
                            if (ryzenAdjCommandValue <= 0 && !ryzenAdjCommandString.Contains("co")) SMUCommands.applySettings(ryzenAdjCommandString, 0x0);
                            else SMUCommands.applySettings(ryzenAdjCommandString, ryzenAdjCommandValue);
                            Task.Delay(2);

                        }
                        catch (Exception ex) { }
                    });
                }
            }
            catch (Exception ex) { }
        }
    }
}

