using System;

namespace sugarscape
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new GameDriver())
                game.Run();
        }
    }
}
