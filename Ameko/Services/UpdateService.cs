// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Holo.Providers;
using Microsoft.Extensions.Logging;
using Semver;

namespace Ameko.Services;

public class UpdateService(
    IMessageService messageService,
    ILogger<UpdateService> logger,
    HttpClient client
)
{
    private const string Latest = "https://api.github.com/repos/9vult/Ameko/releases/latest";

    /// <summary>
    /// Check for updates
    /// </summary>
    /// <remarks>If an update is found, the user is notified via the <see cref="IMessageService"/>.</remarks>
    public async Task CheckForUpdates()
    {
        logger.LogInformation("Checking for updates...");
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, Latest);
            request.Headers.Add("User-Agent", "Mozilla/5.0 (compatible; Ameko)");
            var result = await client.SendAsync(request);

            if (!result.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Failed to get updates - Request returned HTTP {StatusCode}",
                    result.StatusCode
                );
                return;
            }

            var data = await JsonSerializer.DeserializeAsync<JsonNode>(
                await result.Content.ReadAsStreamAsync()
            );
            if (data is null)
            {
                logger.LogWarning("Failed to get updates - no data");
                return;
            }

            var localTag = VersionService.FullLabel;
            var remoteTag = data["tag_name"]?.GetValue<string>();
            if (string.IsNullOrEmpty(remoteTag))
                return;

            if (remoteTag.StartsWith('v'))
                remoteTag = remoteTag[1..];

            if (
                !SemVersion.TryParse(remoteTag, SemVersionStyles.AllowV, out var remote)
                || !SemVersion.TryParse(localTag, SemVersionStyles.AllowV, out var local)
            )
            {
                logger.LogInformation("No updates found");
                return;
            }

            if (remote.ComparePrecedenceTo(local) > 0)
            {
                logger.LogInformation("Update found!");
                messageService.Enqueue(
                    string.Format(I18N.Other.Message_UpdateAvailable, remote),
                    TimeSpan.FromSeconds(7)
                );
            }
        }
        catch (HttpRequestException) { } // No need to do anything here
    }
}
