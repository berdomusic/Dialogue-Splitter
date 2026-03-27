using VO_Tool.Selectors;
using VO_Tool.Services;
using VO_Tool.Status;
using VO_Tool.UI;

namespace VO_Tool
{
    public class UIBuilder
    {
        private Form form;
        private int currentY;
        private int maxWidth;
        
        public UIBuilder(Form form, int startY = 30)
        {
            this.form = form;
            this.currentY = startY;
            this.maxWidth = 0;
            SetupFormProperties();
        }
        
        private void SetupFormProperties()
        {
            form.Text = "VO Splitter";
            form.StartPosition = FormStartPosition.CenterScreen;
            form.FormBorderStyle = FormBorderStyle.FixedSingle;
            form.MaximizeBox = false;
        }
        
        public void SetFormSize(int width, int height)
        {
            form.ClientSize = new Size(width, height);
            form.FormBorderStyle = FormBorderStyle.FixedSingle;
            form.MaximizeBox = false;
        }
        
        public void CenterForm()
        {
            form.StartPosition = FormStartPosition.CenterScreen;
        }
        
        public (Label label, ComboBox comboBox) CreateLabeledComboBox(
            string labelText, 
            int x, 
            int y, 
            string? defaultSelectedValue = null,
            int labelWidth = 130, 
            int comboBoxWidth = 200, 
            bool enabled = false,
            EventHandler? onSelectedIndexChanged = null
            )
        {
            var label = new Label
            {
                Text = labelText,
                Location = new Point(x, y),
                Size = new Size(labelWidth, 25)
            };
    
            var comboBox = new ComboBox
            {
                Location = new Point(x + labelWidth + 10, y - 3),
                Size = new Size(comboBoxWidth, 23),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Enabled = enabled
            };
    
            // If default value provided, add it temporarily until real data loads
            if (!string.IsNullOrEmpty(defaultSelectedValue))
            {
                comboBox.Items.Add(defaultSelectedValue);
                comboBox.SelectedIndex = 0;
            }
            
            // Attach event if provided
            if (onSelectedIndexChanged != null)
            {
                comboBox.SelectedIndexChanged += onSelectedIndexChanged;
            }
    
            return (label, comboBox);
        }
        
        public (Label label, FileSelector selector) AddFileSelectorWithLabel(
            string labelText, 
            string fileFilter, 
            int x = 20,
            Action<string>? onFileSelected = null)
        {
            var label = new Label
            {
                Text = labelText,
                Location = new Point(x, currentY),
                Size = new Size(120, 25)
            };
            form.Controls.Add(label);
    
            var selector = new FileSelector(labelText, fileFilter, x, currentY);
            
            if (onFileSelected != null)
            {
                selector.OnFileSelected += onFileSelected;
            }
            
            selector.AddToForm(form);
            currentY += 50;
            UpdateMaxWidth(x + 550);
    
            return (label, selector);
        }
        
        public (Label label, FolderSelector selector) AddFolderSelectorWithLabel(
            string labelText, 
            int x = 20,
            Action<string>? onFolderSelected = null)
        {
            var label = new Label
            {
                Text = labelText,
                Location = new Point(x, currentY),
                Size = new Size(120, 25)
            };
            form.Controls.Add(label);

            var selector = new FolderSelector(labelText, x, currentY);
            
            if (onFolderSelected != null)
            {
                selector.OnFolderSelected += onFolderSelected;
            }
            
            selector.AddToForm(form);
            currentY += 50;
            UpdateMaxWidth(x + 550);

            return (label, selector);
        }
        
        public Button AddButton(string text, EventHandler clickHandler, int x = 250, int width = 100, int height = 35)
        {
            var button = new Button
            {
                Text = text,
                Location = new Point(x, currentY + 10),
                Size = new Size(width, height),
                BackColor = Color.LightBlue
            };
            button.Click += clickHandler;
            form.Controls.Add(button);
            currentY += 50;
            UpdateMaxWidth(x + width);
            
            return button;
        }
        
        public TrackBar AddSimilaritySlider(
            int x = 20, 
            int min = 10, 
            int max = 100, 
            int defaultValue = 75,
            EventHandler? onScroll = null)
        {
            var label = new Label
            {
                Text = "Similarity Threshold:",
                Location = new Point(x, currentY),
                Size = new Size(130, 25)
            };
            form.Controls.Add(label);
            
            int currentValue = Settings.Settings.Get().LastSimilarityThreshold;
            
            var trackBar = new TrackBar
            {
                Location = new Point(x + 140, currentY - 5),
                Size = new Size(200, 25),
                Minimum = min,
                Maximum = max,
                Value = currentValue,
                TickFrequency = 10,
                TickStyle = TickStyle.BottomRight
            };
            
            if (onScroll != null)
            {
                trackBar.Scroll += onScroll;
            }
            
            form.Controls.Add(trackBar);
            
            var valueLabel = new Label
            {
                Text = $"{currentValue / 100.0:F2}",
                Location = new Point(x + 350, currentY),
                Size = new Size(50, 25),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            form.Controls.Add(valueLabel);
            
            trackBar.Scroll += (s, e) =>
            {
                valueLabel.Text = $"{trackBar.Value / 100.0:F2}";
            };
            
            currentY += 45;
            UpdateMaxWidth(x + 400);
            
            return trackBar;
        }
        
        public StatusManager AddStatusBar(int x = 20, int width = 560, int height = 30)
        {
            var status = new StatusManager(x, currentY, width, height);
            status.AddToForm(form);
            
            int bottomPadding = 50;
            int formHeight = currentY + height + bottomPadding;
            int formWidth = maxWidth + 40;
            
            form.ClientSize = new Size(formWidth, formHeight);
            
            return status;
        }
        
        public (Label label, ComboBox comboBox) AddModelSelector(
            int x = 20, 
            WhisperModel? selectedModel = null,
            EventHandler? onSelectedIndexChanged = null)
        {
            int modelLabelX = 150;
            var label = new Label
            {
                
                Text = "Whisper Model:",
                Location = new Point(x, currentY),
                Size = new Size(modelLabelX, 25)
            };

            var comboBox = new ComboBox
            {
                Location = new Point(x + modelLabelX, currentY - 3),
                Size = new Size(120, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Populate with installed models
            var installedModels = WhisperModelExtensions.GetInstalledModels();
            foreach (var model in installedModels)
            {
                comboBox.Items.Add(model);
            }

            // Select first by default
            if (comboBox.Items.Count > 0)
            {
                comboBox.SelectedIndex = 0;
            }

            // Restore last selected model if provided
            if (selectedModel.HasValue && comboBox.Items.Contains(selectedModel.Value))
            {
                comboBox.SelectedItem = selectedModel.Value;
            }
            
            // Attach event if provided
            if (onSelectedIndexChanged != null)
            {
                comboBox.SelectedIndexChanged += onSelectedIndexChanged;
            }

            form.Controls.Add(label);
            form.Controls.Add(comboBox);
            currentY += 35;
            UpdateMaxWidth(x + 240);

            return (label, comboBox);
        }
        
        public (Label label, ComboBox comboBox) AddLanguageSelector(
            int x = 20, 
            WhisperLanguage? selectedLanguage = null,
            EventHandler? onSelectedIndexChanged = null)
        {
            var label = new Label
            {
                Text = "Language:",
                Location = new Point(x, currentY),
                Size = new Size(100, 25)
            };

            var comboBox = new ComboBox
            {
                Location = new Point(x + 110, currentY - 3),
                Size = new Size(150, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Add Auto option first
            comboBox.Items.Add(WhisperLanguage.Auto);
    
            // Add all supported languages
            var languages = WhisperLanguageExtensions.GetSupportedLanguages();
            foreach (var language in languages)
            {
                comboBox.Items.Add(language);
            }

            // Select default language
            if (selectedLanguage.HasValue)
            {
                for (int i = 0; i < comboBox.Items.Count; i++)
                {
                    if (comboBox.Items[i] is WhisperLanguage lang && lang == selectedLanguage.Value)
                    {
                        comboBox.SelectedIndex = i;
                        break;
                    }
                }
            }
    
            // If nothing selected, default to English
            if (comboBox.SelectedIndex == -1)
            {
                for (int i = 0; i < comboBox.Items.Count; i++)
                {
                    if (comboBox.Items[i] is WhisperLanguage lang && lang == WhisperLanguage.English)
                    {
                        comboBox.SelectedIndex = i;
                        break;
                    }
                }
            }
            
            // Attach event if provided
            if (onSelectedIndexChanged != null)
            {
                comboBox.SelectedIndexChanged += onSelectedIndexChanged;
            }

            form.Controls.Add(label);
            form.Controls.Add(comboBox);
            currentY += 35;
            UpdateMaxWidth(x + 270);

            return (label, comboBox);
        }
        
        public CheckBox AddCheckBox(
            string text, 
            int x = 20, 
            bool defaultChecked = false,
            EventHandler? onCheckedChanged = null)
        {
            var checkBox = new CheckBox
            {
                Text = text,
                Location = new Point(x, currentY),
                Size = new Size(200, 25),
                Checked = defaultChecked
            };
            
            if (onCheckedChanged != null)
            {
                checkBox.CheckedChanged += onCheckedChanged;
            }
            
            form.Controls.Add(checkBox);
            currentY += 35;
            UpdateMaxWidth(x + 200);
    
            return checkBox;
        }
        
        public void SetupFileDrop(Control control, 
            Action<string> onExcelFile, 
            Action<string> onAudioFile,
            Action<string>? onOtherFile = null)
        {
            control.AllowDrop = true;
            control.DragEnter += (s, e) => UIControls.OnDragEnter(s, e);
            control.DragDrop += (s, e) => UIControls.OnFileDrop(s, e, onExcelFile, onAudioFile, onOtherFile);
        }
        
        public static void PopulateComboBox<T>(ComboBox comboBox, IEnumerable<T> items, bool selectFirst = true)
        {
            comboBox.Items.Clear();
            foreach (var item in items)
            {
                if (item != null)
                {
                    comboBox.Items.Add(item);
                }
            }
            if (selectFirst && comboBox.Items.Count > 0)
            {
                comboBox.SelectedIndex = 0;
            }
        }
        
        public void UpdateCurrentY(int newY)
        {
            currentY = newY;
        }
        
        private void UpdateMaxWidth(int width)
        {
            if (width > maxWidth)
                maxWidth = width;
        }
        
        public int GetCurrentY() => currentY;
    }
}