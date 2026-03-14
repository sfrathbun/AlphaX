using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Net.NetworkInformation;
using System.Linq;
using AgentX.Models;
using NetworkInterface = AgentX.Models.NetworkInterface;

namespace AgentX.Services
{
    public class SystemInfoCollector
    {
        private readonly ILogger<SystemInfoCollector> _logger;

        public SystemInfoCollector(ILogger<SystemInfoCollector> logger = null)
        {
            _logger = logger;
        }

        public EndpointData CollectSystemInfo()
        {
            LogInfo("Starting system information collection");

            var endpointData = new EndpointData
            {
                Hostname = GetHostname(),
                OperatingSystem = GetOperatingSystem(),
                OSVersion = GetOSVersion(),
                OSBuildNumber = GetOSBuildNumber(),
                CurrentUser = GetCurrentUser(),
                NetworkInterfaces = GetNetworkInterfaces(),
                ProcessorInfo = GetProcessorInfo(),
                TotalMemory = GetTotalMemory(),
                LastBootTime = GetLastBootTime(),
                InstalledSoftware = GetInstalledSoftware()
            };

            LogInfo("System information collection completed");
            return endpointData;
        }

        private string GetHostname()
        {
            try
            {
                return Environment.MachineName;
            }
            catch (Exception ex)
            {
                LogError("Error getting hostname", ex);
                return "Unknown";
            }
        }

        private string GetOperatingSystem()
        {
            try
            {
                var osVersion = Environment.OSVersion;
                var platformId = osVersion.Platform;

                return platformId switch
                {
                    PlatformID.Win32NT => "Windows",
                    PlatformID.Unix => "Linux",
                    PlatformID.MacOSX => "macOS",
                    _ => platformId.ToString()
                };
            }
            catch (Exception ex)
            {
                LogError("Error getting operating system", ex);
                return "Unknown";
            }
        }

        private string GetOSVersion()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        var caption = obj["Caption"]?.ToString();
                        if (!string.IsNullOrEmpty(caption))
                        {
                            LogInfo($"OS Caption: {caption}");
                            return caption;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("Error getting OS version from WMI", ex);
            }

            // Fallback to environment version
            try
            {
                return Environment.OSVersion.VersionString;
            }
            catch (Exception ex)
            {
                LogError("Error getting OS version fallback", ex);
                return "Unknown";
            }
        }

        private string GetOSBuildNumber()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT BuildNumber FROM Win32_OperatingSystem"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        var buildNumber = obj["BuildNumber"]?.ToString();
                        if (!string.IsNullOrEmpty(buildNumber))
                        {
                            LogInfo($"OS Build: {buildNumber}");
                            return buildNumber;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("Error getting OS build number", ex);
            }

            return "Unknown";
        }

        private string GetCurrentUser()
        {
            try
            {
                return Environment.UserName;
            }
            catch (Exception ex)
            {
                LogError("Error getting hostname", ex);
                return "Unknown";
            }
        }

        private List<NetworkInterface> GetNetworkInterfaces()
        {
            var interfaces = new List<NetworkInterface>();

            try
            {
                var nics = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
                LogInfo($"Found {nics.Length} network interfaces");

                foreach (var nic in nics)
                {
                    try
                    {
                        // Skip loopback and tunnel adapters
                        if (nic.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                            nic.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                        {
                            continue;
                        }

                        var iface = new NetworkInterface
                        {
                            Name = nic.Name,
                            MacAddress = nic.GetPhysicalAddress().ToString(),
                            Status = nic.OperationalStatus.ToString(),
                            IpAddresses = new List<string>()
                        };

                        // Get IPv4 and IPv6 addresses
                        var ipProps = nic.GetIPProperties();
                        foreach (var ip in ipProps.UnicastAddresses)
                        {
                            // Filter out link-local addresses
                            if (!ip.Address.ToString().StartsWith("fe80:"))
                            {
                                iface.IpAddresses.Add(ip.Address.ToString());
                            }
                        }

                        if (iface.IpAddresses.Count > 0 || !string.IsNullOrEmpty(iface.MacAddress))
                        {
                            interfaces.Add(iface);
                            LogInfo($"Network Interface: {iface.Name} - MAC: {iface.MacAddress} - IPs: {string.Join(", ", iface.IpAddresses)}");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError($"Error processing network interface {nic.Name}", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("Error getting network interfaces", ex);
            }

            return interfaces;
        }

        private string GetProcessorInfo()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Name, NumberOfCores, NumberOfLogicalProcessors FROM Win32_Processor"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        try
                        {
                            var name = obj["Name"]?.ToString() ?? "Unknown";
                            var cores = obj["NumberOfCores"]?.ToString() ?? "Unknown";
                            var threads = obj["NumberOfLogicalProcessors"]?.ToString() ?? "Unknown";

                            var result = $"{name} ({cores} cores, {threads} threads)";
                            LogInfo($"Processor: {result}");
                            return result;
                        }
                        catch (Exception ex)
                        {
                            LogError("Error parsing processor object", ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("Error getting processor info", ex);
            }

            return "Unknown";
        }

        private long GetTotalMemory()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        try
                        {
                            var memoryStr = obj["TotalVisibleMemorySize"]?.ToString() ?? "0";
                            if (long.TryParse(memoryStr, out long memory))
                            {
                                var memoryGB = memory * 1024 / (1024 * 1024 * 1024);
                                LogInfo($"Total Memory: {memoryGB} GB");
                                return memory * 1024; // Convert KB to Bytes
                            }
                        }
                        catch (Exception ex)
                        {
                            LogError("Error parsing memory object", ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("Error getting total memory", ex);
            }

            return 0;
        }

        private string GetLastBootTime()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT LastBootUpTime FROM Win32_OperatingSystem"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        try
                        {
                            var bootTime = obj["LastBootUpTime"]?.ToString();
                            if (!string.IsNullOrEmpty(bootTime))
                            {
                                LogInfo($"Last Boot Time: {bootTime}");
                                return bootTime;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogError("Error parsing boot time object", ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("Error getting last boot time", ex);
            }

            // Fallback: Calculate from uptime
            try
            {
                var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
                var bootTime = DateTime.UtcNow.Subtract(uptime);
                LogInfo($"Last Boot Time (calculated): {bootTime:O}");
                return bootTime.ToString("O");
            }
            catch (Exception ex)
            {
                LogError("Error calculating boot time", ex);
                return "Unknown";
            }
        }

        private List<InstalledSoftware> GetInstalledSoftware()
        {
            var software = new List<InstalledSoftware>();

            try
            {
                LogInfo("Collecting installed software list");

                // Query Win32_Product (slower but more reliable)
                using (var searcher = new ManagementObjectSearcher("SELECT Name, Version, InstallDate FROM Win32_Product"))
                {
                    var results = searcher.Get();
                    LogInfo($"Found {results.Count} installed products");

                    foreach (var obj in results)
                    {
                        try
                        {
                            var name = obj["Name"]?.ToString();
                            if (string.IsNullOrEmpty(name))
                                continue;

                            var version = obj["Version"]?.ToString() ?? "Unknown";
                            var installDateStr = obj["InstallDate"]?.ToString() ?? "";

                            DateTime installDate = DateTime.MinValue;
                            if (!string.IsNullOrEmpty(installDateStr) && installDateStr.Length >= 8)
                            {
                                if (DateTime.TryParseExact(installDateStr.Substring(0, 8), "yyyyMMdd", null,
                                    System.Globalization.DateTimeStyles.None, out var parsed))
                                {
                                    installDate = parsed;
                                }
                            }

                            software.Add(new InstalledSoftware
                            {
                                Name = name,
                                Version = version,
                                InstallDate = installDate
                            });
                        }
                        catch (Exception ex)
                        {
                            LogError("Error parsing software object", ex);
                        }
                    }
                }

                LogInfo($"Total software collected: {software.Count}");
            }
            catch (Exception ex)
            {
                LogError("Error getting installed software", ex);
            }

            return software.OrderBy(s => s.Name).ToList();
        }

        private void LogInfo(string message)
        {
            _logger?.LogInformation(message);
            Console.WriteLine($"[INFO] {message}");
        }

        private void LogError(string message, Exception ex)
        {
            _logger?.LogError(ex, message);
            Console.WriteLine($"[ERROR] {message}: {ex?.Message}");
        }
    }
}