// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Threading.Tasks;
using Ameko.DataModels.Sdk;
using Ameko.Views.Sdk;
using Ameko.Views.Windows;
using Holo.Scripting.Models;
using Material.Icons;
using MsBox.Avalonia;
using MsBox.Avalonia.Base;
using MsBox.Avalonia.Enums;

namespace Ameko.Services;

public interface IMessageBoxService
{
    Task<MessageBoxResult> ShowAsync(
        string title,
        string header,
        string message,
        MessageBoxButtons buttonSet,
        MaterialIconKind iconKind = MaterialIconKind.Info
    );
    IMsBox<ButtonResult> GetSuccessBox(string title, string content);
    IMsBox<ButtonResult> GetErrorBox(string title, string content);
    IMsBox<ButtonResult> GetQuestionBox(string title, string content);
    IMsBox<ButtonResult> GetInfoBox(string title, string content);
    IMsBox<ButtonResult> GetBox(InstallationResult result);
}

public class MessageBoxService(MainWindow mainWindow) : IMessageBoxService
{
    public async Task<MessageBoxResult> ShowAsync(
        string title,
        string header,
        string message,
        MessageBoxButtons buttonSet,
        MaterialIconKind iconKind = MaterialIconKind.Info
    )
    {
        var box = new MessageBox(title, header, message, buttonSet, iconKind);
        return await box.ShowDialog<MessageBoxResult>(mainWindow);
    }

    public IMsBox<ButtonResult> GetSuccessBox(string title, string content)
    {
        return MessageBoxManager.GetMessageBoxStandard(title, content, ButtonEnum.Ok, Icon.Success);
    }

    public IMsBox<ButtonResult> GetErrorBox(string title, string content)
    {
        return MessageBoxManager.GetMessageBoxStandard(title, content, ButtonEnum.Ok, Icon.Error);
    }

    public IMsBox<ButtonResult> GetQuestionBox(string title, string content)
    {
        return MessageBoxManager.GetMessageBoxStandard(
            title,
            content,
            ButtonEnum.Ok,
            Icon.Question
        );
    }

    public IMsBox<ButtonResult> GetInfoBox(string title, string content)
    {
        return MessageBoxManager.GetMessageBoxStandard(title, content, ButtonEnum.Ok, Icon.Info);
    }

    public IMsBox<ButtonResult> GetBox(InstallationResult result)
    {
        return MessageBoxManager.GetMessageBoxStandard(
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
            ButtonEnum.Ok,
            result switch
            {
                InstallationResult.Success => Icon.Success,
                _ => Icon.Error,
            }
        );
    }
}
