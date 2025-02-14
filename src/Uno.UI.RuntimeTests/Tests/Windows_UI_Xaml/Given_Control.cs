﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Uno.UI.Extensions;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Private.Infrastructure;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Shapes;
using Windows.UI;
using Windows.Foundation;

namespace Uno.UI.RuntimeTests.Tests.Windows_UI_Xaml
{
	[TestClass]
	public partial class Given_Control
	{
		private partial class CustomControl : Control
		{
			public Size AvailableSizePassedToMeasureOverride { get; private set; }
			protected override Size MeasureOverride(Size availableSize)
			{
				AvailableSizePassedToMeasureOverride = availableSize;
				return new(2000, 2000);
			}
		}

		[TestMethod]
		[RunsOnUIThread]
		public async Task When_Limited_By_Available_Size_Before_Margin_Application()
		{
			// This assert makes sure that:
			// 1. The availableSize passed to MeasureOverride is correct
			// 2. DesiredSize is properly limited by the available size used for measure, before applying the margin.

			// Note: This test might seem like it can be a unit test rather than a runtime test. But it's
			// intentional to make it a runtime test. This is to make sure we test measure logic specific to each platform.

			var SUT = new CustomControl
			{
				Margin = new Thickness(-70),
			};

			// availableSize passed to the core measure is 200x200
			SUT.Measure(new Size(200, 200));

			// The MeasureOverride is passed the size after applying margin, which is 340x340.
			Assert.AreEqual(new Size(340, 340), SUT.AvailableSizePassedToMeasureOverride);

			// The desiredSize is limited by the available size before applying the margin.
			Assert.AreEqual(new Size(200, 200), SUT.DesiredSize);
		}

		[TestMethod]
		[RunsOnUIThread]
		public async Task When_Limited_By_Available_Size_After_Margin_Application()
		{
			// This assert makes sure that:
			// 1. The availableSize passed to MeasureOverride is correct
			// 2. DesiredSize is not limited by the available size used for measure, after applying the margin.

			var SUT = new CustomControl
			{
				Margin = new Thickness(70),
			};

			// availableSize passed to the core measure is 200x200
			SUT.Measure(new Size(200, 200));

			// The MeasureOverride is passed the size after applying margin, which is 60x60.
			Assert.AreEqual(new Size(60, 60), SUT.AvailableSizePassedToMeasureOverride);

			// The desiredSize is NOT limited by the available size after applying the margin.
			Assert.AreEqual(new Size(200, 200), SUT.DesiredSize);
		}

		[TestMethod]
		[RunsOnUIThread]
		public async Task When_SetChildTemplateUsingVisualState()
		{
			var parent = (Button)XamlReader.Load(_when_SetChildTemplateUsingVisualState);
			TestServices.WindowHelper.WindowContent = parent;
			await TestServices.WindowHelper.WaitForIdle();

			var tb = parent.FindFirstChild<TextBlock>();

			Assert.IsNotNull(tb);
			Assert.AreEqual("Template loaded!", tb.Text);
		}

		private const string _when_SetChildTemplateUsingVisualState =
			"""
			<Button xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
				<Button.Template>
					<ControlTemplate TargetType="ContentControl">
						<Grid>
							<VisualStateManager.VisualStateGroups>
								<VisualStateGroup x:Name="CommonStates">
									<VisualState x:Name="Normal">
										<VisualState.Setters>
											<Setter  Target="_sut.Template">
												<Setter.Value>
													<ControlTemplate TargetType="ContentControl">
														<TextBlock Text="Template loaded!" />
													</ControlTemplate>
												</Setter.Value>
											</Setter>
										</VisualState.Setters>
									</VisualState>
								</VisualStateGroup>
							</VisualStateManager.VisualStateGroups>

							<ContentControl x:Name="_sut" />
						</Grid>
					</ControlTemplate>
				</Button.Template>
			</Button>
			""";

		[TestMethod]
		[RunsOnUIThread]
		public async Task When_Refresh_Setter_BindingOnInvocation()
		{
			var SUT = new When_Refresh_Setter_BindingOnInvocation();
			TestServices.WindowHelper.WindowContent = SUT;
			await TestServices.WindowHelper.WaitForIdle();

			SUT.root.Content = 42;

			var testTransform = (SUT.root.FindName("ContentElement") as FrameworkElement).RenderTransform as CompositeTransform;

			Assert.IsNotNull(testTransform);

			Assert.AreEqual(0, testTransform.TranslateX);

			VisualStateManager.GoToState(SUT.root, "Normal", false);
			await TestServices.WindowHelper.WaitForIdle();
			Assert.AreEqual(-10, testTransform.TranslateY);

			SUT.root.Tag = 42;
			VisualStateManager.GoToState(SUT.root, "Focused", false);
			await TestServices.WindowHelper.WaitForIdle();
			Assert.AreEqual(42, testTransform.TranslateY);

			SUT.root.Tag = 43;
			VisualStateManager.GoToState(SUT.root, "Normal", false);
			await TestServices.WindowHelper.WaitForIdle();
			Assert.AreEqual(43, testTransform.TranslateY);
		}

		[TestMethod]
		[RunsOnUIThread]
		public async Task When_Refresh_Setter_BindingOnInvocation_ElementName()
		{
			var SUT = new When_Refresh_Setter_BindingOnInvocation_ElementName();
			TestServices.WindowHelper.WindowContent = SUT;
			await TestServices.WindowHelper.WaitForIdle();

			SUT.root.Content = 42;

			var testTransform = (SUT.root.FindName("ContentElement") as FrameworkElement).RenderTransform as CompositeTransform;

			Assert.IsNotNull(testTransform);

			Assert.AreEqual(0, testTransform.TranslateX);

			VisualStateManager.GoToState(SUT.root, "Normal", false);
			await TestServices.WindowHelper.WaitForIdle();
			Assert.AreEqual(-10, testTransform.TranslateY);

			SUT.sp01.Tag = 42;
			VisualStateManager.GoToState(SUT.root, "Focused", false);
			await TestServices.WindowHelper.WaitForIdle();
			Assert.AreEqual(42, testTransform.TranslateY);

			SUT.sp01.Tag = 43;
			VisualStateManager.GoToState(SUT.root, "Normal", false);
			await TestServices.WindowHelper.WaitForIdle();
			Assert.AreEqual(43, testTransform.TranslateY);
		}

		[TestMethod]
		[RunsOnUIThread]
#if !__CROSSRUNTIME__
		[Ignore("We override <Measure|Arrange>Override to include padding in ContentControl which is a superclass of UserControl on Uno")]
#endif
		public async Task When_Padding_Set_In_SizeChanged()
		{
			var SUT = new UserControl()
			{
				Width = 200,
				Height = 200,
				Content = new Border()
				{
					Child = new Ellipse()
					{
						Fill = new SolidColorBrush(Colors.DarkOrange)
					}
				}
			};

			SUT.SizeChanged += (sender, args) => SUT.Padding = new Thickness(0, 200, 0, 0);

			TestServices.WindowHelper.WindowContent = SUT;
			await TestServices.WindowHelper.WaitForLoaded(SUT);
			await TestServices.WindowHelper.WaitForIdle();

			// We have a problem on IOS and Android where SUT isn't relayouted after the padding
			// change even though IsMeasureDirty is true. This is a workaround to explicity relayout.
#if __IOS__ || __ANDROID__
			SUT.InvalidateMeasure();
			SUT.UpdateLayout();
#endif

			// Padding shouldn't affect measure
			Assert.AreEqual(0, ((UIElement)VisualTreeHelper.GetChild(SUT, 0)).ActualOffset.Y);
		}
	}
}
