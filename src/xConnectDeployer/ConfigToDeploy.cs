namespace xConnectDeployer
{
	internal class ConfigToDeploy
	{
		public string SourceFolder { get; }
		public string DestinationFolder { get; }
		public string[] Files { get; }

		public ConfigToDeploy(string sourceFolder, string destinationFolder, string[] files)
		{
			SourceFolder = sourceFolder;
			DestinationFolder = destinationFolder;
			Files = files;
		}
	}
}