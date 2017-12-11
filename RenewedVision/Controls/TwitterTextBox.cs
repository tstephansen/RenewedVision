using RenewedVision.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace RenewedVision.Controls
{
    public class TwitterTextBox : RichTextBox
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(TwitterTextBox),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnTextPropertyChanged, CoerceTextProperty, true, UpdateSourceTrigger.LostFocus));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TwitterTextBox)d).UpdateFlowDocument();
        }

        private static object CoerceTextProperty(DependencyObject d, object obj)
        {
            return obj ?? "";
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            UpdateText();
        }

        /// <summary>
        /// Gets the count of hash tags and @ symbols in the text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>System.Int32.</returns>
        private static int GetSpecialCharacterCount(string text)
        {
            var origHashcount = text.Count(x => x == '#');
            var origAtCount = text.Count(x => x == '@');
            return origHashcount + origAtCount;
        }

        /// <summary>
        /// Updates the flow document.
        /// </summary>
        public void UpdateFlowDocument()
        {
            if (_isUpdating)
                return;
            _isUpdating = true;
            Document.Blocks.Clear();
            var paragraphs = FormatParagraphs(Text);
            Document.Blocks.AddRange(paragraphs);
            _isUpdating = false;
        }

        private void UpdateText()
        {
            if (_isUpdating)
                return;
            _isUpdating = true;
            var plainText = GetPlainText(Document);
            var caratModel = new CaratPositionModel
            {
                StartPosition = CaretPosition.DocumentStart.GetOffsetToPosition(CaretPosition),
                OriginalTextLength = Text.Trim().Length,
                NewTextLength = plainText.Trim().Length,
                OriginalSpecialCharacterCount = GetSpecialCharacterCount(Text),
                NewSpecialCharacterCount = GetSpecialCharacterCount(plainText)
            };
            Text = plainText;
            Document.Blocks.Clear();
            var paragraphs = FormatParagraphs(plainText);
            Document.Blocks.AddRange(paragraphs);
            caratModel.RunCount = paragraphs.SelectMany(o => o.Inlines).OfType<Run>().Count();
            SetCaratPosition(caratModel);
            _isUpdating = false;
        }

        /// <summary>
        /// Returns the plain text contained in the TwitterTextBox.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>string</returns>
        public static string GetPlainText(FlowDocument document) => new TextRange(document.ContentStart, document.ContentEnd).Text;

        /// <summary>
        /// Formats the paragraphs.
        /// </summary>
        /// <param name="inputText">The input text.</param>
        /// <returns>List&lt;Paragraph&gt;.</returns>
        public static List<Paragraph> FormatParagraphs(string inputText)
        {
            var paragraphs = new List<Paragraph>();
            inputText = inputText.Replace("\r\n", "\n");
            var splitText = inputText.Split('\n').ToList();
            for (var i = 0; i < splitText.Count; i++)
            {
                var para = new Paragraph();
                var text = splitText[i];
                if (text.Contains('#') || text.Contains('@'))
                {
                    para = CreateFormattedParagraph(text);
                    paragraphs.Add(para);
                }
                else
                {
                    para.Inlines.Add(new Run(text));
                    paragraphs.Add(para);
                }
            }
            return paragraphs;
        }

        private static Paragraph CreateFormattedParagraph(string text)
        {
            var paragraph = new Paragraph();
            var runs = new List<Run>();
            var builder = new StringBuilder();
            var run = new Run();
            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                switch (c)
                {
                    case '#':
                        var afterHashText = text.Substring(i + 1);
                        var hashText = afterHashText.TakeWhile(x => x != '#' && x != '@' && x != ' ').Aggregate(string.Empty, (current, x) => current + x.ToString());
                        if (afterHashText.TakeWhile(x => x != ' ').Contains('@') ||
                            afterHashText.TakeWhile(x => x != ' ').Contains('#') ||
                            !afterHashText.TakeWhile(x => x != ' ').Any())
                        {
                            builder.Append(c);
                            run.Text = builder.ToString();
                        }
                        else
                        {
                            runs.Add(run);
                            run = new Run();
                            builder = new StringBuilder();
                            i += hashText.Length;
                            hashText = $"#{hashText}";
                            runs.Add(new Run
                            {
                                Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 69)),
                                Text = hashText
                            });
                        }
                        break;
                    case '@':
                        var preAtText = text.Substring(0, i);
                        var afterAtText = text.Substring(i + 1);
                        var atText = afterAtText.TakeWhile(x => x != '#' && x != '@' && x != ' ').Aggregate(string.Empty, (current, x) => current + x.ToString());
                        if (!afterAtText.TakeWhile(x => x != ' ').Contains('@') &&
                            !afterAtText.TakeWhile(x => x != ' ').Contains('#') &&
                            !afterAtText.StartsWith(" ") && (preAtText.EndsWith(" ") || i == 0))
                        {
                            runs.Add(run);
                            run = new Run();
                            builder = new StringBuilder();
                            i += atText.Length;
                            atText = $"@{atText}";
                            runs.Add(new Run
                            {
                                Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 69)),
                                Text = atText
                            });
                        }
                        else
                        {
                            builder.Append(c);
                            run.Text = builder.ToString();
                        }
                        break;
                    default:
                        builder.Append(c);
                        run.Text = builder.ToString();
                        break;
                }
            }
            if (!runs.Contains(run)) runs.Add(run);
            paragraph.Inlines.AddRange(runs);
            return paragraph;
        }

        private void SetCaratPosition(CaratPositionModel model)
        {
            if (model.RunCount > _runCounter)
            {
                try
                {
                    CaretPosition = CaretPosition.DocumentStart.GetPositionAtOffset(model.StartPosition + 2);
                    _runCounter = model.RunCount;
                }
                catch (Exception)
                {
                    CaretPosition = Document.ContentEnd;
                }
            }
            else
            {
                if (model.NewSpecialCharacterCount > model.OriginalSpecialCharacterCount)
                {
                    try
                    {
                        CaretPosition = Document.ContentStart.GetPositionAtOffset(model.StartPosition + model.PointerOffset);
                    }
                    catch (Exception)
                    {
                        CaretPosition = Document.ContentStart.GetPositionAtOffset(model.PointerOffset);
                    }
                }
                else
                {
                    CaretPosition = CaretPosition.DocumentStart.GetPositionAtOffset(model.StartPosition);
                }
            }
        }

        private int _runCounter;
        private bool _isUpdating;
    }
}