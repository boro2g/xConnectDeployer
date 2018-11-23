using System;
using System.IO;
using System.Linq;
using demo.CustomerInformation.LatestFlight;

namespace xConnectDeployer
{
	class Program
	{
		protected internal const string DeploymentFolderKey = "deploymentFolder";

		static void Main(string[] args)
		{
			var deploymentFolder = "C:\\inetpub\\wwwroot\\demo.xconnect\\";

			if (args.Any())
			{
				var parsedArgs = args
					.Select(s => s.Split(new[] { ':' }, 1))
					.ToDictionary(s => s[0], s => s[1]);

				if (parsedArgs.ContainsKey(DeploymentFolderKey))
				{
					deploymentFolder = parsedArgs[DeploymentFolderKey];
				}
			}

			if (!Directory.Exists(deploymentFolder))
			{
				throw new NotSupportedException($"Could not find folder: {deploymentFolder}");
			}

			Console.WriteLine($"deploymentFolder: {deploymentFolder}");

			var currentFolder = AppDomain.CurrentDomain.BaseDirectory;

			WriteSchema(currentFolder);

			DeployModelConfig(deploymentFolder, currentFolder);

			DeployDlls(deploymentFolder, currentFolder);

			DeployPatchConfig(deploymentFolder, currentFolder);

			DeployAgentConfigs(deploymentFolder, currentFolder);
		}

		private static void WriteSchema(string currentFolder)
		{
			using (var logger = new OperationLogger("WriteSchema"))
			{
				var model = Sitecore.XConnect.Serialization.XdbModelWriter.Serialize(LatestFlightModel.Model);

				var schemaPath = currentFolder + LatestFlightModel.Model.FullName + ".json";

				logger.Log($"Writing schema to {schemaPath}");

				File.WriteAllText(schemaPath, model);
			}
		}

		private static void DeployAgentConfigs(string deploymentFolder, string currentFolder)
		{
			using (var logger = new OperationLogger("DeployAgentConfigs"))
			{
				var configsToDeploy = new[]
				{
					new ConfigToDeploy("AutomationEngine", "App_data\\jobs\\continuous\\AutomationEngine\\App_Config",
						new[] {"AppSettings.config", "ConnectionStrings.config"}),
					new ConfigToDeploy("IndexWorker", "App_data\\jobs\\continuous\\IndexWorker\\App_Config",
						new[] {"AppSettings.config", "ConnectionStrings.config"}),
					new ConfigToDeploy("xConnect", "App_Config",
						new[] {"AppSettings.config", "ConnectionStrings.config"})
				};

				foreach (var configToDeploy in configsToDeploy)
				{
					var sourceFolder = $"{currentFolder}\\{configToDeploy.SourceFolder}";

					var destinationFolder = $"{deploymentFolder}{configToDeploy.DestinationFolder}";

					logger.Log($"Copying config files from {sourceFolder} to {destinationFolder}");

					foreach (var file in configToDeploy.Files)
					{
						File.Copy($"{sourceFolder}//{file}", $"{destinationFolder}//{file}", true);
					}
				}
			}
		}

		private static void DeployPatchConfig(string deploymentFolder, string currentFolder)
		{
			using (var logger = new OperationLogger("DeployPatchConfig"))
			{
				var patchConfigFolder =
					"App_data\\jobs\\continuous\\AutomationEngine\\App_Data\\Config\\sitecore\\MarketingAutomation_patch";

				var configToCopy = "sc.MarketingAutomation.ActivityTypes.xml";

				var patchDeploymentFolder = $"{deploymentFolder}{patchConfigFolder}";

				EnsureFolderExists(patchDeploymentFolder);

				logger.Log($"Copying {configToCopy} to {patchDeploymentFolder}");

				File.Copy($"{currentFolder}\\{configToCopy}", $"{patchDeploymentFolder}\\{configToCopy}", true);
			}
		}

		private static void DeployDlls(string deploymentFolder, string currentFolder)
		{
			using (var logger = new OperationLogger("DeployDlls"))
			{
				var binFolders = new[]
					{"bin", "App_data\\jobs\\continuous\\AutomationEngine", "App_data\\jobs\\continuous\\IndexWorker"};

				var dllsToCopy = new[]
				{
					"demo.CustomerInformation.dll", "demo.AutomationEngine.dll", "demo.Content.dll",
					"demo.Rules.dll", "demo.Email.dll", "demo.Sms.dll", "RestSharp.dll", "SendGrid.dll",
					"System.Net.Http.dll"
				};

				foreach (var binFolder in binFolders)
				{
					logger.Log($"Copying bin to {deploymentFolder}{binFolder}");

					foreach (var dllToCopy in dllsToCopy)
					{
						try
						{
							File.Copy($"{currentFolder}\\{dllToCopy}", $"{deploymentFolder}{binFolder}\\{dllToCopy}",
								true);
						}
						catch (Exception e)
						{
							logger.Log($"Could not copy {dllToCopy} to {binFolder}");
						}
					}
				}
			}
		}

		private static void DeployModelConfig(string deploymentFolder, string currentFolder)
		{
			using (var logger = new OperationLogger("DeployModelConfig"))
			{
				var configFolders = new[]
					{"App_data\\jobs\\continuous\\IndexWorker\\App_data\\Models", "App_data\\Models"};

				foreach (var configFolder in configFolders)
				{
					logger.Log($"Copying config to {deploymentFolder}{configFolder}");

					File.Copy($"{currentFolder}\\{LatestFlightModel.Model.FullName}.json",
						$"{deploymentFolder}{configFolder}\\{LatestFlightModel.Model.FullName}.json", true);
				}
			}
		}

		private static void EnsureFolderExists(string folder)
		{
			bool folderExists = Directory.Exists(folder);

			if (!folderExists)
			{
				Directory.CreateDirectory(folder);
			}
		}
	}
}
