// SPDX-License-Identifier: GPL-3.0-only

using System;
using Holo.Scripting.Models;
using MsBox.Avalonia;
using MsBox.Avalonia.Base;
using MsBox.Avalonia.Enums;

namespace Ameko.Services;

public interface IMessageBoxService
{
    IMsBox<ButtonResult> GetSuccessBox(string title, string content);
    IMsBox<ButtonResult> GetErrorBox(string title, string content);
    IMsBox<ButtonResult> GetQuestionBox(string title, string content);
    IMsBox<ButtonResult> GetInfoBox(string title, string content);
    IMsBox<ButtonResult> GetBox(InstallationResult result);
}

public class MessageBoxService : IMessageBoxService
{
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
            I18N.Resources.DepCtrlWindow_Title,
            result switch
            {
                InstallationResult.Success => I18N.Resources.DepCtrl_Result_Success,
                InstallationResult.Failure => I18N.Resources.DepCtrl_Result_Failure,
                InstallationResult.DependencyNotFound => I18N.Resources.DepCtrl_Result_DepNotFound,
                InstallationResult.AlreadyInstalled =>
                    I18N.Resources.DepCtrl_Result_AlreadyInstalled,
                InstallationResult.FilesystemFailure => I18N.Resources.DepCtrl_Result_FS_Failure,
                InstallationResult.NotInstalled => I18N.Resources.DepCtrl_Result_NotInstalled,
                InstallationResult.IsRequiredDependency =>
                    I18N.Resources.DepCtrl_Result_IsRequiredDep,
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
