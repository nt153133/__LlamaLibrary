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

        public static T Instance
        {
            get { return _instance ??= new T(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            Save();
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}