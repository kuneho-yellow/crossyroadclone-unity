using System;
namespace Soomla
{
		public interface ISoomlaManifestTools
		{
#if UNITY_EDITOR
			void UpdateManifest();
#endif
		}
}

