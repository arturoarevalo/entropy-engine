using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entropy
{
    using Win32Api;

    public class RenderingWindow : NativeWindow
    {
        public RenderingWindow (int width, int height) : base (@"Entropy.RenderingWindow", @"Entropy Engine Rendering Window", width, height)
        {
        }
    }


    internal class Program
    {
        private static void Main (string [] args)
        {

            var w = new RenderingWindow (1024, 768);

            while (w.RealtimeMessageLoop () == false) {
                //Thread.Sleep (2000);
            }



        }
    }
}