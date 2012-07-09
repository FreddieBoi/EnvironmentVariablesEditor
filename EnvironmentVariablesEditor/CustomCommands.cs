using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace EnvironmentVariablesEditor
{

    public static class CustomCommands
    {

        public static RoutedCommand Load = new RoutedCommand();

        public static RoutedCommand Save = new RoutedCommand();

        public static RoutedCommand About = new RoutedCommand();

        public static RoutedCommand Export = new RoutedCommand();

        public static RoutedCommand Restore = new RoutedCommand();

    }

}
