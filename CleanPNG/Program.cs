using System;
using System.Collections.Generic;
using TextureSplitter;

namespace CleanPNG
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        #region Properties

        public static IReadOnlyList<string> Args { get; private set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            Args = args;
            using (var game = new Game1())
                game.Run();
        }

        #endregion Methods
    }
}