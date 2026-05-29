using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SmartFocus.Core;
using SmartFocus.Core.Interfaces;
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
        private readonly ISearchEngine _searchEngine = AppServices.Search;
        private CancellationTokenSource? _searchCts;

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    DebouncedSearch();
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
                if (!string.IsNullOrEmpty(_searchText))
                    DebouncedSearch();
            }
        }

        public void UpdateWindows(List<WindowInfo> windows)
        {
            AllWindows = windows;
        }

        private async void DebouncedSearch()
        {
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();
            var token = _searchCts.Token;
            try
            {
                await Task.Delay(150, token);
                if (!token.IsCancellationRequested)
                    Search();
            }
            catch (TaskCanceledException) { }
        }

        private void Search()
        {
            var previousSelection = _selectedResult;

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

            if (previousSelection != null && Results.Any(r => r.Handle == previousSelection.Handle))
                SelectedResult = Results.First(r => r.Handle == previousSelection.Handle);
            else
                SelectedResult = Results.FirstOrDefault();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
