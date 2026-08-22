using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace FinancePro.UI.Common;

public static class DataGridPager
{
    private static readonly DependencyProperty StateProperty = DependencyProperty.RegisterAttached(
        "State", typeof(PagerState), typeof(DataGridPager), new PropertyMetadata(null));

    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled", typeof(bool), typeof(DataGridPager), new PropertyMetadata(false, OnIsEnabledChanged));

    public static readonly DependencyProperty PageSizeProperty = DependencyProperty.RegisterAttached(
        "PageSize", typeof(int), typeof(DataGridPager), new PropertyMetadata(10, OnPageSizeChanged));

    private static readonly DependencyPropertyKey PageNumberPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
        "PageNumber", typeof(int), typeof(DataGridPager), new PropertyMetadata(1));
    public static readonly DependencyProperty PageNumberProperty = PageNumberPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey PageCountPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
        "PageCount", typeof(int), typeof(DataGridPager), new PropertyMetadata(1));
    public static readonly DependencyProperty PageCountProperty = PageCountPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey StatusTextPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
        "StatusText", typeof(string), typeof(DataGridPager), new PropertyMetadata("0 registo(s) carregado(s)"));
    public static readonly DependencyProperty StatusTextProperty = StatusTextPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey PreviousCommandPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
        "PreviousCommand", typeof(ICommand), typeof(DataGridPager), new PropertyMetadata(null));
    public static readonly DependencyProperty PreviousCommandProperty = PreviousCommandPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey NextCommandPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
        "NextCommand", typeof(ICommand), typeof(DataGridPager), new PropertyMetadata(null));
    public static readonly DependencyProperty NextCommandProperty = NextCommandPropertyKey.DependencyProperty;

    public static void SetIsEnabled(DependencyObject element, bool value) => element.SetValue(IsEnabledProperty, value);
    public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);
    public static void SetPageSize(DependencyObject element, int value) => element.SetValue(PageSizeProperty, value);
    public static int GetPageSize(DependencyObject element) => (int)element.GetValue(PageSizeProperty);
    public static int GetPageNumber(DependencyObject element) => (int)element.GetValue(PageNumberProperty);
    public static int GetPageCount(DependencyObject element) => (int)element.GetValue(PageCountProperty);
    public static string GetStatusText(DependencyObject element) => (string)element.GetValue(StatusTextProperty);
    public static ICommand? GetPreviousCommand(DependencyObject element) => (ICommand?)element.GetValue(PreviousCommandProperty);
    public static ICommand? GetNextCommand(DependencyObject element) => (ICommand?)element.GetValue(NextCommandProperty);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGrid grid) return;

        if ((bool)e.NewValue)
        {
            if (grid.GetValue(StateProperty) is PagerState) return;
            var state = new PagerState(grid);
            grid.SetValue(StateProperty, state);
            state.Start();
        }
        else if (grid.GetValue(StateProperty) is PagerState state)
        {
            state.Stop();
            grid.ClearValue(StateProperty);
        }
    }

    private static void OnPageSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d.GetValue(StateProperty) is PagerState state) state.Reset();
    }

    private sealed class PagerState
    {
        private readonly DataGrid _grid;
        private readonly DependencyPropertyDescriptor? _itemsSourceDescriptor;
        private ICollectionView? _view;
        private IEnumerable? _source;
        private INotifyCollectionChanged? _observableSource;
        private Predicate<object>? _originalFilter;
        private Predicate<object>? _pageFilter;
        private int _page = 1;
        private int _pageCount = 1;
        private readonly PagerCommand _previousCommand;
        private readonly PagerCommand _nextCommand;

        public PagerState(DataGrid grid)
        {
            _grid = grid;
            _previousCommand = new PagerCommand(() => _page > 1, () => ChangePage(_page - 1));
            _nextCommand = new PagerCommand(() => _page < _pageCount, () => ChangePage(_page + 1));
            _itemsSourceDescriptor = DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(DataGrid));
        }

        public void Start()
        {
            _grid.SetValue(PreviousCommandPropertyKey, _previousCommand);
            _grid.SetValue(NextCommandPropertyKey, _nextCommand);
            _grid.Loaded += OnLoaded;
            _grid.Unloaded += OnUnloaded;
            _itemsSourceDescriptor?.AddValueChanged(_grid, OnItemsSourceChanged);
            if (_grid.IsLoaded) AttachSource();
        }

        public void Stop()
        {
            _grid.Loaded -= OnLoaded;
            _grid.Unloaded -= OnUnloaded;
            _itemsSourceDescriptor?.RemoveValueChanged(_grid, OnItemsSourceChanged);
            DetachSource();
            _grid.ClearValue(PreviousCommandPropertyKey);
            _grid.ClearValue(NextCommandPropertyKey);
        }

        public void Reset()
        {
            _page = 1;
            UpdateStateAndRefresh();
        }

        private void OnLoaded(object sender, RoutedEventArgs e) => AttachSource();
        private void OnUnloaded(object sender, RoutedEventArgs e) => DetachSource();
        private void OnItemsSourceChanged(object? sender, EventArgs e) => AttachSource();
        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => UpdateStateAndRefresh();

        private void AttachSource()
        {
            DetachSource();
            _source = _grid.ItemsSource;
            if (_source is null)
            {
                UpdateState();
                return;
            }

            _view = CollectionViewSource.GetDefaultView(_source);
            if (_view.CanFilter)
            {
                _originalFilter = _view.Filter;
                _pageFilter = IsItemOnCurrentPage;
                _view.Filter = _pageFilter;
            }

            _observableSource = _source as INotifyCollectionChanged;
            if (_observableSource is not null) _observableSource.CollectionChanged += OnCollectionChanged;
            _page = 1;
            UpdateStateAndRefresh();
        }

        private void DetachSource()
        {
            if (_observableSource is not null) _observableSource.CollectionChanged -= OnCollectionChanged;
            _observableSource = null;

            if (_view is not null && ReferenceEquals(_view.Filter, _pageFilter))
            {
                _view.Filter = _originalFilter;
                _view.Refresh();
            }

            _view = null;
            _source = null;
            _originalFilter = null;
            _pageFilter = null;
        }

        private bool IsItemOnCurrentPage(object item)
        {
            if (_source is null) return false;
            var pageSize = Math.Max(1, GetPageSize(_grid));
            var first = (_page - 1) * pageSize;
            var last = first + pageSize;
            var eligibleIndex = 0;

            foreach (var candidate in _source)
            {
                if (candidate is null || (_originalFilter is not null && !_originalFilter(candidate))) continue;
                if (ReferenceEquals(candidate, item) || Equals(candidate, item))
                    return eligibleIndex >= first && eligibleIndex < last;
                eligibleIndex++;
            }
            return false;
        }

        private int CountEligibleItems()
        {
            if (_source is null) return 0;
            var count = 0;
            foreach (var item in _source)
            {
                if (item is not null && (_originalFilter is null || _originalFilter(item))) count++;
            }
            return count;
        }

        private void ChangePage(int newPage)
        {
            _page = Math.Clamp(newPage, 1, _pageCount);
            UpdateStateAndRefresh();
        }

        private void UpdateStateAndRefresh()
        {
            UpdateState();
            _view?.Refresh();
        }

        private void UpdateState()
        {
            var total = CountEligibleItems();
            var pageSize = Math.Max(1, GetPageSize(_grid));
            _pageCount = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
            _page = Math.Clamp(_page, 1, _pageCount);

            _grid.SetValue(PageNumberPropertyKey, _page);
            _grid.SetValue(PageCountPropertyKey, _pageCount);
            _grid.SetValue(StatusTextPropertyKey, $"{total} registo(s) carregado(s)");
            _previousCommand.NotifyCanExecuteChanged();
            _nextCommand.NotifyCanExecuteChanged();
        }
    }

    private sealed class PagerCommand(Func<bool> canExecute, Action execute) : ICommand
    {
        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object? parameter) => canExecute();
        public void Execute(object? parameter) => execute();
        public void NotifyCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
