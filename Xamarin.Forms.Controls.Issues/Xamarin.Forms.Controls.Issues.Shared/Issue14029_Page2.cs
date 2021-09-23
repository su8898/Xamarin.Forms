using System;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.CollectionView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 14029, "[Bug] [macOS] Fix back button for nested nav pages",
		PlatformAffected.macOS)]
	public partial class Issue14029_Page2 : TestContentPage
	{

		public Issue14029_Page2()
		{
#if APP
			InitializeComponent();
#endif
		}

		protected override void Init()
		{
			
		}
	}
}