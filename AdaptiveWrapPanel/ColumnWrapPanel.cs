using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace Voron.AdaptiveWrapPanel
{
	/// <summary>
	/// A column based layout panel, that automatically
	/// wraps to new column when required. The user
	/// may also create a new column before an element
	/// using the 
	/// </summary>
	internal class ColumnWrapPanel : Panel
	{
		public Size? ParentScrollViewerConstraint { get; set; }

		private Size CalcPlacement(double overflowBorder, MeasureData[] data)
		{
			int firstInLine = 0;
			double accumulatedHeight = 0;
			var panelSize = new Size();
			bool newColumn = true;

			for (int i = 0; i < InternalChildren.Count; i++)
			{
				if (newColumn)
				{
					accumulatedHeight = 0;
					firstInLine = i;
				}

				data[i].Overflow = accumulatedHeight
					+ InternalChildren[i].DesiredSize.Height - overflowBorder;

				//need to switch to another column
				if (data[i].ForceNewColumn
					|| data[i].FillNewColumn
					|| (data[i].Overflow > 0 && !data[i].OverflowIgnore))
				{
					CompleteColumn(firstInLine, i, ref panelSize, data);

					accumulatedHeight = InternalChildren[i].DesiredSize.Height;
					firstInLine = i;
					newColumn = false;

					if (data[i].FillNewColumn ||
						InternalChildren[i].DesiredSize.Height >= overflowBorder)
					{
						CompleteColumn(i, i + 1, ref panelSize, data);
						newColumn = true;
					}

				}
				else //continue to accumulate a column
				{
					accumulatedHeight += InternalChildren[i].DesiredSize.Height;
					newColumn = false;

					if (data[i].Fill && !data[i].FillIgnore)
					{
						CompleteColumn(firstInLine, i + 1, ref panelSize, data);
						newColumn = true;
					}
				}
			}

			if (!newColumn)
				CompleteColumn(firstInLine, InternalChildren.Count, ref panelSize, data);
			return panelSize;
		}

		private bool ShrinkFill(Size visibleConstraint, MeasureData[] data, ref bool newColumnOnly)
		{
			double freeSpace = 0;
			int bestIndex = -1;

			for (int i = 1; i < InternalChildren.Count; i++)
			{
				double value = 0;
				if (newColumnOnly)
				{
					if (!data[i].FillNewColumn)
						continue;
					value = visibleConstraint.Height -
						(data[i - 1].Rect.Top + InternalChildren[i - 1].DesiredSize.Height);
				}
				else
				{
					if (!data[i].Fill || data[i].FillIgnore)
						continue;
					value = data[i].Rect.Height;
				}

				if (value > freeSpace)
				{
					freeSpace = value;
					bestIndex = i;
				}
			}
			if (bestIndex != -1)
			{
				if (newColumnOnly)
					data[bestIndex].FillNewColumn = false;
				else
					data[bestIndex].FillIgnore = true;

				return true;
			}
			if (newColumnOnly)
			{
				newColumnOnly = false;
				return ShrinkFill(visibleConstraint, data, ref newColumnOnly);
			}
			return false;
		}

		//	if (mode <= 1)
		//	{
		//		for (int i = 0; i < InternalChildren.Count; i++)
		//		{
		//			if (!data[i].Fill || data[i].FillIgnore)
		//				continue;

		//			if (data[i].Rect.Height > freeSpace)
		//			{
		//				freeSpace = data[i].Rect.Height;
		//				bestIndex = i;
		//			}
		//		}
		//		if (bestIndex != -1)
		//		{
		//			data[bestIndex].FillIgnore = true;
		//			return true;
		//		}
		//		mode = 2;
		//	}

		//	if (mode <= 2)
		//	{
		//		//return false;
		//		freeSpace = double.MaxValue;
		//		for (int i = 1; i < InternalChildren.Count; i++)
		//		{
		//			if (data[i].OverflowIgnore ||
		//				data[i].ForceNewColumn ||
		//				data[i].Overflow <= 0)
		//				continue;

		//			if (data[i - 1].Rect.Bottom + InternalChildren[i].DesiredSize.Height
		//				- visibleConstraint.Height < freeSpace)
		//			{
		//				freeSpace = data[i - 1].Rect.Bottom + InternalChildren[i].DesiredSize.Height
		//				- visibleConstraint.Height;
		//				bestIndex = i;
		//			}
		//		}
		//		if (bestIndex != -1)
		//		{
		//			data[bestIndex].OverflowIgnore = true;
		//			//for (int i = bestIndex + 1; i < InternalChildren.Count; i++)
		//			//{
		//			//	data[i].OverflowIgnore = false;
		//			//}
		//			return true;
		//		}
		//		mode = 3;
		//	}
		//	return false;
		//}

		private void CompleteColumn(int elStart, int elEnd, ref Size panelSize, MeasureData[] data)
		{
			double width = 0;
			for (int i = elStart; i < elEnd; i++)
			{
				width = Math.Max(width, InternalChildren[i].DesiredSize.Width);
			}

			double top = 0;
			for (int i = elStart; i < elEnd; i++)
			{
				var newRect = new Rect
				{
					X = panelSize.Width,
					Y = top,
					Width = width,
					Height = InternalChildren[i].DesiredSize.Height
				};

				if (i == elEnd - 1 && data[i].Fill && ParentScrollViewerConstraint.HasValue)
				{
					newRect.Height = Math.Max(newRect.Height, ParentScrollViewerConstraint.Value.Height - newRect.Y);
				}

				data[i].Rect = newRect;

				top += newRect.Height;
			}

			panelSize.Height = Math.Max(top, panelSize.Height);
			panelSize.Width += width;
		}

		private struct MeasureData
		{
			public Rect Rect { get; set; }
			public bool Fill { get; set; }
			public bool FillNewColumn { get; set; }
			public double Overflow { get; set; }
			public bool OverflowIgnore { get; set; }
			public bool FillIgnore { get; set; }
			public bool ForceNewColumn { get; set; }

			public override string ToString()
			{
				return string.Join(", " + Environment.NewLine,
					$"{nameof(Fill)} {Fill}",
					$"{nameof(FillNewColumn)} {FillNewColumn}",
					$"{nameof(Overflow)} {Overflow}",
					$"{nameof(OverflowIgnore)} {OverflowIgnore}",
					$"{nameof(FillIgnore)} {FillIgnore}",
					$"{nameof(ForceNewColumn)} {ForceNewColumn}",
					$"{nameof(Rect)} {Rect}");
			}
		}

		private MeasureData[] measureData = new MeasureData[0];

		private Size MeasureArrange(Size constraint, bool arrange)
		{
			measureData = measureData.Length >= InternalChildren.Count ?
				measureData : new MeasureData[InternalChildren.Count];

			for (int i = 0; i < InternalChildren.Count; i++)
			{
				measureData[i].Rect = new Rect();
				measureData[i].Fill = AdaptiveWrapPanel.GetFillColumnHeight(InternalChildren[i]);
				measureData[i].FillNewColumn = measureData[i].Fill;
				measureData[i].FillIgnore = false;
				measureData[i].Overflow = 0;
				measureData[i].OverflowIgnore = false;
				measureData[i].ForceNewColumn = AdaptiveWrapPanel.GetForceNewColumn(InternalChildren[i]);

				if (!arrange)
					InternalChildren[i].Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
				//visibleConstraint.Width - panelSize.Width,
				//visibleConstraint.Height));
			}

			var visibleConstraint = ParentScrollViewerConstraint ?? constraint;

			var panelSize = CalcPlacement(visibleConstraint.Height, measureData);

			bool widthFit = panelSize.Width < visibleConstraint.Width;
			if (!widthFit)
			{
				if (!TryFitByShrinkFill(visibleConstraint, ref panelSize, ref measureData))
				{
					var lowBorder = panelSize.Height;
					panelSize = CalcPlacement(double.PositiveInfinity, measureData);
					var highBorder = panelSize.Height;

					// Is it possible to fit in width?
					if (panelSize.Width < visibleConstraint.Width)
					{

						// Binary search first border height fitting in width

						const double epsilon = 1d; // rational accuracy
						while ((highBorder - lowBorder) > epsilon || panelSize.Width > visibleConstraint.Width)
						{
							var currentBorder = lowBorder + (highBorder - lowBorder) / 2d;

							panelSize = CalcPlacement(currentBorder, measureData);

							if (panelSize.Width < visibleConstraint.Width)
								highBorder = currentBorder;
							else
								lowBorder = currentBorder;
						}
					}
				}
			}

			if (arrange)
			{
				for (var i = 0; i < InternalChildren.Count; i++)
				{
					double xOffset = 0;
					if (InternalChildren[i].DesiredSize.Width < measureData[i].Rect.Width)
					{
						xOffset = ((measureData[i].Rect.Width - InternalChildren[i].DesiredSize.Width) / 2);
					}

					InternalChildren[i].Arrange(new Rect(measureData[i].Rect.X + xOffset, measureData[i].Rect.Y,
						InternalChildren[i].DesiredSize.Width, measureData[i].Rect.Height));
				}
			}

			panelSize.Height = Math.Max(arrange ? constraint.Height : visibleConstraint.Height, panelSize.Height);
			panelSize.Width = Math.Max(arrange ? constraint.Width : visibleConstraint.Width, panelSize.Width);

#if DEBUG
			if (AdaptiveWrapPanel.Debug)
			{
				for (var i = 0; i < InternalChildren.Count; i++)
				{
					var el = InternalChildren[i] as FrameworkElement;
					if (el != null)
						el.DataContext = measureData[i];
				}
			}
#endif
			return panelSize;
		}

		private bool TryFitByShrinkFill(Size visibleConstraint,
			ref Size panelSize, ref MeasureData[] data)
		{
			var bestData = (MeasureData[])measureData.Clone();
			Size bestPanelSize = panelSize;
			bool widthFit = false;
			bool mode = true;
			while (ShrinkFill(visibleConstraint, data, ref mode))
			{
				panelSize = CalcPlacement(visibleConstraint.Height, measureData);

				if (panelSize.Width <= visibleConstraint.Width && (!widthFit ||
					(bestPanelSize.Height > visibleConstraint.Height &&
					panelSize.Height < bestPanelSize.Height)))
				{
					bestData = (MeasureData[])measureData.Clone();
					bestPanelSize = panelSize;

					widthFit = true;
				}
			}

			if (widthFit)
			{
				data = bestData;
				panelSize = bestPanelSize;
			}
			return widthFit;
		}

		// From MSDN : When overridden in a derived class, measures the 
		// size in layout required for child elements and determines a
		// size for the FrameworkElement-derived class
		protected override Size MeasureOverride(Size constraint)
		{
			Debug.WriteLine($"Measure {constraint} {ParentScrollViewerConstraint}");
			return MeasureArrange(constraint, false);
		}

		//From MSDN : When overridden in a derived class, positions child
		//elements and determines a size for a FrameworkElement derived
		//class.
		protected override Size ArrangeOverride(Size arrangeBounds)
		{
			Debug.WriteLine($"Arrange {arrangeBounds} {ParentScrollViewerConstraint}");
			return MeasureArrange(arrangeBounds, true);
		}

	}


}
