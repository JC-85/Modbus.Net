using System;
using System.Collections.Generic;
using System.Text;
using Dbg = System.Diagnostics.Debug;

#pragma warning disable CS1591

namespace Modbus.Net
{

    public class Log
    {
        /// <summary>
        /// 0 = Errors
        /// 1 = Warnings
        /// 2 = Information
        /// 3 = Verbose
        /// </summary>
        internal static int Verbosity { get; set; } = 0;

        public static void Error(string message)
        {
            Error(null, message);
        }

        public static void Error(Exception e, string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
            if(e!=null) System.Diagnostics.Debug.WriteLine(e.Message);
            if(Verbosity > 0) Dbg.WriteLine(e.StackTrace);
        }

        public static void Information(params string[] v)
        {
            Dbg.WriteLine(v);
        }

        internal static void Verbose(params object[] v)
        {
            if(Verbosity > 3)
            Dbg.WriteLine(v);
        }

        internal static void Debug(string v, string connectionToken)
        {
            Dbg.WriteLine(v);
        }
    }
}
