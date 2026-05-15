using System;
using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ff14bot.AClasses;
using ff14bot.Forms.ugh;
using ff14bot.Interfaces;

namespace LlamaLibrary.Helpers;

/// <summary>
/// Manages custom buttons injected into the RebornBuddy main WPF window UI.
/// Allows plugins and botbases to add, remove, and update labeled action buttons
/// in a shared vertical panel next to the bot selector dropdown.
/// </summary>
public static class RbButtonHelper
{
    /// <summary>All currently registered buttons, keyed by their unique name identifier.</summary>
    public static readonly ConcurrentDictionary<string, Button> Buttons = new ConcurrentDictionary<string, Button>(StringComparer.Ordinal);
    private static StackPanel? _stackPanel;

    /// <summary>
    /// Adds a new button to the RebornBuddy UI with the specified name, label, and click handler.
    /// Creates the container panel on first use. Does nothing if a button with the same name already exists.
    /// </summary>
    /// <param name="name">Unique key identifying this button.</param>
    /// <param name="content">Visible label text on the button.</param>
    /// <param name="handler">Action to invoke when the button is clicked.</param>
    public static void AddButton(string name, string content, Action handler)
    {
        MainWpf.current.Dispatcher.Invoke(() =>
        {
            if (_stackPanel == null)
            {
                _stackPanel = GenerateStackPanel();
                ((MainWpf.current.FindName("BotBox") as ComboBox)?.Parent as Grid)?.Children.Add(_stackPanel);
            }

            if (Buttons.ContainsKey(name))
            {
                return;
            }

            var button = new Button
            {
                Name = name.Replace(" ", ""),
                Content = content,
                Margin = new Thickness(0, 0, 0, 5),
                Width = 129,
                Height = 18,
                Visibility = Visibility.Visible,
                IsEnabled = true,
            };
            button.Click += (_, _) => handler.Invoke();
            Buttons.TryAdd(name, button);
            ff14bot.Helpers.Logging.WriteDiagnostic($"Adding Button: {name.Replace("Btn", "")}");
            _stackPanel.Children.Add(button);
        });
    }

    /// <summary>Updates the label text of an existing button.</summary>
    /// <param name="buttonName">The unique name of the button to update.</param>
    /// <param name="content">New label text to display.</param>
    public static void ChangeButtonText(string buttonName, string content)
    {
        if (!Buttons.ContainsKey(buttonName))
        {
            return;
        }

        MainWpf.current.Dispatcher.Invoke(() => { Buttons[buttonName].Content = content; });
    }

    /// <summary>Updates the label text, background color, and foreground color of an existing button.</summary>
    /// <param name="buttonName">The unique name of the button to update.</param>
    /// <param name="content">New label text.</param>
    /// <param name="backgroundColor">New background color for the button.</param>
    /// <param name="foregroundColor">New foreground (text) color for the button.</param>
    public static void ChangeButtonText(string buttonName, string content, Color backgroundColor, Color foregroundColor)
    {
        if (!Buttons.ContainsKey(buttonName))
        {
            return;
        }

        MainWpf.current.Dispatcher.Invoke(() =>
        {
            Buttons[buttonName].Content = content;
            Buttons[buttonName].Background = new SolidColorBrush(backgroundColor);
            Buttons[buttonName].Foreground = new SolidColorBrush(foregroundColor);
        });
    }

    /// <summary>Adds a button for an <see cref="IBotPlugin"/>, using its name as both key and label.</summary>
    /// <param name="plugin">The plugin to register a button for.</param>
    public static void AddButton(IBotPlugin plugin)
    {
        AddButton($"Btn{plugin.Name.Replace(" ", "")}", plugin.Name, plugin.OnButtonPress);
    }

    /// <summary>Adds a button for a <see cref="BotBase"/>, using its name as both key and label.</summary>
    /// <param name="botBase">The botbase to register a button for.</param>
    public static void AddButton(BotBase botBase)
    {
        AddButton($"Btn{botBase.Name.Replace(" ", "")}", botBase.Name, botBase.OnButtonPress);
    }

    /// <summary>Removes the button registered for the given <see cref="IBotPlugin"/>.</summary>
    /// <param name="plugin">The plugin whose button should be removed.</param>
    public static void RemoveButton(IBotPlugin plugin)
    {
        RemoveButton($"Btn{plugin.Name.Replace(" ", "")}");
    }

    /// <summary>Removes the button registered for the given <see cref="BotBase"/>.</summary>
    /// <param name="botBase">The botbase whose button should be removed.</param>
    public static void RemoveButton(BotBase botBase)
    {
        RemoveButton($"Btn{botBase.Name.Replace(" ", "")}");
    }

    /// <summary>Removes the button with the specified unique name from the UI panel.</summary>
    /// <param name="name">The unique key of the button to remove.</param>
    public static void RemoveButton(string name)
    {
        if (!Buttons.ContainsKey(name))
        {
            return;
        }

        if (_stackPanel == null)
        {
            return;
        }

        ff14bot.Helpers.Logging.WriteDiagnostic($"Removing Button: {name.Replace("Btn", "")}");
        MainWpf.current.Dispatcher.Invoke(() =>
        {
            _stackPanel.Children.Remove(Buttons[name]);
            Buttons.TryRemove(name, out _);
        });
    }

    private static StackPanel GenerateStackPanel()
    {
        var y = 200;
        if (MainWpf.current.FindName("BotBaseButton") is Button button)
        {
            y = (int)button.Margin.Top + (int)button.Height + 5;
        }

        return new StackPanel
        {
            Name = "CustomButtonsStack",
            Orientation = Orientation.Vertical,
            Margin = new Thickness(0, y, 10, 0),
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Right,
            Width = 129,
            Visibility = Visibility.Visible,
            IsEnabled = true,
        };
    }
}