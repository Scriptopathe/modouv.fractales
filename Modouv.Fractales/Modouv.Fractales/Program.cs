using System;
using System.Collections.Generic;
namespace Modouv.Fractales
{
#if WINDOWS || XBOX
    static class Program
    {
        public const bool LOG_EXCEPTIONS = false;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {

            using (Game1 game = new Game1())
            {
                if (LOG_EXCEPTIONS)
                {
                    try
                    {
                        game.Run();
                    }
                    catch (Exception e)
                    {

                        var stream = System.IO.File.Open("log.txt", System.IO.FileMode.OpenOrCreate);
                        System.IO.StreamWriter w = new System.IO.StreamWriter(stream);
                        w.WriteLine(e.Message);
                        w.WriteLine(e.StackTrace);
                        w.WriteLine(e.InnerException);
                        w.Close();
                        stream.Close();
                    }

                }
                else
                    game.Run();
            }
        }
    }
#endif
}

