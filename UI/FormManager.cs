using VO_Tool.Services;
using VO_Tool.Status;

namespace VO_Tool.UI
{
    public class FormManager
    {
        private readonly UIControls ui = new UIControls();
        
        public FormManager(Main form)
        {
            ui.ExcelService = new ExcelService();
            
            ui.MainForm = form;

            var builder = new UIBuilder(form);
            
            builder.SetFormSize(600, 600);
            builder.CenterForm();
            form.Text = "Audio Splitter - Speech to Text";

            // Add Excel file selector
            (ui.Lbl_ExcelFile, ui.ExcelSelector) = builder.AddFileSelectorWithLabel("Excel File:", "Excel Files|*.xlsx;*.xls|All Files|*.*");

            // Add Audio file selector
            (ui.Lbl_AudioFile, ui.AudioSelector) = builder.AddFileSelectorWithLabel("Audio File:", "Audio Files|*.wav;*.mp3;*.flac;*.m4a|All Files|*.*");
            
            // Add Output folder selector
            (ui.Lbl_OutputFolder, ui.OutputFolderSelector) = builder.AddFolderSelectorWithLabel("Output Folder:");

            // Get current Y position
            int yPos = builder.GetCurrentY();

            // Create VO Text Column dropdown
            (ui.Lbl_VO_Text_Column, ui.Cmb_VO_Text_Column) = builder.CreateLabeledComboBox("VO Text Column (A=1):", 20, yPos, enabled: false);
            form.Controls.Add(ui.Lbl_VO_Text_Column);
            form.Controls.Add(ui.Cmb_VO_Text_Column);

            yPos += 35;

            // Create VO Audio File Name Column dropdown
            (ui.Lbl_VO_Audio_Column, ui.Cmb_VO_Audio_Column) = builder.CreateLabeledComboBox("VO Audio File Name Column (A=1):", 20, yPos, enabled: false);
            form.Controls.Add(ui.Lbl_VO_Audio_Column);
            form.Controls.Add(ui.Cmb_VO_Audio_Column);

            yPos += 35;

            // Add similarity threshold slider using builder
            builder.UpdateCurrentY(yPos);
            ui.Tb_SimilarityThreshold = builder.AddSimilaritySlider();
            
            // Update UI builder's Y position
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
            ui.ExcelSelector.OnFileSelected += file => _ = fileHandler.HandleExcelFileAsync(file, ui.Cmb_VO_Text_Column, ui.Cmb_VO_Audio_Column);
            ui.AudioSelector.OnFileSelected += file => fileHandler.HandleAudioFileSelected(file);
            
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
        }
        
        private async void OnProcessClick(object? sender, EventArgs e)
        {
            await ui.OnProcessClick(sender, e);
        }
    }
}