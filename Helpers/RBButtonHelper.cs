using System;
using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ff14bot.AClasses;
using ff14bot.Forms.ugh;
using ff14bot.Interfaces;

namespace LlamaLibrary.Helpers;

public static class RbButtonHelper
{
    public static readonly ConcurrentDictionary<string, Button> Buttons = new ConcurrentDictionary<string, Button>();
    private static StackPanel? _stackPanel;

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

    public static void ChangeButtonText(string buttonName, string content)
    {
        if (!Buttons.ContainsKey(buttonName))
        {
            return;
        }

        MainWpf.current.Dispatcher.Invoke(() => { Buttons[buttonName].Content = content; });
    }

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

    public static void AddButton(IBotPlugin plugin)
    {
        AddButton($"Btn{plugin.Name.Replace(" ", "")}", plugin.Name, plugin.OnButtonPress);
    }

    public static void AddButton(BotBase botBase)
    {
        AddButton($"Btn{botBase.Name.Replace(" ", "")}", botBase.Name, botBase.OnButtonPress);
    }

    public static void RemoveButton(IBotPlugin plugin)
    {
        RemoveButton($"Btn{plugin.Name.Replace(" ", "")}");
    }

    public static void RemoveButton(BotBase botBase)
    {
        RemoveButton($"Btn{botBase.Name.Replace(" ", "")}");
    }

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