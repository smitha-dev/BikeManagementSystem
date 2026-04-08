using Microsoft.Win32;
using System;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;


namespace FindlayBikeShop
{
     public static class UIHelper
 {
     public static void ClearAllImages(DependencyObject parent)
     {
         if (parent == null) return;

         for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
         {
             var child = VisualTreeHelper.GetChild(parent, i);

             if (child is Image img)
             {
                 img.Source = null; // THIS releases the file lock
             }

             ClearAllImages(child);
         }
     }
 }
    public class BackupHelper
    {
        // Helper to copy folder recursively
        private static void CopyDirectory(string sourceDir, string destinationDir)
        {
            Directory.CreateDirectory(destinationDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                File.Copy(file, Path.Combine(destinationDir, Path.GetFileName(file)), true);
            }

            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                CopyDirectory(dir, Path.Combine(destinationDir, Path.GetFileName(dir)));
            }
        }

        public static void BackupBikeData()
        {
            // Ask the user where to save the backup
            var dialog = new SaveFileDialog
            {
                Filter = "Zip Files (*.zip)|*.zip",
                FileName = "BikeBackup.zip"
            };

            if (dialog.ShowDialog() != true) return;

            string backupPath = dialog.FileName;

            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BikeDatabase.db");
            string imagesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");

            try
            {
                // Create a temporary folder to gather files
                string tempFolder = Path.Combine(Path.GetTempPath(), "BikeBackup_" + DateTime.Now.Ticks);
                Directory.CreateDirectory(tempFolder);

                // Copy DB
                File.Copy(dbPath, Path.Combine(tempFolder, "BikeDatabase.db"));

                // Copy Images folder
                string imagesBackupPath = Path.Combine(tempFolder, "Images");
                if (Directory.Exists(imagesPath))
                {
                    CopyDirectory(imagesPath, imagesBackupPath);
                }

                // Create ZIP
                if (File.Exists(backupPath)) File.Delete(backupPath);
                ZipFile.CreateFromDirectory(tempFolder, backupPath);

                // Clean temp folder
                Directory.Delete(tempFolder, true);

                MessageBox.Show("Backup successful!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Backup failed: " + ex.Message);
            }
        }



        // =============================
        // RESTORE FUNCTION
        // =============================
        public static void RestoreBikeData()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Zip Files (*.zip)|*.zip",
                Title = "Select a backup to restore"
            };

            if (dialog.ShowDialog() != true) return;

            string backupPath = dialog.FileName;
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string dbPath = Path.Combine(baseDir, "BikeDatabase.db");
            string imagesPath = Path.Combine(baseDir, "Images");

            try
            {
                string tempFolder = Path.Combine(Path.GetTempPath(), "BikeRestore_" + DateTime.Now.Ticks);
                Directory.CreateDirectory(tempFolder);

                // Extract backup to temp folder
                ZipFile.ExtractToDirectory(backupPath, tempFolder);

                // Restore DB
                string backupDb = Path.Combine(tempFolder, "BikeDatabase.db");
                if (File.Exists(backupDb))
                {
                    File.Copy(backupDb, dbPath, true);
                }

                // Restore Images folder
                string backupImages = Path.Combine(tempFolder, "Images");
                if (Directory.Exists(backupImages))
                {
                    // Delete current Images folder first
                    if (Directory.Exists(imagesPath))
                        Directory.Delete(imagesPath, true);

                    CopyDirectory(backupImages, imagesPath);
                }

                // Clean temp folder
                Directory.Delete(tempFolder, true);

                MessageBox.Show("Restore successful!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Restore failed: " + ex.Message);
            }
        }
    }
}
