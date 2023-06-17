using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Universal_x86_Tuning_Utility_Handheld.Properties;

namespace Universal_x86_Tuning_Utility_Handheld.Scripts
{
    internal class ToastNotification
    {
        public static void ShowToastNotification(bool isXg = false, string title = "", string body = "")
        {
            string iconUri = "";
            string icon2Uri = "";
            iconUri = "file:///" + Settings.Default.Path + "Assets\\applicationIcon.png";
            icon2Uri = "file:///" + Settings.Default.Path + "Images\\XGMobile\\XGMobile-1.png";

            if (isXg)
            {
                new ToastContentBuilder()
                    .AddText(title)
                    .AddText(body)
                    .AddAppLogoOverride(new Uri(iconUri))
                    .AddInlineImage(new Uri(icon2Uri))
                    .Show();
            }
            else
            {
                new ToastContentBuilder()
                    .AddText(title)
                    .AddText(body)
                    .AddAppLogoOverride(new Uri(iconUri))
                    .Show();
            }
        }

        public static void PromptXgMobileActivate()
        {
            new ToastContentBuilder()
                  .AddArgument("XgMobileOpen")
                  .AddText("XG Mobile detected!")
                  .AddAppLogoOverride(new Uri("file:///" + Settings.Default.Path + "Assets\\applicationIcon.png"))
                  .AddInlineImage(new Uri("file:///" + Settings.Default.Path + "Images\\XGMobile\\XGMobile-1.png"))
                  .AddButton(new ToastButton().SetContent("Activate")
                  .AddArgument("XgMobileActivate"))
                  .Show(toast => {
                      toast.Tag = "AcXgMobile";
                  });

        }

        public static bool IsActivateXgMobileToastButtonClicked(ToastNotificationActivatedEventArgsCompat toastArgs)
        {
            return toastArgs.Argument.Split(";", StringSplitOptions.RemoveEmptyEntries).Any((value) => "XgMobileActivate".Equals(value));
        }

        public static bool IsOpenXgMobileToastClicked(ToastNotificationActivatedEventArgsCompat toastArgs)
        {
            return toastArgs.Argument.Split(";", StringSplitOptions.RemoveEmptyEntries).Any((value) => "XgMobileOpen".Equals(value));
        }

        public static void HideXgMobileActivateToasts()
        {
            ToastNotificationManagerCompat.History.Remove("AcXgMobile");
        }
    }
}

