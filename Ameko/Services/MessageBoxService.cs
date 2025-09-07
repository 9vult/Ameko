// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Threading.Tasks;
using Ameko.DataModels.Sdk;
using Ameko.Views.Sdk;
using Ameko.Views.Windows;
using Holo.Scripting.Models;
using Material.Icons;

namespace Ameko.Services;

public interface IMessageBoxService
{
    /// <summary>
    /// Display a message box
    /// </summary>
    /// <param name="title">Title of the box</param>
    /// <param name="text">Body of the box</param>
    /// <param name="buttonSet">Which buttons to put on the box</param>
    /// <param name="iconKind">Icon to use</param>
    /// <returns>Which button was clicked</returns>
    Task<MessageBoxResult?> ShowAsync(
        string title,
        string text,
        MessageBoxButtons buttonSet,
        MaterialIconKind iconKind = MaterialIconKind.Info
    );

    /// <summary>
    /// Display an input box
    /// </summary>
    /// <param name="title">Title of the box</param>
    /// <param name="text">Body of the box</param>
    /// <param name="initialText">Initial text to have in the input</param>
    /// <param name="buttonSet">Which buttons to put on the box</param>
    /// <param name="iconKind">Icon to use</param>
    /// <returns>Which button was clicked and the user input</returns>
    Task<(MessageBoxResult, string)?> ShowInputAsync(
        string title,
        string text,
        string initialText,
        MessageBoxButtons buttonSet,
        MaterialIconKind iconKind = MaterialIconKind.Info
    );

    /// <summary>
    /// Show boxes for <see cref="InstallationResult"/>s
    /// </summary>
    /// <param name="result">iunstallation result</param>
    /// <returns>Button that was clicked</returns>
    Task<MessageBoxResult?> ShowAsync(InstallationResult result);
}

/// <summary>
/// Service for displaying message boxes
/// </summary>
/// <param name="mainWindow">Application main window</param>
public class MessageBoxService(MainWindow mainWindow) : IMessageBoxService
{
    public async Task<MessageBoxResult?> ShowAsync(
        string title,
        string text,
        MessageBoxButtons buttonSet,
        MaterialIconKind iconKind = MaterialIconKind.Info
    )
    {
        var box = new MessageBox(title, text, buttonSet, iconKind);
        return await box.ShowDialog<MessageBoxResult>(mainWindow);
    }

    /// <inheritdoc />
    public async Task<(MessageBoxResult, string)?> ShowInputAsync(
        string title,
        string text,
        string initialText,
        MessageBoxButtons buttonSet,
        MaterialIconKind iconKind = MaterialIconKind.Information
    )
    {
        var box = new InputBox(title, text, initialText, buttonSet, iconKind);
        var result = await box.ShowDialog<MessageBoxResult>(mainWindow);
        return (result, box.InputText);
    }

    /// <inheritdoc />
    public async Task<MessageBoxResult?> ShowAsync(InstallationResult result)
    {
        var box = new MessageBox(
            I18N.PkgMan.PkgManWindow_Title,
            result switch
            {
                InstallationResult.Success => I18N.PkgMan.PkgMan_Result_Success,
                InstallationResult.Failure => I18N.PkgMan.PkgMan_Result_Failure,
                InstallationResult.DependencyNotFound => I18N.PkgMan.PkgMan_Result_DepNotFound,
                InstallationResult.AlreadyInstalled => I18N.PkgMan.PkgMan_Result_AlreadyInstalled,
                InstallationResult.FilesystemFailure => I18N.PkgMan.PkgMan_Result_FS_Failure,
                InstallationResult.NotInstalled => I18N.PkgMan.PkgMan_Result_NotInstalled,
                InstallationResult.InvalidName => I18N.PkgMan.PkgMan_Result_InvalidName,
                InstallationResult.IsRequiredDependency => I18N.PkgMan.PkgMan_Result_IsRequiredDep,
                _ => throw new ArgumentOutOfRangeException(nameof(result)),
            },
            MessageBoxButtons.Ok,
            result switch
            {
                InstallationResult.Success => MaterialIconKind.CheckBold,
                _ => MaterialIconKind.ExclamationThick,
            }
        );
        return await box.ShowDialog<MessageBoxResult>(mainWindow);
    }
}
