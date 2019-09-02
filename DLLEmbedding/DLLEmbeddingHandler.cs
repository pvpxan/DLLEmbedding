using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;

namespace DLLEmbedding
{
    // START DLLEmbeddingHandler_Class --------------------------------------------------------------------------------------------------
    public static class DLLEmbeddingHandler
    {
        // NECESSARY for loading embedded resources.
        // ----------------------------------------------------------------------------------------------
        public static void LoadAssemblies()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                return resolve(sender, args);
            };
        }

        private static Assembly resolve(object sender, ResolveEventArgs args)
        {
            string dllName = new AssemblyName(args.Name).Name + ".dll";
            Assembly assembly = Assembly.GetExecutingAssembly();

            string resourceName = assembly.GetManifestResourceNames().FirstOrDefault(rn => rn.EndsWith(dllName));
            if (resourceName == null)
            {
                return null; // Not found, maybe another handler will find it
            }

            System.IO.Stream stream = null;
            Assembly loadedAssembly = null;
            try
            {
                stream = assembly.GetManifestResourceStream(resourceName);
                byte[] assemblyData = new byte[stream.Length];
                stream.Read(assemblyData, 0, assemblyData.Length);
                loadedAssembly = Assembly.Load(assemblyData);
            }
            catch (Exception Ex)
            {
                loadedAssembly = null;

                MessageBox.Show("Error loading embedded assembly resource. Application will now close." + Environment.NewLine + Convert.ToString(Ex));
                showError("DLL", Ex);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Dispose();
                }
            }

            return loadedAssembly;
        }

        // This is for WPF applications only that reference XAML files built into an assembly.
        public static void LoadResourceDictionary(string assembly, string path)
        {
            // Uri path of assembly resource.
            string uri = @"pack://application:,,,/" + assembly + ";component/" + path;

            // Add Uri to App ResourceDictionary.
            try
            {
                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(uri) });
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Error loading embedded XAML resource. Application will now close." + Environment.NewLine + Convert.ToString(Ex));
                showError("XAML", Ex);
            }
        }

        private static void showError(string resource, Exception Ex)
        {
            string message = "Error loading embedded " + resource + " resource. Application will now close." + Environment.NewLine + Convert.ToString(Ex);

            //Choose one of the two blocks below:

            // WPF
            // -----------
            MessageBox.Show(message);
            // -----------

            // Console
            // -----------
            Console.WriteLine(message);
            // -----------

            Shutdown();
        }

        public static void Shutdown()
        {
            //Choose one of the two blocks below:

            // WPF
            // -----------
            if (Application.Current != null)
            {
                Application.Current.Shutdown();
            }
            else
            {
                Environment.Exit(0);
            }
            // -----------

            // Console
            // -----------
            Environment.Exit(0);
            // -----------
        }
    }
    // END DLLEmbeddingHandler_Class ----------------------------------------------------------------------------------------------------
}
