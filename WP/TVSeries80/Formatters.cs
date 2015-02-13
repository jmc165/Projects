using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Net;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows;
using Microsoft.Phone.Shell;

namespace TVSeries80
{
    public class HighlightSearchQuery : DependencyObject
    {
        public static string GetFormattedText(DependencyObject obj)
        {
            return (string)obj.GetValue(FormattedTextProperty);
        }

        public static void SetFormattedText(DependencyObject obj, string value)
        {
            obj.SetValue(FormattedTextProperty, value);
        }

        public static readonly DependencyProperty FormattedTextProperty =
            DependencyProperty.RegisterAttached("FormattedText",
                                                typeof(string),
                                                typeof(HighlightSearchQuery),
                                                new PropertyMetadata("", FormattedTextChanged));


        private static void FormattedTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)

        {
            TextBlock tb = sender as TextBlock;
            if (tb != null)
            {
                string newValue = e.NewValue as string;
                string query = App.ViewModel.CurrentQuery;
                string[] mwords = query.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string[] words = newValue.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                tb.Inlines.Clear();
                foreach (string w in words)
                {
                    bool match = false;
                    foreach (string mw in mwords)
                    {
                        if (w.IndexOf(mw, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        {
                            match = true;
                            break;
                        }
                    }
                    if (!match)
                    {
                        tb.Inlines.Add(w + " ");
                    }
                    else
                    {
                        tb.Inlines.Add(new Run() { Text = w, FontWeight = FontWeights.ExtraBold });
                        tb.Inlines.Add(new Run() { Text = " ", FontWeight = FontWeights.Normal });
                    }
                }
             }
        }
    }

    public class HypertextRichTextBox : RichTextBox
    {
        private const string UrlPattern = @"(http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?";
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(HypertextRichTextBox), new PropertyMetadata(default(string), TextPropertyChanged));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static void TextPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var richTextBox = (HypertextRichTextBox)dependencyObject;
            var text = (string)dependencyPropertyChangedEventArgs.NewValue;
            int textPosition = 0;
            var paragraph = new Paragraph();

            var urlMatches = Regex.Matches(text, UrlPattern);
            foreach (Match urlMatch in urlMatches)
            {
                int urlOccurrenceIndex = text.IndexOf(urlMatch.Value, textPosition, StringComparison.Ordinal);

                if (urlOccurrenceIndex == 0)
                {
                    var hyperlink = new Hyperlink
                    {
                        NavigateUri = new Uri(urlMatch.Value),
                        TargetName = "_blank",
                        //Foreground = Application.Current.Resources["PhoneAccentBrush"] as Brush
                    };
                    hyperlink.Inlines.Add(urlMatch.Value);
                    paragraph.Inlines.Add(hyperlink);
                    textPosition += urlMatch.Value.Length;
                }
                else
                {
                    paragraph.Inlines.Add(text.Substring(textPosition, urlOccurrenceIndex - textPosition));
                    textPosition += urlOccurrenceIndex - textPosition;
                    var hyperlink = new Hyperlink
                    {
                        NavigateUri = new Uri(urlMatch.Value),
                        TargetName = "_blank",
                        //Foreground = Application.Current.Resources["PhoneAccentBrush"] as Brush
                    };
                    //hyperlink.Click += OnHyperlinkClick;
                    hyperlink.Inlines.Add(urlMatch.Value);
                    paragraph.Inlines.Add(hyperlink);
                    textPosition += urlMatch.Value.Length;
                }
            }

            if (urlMatches.Count == 0)
            {
                paragraph.Inlines.Add(text);
            }

            richTextBox.Blocks.Add(paragraph);
        }

        static void OnHyperlinkClick(object sender, RoutedEventArgs e)
        {
            Hyperlink hl = sender as Hyperlink;
            if (hl != null)
            {
                Page page = App.ViewModel.CurrentPage;
                if (page != null)
                {
                    Uri uri = new Uri(hl.TargetName);
                    object setting = App.ViewModel.LoadSettingsKey(App.INTERNAL_BROWSER_KEY);
                    if (setting != null)
                    {
                        if ((int)setting == 1)
                            page.NavigationService.Navigate(new Uri(String.Format("/WebBrowserPage.xaml?URI={0}", uri.AbsoluteUri), UriKind.Relative));
                        else
                            Utilities.Utilities.Instance.ExplorerLaunch(uri);
                    }
                }
            }
        }
    }

    public class RssTextTrimmer : IValueConverter
    {
        // Clean up text fields from each SyndicationItem. 
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            int maxLength = 1000;
            int strLength = 0;
            string fixedString = "";

            // Remove HTML tags and newline characters from the text, and decode HTML encoded characters. 
            // This is a basic method. Additional code would be needed to more thoroughly  
            // remove certain elements, such as embedded Javascript. 

            // Remove HTML tags. 
            fixedString = Regex.Replace(value.ToString(), "<[^>]+>", string.Empty);

            // Remove newline characters.
            fixedString = fixedString.Replace("\r", "").Replace("\n", "");

            // Remove encoded HTML characters.
            fixedString = HttpUtility.HtmlDecode(fixedString);

            strLength = fixedString.ToString().Length;

            // Some feed management tools include an image tag in the Description field of an RSS feed, 
            // so even if the Description field (and thus, the Summary property) is not populated, it could still contain HTML. 
            // Due to this, after we strip tags from the string, we should return null if there is nothing left in the resulting string. 
            if (strLength == 0)
            {
                return null;
            }

            // Truncate the text if it is too long. 
            else if (strLength >= maxLength)
            {
                fixedString = fixedString.Substring(0, maxLength);

                // Unless we take the next step, the string truncation could occur in the middle of a word.
                // Using LastIndexOf we can find the last space character in the string and truncate there. 
                fixedString = fixedString.Substring(0, fixedString.LastIndexOf(" "));
            }

            //fixedString += "...";

            return fixedString.Trim();
        }

        // This code sample does not use TwoWay binding, so we do not need to flesh out ConvertBack.  
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
