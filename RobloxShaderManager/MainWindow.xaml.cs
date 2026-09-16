using System;
using System.IO;
using System.Management;
using System.Windows;
using System.Windows.Media;
using System.Threading.Tasks;

namespace RobloxShaderManager
{
    public partial class MainWindow : Window
    {
        // Ansel folder path
        private readonly string anselPath = @"C:\Program Files\NVIDIA Corporation\Ansel";
        // Local Shaders Pack path (relative to the exe)
        private readonly string localShadersPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Shaders_Pack");

        private bool isShaderActive = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await CheckSystemStatus();
        }

        private async Task CheckSystemStatus()
        {
            Log("Vérification du système en cours...");

            // 1. Check NVIDIA Graphics Card
            bool hasNvidia = CheckForNvidiaGPU();
            if (hasNvidia)
            {
                TxtNvidiaStatus.Text = "Détectée";
                TxtNvidiaStatus.Foreground = new SolidColorBrush(Colors.LimeGreen);
            }
            else
            {
                TxtNvidiaStatus.Text = "Non détectée (ou WMI inaccessible)";
                TxtNvidiaStatus.Foreground = new SolidColorBrush(Colors.Red);
                Log("Attention : Aucune carte graphique NVIDIA n'a été trouvée. Le logiciel peut ne pas fonctionner.");
            }

            // 2. Check Ansel Folder
            if (Directory.Exists(anselPath))
            {
                TxtAnselStatus.Text = "Trouvé";
                TxtAnselStatus.Foreground = new SolidColorBrush(Colors.LimeGreen);
            }
            else
            {
                TxtAnselStatus.Text = "Non trouvé (sera créé)";
                TxtAnselStatus.Foreground = new SolidColorBrush(Colors.Orange);
            }

            // 3. Check Local Shaders Pack
            if (Directory.Exists(localShadersPath))
            {
                TxtLocalShadersStatus.Text = "Trouvé";
                TxtLocalShadersStatus.Foreground = new SolidColorBrush(Colors.LimeGreen);
            }
            else
            {
                try
                {
                    Directory.CreateDirectory(localShadersPath);
                    TxtLocalShadersStatus.Text = "Créé";
                    TxtLocalShadersStatus.Foreground = new SolidColorBrush(Colors.LimeGreen);
                    Log($"Dossier '{localShadersPath}' créé. Veuillez y placer vos fichiers .fx.");
                }
                catch (Exception ex)
                {
                    TxtLocalShadersStatus.Text = "Erreur";
                    TxtLocalShadersStatus.Foreground = new SolidColorBrush(Colors.Red);
                    Log($"Erreur lors de la création du dossier local : {ex.Message}");
                }
            }

            UpdateShaderStateUI();
        }

        private bool CheckForNvidiaGPU()
        {
            try
            {
                if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                    return true; // Bypassing check if not strictly Windows in this context

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string name = obj["Name"]?.ToString() ?? "";
                        if (name.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Erreur lors de la vérification du GPU : {ex.Message}");
                // Return true to not completely block the user if WMI fails
                return true;
            }
            return false;
        }

        private void BtnToggleShaders_Click(object sender, RoutedEventArgs e)
        {
            if (isShaderActive)
            {
                DeactivateShaders();
            }
            else
            {
                ActivateShaders();
            }
        }

        private void ActivateShaders()
        {
            try
            {
                Log("Activation des shaders...");

                // Ensure local shaders exist
                if (!Directory.Exists(localShadersPath))
                {
                    Log("Le dossier Shaders_Pack n'existe pas.");
                    return;
                }

                string[] files = Directory.GetFiles(localShadersPath, "*.*", SearchOption.AllDirectories);
                if (files.Length == 0)
                {
                    MessageBox.Show("Le dossier 'Shaders_Pack' est vide. Veuillez y placer vos fichiers .fx et textures.", "Dossier vide", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Log("Aucun fichier à copier dans Shaders_Pack.");
                    return;
                }

                // Ensure Ansel folder exists (requires admin rights, requested in manifest)
                if (!Directory.Exists(anselPath))
                {
                    Directory.CreateDirectory(anselPath);
                    Log("Dossier NVIDIA Ansel créé.");
                }

                // Copy files
                foreach (string file in files)
                {
                    string relativePath = Path.GetRelativePath(localShadersPath, file);
                    string destFile = Path.Combine(anselPath, relativePath);

                    string? destDir = Path.GetDirectoryName(destFile);
                    if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                    {
                        Directory.CreateDirectory(destDir);
                    }

                    File.Copy(file, destFile, true); // true = overwrite
                }

                isShaderActive = true;
                UpdateShaderStateUI();
                Log("Shaders activés avec succès ! (Utilisez Alt+F3 en jeu)");
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Le programme nécessite les droits administrateur pour écrire dans le dossier NVIDIA.", "Erreur de droits", MessageBoxButton.OK, MessageBoxImage.Error);
                Log("Erreur: Droits administrateur manquants.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Une erreur est survenue :\n{ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                Log($"Erreur d'activation : {ex.Message}");
            }
        }

        private void DeactivateShaders()
        {
            try
            {
                Log("Désactivation des shaders...");

                if (!Directory.Exists(anselPath) || !Directory.Exists(localShadersPath))
                {
                    Log("Dossier Ansel ou Shaders_Pack introuvable, impossible de nettoyer.");
                    isShaderActive = false;
                    UpdateShaderStateUI();
                    return;
                }

                // Delete only files that came from our Shaders_Pack
                string[] localFiles = Directory.GetFiles(localShadersPath, "*.*", SearchOption.AllDirectories);
                foreach (string localFile in localFiles)
                {
                    string relativePath = Path.GetRelativePath(localShadersPath, localFile);
                    string targetFile = Path.Combine(anselPath, relativePath);

                    if (File.Exists(targetFile))
                    {
                        File.Delete(targetFile);
                    }
                }

                isShaderActive = false;
                UpdateShaderStateUI();
                Log("Shaders désactivés (fichiers retirés).");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Une erreur est survenue lors de la désactivation :\n{ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                Log($"Erreur de désactivation : {ex.Message}");
            }
        }

        private void UpdateShaderStateUI()
        {
            if (isShaderActive)
            {
                TxtShaderState.Text = "ACTIVÉ";
                TxtShaderState.Foreground = new SolidColorBrush(Colors.LimeGreen);
                BtnToggleShaders.Content = "DÉSACTIVER LES SHADERS";
                BtnToggleShaders.Background = new SolidColorBrush(Color.FromRgb(200, 50, 50)); // Red-ish
            }
            else
            {
                TxtShaderState.Text = "DÉSACTIVÉ";
                TxtShaderState.Foreground = new SolidColorBrush(Colors.Red);
                BtnToggleShaders.Content = "ACTIVER LES SHADERS";
                BtnToggleShaders.Background = new SolidColorBrush(Color.FromRgb(51, 51, 51)); // Dark Gray #333333
            }
        }

        private void Log(string message)
        {
            TxtLog.Text = $"[{DateTime.Now:HH:mm:ss}] {message}";
        }
    }
}