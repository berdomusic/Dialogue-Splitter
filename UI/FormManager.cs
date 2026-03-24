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
        private readonly FileSelectionHandler fileHandler;
        
        public FormManager(Main form)
        {
            excelService = new ExcelService();
            
            ui.MainForm = form;

            // Setup form
            UIHelpers.SetFormSize(form, 600, 380);
            UIHelpers.CenterForm(form);
            form.Text = "Audio Splitter - Speech to Text";

            var builder = new UIBuilder(form);

            // Add Excel file selector
            (ui.Lbl_ExcelFile, ui.ExcelSelector) = builder.AddFileSelectorWithLabel("Excel File:", "Excel Files|*.xlsx;*.xls|All Files|*.*");

            // Add Audio file selector
            (ui.Lbl_AudioFile, ui.AudioSelector) = builder.AddFileSelectorWithLabel("Audio File:", "Audio Files|*.wav;*.mp3;*.flac;*.m4a|All Files|*.*");

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

            // Update UI builder's Y position
            builder.UpdateCurrentY(currentY);

            // Add button and status bar
            ui.BtnProcess = builder.AddButton("Process", OnProcessClick!);
            ui.StatusManager = builder.AddStatusBar();

            // Setup all tooltips
            var tooltips = new TooltipManager();
            tooltips.SetupAllTooltips(ui);

            // Create file handler
            fileHandler = new FileSelectionHandler(ui.StatusManager, excelService);

            // Subscribe to browse events
            ui.ExcelSelector.OnFileSelected += file => _ = fileHandler.HandleExcelFileAsync(file, ui.Cmb_VO_Text_Column, ui.Cmb_VO_Audio_Column);
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
                
                // Convert column letters to numbers
                int textColumnNum = await excelService.GetColumnNumberFromLetter(ui.Cmb_VO_Text_Column.SelectedItem?.ToString() ?? "A");
                int audioColumnNum = await excelService.GetColumnNumberFromLetter(ui.Cmb_VO_Audio_Column.SelectedItem?.ToString() ?? "A");
                
                // Read text from Excel
                var texts = await UIHelpers.ExecuteWithStatusAsync(
                    ui.StatusManager,
                    () => excelService.ReadColumnByNumberAsync(ui.ExcelSelector.FilePath, textColumnNum),
                    "Reading text from Excel...",
                    $"Found {ui.Cmb_VO_Text_Column.SelectedItem} column entries"
                );
                
                // Read audio file names from Excel
                var audioFileNames = await UIHelpers.ExecuteWithStatusAsync(
                    ui.StatusManager,
                    () => excelService.ReadColumnByNumberAsync(ui.ExcelSelector.FilePath, audioColumnNum),
                    "Reading audio file names...",
                    $"Found {ui.Cmb_VO_Audio_Column.SelectedItem} column entries"
                );
                
                ui.StatusManager.UpdateStatus($"Loaded {texts.Count} text entries and {audioFileNames.Count} file names");
                
                UIHelpers.ShowSuccess(
                    $"Successfully loaded:\n" +
                    $"- {texts.Count} text entries from column {ui.Cmb_VO_Text_Column.SelectedItem}\n" +
                    $"- {audioFileNames.Count} audio file names from column {ui.Cmb_VO_Audio_Column.SelectedItem}\n" +
                    $"- Audio file: {UIHelpers.GetFileName(ui.AudioSelector.FilePath)}"
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