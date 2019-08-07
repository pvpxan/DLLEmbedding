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
                string dllName = new AssemblyName(args.Name).Name + ".dll";
                var assembly = Assembly.GetExecutingAssembly();

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

                    //Choose one of the two blocks below:

                    // WPF
                    MessageBox.Show("Error loading embedded assembly resource. Application will now close." + Environment.NewLine + Convert.ToString(Ex));
                    Application.Current.Shutdown();

                    // Console
                    Console.WriteLine("Error loading embedded assembly resource. Console Application will now close." + Environment.NewLine + Convert.ToString(Ex));
                    Environment.Exit(0);
                }
                finally
                {
                    if (stream != null)
                    {
                        stream.Dispose();
                    }
                }

                return loadedAssembly;
            };
        }

        // This is for WPF applications only that reference XAML files built into an assembly.
        public static void LoadResourceDictionary(App app, string assembly, string path)
        {
            // Uri path of assembly resource.
            string uri = @"pack://application:,,,/" + assembly + ";component/" + path;

            // Add Uri to App ResourceDictionary.
            app.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(uri) });
        }
    }
    // END DLLEmbeddingHandler_Class ----------------------------------------------------------------------------------------------------
}
