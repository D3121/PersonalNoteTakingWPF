using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WpfMain
{
    class Anims
    {
        public static IEasingFunction Smooth
        {
            get;
            set;
        } = new QuarticEase() { EasingMode = EasingMode.EaseOut };

        public static void FadeIn(DependencyObject Object)
        {
            DoubleAnimation FadeIn = new DoubleAnimation()
            {
                From = 0.0,
                To = 1.0,
                Duration = new Duration(TimeSpan.FromMilliseconds(500)),
            };
            Timeline.SetDesiredFrameRate(FadeIn, 60);
            Storyboard.SetTarget(FadeIn, Object);
            Storyboard.SetTargetProperty(FadeIn, new PropertyPath("Opacity", 1));

            Storyboard StoryBoard = new Storyboard();

            StoryBoard.Children.Add(FadeIn);
            StoryBoard.Begin();
        }

        public static void FadeOut(DependencyObject Object)
        {
            DoubleAnimation Fade = new DoubleAnimation()
            {
                From = 1.0,
                To = 0.0,
                Duration = new Duration(TimeSpan.FromMilliseconds(500)),
            };
            Timeline.SetDesiredFrameRate(Fade, 60);
            Storyboard.SetTarget(Fade, Object);
            Storyboard.SetTargetProperty(Fade, new PropertyPath("Opacity", 1));

            Storyboard StoryBoard = new Storyboard();

            StoryBoard.Children.Add(Fade);
            StoryBoard.Begin();
        }

        public static void ObjShiftScriptList(DependencyObject Object, Double Get, Double Set)
        {
            Storyboard StoryBoard2 = new Storyboard();
            DoubleAnimation Animation = new DoubleAnimation()
            {
                From = Get,
                To = Set,
                Duration = new Duration(TimeSpan.FromMilliseconds(750)),
                EasingFunction = Smooth,
            };
            Timeline.SetDesiredFrameRate(Animation, 60);
            Storyboard.SetTarget(Animation, Object);
            Storyboard.SetTargetProperty(Animation, new PropertyPath(Window.WidthProperty));
            StoryBoard2.Children.Add(Animation);
            StoryBoard2.Begin();
        }

        public static void ObjectShift(DependencyObject Object, Thickness Get, Thickness Set)
        {
            ThicknessAnimation Animation = new ThicknessAnimation()
            {
                From = Get,
                To = Set,
                Duration = new Duration(TimeSpan.FromMilliseconds(500)),
                EasingFunction = Smooth,
            };
            Timeline.SetDesiredFrameRate(Animation, 60);
            Storyboard.SetTarget(Animation, Object);
            Storyboard.SetTargetProperty(Animation, new PropertyPath(Window.MarginProperty));

            Storyboard StoryBoard = new Storyboard();

            StoryBoard.Children.Add(Animation);
            StoryBoard.Begin();
        }

        public static void ObjShiftBottomToTop(FrameworkElement element, double fromY, double toY)
        {
            var transform = element.RenderTransform as TranslateTransform;
            if (transform == null)
            {
                element.RenderTransform = new TranslateTransform();
            }

            DoubleAnimation animation = new DoubleAnimation()
            {
                From = fromY,
                To = toY,
                Duration = TimeSpan.FromMilliseconds(750),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            Timeline.SetDesiredFrameRate(animation, 60);
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            Storyboard.SetTarget(animation, element);
            Storyboard.SetTargetProperty(animation, new PropertyPath("RenderTransform.(TranslateTransform.Y)"));
            storyboard.Begin();
        }

    }
}
