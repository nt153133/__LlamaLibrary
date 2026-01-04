using System.ComponentModel;
using System.Runtime.CompilerServices;
using ff14bot.Helpers;

namespace LlamaLibrary.JsonObjects
{
    public class JsonSettings<T> : JsonSettings, INotifyPropertyChanged
        where T : JsonSettings<T>, new()
    {
        private static T? _instance;

        public JsonSettings() : base(GetSettingsFilePath($"{typeof(T).Name}.json"))
        {
        }

        public JsonSettings(string settingsFilePath) : base(settingsFilePath)
        {
        }

        public static T Instance => _instance ??= new T();

        public new event PropertyChangedEventHandler? PropertyChanged;

        protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            Save();
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}