﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using OpenQA.Selenium.Appium.iOS;
using BrowserStack;
using System.Runtime.InteropServices;
using OpenQA.Selenium.Appium;

namespace ios.local
{
	public class BrowserStackNUnitTest
	{
		protected IOSDriver<IOSElement> driver;
		protected string profile;
		protected string device;
		private Local browserStackLocal;

		public BrowserStackNUnitTest(string profile, string device)
		{
			this.profile = profile;
			this.device = device;
		}

		[SetUp]
		public void Init()
		{
			NameValueCollection caps = ConfigurationManager.GetSection("capabilities/" + profile) as NameValueCollection;
			NameValueCollection devices = ConfigurationManager.GetSection("environments/" + device) as NameValueCollection;


			AppiumOptions options = new AppiumOptions();

			foreach (string key in caps.AllKeys)
			{
				options.AddAdditionalCapability(key, caps[key]);
			}

			foreach (string key in devices.AllKeys)
			{
				options.AddAdditionalCapability(key, devices[key]);
			}

			String username = Environment.GetEnvironmentVariable("BROWSERSTACK_USERNAME");
			if (username == null)
			{
				username = ConfigurationManager.AppSettings.Get("user");
			}

			String accesskey = Environment.GetEnvironmentVariable("BROWSERSTACK_ACCESS_KEY");
			if (accesskey == null)
			{
				accesskey = ConfigurationManager.AppSettings.Get("key");
			}

			options.AddAdditionalCapability("browserstack.user", username);
			options.AddAdditionalCapability("browserstack.key", accesskey);

			String appId = Environment.GetEnvironmentVariable("BROWSERSTACK_APP_ID");
			if (appId != null)
			{
				options.AddAdditionalCapability("app", appId);
			}

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
					&& options.ToCapabilities().HasCapability("browserstack.local")
					&& options.ToCapabilities().GetCapability("browserstack.local").ToString() == "true")
			{
				browserStackLocal = new Local();
				List<KeyValuePair<string, string>> bsLocalArgs = new List<KeyValuePair<string, string>>() {
					new KeyValuePair<string, string>("key", accesskey)
				};
				browserStackLocal.start(bsLocalArgs);
			}

			Uri uri = new Uri("http://" + ConfigurationManager.AppSettings.Get("server") + "/wd/hub/");
			driver = new IOSDriver<IOSElement>(uri, options);
		}

		[TearDown]
		public void Cleanup()
		{
			driver.Quit();
			if (browserStackLocal != null)
			{
				browserStackLocal.stop();
			}
		}

	}
}
