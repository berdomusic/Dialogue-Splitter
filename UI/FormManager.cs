using VO_Tool.Services;
using VO_Tool.Status;
using VO_Tool.Settings;

namespace VO_Tool.UI
{
    public class FormManager
    {
        private readonly UIControls ui = new UIControls();
        private readonly AppSettings settings;
        private bool _isInitializing = true;
        
        public FormManager(Main form)
        {
            // Load saved settings
            settings = VO_Tool.Settings.Settings.Get();
            
            ui.ExcelService = new ExcelService();
            ui.MainForm = form;

            var builder = new UIBuilder(form);
            
            builder.SetFormSize(600, 700);
            builder.CenterForm();
            form.Text = "Audio Splitter - Speech to Text";

            // Add Excel file selector (without callback yet)
            (ui.Lbl_ExcelFile, ui.ExcelSelector) = builder.AddFileSelectorWithLabel("Excel File:", "Excel Files|*.xlsx;*.xls|All Files|*.*");
            
            if (!string.IsNullOrEmpty(settings.LastExcelFile) && File.Exists(settings.LastExcelFile))
            {
                ui.ExcelSelector.SetFilePath(settings.LastExcelFile);
            }

            // Add Audio file selector (without callback yet)
            (ui.Lbl_AudioFile, ui.AudioSelector) = builder.AddFileSelectorWithLabel("Audio File:", "Audio Files|*.wav;*.mp3;*.flac;*.m4a|All Files|*.*");
            
            if (!string.IsNullOrEmpty(settings.LastAudioFile) && File.Exists(settings.LastAudioFile))
            {
                ui.AudioSelector.SetFilePath(settings.LastAudioFile);
            }
            
            // Add Output folder selector (without callback yet)
            (ui.Lbl_OutputFolder, ui.OutputFolderSelector) = builder.AddFolderSelectorWithLabel("Output Folder:");
            
            if (!string.IsNullOrEmpty(settings.LastOutputFolder) && Directory.Exists(settings.LastOutputFolder))
            {
                ui.OutputFolderSelector.SetFolderPath(settings.LastOutputFolder);
            }

            // Add event handlers after restoration
            ui.ExcelSelector.OnFileSelected += file => 
            {
                _ = ui.ExcelService.LoadExcelColumnsAsync(file, ui.Cmb_VO_Text_Column, ui.Cmb_VO_Audio_Column, ui.StatusManager);
                SaveSettings();
            };
            
            ui.AudioSelector.OnFileSelected += file => 
            {
                var validation = new FileSelectionHelper().ValidateAudioFile(file);
                if (validation.IsValid)
                {
                    ui.StatusManager.UpdateStatus($"Audio file: {UIHelpers.GetFileName(file)} - {validation.GetStatusMessage()}");
                }
                else
                {
                    ui.StatusManager.UpdateStatus($"Error: {validation.ErrorMessage}");
                    UIHelpers.ShowError(validation.ErrorMessage);
                }
                SaveSettings();
            };
            
            ui.OutputFolderSelector.OnFolderSelected += folder => SaveSettings();

            // Get current Y position
            int yPos = builder.GetCurrentY();

            // Check if we have a saved Excel file
            bool hasExcelFile = !string.IsNullOrEmpty(settings.LastExcelFile) && File.Exists(settings.LastExcelFile);

            // Create VO Text Column dropdown
            string defaultTextColumn = !string.IsNullOrEmpty(settings.LastVO_Text_Column) ? settings.LastVO_Text_Column : "A";
            (ui.Lbl_VO_Text_Column, ui.Cmb_VO_Text_Column) = builder.CreateLabeledComboBox(
                "VO Text Column (A=1):", 
                20, 
                yPos, 
                defaultSelectedValue: defaultTextColumn,
                enabled: hasExcelFile,
                onSelectedIndexChanged: (s, e) => SaveSettings());
            form.Controls.Add(ui.Lbl_VO_Text_Column);
            form.Controls.Add(ui.Cmb_VO_Text_Column);

            yPos += 35;

            // Create VO Audio File Name Column dropdown
            string defaultAudioColumn = !string.IsNullOrEmpty(settings.LastVO_Audio_Column) ? settings.LastVO_Audio_Column : "A";
            (ui.Lbl_VO_Audio_Column, ui.Cmb_VO_Audio_Column) = builder.CreateLabeledComboBox(
                "VO Audio File Name Column (A=1):", 
                20, 
                yPos, 
                defaultSelectedValue: defaultAudioColumn,
                enabled: hasExcelFile,
                onSelectedIndexChanged: (s, e) => SaveSettings());
            form.Controls.Add(ui.Lbl_VO_Audio_Column);
            form.Controls.Add(ui.Cmb_VO_Audio_Column);

            yPos += 35;

            // Add similarity threshold slider with save callback
            builder.UpdateCurrentY(yPos);
            ui.Tb_SimilarityThreshold = builder.AddSimilaritySlider(
                20, 10, 100, 75,
                onScroll: (s, e) => SaveSettings());
            
            // Update builder's Y position after slider and add extra space
            yPos = builder.GetCurrentY();
            yPos += 35;
            builder.UpdateCurrentY(yPos);

            // Add model selector with saved model and save callback
            (ui.Lbl_Model, ui.Cmb_Model) = builder.AddModelSelector(
                20, 
                settings.LastModel,
                onSelectedIndexChanged: (s, e) => SaveSettings());
            
            // Add language selector with saved language and save callback
            (ui.Lbl_Language, ui.Cmb_Language) = builder.AddLanguageSelector(
                20, 
                settings.LastLanguage,
                onSelectedIndexChanged: (s, e) => SaveSettings());
            
            // Add log file checkbox with save callback
            ui.ChkCreateLogFile = builder.AddCheckBox(
                "Create log file", 
                20, 
                settings.CreateLogFile,
                onCheckedChanged: (s, e) => SaveSettings());
            
            // Add CSV file checkbox with save callback
            ui.ChkCreateCsvFile = builder.AddCheckBox(
                "Create CSV file", 
                20, 
                settings.CreateCsvFile,
                onCheckedChanged: (s, e) => SaveSettings());
            
            ui.ChkSplitAudio = builder.AddCheckBox(
                "Split audio files", 
                20, 
                settings.SplitAudio,
                onCheckedChanged: (s, e) => SaveSettings());
            
            // Start padding - can be negative (cut from beginning)
            (ui.LblStartPadding, ui.NudStartPadding) = builder.AddNumericUpDown(
                "Start Padding (s):", 
                (decimal)settings.StartPaddingSeconds, 
                -2, 2, 0.05m, 20,
                (s, e) => SaveSettings());

            // End padding - can be negative (cut from end)
            (ui.LblEndPadding, ui.NudEndPadding) = builder.AddNumericUpDown(
                "End Padding (s):", 
                (decimal)settings.EndPaddingSeconds, 
                -2, 2, 0.05m, 20,
                (s, e) => SaveSettings());

            // Add button and status bar
            ui.BtnProcess = builder.AddButton("Process", OnProcessClick!);
            ui.StatusManager = builder.AddStatusBar();

            // Setup all tooltips
            var tooltips = new TooltipManager();
            tooltips.SetupAllTooltips(ui);

            // Setup drag and drop
            builder.SetupFileDrop(
                form,
                onExcelFile: file => 
                {
                    ui.ExcelSelector.SetFilePath(file);
                },
                onAudioFile: file =>
                {
                    ui.AudioSelector.SetFilePath(file);
                }
            );
            
            // Load Excel columns if there's a saved Excel file
            if (hasExcelFile)
            {
                _ = ui.ExcelService.LoadExcelColumnsAsync(settings.LastExcelFile, ui.Cmb_VO_Text_Column, ui.Cmb_VO_Audio_Column, ui.StatusManager, (textCol, audioCol) =>
                {
                    // Restore saved selections after columns load
                    if (!string.IsNullOrEmpty(settings.LastVO_Text_Column))
                    {
                        for (int i = 0; i < ui.Cmb_VO_Text_Column.Items.Count; i++)
                        {
                            if (ui.Cmb_VO_Text_Column.Items[i]?.ToString() == settings.LastVO_Text_Column)
                            {
                                ui.Cmb_VO_Text_Column.SelectedIndex = i;
                                break;
                            }
                        }
                    }
                    
                    if (!string.IsNullOrEmpty(settings.LastVO_Audio_Column))
                    {
                        for (int i = 0; i < ui.Cmb_VO_Audio_Column.Items.Count; i++)
                        {
                            if (ui.Cmb_VO_Audio_Column.Items[i]?.ToString() == settings.LastVO_Audio_Column)
                            {
                                ui.Cmb_VO_Audio_Column.SelectedIndex = i;
                                break;
                            }
                        }
                    }
                });
            }
            
            // Initialization complete
            _isInitializing = false;
        }
        
        private void SaveSettings()
        {
            if (_isInitializing) return;
            settings.UpdateFromUI(ui);
            Settings.Settings.Save();
        }
        
        private async void OnProcessClick(object? sender, EventArgs e)
        {
            await ui.OnProcessClick(sender, e);
        }
    }
}