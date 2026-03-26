using VO_Tool.Services;
using VO_Tool.Status;
using VO_Tool.Settings;

namespace VO_Tool.UI
{
    public class FormManager
    {
        private bool _isInitializing = true;
        private readonly UIControls ui = new UIControls();
        private readonly AppSettings settings;
        
        public FormManager(Main form)
        {
            // Load saved settings
            settings = Settings.Settings.Load();
            
            ui.ExcelService = new ExcelService();
            
            ui.MainForm = form;

            var builder = new UIBuilder(form);
            
            builder.SetFormSize(600, 700);
            builder.CenterForm();
            form.Text = "Audio Splitter - Speech to Text";

            // Add Excel file selector
            (ui.Lbl_ExcelFile, ui.ExcelSelector) = builder.AddFileSelectorWithLabel("Excel File:", "Excel Files|*.xlsx;*.xls|All Files|*.*");
            
            bool hasExcelFile = !string.IsNullOrEmpty(settings.LastExcelFile) && File.Exists(settings.LastExcelFile);
            // Restore last Excel file if exists
            if (hasExcelFile)
            {
                ui.ExcelSelector.SetFilePath(settings.LastExcelFile);
            }

            // Add Audio file selector
            (ui.Lbl_AudioFile, ui.AudioSelector) = builder.AddFileSelectorWithLabel("Audio File:", "Audio Files|*.wav;*.mp3;*.flac;*.m4a|All Files|*.*");
            
            // Restore last Audio file if exists
            if (!string.IsNullOrEmpty(settings.LastAudioFile) && File.Exists(settings.LastAudioFile))
            {
                ui.AudioSelector.SetFilePath(settings.LastAudioFile);
            }
            
            // Add Output folder selector
            (ui.Lbl_OutputFolder, ui.OutputFolderSelector) = builder.AddFolderSelectorWithLabel("Output Folder:");
            
            // Restore last Output folder if exists
            if (!string.IsNullOrEmpty(settings.LastOutputFolder) && Directory.Exists(settings.LastOutputFolder))
            {
                ui.OutputFolderSelector.SetFolderPath(settings.LastOutputFolder);
            }

            // Get current Y position
            int yPos = builder.GetCurrentY();

            // Create VO Text Column dropdown
            (ui.Lbl_VO_Text_Column, ui.Cmb_VO_Text_Column) = builder.CreateLabeledComboBox(
                "VO Text Column (A=1):",
                20,
                yPos,
                !string.IsNullOrEmpty(settings.LastVO_Text_Column) ? settings.LastVO_Text_Column : "A",
                enabled: hasExcelFile
                );
            form.Controls.Add(ui.Lbl_VO_Text_Column);
            form.Controls.Add(ui.Cmb_VO_Text_Column);

            yPos += 35;

            // Create VO Audio File Name Column dropdown
            (ui.Lbl_VO_Audio_Column, ui.Cmb_VO_Audio_Column) = builder.CreateLabeledComboBox("VO Audio File Name Column (A=1):",
                20,
                yPos,
                !string.IsNullOrEmpty(settings.LastVO_Text_Column) ? settings.LastVO_Audio_Column : "A",
                enabled: hasExcelFile);
            form.Controls.Add(ui.Lbl_VO_Audio_Column);
            form.Controls.Add(ui.Cmb_VO_Audio_Column);

            yPos += 35;

            // Add similarity threshold slider using builder
            builder.UpdateCurrentY(yPos);
            ui.Tb_SimilarityThreshold = builder.AddSimilaritySlider();
            
            // Restore last similarity threshold
            ui.Tb_SimilarityThreshold.Value = settings.LastSimilarityThreshold;
            
            // Update builder's Y position after slider and add extra space
            yPos = builder.GetCurrentY();
            yPos += 35;
            builder.UpdateCurrentY(yPos);

            // Add model selector with saved model
            (ui.Lbl_Model, ui.Cmb_Model) = builder.AddModelSelector(20, settings.LastModel);
            
            // Add language selector with saved language
            (ui.Lbl_Language, ui.Cmb_Language) = builder.AddLanguageSelector(20, settings.LastLanguage);
            
            // Add log file checkbox
            ui.ChkCreateLogFile = builder.AddCheckBox("Create log file in output folder", 20, true);
            
            // Update UI builder's Y position after checkbox
            yPos = builder.GetCurrentY();

            // Add button and status bar
            ui.BtnProcess = builder.AddButton("Process", OnProcessClick!);
            ui.StatusManager = builder.AddStatusBar();

            // Setup all tooltips
            var tooltips = new TooltipManager();
            tooltips.SetupAllTooltips(ui);

            // Create file handler
            var fileHandler = new FileSelectionHandler(ui.StatusManager, ui.ExcelService);

            // Subscribe to browse events
            ui.ExcelSelector.OnFileSelected += file => 
            {
                _ = fileHandler.HandleExcelFileAsync(file, ui.Cmb_VO_Text_Column, ui.Cmb_VO_Audio_Column);
                SaveSettings();
            };
            ui.AudioSelector.OnFileSelected += file => 
            {
                fileHandler.HandleAudioFileSelected(file);
                SaveSettings();
            };
            ui.OutputFolderSelector.OnFolderSelected += folder => SaveSettings();
            
            // Save settings when column selection changes
            ui.Cmb_VO_Text_Column.SelectedIndexChanged += (s, e) => SaveSettings();
            ui.Cmb_VO_Audio_Column.SelectedIndexChanged += (s, e) => SaveSettings();
            ui.Tb_SimilarityThreshold.Scroll += (s, e) => SaveSettings();
            ui.Cmb_Language.SelectedIndexChanged += (s, e) => SaveSettings();
            
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
                    fileHandler.HandleAudioFileSelected(file);
                }
            );
            
            // Load Excel columns if there's a saved Excel file
            if (!string.IsNullOrEmpty(settings.LastExcelFile) && File.Exists(settings.LastExcelFile))
            {
                _ = LoadExcelColumnsAsync(settings.LastExcelFile);
            }
            
            _isInitializing = false;
        }
        
        private async Task LoadExcelColumnsAsync(string filePath)
        {
            var fileHandler = new FileSelectionHandler(ui.StatusManager, ui.ExcelService);
            await fileHandler.HandleExcelFileAsync(filePath, ui.Cmb_VO_Text_Column, ui.Cmb_VO_Audio_Column);
            
            // Restore last selected columns if they exist and are valid
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
        }
        
        private void SaveSettings()
        {
            if (_isInitializing) return;
            settings.UpdateFromUI(ui);
            Settings.Settings.Save(settings);
        }
        
        private async void OnProcessClick(object? sender, EventArgs e)
        {
            await ui.OnProcessClick(sender, e);
        }
    }
}