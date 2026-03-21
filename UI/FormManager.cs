using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using VO_Tool.Selectors;
using VO_Tool.Services;
using VO_Tool.Status;

namespace VO_Tool.UI
{
    public class FormManager
    {
        private readonly UIControls ui = new UIControls();
        private readonly ExcelService excelService;
        private readonly ReaperService reaperService;
        private readonly FileSelectionHandler fileHandler;
        
        public FormManager(Main form)
        {
            excelService = new ExcelService();
            reaperService = new ReaperService();
            
            ui.MainForm = form;

            // Setup form
            UIHelpers.SetFormSize(form, 600, 480);
            UIHelpers.CenterForm(form);
            form.Text = "Reaper Audio Splitter";
            
            var builder = new UIBuilder(form);
            
            // Add Excel file selector
            (ui.Lbl_ExcelFile, ui.ExcelSelector) = builder.AddFileSelectorWithLabel("Excel File:", "Excel Files|*.xlsx;*.xls|All Files|*.*");
            
            // Add Reaper file selector
            (ui.Lbl_ReaperFile, ui.ReaperSelector) = builder.AddFileSelectorWithLabel("Reaper Project:", "Reaper Project Files|*.rpp|All Files|*.*");
            
            // Track Y position
            int currentY = builder.GetCurrentY();
            
            // Create VO Text Column dropdown
            (ui.Lbl_VO_Text_Column, ui.Cmb_VO_Text_Column) = UIHelpers.CreateLabeledComboBox("VO Text Column (A=1):", 20, currentY, enabled: false);
            form.Controls.Add(ui.Lbl_VO_Text_Column);
            form.Controls.Add(ui.Cmb_VO_Text_Column);
            
            currentY += 35;
            
            // Create VO Audio File Name Column dropdown
            (ui.Lbl_VO_Audio_Column, ui.Cmb_VO_Audio_Column) = UIHelpers.CreateLabeledComboBox("VO Audio File Name Column (A=1):", 20, currentY, enabled: false);
            form.Controls.Add(ui.Lbl_VO_Audio_Column);
            form.Controls.Add(ui.Cmb_VO_Audio_Column);
            
            currentY += 35;
            
            // Create Source Track dropdown
            (ui.LblSourceTrack, ui.CmbSourceTrack) = UIHelpers.CreateLabeledComboBox("Source Track:", 20, currentY, enabled: false);
            form.Controls.Add(ui.LblSourceTrack);
            form.Controls.Add(ui.CmbSourceTrack);
            
            currentY += 35;
            
            // Create Output Track dropdown
            (ui.LblOutputTrack, ui.CmbOutputTrack) = UIHelpers.CreateLabeledComboBox("Output Track:", 20, currentY, enabled: false);
            form.Controls.Add(ui.LblOutputTrack);
            form.Controls.Add(ui.CmbOutputTrack);
            
            currentY += 35;
            
            // Update UI builder's Y position
            builder.UpdateCurrentY(currentY);
            
            // Add button and status bar
            ui.BtnProcess = builder.AddButton("Process", OnProcessClick!);
            ui.StatusManager = builder.AddStatusBar();
            
            // Setup all tooltips
            var tooltips = new TooltipManager();
            tooltips.SetupAllTooltips(ui);
            
            // Create file handler
            fileHandler = new FileSelectionHandler(ui.StatusManager, excelService, reaperService);
            
            // Subscribe to browse events
            ui.ExcelSelector.OnFileSelected += file => _ = fileHandler.HandleExcelFileAsync(file, ui.Cmb_VO_Text_Column, ui.Cmb_VO_Audio_Column);
            ui.ReaperSelector.OnFileSelected += file => _ = fileHandler.HandleReaperFileAsync(file, ui.CmbSourceTrack, ui.CmbOutputTrack);
        }
        
        private async void OnProcessClick(object? sender, EventArgs e)
        {
            if (!UIHelpers.ValidateInputs(ui, out string errorMessage))
            {
                UIHelpers.ShowError(errorMessage);
                return;
            }
            
            try
            {
                UIHelpers.DisableControls(ui.BtnProcess);
                
                List<string> texts = null!;
                List<string> audioFiles = null!;
                
                // Convert column letters to numbers
                int textColumnNum = await excelService.GetColumnNumberFromLetter(ui.Cmb_VO_Text_Column.SelectedItem?.ToString() ?? "A");
                int audioColumnNum = await excelService.GetColumnNumberFromLetter(ui.Cmb_VO_Audio_Column.SelectedItem?.ToString() ?? "A");
                
                texts = await UIHelpers.ExecuteWithStatusAsync(
                    ui.StatusManager,
                    () => excelService.ReadColumnByNumberAsync(ui.ExcelSelector.FilePath, textColumnNum),
                    "Reading VO text column...",
                    $"Found entries in column {ui.Cmb_VO_Text_Column.SelectedItem}"
                );
                
                audioFiles = await UIHelpers.ExecuteWithStatusAsync(
                    ui.StatusManager,
                    () => excelService.ReadColumnByNumberAsync(ui.ExcelSelector.FilePath, audioColumnNum),
                    "Reading audio file names...",
                    $"Found entries in column {ui.Cmb_VO_Audio_Column.SelectedItem}"
                );
                
                ui.StatusManager.UpdateStatus($"Project: {UIHelpers.GetFileName(ui.ReaperSelector.FilePath)}");
                ui.StatusManager.UpdateStatus($"Source track: {ui.CmbSourceTrack.SelectedItem}");
                ui.StatusManager.UpdateStatus($"Output track: {ui.CmbOutputTrack.SelectedItem}");
                
                UIHelpers.ShowSuccess(
                    $"Successfully loaded:\n" +
                    $"- {texts.Count} text entries from column {ui.Cmb_VO_Text_Column.SelectedItem}\n" +
                    $"- {audioFiles.Count} audio file names from column {ui.Cmb_VO_Audio_Column.SelectedItem}\n" +
                    $"- Project: {UIHelpers.GetFileName(ui.ReaperSelector.FilePath)}\n" +
                    $"- Source track: {ui.CmbSourceTrack.SelectedItem}\n" +
                    $"- Output track: {ui.CmbOutputTrack.SelectedItem}"
                );
            }
            catch (Exception ex)
            {
                ui.StatusManager.UpdateStatus($"Error: {ex.Message}");
                UIHelpers.ShowException($"An error occurred: {ex.Message}");
            }
            finally
            {
                UIHelpers.EnableControls(ui.BtnProcess);
            }
        }
    }
}