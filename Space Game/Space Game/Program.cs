using System;

namespace Space_Game
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            if (args != null)
            {
                using (Spacegame game = new Spacegame(args))
                {
                    game.Run();
                }
            }
            else
            {
                using (Spacegame game = new Spacegame())
                {
                    game.Run();
                }
            }
        }
    }
#endif
}

