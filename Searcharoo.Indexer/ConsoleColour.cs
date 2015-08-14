using System;
// need this to make API calls
using System.Runtime.InteropServices;
// From CodeProject
// http://www.codeproject.com/KB/cs/console_apps__colour_text.aspx
namespace PFitzsimons.ConsoleColour
{
    /// <summary>
    /// Static class for console colour manipulation.
    /// </summary>
    public class ConsoleColour
    {
        // constants for console streams
        const int STD_INPUT_HANDLE = -10;
        const int STD_OUTPUT_HANDLE = -11;
        const int STD_ERROR_HANDLE = -12;

        [DllImportAttribute("Kernel32.dll")]
        private static extern IntPtr GetStdHandle
        (
            int nStdHandle // input, output, or error device
        );

        [DllImportAttribute("Kernel32.dll")]
        private static extern bool SetConsoleTextAttribute
        (
            IntPtr hConsoleOutput, // handle to screen buffer
            int wAttributes    // text and background colors
        );

        // colours that can be set
        [Flags]
        public enum ForeGroundColour
        {
            Black = 0x0000,
            Blue = 0x0001,
            Green = 0x0002, 
            Cyan = 0x0003,
            Red = 0x0004,
            Magenta = 0x0005,
            Yellow = 0x0006,
            White = 0x0007,
            Grey = 0x0008
        }

        // class can not be created, so we can set colours 
        // without a variable
        private ConsoleColour()
        {
        }

        public static bool SetForeGroundColour()
        {
            // default to a white-grey
            return SetForeGroundColour(ForeGroundColour.White);
        }

        public static bool SetForeGroundColour(
            ForeGroundColour foreGroundColour)
        {
            // default to a bright white-grey
            return SetForeGroundColour(foreGroundColour, true);
        }

        public static bool SetForeGroundColour(
            ForeGroundColour foreGroundColour, 
            bool brightColours)
        {
            // get the current console handle
            IntPtr nConsole = GetStdHandle(STD_OUTPUT_HANDLE);
            int colourMap;
            
            // if we want bright colours OR it with white
            if (brightColours)
                colourMap = (int) foreGroundColour | 
                    (int) ForeGroundColour.Grey;
            else
                colourMap = (int) foreGroundColour;

            // call the api and return the result
            return SetConsoleTextAttribute(nConsole, colourMap);
        }
    }
}