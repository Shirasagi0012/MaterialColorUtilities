using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using MaterialColorUtilities.Avalonia;
using MaterialColorUtilities.Avalonia.Tokens;
using MaterialColorUtilities.Gallery.ViewModels;

namespace MaterialColorUtilities.Gallery.Controls;

public partial class CustomColorEditor : UserControl
{
    private readonly MaterialColorResources _resources = new();
    private readonly List<IDisposable> _roleBindings = [];
    private CustomColorEntryViewModel? _entry;
    private bool _isAttached;

    public CustomColorEditor()
    {
        InitializeComponent();
        RolePreview.Resources.MergedDictionaries.Add(_resources);
        DataContextChanged += (_, _) => SetEntry();
        AttachedToVisualTree += (_, _) =>
        {
            _isAttached = true;
            SetEntry();
        };
        DetachedFromVisualTree += (_, _) =>
        {
            _isAttached = false;
            if (_entry is not null) _entry.PropertyChanged -= EntryChanged;
            _entry = null;
            _resources.Scheme = null;
            ClearRoleBindings();
        };
    }

    private void SetEntry()
    {
        if (_entry is not null) _entry.PropertyChanged -= EntryChanged;
        _entry = DataContext as CustomColorEntryViewModel;
        if (_isAttached && _entry is not null) _entry.PropertyChanged += EntryChanged;
        _resources.Scheme = _entry?.Scheme;
        BindRoles();
    }

    private void EntryChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CustomColorEntryViewModel.Scheme)) _resources.Scheme = _entry?.Scheme;
        if (e.PropertyName == nameof(CustomColorEntryViewModel.ResourceName)) BindRoles();
    }

    private void BindRoles()
    {
        ClearRoleBindings();
        if (_entry is null) return;
        BindRole(ColorBand, ColorLabel, CustomColorRole.Color, CustomColorRole.OnColor);
        BindRole(OnColorBand, OnColorLabel, CustomColorRole.OnColor, CustomColorRole.Color);
        BindRole(ContainerBand, ContainerLabel, CustomColorRole.Container, CustomColorRole.OnContainer);
        BindRole(OnContainerBand, OnContainerLabel, CustomColorRole.OnContainer, CustomColorRole.Container);
    }

    private void ClearRoleBindings()
    {
        foreach (var binding in _roleBindings) binding.Dispose();
        _roleBindings.Clear();
    }

    private void BindRole(Border band, TextBlock label, CustomColorRole background, CustomColorRole foreground)
    {
        _roleBindings.Add(band.Bind(Border.BackgroundProperty,
            new DynamicResourceExtension(new CustomColorKey(_entry!.ResourceName, background))));
        _roleBindings.Add(label.Bind(TextBlock.ForegroundProperty,
            new DynamicResourceExtension(new CustomColorKey(_entry.ResourceName, foreground))));
    }
}
