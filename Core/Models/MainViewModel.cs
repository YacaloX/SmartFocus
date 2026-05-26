using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SmartFocus.Core;
using SmartFocus.Models;

namespace SmartFocus.Models
{
    public class SearchResult
    {
        public string DisplayName { get; set; } = "";
        public string WindowTitle { get; set; } = "";
        public IntPtr Handle { get; set; }
        public string ProcessName { get; set; } = "";
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        private string _searchText = "";
        private SearchResult? _selectedResult;
        private List<WindowInfo> _allWindows = new();
        private readonly SearchEngine _searchEngine = new();

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    Search();
                }
            }
        }

        public ObservableCollection<SearchResult> Results { get; } = new();

        public SearchResult? SelectedResult
        {
            get => _selectedResult;
            set
            {
                if (_selectedResult != value)
                {
                    _selectedResult = value;
                    OnPropertyChanged();
                }
            }
        }

        public List<WindowInfo> AllWindows
        {
            get => _allWindows;
            set
            {
                _allWindows = value;
                Search();
            }
        }

        public void UpdateWindows(List<WindowInfo> windows)
        {
            AllWindows = windows;
        }

        private void Search()
        {
            if (_searchEngine == null || _allWindows == null) return;
            var results = _searchEngine.Search(_searchText, _allWindows);
            Results.Clear();
            foreach (var w in results)
            {
                Results.Add(new SearchResult
                {
                    DisplayName = w.ProcessName ?? w.Title,
                    WindowTitle = w.Title,
                    Handle = w.Handle,
                    ProcessName = w.ProcessName ?? ""
                });
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}