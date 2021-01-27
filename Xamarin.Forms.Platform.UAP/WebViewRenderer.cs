//using System;
//using System.ComponentModel;
//using Windows.UI.Core;
//using Microsoft.UI.Xaml.Controls;
//using Xamarin.Forms.Internals;
//using static System.String;
//using Xamarin.Forms.PlatformConfiguration.WindowsSpecific;
//using System.Threading.Tasks;
//using System.Net;
//using Windows.Web.Http;
//using System.Collections.Generic;
//using System.Linq;

//namespace Xamarin.Forms.Platform.UWP
//{
//#pragma warning disable CS8305 // Type is for evaluation purposes only and is subject to change or removal in future updates.
//	public class WebViewRenderer : ViewRenderer<WebView, Microsoft.UI.Xaml.Controls.WebView2>, IWebViewDelegate
//#pragma warning restore CS8305 // Type is for evaluation purposes only and is subject to change or removal in future updates.
//	{
//		public WebViewRenderer()
//		{
//			SetElement(new WebView());
//		}

//		protected override void OnElementChanged(ElementChangedEventArgs<WebView> e)
//		{
//			base.OnElementChanged(e);
//#pragma warning disable CS8305 // Type is for evaluation purposes only and is subject to change or removal in future updates.
//			SetNativeControl(new WebView2());
//#pragma warning restore CS8305 // Type is for evaluation purposes only and is subject to change or removal in future updates.
//			Control.Source = new Uri("https://www.microsoft.com");
//		}

//		public void LoadHtml(string html, string baseUrl)
//		{
//		}

//		public void LoadUrl(string url)
//		{
//		}
//	}
//}

using System;
using System.ComponentModel;
using Windows.UI.Core;
using Xamarin.Forms.Internals;
using static System.String;
using Xamarin.Forms.PlatformConfiguration.WindowsSpecific;
using System.Threading.Tasks;
using System.Net;
using Windows.Web.Http;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml.Controls;

namespace Xamarin.Forms.Platform.UWP
{
#pragma warning disable CS8305 // Type is for evaluation purposes only and is subject to change or removal in future updates.
	public class WebViewRenderer : ViewRenderer<WebView, WebView2>, IWebViewDelegate
#pragma warning restore CS8305 // Type is for evaluation purposes only and is subject to change or removal in future updates.
	{
		WebNavigationEvent _eventState;
		bool _updating;
#pragma warning disable CS8305 // Type is for evaluation purposes only and is subject to change or removal in future updates.
		WebView2 _internalWebView;
#pragma warning restore CS8305 // Type is for evaluation purposes only and is subject to change or removal in future updates.
		const string LocalScheme = "ms-appx-web:///";

		// Script to insert a <base> tag into an HTML document
		const string BaseInsertionScript = @"
var head = document.getElementsByTagName('head')[0];
var bases = head.getElementsByTagName('base');
if(bases.length == 0){
    head.innerHTML = 'baseTag' + head.innerHTML;
}";
		public void LoadHtml(string html, string baseUrl)
		{
			if (IsNullOrEmpty(baseUrl))
			{
				baseUrl = LocalScheme;
			}

			// Generate a base tag for the document
			var baseTag = $"<base href=\"{baseUrl}\"></base>";

			string htmlWithBaseTag;

			// Set up an internal WebView we can use to load and parse the original HTML string
			// Make _internalWebView a field instead of local variable to avoid garbage collection
#pragma warning disable CS8305 // Type is for evaluation purposes only and is subject to change or removal in future updates.
			_internalWebView = new WebView2();
#pragma warning restore CS8305 // Type is for evaluation purposes only and is subject to change or removal in future updates.

			// When the 'navigation' to the original HTML string is done, we can modify it to include our <base> tag
			_internalWebView.NavigationCompleted += async (sender, args) =>
			{
				// Generate a version of the <base> script with the correct <base> tag
				var script = BaseInsertionScript.Replace("baseTag", baseTag);

				// Run it and retrieve the updated HTML from our WebView
				await sender.ExecuteScriptAsync(script);
				htmlWithBaseTag = await sender.ExecuteScriptAsync("document.documentElement.outerHTML;");

				// Set the HTML for the 'real' WebView to the updated HTML
				Control.NavigateToString(!IsNullOrEmpty(htmlWithBaseTag) ? htmlWithBaseTag : html);
				// free up memory after we're done with _internalWebView
				_internalWebView = null;
			};

			// Kick off the initial navigation
			_internalWebView.NavigateToString(html);
		}

		public void LoadUrl(string url)
		{
			Uri uri = new Uri(url, UriKind.RelativeOrAbsolute);

			if (!uri.IsAbsoluteUri)
			{
				uri = new Uri(LocalScheme + url, UriKind.RelativeOrAbsolute);
			}

			var cookies = Element.Cookies?.GetCookies(uri);
			if (cookies != null)
			{
				SyncNativeCookies(url);

				try
				{
					// TODO WINUI2
					//var httpRequestMessage = new Windows.Web.Http.HttpRequestMessage(Windows.Web.Http.HttpMethod.Get, uri);
					Control.Source = uri; //
				}
				catch (System.Exception exc)
				{
					Internals.Log.Warning(nameof(WebViewRenderer), $"Failed to load: {uri} {exc}");
				}
			}
			else
			{
				try
				{
					//No Cookies so just navigate...
					Control.Source = uri;
				}
				catch (System.Exception exc)
				{
					Internals.Log.Warning(nameof(WebViewRenderer), $"Failed to load: {uri} {exc}");
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Control != null)
				{
					Control.NavigationStarting -= OnNavigationStarted;
					Control.NavigationCompleted -= OnNavigationCompleted;

					// TODO WINUI
					//Control.NavigationFailed -= OnNavigationFailed;
					//Control.ScriptNotify -= OnScriptNotify;
				}
			}

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<WebView> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement != null)
			{
				var oldElement = e.OldElement;
				oldElement.EvalRequested -= OnEvalRequested;
				oldElement.EvaluateJavaScriptRequested -= OnEvaluateJavaScriptRequested;
				oldElement.GoBackRequested -= OnGoBackRequested;
				oldElement.GoForwardRequested -= OnGoForwardRequested;
				oldElement.ReloadRequested -= OnReloadRequested;
			}

			if (e.NewElement != null)
			{
				if (Control == null)
				{
#pragma warning disable CS8305 // Type is for evaluation purposes only and is subject to change or removal in future updates.
					var webView = new WebView2();
#pragma warning restore CS8305 // Type is for evaluation purposes only and is subject to change or removal in future updates.
					webView.NavigationStarting += OnNavigationStarted;
					webView.NavigationCompleted += OnNavigationCompleted;

					// TODO WINUI
					//webView.NavigationFailed += OnNavigationFailed;
					//webView.ScriptNotify += OnScriptNotify;
					SetNativeControl(webView);
				}

				var newElement = e.NewElement;
				newElement.EvalRequested += OnEvalRequested;
				newElement.EvaluateJavaScriptRequested += OnEvaluateJavaScriptRequested;
				newElement.GoForwardRequested += OnGoForwardRequested;
				newElement.GoBackRequested += OnGoBackRequested;
				newElement.ReloadRequested += OnReloadRequested;

				Load();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == WebView.SourceProperty.PropertyName)
			{
				if (!_updating)
					Load();
			}
		}

		HashSet<string> _loadedCookies = new HashSet<string>();

		Uri CreateUriForCookies(string url)
		{
			if (url == null)
				return null;

			Uri uri;

			if (url.Length > 2000)
				url = url.Substring(0, 2000);

			if (Uri.TryCreate(url, UriKind.Absolute, out uri))
			{
				if (String.IsNullOrWhiteSpace(uri.Host))
					return null;

				return uri;
			}

			return null;
		}

		HttpCookieCollection GetCookiesFromNativeStore(string url)
		{
			var uri = CreateUriForCookies(url);
			CookieContainer existingCookies = new CookieContainer();
			var filter = new Windows.Web.Http.Filters.HttpBaseProtocolFilter();
			var nativeCookies = filter.CookieManager.GetCookies(uri);
			return nativeCookies;
		}

		void InitialCookiePreloadIfNecessary(string url)
		{
			var myCookieJar = Element.Cookies;
			if (myCookieJar == null)
				return;

			var uri = new System.Uri(url);

			if (!_loadedCookies.Add(uri.Host))
				return;

			var cookies = myCookieJar.GetCookies(uri);

			if (cookies != null)
			{
				var existingCookies = GetCookiesFromNativeStore(url);
				foreach (HttpCookie cookie in existingCookies)
				{
					if (cookies[cookie.Name] == null)
						myCookieJar.SetCookies(uri, cookie.ToString());
				}
			}
		}

		void SyncNativeCookiesToElement(string url)
		{
			var myCookieJar = Element.Cookies;
			if (myCookieJar == null)
				return;

			var uri = CreateUriForCookies(url);

			if (uri == null)
				return;

			var cookies = myCookieJar.GetCookies(uri);
			var retrieveCurrentWebCookies = GetCookiesFromNativeStore(url);

			var filter = new Windows.Web.Http.Filters.HttpBaseProtocolFilter();
			var nativeCookies = filter.CookieManager.GetCookies(uri);

			foreach (Cookie cookie in cookies)
			{
				var httpCookie = nativeCookies
					.FirstOrDefault(x => x.Name == cookie.Name);

				if (httpCookie == null)
					cookie.Expired = true;
				else
					cookie.Value = httpCookie.Value;
			}

			SyncNativeCookies(url);
		}

		void SyncNativeCookies(string url)
		{
			var uri = CreateUriForCookies(url);
			if (uri == null)
				return;

			var myCookieJar = Element.Cookies;
			if (myCookieJar == null)
				return;

			InitialCookiePreloadIfNecessary(url);
			var cookies = myCookieJar.GetCookies(uri);
			if (cookies == null)
				return;

			var retrieveCurrentWebCookies = GetCookiesFromNativeStore(url);

			var filter = new Windows.Web.Http.Filters.HttpBaseProtocolFilter();
			foreach (Cookie cookie in cookies)
			{
				HttpCookie httpCookie = new HttpCookie(cookie.Name, cookie.Domain, cookie.Path);
				httpCookie.Value = cookie.Value;
				filter.CookieManager.SetCookie(httpCookie, false);
			}

			foreach (HttpCookie cookie in retrieveCurrentWebCookies)
			{
				if (cookies[cookie.Name] != null)
					continue;

				filter.CookieManager.DeleteCookie(cookie);
			}
		}

		void Load()
		{
			if (Element.Source != null)
				Element.Source.Load(this);

			UpdateCanGoBackForward();
		}

		async void OnEvalRequested(object sender, EvalRequested eventArg)
		{
			await Control.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
				async () =>
				{
					try
					{
						await Control.ExecuteScriptAsync(eventArg.Script);
					}
					catch (Exception exc)
					{
						Log.Warning(nameof(WebView), $"Eval of script failed: {exc} Script: {eventArg.Script}");
					}
				});
		}

		async Task<string> OnEvaluateJavaScriptRequested(string script)
		{
			return await Control.ExecuteScriptAsync(script);
		}

		void OnGoBackRequested(object sender, EventArgs eventArgs)
		{
			if (Control.CanGoBack)
			{
				_eventState = WebNavigationEvent.Back;
				Control.GoBack();
			}

			UpdateCanGoBackForward();
		}

		void OnGoForwardRequested(object sender, EventArgs eventArgs)
		{
			if (Control.CanGoForward)
			{
				_eventState = WebNavigationEvent.Forward;
				Control.GoForward();
			}

			UpdateCanGoBackForward();
		}

		void OnReloadRequested(object sender, EventArgs eventArgs)
		{
			SyncNativeCookies(Control?.Source?.ToString());
			Control.Reload();
		}

#pragma warning disable CS8305 // Type is for evaluation purposes only and is subject to change or removal in future updates.
		async void OnNavigationCompleted(WebView2 sender, WebView2NavigationCompletedEventArgs e)
#pragma warning restore CS8305 // Type is for evaluation purposes only and is subject to change or removal in future updates.
		{
			//TODO WINUI
			Uri uri = sender.Source;
			if (uri != null)
				SendNavigated(new UrlWebViewSource { Url = uri.AbsoluteUri }, _eventState, WebNavigationResult.Success);

			UpdateCanGoBackForward();

			if (Element.OnThisPlatform().IsJavaScriptAlertEnabled())
				await Control.ExecuteScriptAsync("window.alert = function(message){ window.external.notify(message); };");
		}

		//void OnNavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
		//{
		//	if (e.Uri != null)
		//		SendNavigated(new UrlWebViewSource { Url = e.Uri.AbsoluteUri }, _eventState, WebNavigationResult.Failure);
		//}

#pragma warning disable CS8305 // Type is for evaluation purposes only and is subject to change or removal in future updates.
		void OnNavigationStarted(WebView2 sender, WebView2NavigationStartingEventArgs e)
#pragma warning restore CS8305 // Type is for evaluation purposes only and is subject to change or removal in future updates.
		{
			Uri uri;
			
			if (Uri.TryCreate(e.Uri, UriKind.Absolute, out uri) && uri != null)
			{
				var args = new WebNavigatingEventArgs(_eventState, new UrlWebViewSource { Url = uri.AbsoluteUri }, uri.AbsoluteUri);

				Element.SendNavigating(args);
				e.Cancel = args.Cancel;

				// reset in this case because this is the last event we will get
				if (args.Cancel)
					_eventState = WebNavigationEvent.NewPage;
			}
		}

		//async void OnScriptNotify(object sender, NotifyEventArgs e)
		//{
		//	if (Element.OnThisPlatform().IsJavaScriptAlertEnabled())
		//		await new Windows.UI.Popups.MessageDialog(e.Value).ShowAsync();
		//}

		void SendNavigated(UrlWebViewSource source, WebNavigationEvent evnt, WebNavigationResult result)
		{
			_updating = true;
			((IElementController)Element).SetValueFromRenderer(WebView.SourceProperty, source);
			_updating = false;

			SyncNativeCookiesToElement(source.Url);
			Element.SendNavigated(new WebNavigatedEventArgs(evnt, source, source.Url, result));

			UpdateCanGoBackForward();
			_eventState = WebNavigationEvent.NewPage;
		}

		void UpdateCanGoBackForward()
		{
			((IWebViewController)Element).CanGoBack = Control.CanGoBack;
			((IWebViewController)Element).CanGoForward = Control.CanGoForward;
		}
	}
}