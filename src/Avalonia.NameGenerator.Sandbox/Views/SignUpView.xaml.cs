﻿using System.Reactive.Disposables;
using Avalonia.NameGenerator.Sandbox.ViewModels;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Avalonia.NameGenerator.Sandbox.Views;

/// <summary>
/// This is a sample view class with typed x:Name references generated using
/// .NET 5 source generators. The class has to be partial because x:Name
/// references are living in a separate partial class file. See also:
/// https://devblogs.microsoft.com/dotnet/new-c-source-generator-samples/
/// </summary>
public partial class SignUpView : ReactiveWindow<SignUpViewModel>
{
    public SignUpView()
    {
        // The InitializeComponent method is also generated automatically
        // and lives in the autogenerated part of the partial class.
        InitializeComponent();
        this.WhenActivated(disposables =>
        {
            this.WhenAnyValue(view => view.ViewModel)
                .BindTo(this, view => view.SignUpControl.ViewModel)
                .DisposeWith(disposables);
        });
    }
}