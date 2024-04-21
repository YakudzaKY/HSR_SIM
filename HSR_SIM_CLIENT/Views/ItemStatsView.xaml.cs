using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using HSR_SIM_CLIENT.Models;
using HSR_SIM_CLIENT.Utils;
using HSR_SIM_CLIENT.ViewModels;

namespace HSR_SIM_CLIENT.Views;

public partial class ItemStatsView : INotifyPropertyChanged
{
    public static readonly DependencyProperty GroupCaptionProperty;
    public static readonly DependencyProperty ItemStatsModelProperty;

    private ItemStatsViewModel itemStatsVm;

    static ItemStatsView()
    {
        GroupCaptionProperty = DependencyProperty.Register(nameof(GroupCaption), typeof(string),
            typeof(ItemStatsView), new FrameworkPropertyMetadata());
        ItemStatsModelProperty = DependencyProperty.Register(nameof(ItemStatsModel), typeof(ItemStatsModel),
            typeof(ItemStatsView), new FrameworkPropertyMetadata(ItemChangedCb));
    }

    public ItemStatsView()
    {
        ItemStatsModel = new ItemStatsModel();
        ItemStatsVm = new ItemStatsViewModel(ItemStatsModel);
        InitializeComponent();
    }

    public IEnumerable<string> MainStatsNames { get; } =
        new[] { string.Empty }.Concat(StatData.MainStatsUpgrades.Select(x => x.Key));

    public IEnumerable<string> SubStatsNames { get; } =
        new[] { string.Empty }.Concat(StatData.SubStatsUpgrades.Select(x => x.Key));

    public string GroupCaption
    {
        get => (string)GetValue(GroupCaptionProperty);
        set => SetValue(GroupCaptionProperty, value);
    }

    public ItemStatsModel ItemStatsModel
    {
        get => (ItemStatsModel)GetValue(ItemStatsModelProperty);
        set => SetValue(ItemStatsModelProperty, value);
    }

    public ItemStatsViewModel ItemStatsVm
    {
        get => itemStatsVm;

        private set
        {
            if (Equals(value, itemStatsVm)) return;
            itemStatsVm = value;

            OnPropertyChanged();
        }
    }


    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///     callback handler
    /// </summary>
    /// <param name="d"></param>
    /// <param name="e"></param>
    private static void ItemChangedCb(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ItemStatsView isv)
            isv.RefreshData();
    }

    /// <summary>
    /// </summary>
    public void RefreshData()
    {
        ItemStatsVm = new ItemStatsViewModel(ItemStatsModel);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void NotifyPropertyChanged(string name)
    {
        if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(name));
    }
}