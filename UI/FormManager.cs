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
        private readonly FileSelector excelSelector;
        private readonly ComboBox cmbReaperProjects;
        private readonly FolderSelector outputFolderSelector;
        private readonly ComboBox cmbSourceTrack;
        private readonly ComboBox cmbOutputTrack;
        private readonly ComboBox cmb_VO_Text_Name;
        private readonly ComboBox cmb_VO_Audio_Name;
        private readonly Button btnProcess;
        private readonly StatusManager statusManager;
        private readonly ExcelService excelService;
        private readonly ReaperService reaperService;
        private List<ReaperProjectInfo> _openProjects = new();
        private ReaperProjectInfo? _selectedProject;
        
        public FormManager(Main form)
        {
            excelService = new ExcelService();
            reaperService = new ReaperService();

            // Setup form
            UIHelpers.SetFormSize(form, 600, 530);
            UIHelpers.CenterForm(form);
            form.Text = "Reaper Audio Splitter";
            
            var ui = new UIBuilder(form);
            
            // Add Excel file selector
            (var lbl_ExcelFile, excelSelector) = ui.AddFileSelectorWithLabel("Excel File:", "Excel Files|*.xlsx;*.xls|All Files|*.*");
            
            // Add Reaper projects dropdown instead of file selector
            var lbl_ReaperProject = UIHelpers.CreateLabel("Reaper Project:", 20, ui.GetCurrentY());
            cmbReaperProjects = UIHelpers.CreateComboBox(160, ui.GetCurrentY() - 3);
            cmbReaperProjects.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbReaperProjects.SelectedIndexChanged += OnReaperProjectSelected!;
            form.Controls.Add(lbl_ReaperProject);
            form.Controls.Add(cmbReaperProjects);
            ui.UpdateCurrentY(ui.GetCurrentY() + 50);
            
            // Add folder selector
            (var lbl_OutputFolder, outputFolderSelector) = ui.AddFolderSelectorWithLabel("Output Folder:");
            
            // Track Y position
            int currentY = ui.GetCurrentY();
            
            // Create VO Text Column dropdown
            (var lbl_VO_Text_Name, cmb_VO_Text_Name) = UIHelpers.CreateLabeledComboBox("VO Text Column:", 20, currentY, enabled: false);
            form.Controls.Add(lbl_VO_Text_Name);
            form.Controls.Add(cmb_VO_Text_Name);
            
            currentY += 35;
            
            // Create VO Audio File Name Column dropdown
            (var lbl_VO_Audio_Name, cmb_VO_Audio_Name) = UIHelpers.CreateLabeledComboBox("VO Audio File Name Column:", 20, currentY, enabled: false);
            form.Controls.Add(lbl_VO_Audio_Name);
            form.Controls.Add(cmb_VO_Audio_Name);
            
            currentY += 35;
            
            // Create Source Track dropdown
            (var lblSourceTrack, cmbSourceTrack) = UIHelpers.CreateLabeledComboBox("Source Track:", 20, currentY, enabled: false);
            form.Controls.Add(lblSourceTrack);
            form.Controls.Add(cmbSourceTrack);
            
            currentY += 35;
            
            // Create Output Track dropdown
            (var lblOutputTrack, cmbOutputTrack) = UIHelpers.CreateLabeledComboBox("Output Track:", 20, currentY, enabled: false);
            form.Controls.Add(lblOutputTrack);
            form.Controls.Add(cmbOutputTrack);
            
            currentY += 35;
            
            // Update UI builder's Y position
            ui.UpdateCurrentY(currentY);
            
            // Add button and status bar
            btnProcess = ui.AddButton("Process", OnProcessClick!);
            statusManager = ui.AddStatusBar();
            
            // Setup all tooltips
            var tooltips = new TooltipManager();
            tooltips.SetupAllTooltips(
                form,
                excelSelector,
                cmbReaperProjects,
                outputFolderSelector,
                lbl_ExcelFile,
                lbl_ReaperProject,
                lbl_OutputFolder,
                lbl_VO_Text_Name,
                cmb_VO_Text_Name,
                lbl_VO_Audio_Name,
                cmb_VO_Audio_Name,
                lblSourceTrack,
                cmbSourceTrack,
                lblOutputTrack,
                cmbOutputTrack,
                btnProcess,
                statusManager
            );
            
            // Load open Reaper projects
            LoadOpenProjectsAsync();
        }
        
        private async void LoadOpenProjectsAsync()
        {
            try
            {
                statusManager.UpdateStatus("Checking for open Reaper projects...");
                _openProjects = await reaperService.GetOpenProjectsAsync();
                
                cmbReaperProjects.Items.Clear();
                
                if (_openProjects.Count == 0)
                {
                    cmbReaperProjects.Items.Add("No open Reaper projects found");
                    cmbReaperProjects.Enabled = false;
                    statusManager.UpdateStatus("No open Reaper projects detected. Open a project in Reaper first.");
                }
                else
                {
                    foreach (var project in _openProjects)
                    {
                        cmbReaperProjects.Items.Add(project.Name);
                    }
                    cmbReaperProjects.SelectedIndex = 0;
                    statusManager.UpdateStatus($"Found {_openProjects.Count} open Reaper project(s)");
                }
            }
            catch (Exception ex)
            {
                statusManager.UpdateStatus($"Error: {ex.Message}");
                cmbReaperProjects.Items.Add("Error loading Reaper projects");
                cmbReaperProjects.Enabled = false;
                UIHelpers.ShowException($"Failed to load Reaper projects: {ex.Message}");
            }
        }
        
        private async void OnReaperProjectSelected(object? sender, EventArgs e)
        {
            if (cmbReaperProjects.SelectedIndex < 0 || cmbReaperProjects.SelectedIndex >= _openProjects.Count)
                return;
            
            _selectedProject = _openProjects[cmbReaperProjects.SelectedIndex];
            
            statusManager.UpdateStatus($"Loading tracks from {_selectedProject.Name}...");
            
            // Populate source and output track dropdowns with tracks from this project
            cmbSourceTrack.Items.Clear();
            cmbOutputTrack.Items.Clear();
            
            // Use the tracks already from the project info
            foreach (var track in _selectedProject.Tracks)
            {
                cmbSourceTrack.Items.Add(track);
                cmbOutputTrack.Items.Add(track);
            }
            
            cmbSourceTrack.Enabled = _selectedProject.Tracks.Count > 0;
            cmbOutputTrack.Enabled = _selectedProject.Tracks.Count > 0;
            
            if (_selectedProject.Tracks.Count > 0)
            {
                cmbSourceTrack.SelectedIndex = 0;
                cmbOutputTrack.SelectedIndex = 0;
            }
            
            statusManager.UpdateStatus($"Loaded {_selectedProject.Tracks.Count} tracks from {_selectedProject.Name}");
        }
        
        private async void OnProcessClick(object? sender, EventArgs e)
        {
            if (!UIHelpers.ValidateInputs(
                excelSelector.FilePath,
                _selectedProject?.FullPath ?? string.Empty,
                outputFolderSelector.FolderPath,
                cmb_VO_Text_Name.SelectedItem?.ToString(),
                cmb_VO_Audio_Name.SelectedItem?.ToString(),
                cmbSourceTrack.SelectedItem?.ToString(),
                cmbOutputTrack.SelectedItem?.ToString(),
                out string errorMessage))
            {
                UIHelpers.ShowError(errorMessage);
                return;
            }
            
            try
            {
                UIHelpers.DisableControls(btnProcess);
                
                List<string> texts = null!;
                List<string> audioFiles = null!;
                
                texts = await UIHelpers.ExecuteWithStatusAsync(
                    statusManager,
                    () => excelService.ReadTextColumnAsync(
                        excelSelector.FilePath, 
                        cmb_VO_Text_Name.SelectedItem?.ToString() ?? string.Empty
                    ),
                    "Reading VO text column...",
                    $"Found entries in column '{cmb_VO_Text_Name.SelectedItem}'"
                );
                
                audioFiles = await UIHelpers.ExecuteWithStatusAsync(
                    statusManager,
                    () => excelService.ReadTextColumnAsync(
                        excelSelector.FilePath, 
                        cmb_VO_Audio_Name.SelectedItem?.ToString() ?? string.Empty
                    ),
                    "Reading audio file names...",
                    $"Found entries in column '{cmb_VO_Audio_Name.SelectedItem}'"
                );
                
                statusManager.UpdateStatus($"Project: {_selectedProject?.Name}");
                statusManager.UpdateStatus($"Source track: {cmbSourceTrack.SelectedItem}");
                statusManager.UpdateStatus($"Output track: {cmbOutputTrack.SelectedItem}");
                statusManager.UpdateStatus($"Output folder: {outputFolderSelector.FolderPath}");
                
                UIHelpers.ShowSuccess(
                    $"Successfully loaded:\n" +
                    $"- {texts.Count} text entries from column '{cmb_VO_Text_Name.SelectedItem}'\n" +
                    $"- {audioFiles.Count} audio file names from column '{cmb_VO_Audio_Name.SelectedItem}'\n" +
                    $"- Project: {_selectedProject?.Name}\n" +
                    $"- Source track: {cmbSourceTrack.SelectedItem}\n" +
                    $"- Output track: {cmbOutputTrack.SelectedItem}\n" +
                    $"- Output folder: {outputFolderSelector.FolderPath}"
                );
            }
            catch (Exception ex)
            {
                statusManager.UpdateStatus($"Error: {ex.Message}");
                UIHelpers.ShowException($"An error occurred: {ex.Message}");
            }
            finally
            {
                UIHelpers.EnableControls(btnProcess);
            }
        }
    }
}