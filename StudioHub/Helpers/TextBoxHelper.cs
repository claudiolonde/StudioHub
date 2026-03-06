using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace StudioHub.Helpers;

public static class TextBoxHelper {
    // Creiamo la proprietà "LeadingIcon" di tipo stringa
    public static readonly DependencyProperty LeadingIconProperty =
        DependencyProperty.RegisterAttached(
            "LeadingIcon",
            typeof(string),
            typeof(TextBoxHelper),
            new PropertyMetadata(string.Empty));

    public static string GetLeadingIcon(DependencyObject obj) {
        return (string)obj.GetValue(LeadingIconProperty);
    }

    public static void SetLeadingIcon(DependencyObject obj, string value) {
        obj.SetValue(LeadingIconProperty, value);
    }
}