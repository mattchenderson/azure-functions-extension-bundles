﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Build
{
    public class Helper
    {
        public static HttpClient HttpClient => _lazyClient.Value;

        private static Lazy<HttpClient> _lazyClient = new Lazy<HttpClient>(() =>
        {
            return new HttpClient(new HttpClientHandler
            {
                MaxConnectionsPerServer = 50
            });
        });

        public static string GetLatestPackageVersion(string packageId, int majorVersion)
        {
            string url = $"https://api.nuget.org/v3-flatcontainer/{packageId.ToLower()}/index.json";
            var response = HttpClient.GetStringAsync(url).Result;
            var versionsObject = JObject.Parse(response);

            var versions = JsonConvert.DeserializeObject<IEnumerable<string>>(versionsObject["versions"].ToString());

            var nuGetVersions = versions.Select(p =>
            {
                if (NuGetVersion.TryParse(p, out NuGetVersion nuGetVersion) && nuGetVersion.Major == majorVersion)
                {
                    return nuGetVersion;
                }
                return null;
            }).Where(v => v != null && v.IsPrerelease == false);

            return nuGetVersions.OrderByDescending(p => p.Version).FirstOrDefault()?.ToString();
        }
    }
}
