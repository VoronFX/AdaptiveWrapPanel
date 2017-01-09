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

		private Size CalcPlacement(Size visibleConstraint, MeasureData[] data)
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
					+ InternalChildren[i].DesiredSize.Height - visibleConstraint.Height;

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
						InternalChildren[i].DesiredSize.Height >= visibleConstraint.Height)
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

		private bool Shrink(Size visibleConstraint, MeasureData[] data, ref int mode)
		{
			double freeSpace = 0;
			int bestIndex = -1;

			if (mode <= 0)
			{
				for (int i = 1; i < InternalChildren.Count; i++)
				{
					if (!data[i].FillNewColumn)
						continue;

					if (visibleConstraint.Height -
						(data[i - 1].Rect.Top + InternalChildren[i - 1].DesiredSize.Height) > freeSpace)
					{
						freeSpace = visibleConstraint.Height - data[i - 1].Rect.Bottom;
						bestIndex = i;
					}
				}
				if (bestIndex != -1)
				{
					data[bestIndex].FillNewColumn = false;
					return true;
				}
				mode = 1;
			}

			if (mode <= 1)
			{
				for (int i = 0; i < InternalChildren.Count; i++)
				{
					if (!data[i].Fill || data[i].FillIgnore)
						continue;

					if (data[i].Rect.Height > freeSpace)
					{
						freeSpace = data[i].Rect.Height;
						bestIndex = i;
					}
				}
				if (bestIndex != -1)
				{
					data[bestIndex].FillIgnore = true;
					return true;
				}
				mode = 2;
			}

			if (mode <= 2)
			{
				//return false;
				freeSpace = double.MaxValue;
				for (int i = 1; i < InternalChildren.Count; i++)
				{
					if (data[i].OverflowIgnore ||
						data[i].ForceNewColumn ||
						data[i].Overflow <= 0)
						continue;

					if (data[i - 1].Rect.Bottom + InternalChildren[i].DesiredSize.Height
						- visibleConstraint.Height < freeSpace)
					{
						freeSpace = data[i].Overflow;
						bestIndex = i;
					}
				}
				if (bestIndex != -1)
				{
					data[bestIndex].OverflowIgnore = true;
					//for (int i = bestIndex + 1; i < InternalChildren.Count; i++)
					//{
					//	data[i].OverflowIgnore = false;
					//}
					return true;
				}
				mode = 3;
			}
			return false;
		}

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
					$"{nameof(OverflowIgnore)} {OverflowIgnore}",
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

			var panelSize = CalcPlacement(visibleConstraint, measureData);

			var bestData = (MeasureData[])measureData.Clone();
			var bestPanelSize = panelSize;

			bool widthFit = panelSize.Width < visibleConstraint.Width;
			int mode = 0;

			while (Shrink(visibleConstraint, measureData, ref mode))
			{

				panelSize = CalcPlacement(visibleConstraint, measureData);



				if (widthFit)
				{
					if (panelSize.Width <= visibleConstraint.Width &&
						bestPanelSize.Height > visibleConstraint.Height &&
						panelSize.Height < bestPanelSize.Height)
					{
						bestData = (MeasureData[])measureData.Clone();
						bestPanelSize = panelSize;
					}
				}
				else if (panelSize.Width < bestPanelSize.Width ||
					(panelSize.Width == bestPanelSize.Width &&
					bestPanelSize.Height > visibleConstraint.Height &&
					panelSize.Height < bestPanelSize.Height))
				{
					bestData = (MeasureData[])measureData.Clone();
					bestPanelSize = panelSize;

					if (panelSize.Width < visibleConstraint.Width)
						widthFit = true;
				}

			}

			if (arrange)
			{
				for (var i = 0; i < InternalChildren.Count; i++)
				{
					double xOffset = 0;
					if (InternalChildren[i].DesiredSize.Width < bestData[i].Rect.Width)
					{
						xOffset = ((bestData[i].Rect.Width - InternalChildren[i].DesiredSize.Width) / 2);
					}

					InternalChildren[i].Arrange(new Rect(bestData[i].Rect.X + xOffset, bestData[i].Rect.Y,
						InternalChildren[i].DesiredSize.Width, bestData[i].Rect.Height));
				}
			}

			bestPanelSize.Height = Math.Max(arrange ? constraint.Height : visibleConstraint.Height, bestPanelSize.Height);
			bestPanelSize.Width = Math.Max(arrange ? constraint.Width : visibleConstraint.Width, bestPanelSize.Width);

#if DEBUG
			if (AdaptiveWrapPanel.Debug)
			{
				for (var i = 0; i < InternalChildren.Count; i++)
				{
					var el = InternalChildren[i] as FrameworkElement;
					if (el != null)
						el.DataContext = bestData[i];
				}
			}
#endif
			return bestPanelSize;
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
