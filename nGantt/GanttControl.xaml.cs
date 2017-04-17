using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using nGantt.GanttChart;
using nGantt.PeriodSplitter;
using System.Collections.ObjectModel;

namespace nGantt
{
    public partial class GanttControl : UserControl
    {
        public enum SelectionMode
        {
            None,
            Single,
            Multiple
        }

        private GanttChartData ganttChartData = new GanttChartData();
        private TimeLine gridLineTimeLine;
        private double selectionStartX;
        private ObservableCollection<TimeLine> gridLineTimeLines = new ObservableCollection<TimeLine>();
        public event EventHandler SelectedItemChanged;
        public event EventHandler<PeriodEventArgs> GanttRowAreaSelected;

        public delegate string PeriodNameFormatter(Period period);
        public delegate Brush BackgroundFormatter(TimeLineItem timeLineItem);

        public ObservableCollection<ContextMenuItem> GanttTaskContextMenuItems { get; set; }
        public ObservableCollection<SelectionContextMenuItem> SelectionContextMenuItems { get; set; }
        public ObservableCollection<TimeLine> GridLineTimeLine => gridLineTimeLines;
        public SelectionMode TaskSelectionMode { get; set; }
        public List<GanttTask> SelectedItems => GetSelectedItems().ToList();

        private IEnumerable<GanttTask> GetSelectedItems()
        {
            foreach (GanttRowGroup group in ganttChartData.RowGroups)
                foreach (GanttRow row in group.Rows)
                    foreach (GanttTask item in from ganttTask in row.Tasks where ganttTask.IsSelected == true select ganttTask)
                        yield return item;
        }
        

        public GanttChartData GanttData => ganttChartData; 
        public ObservableCollection<TimeLine> TimeLines { get; private set; }
        public bool AllowUserSelection { get; set; }
        public Period SelectionPeriod { get; private set; }

        public GanttControl()
        {
            InitializeComponent();
            DataContext = this;
            SelectionPeriod = new Period();
        }

        public void Initialize(DateTime minDate, DateTime maxDate)
        {
            ganttChartData.MinDate = minDate;
            ganttChartData.MaxDate = maxDate;
        }

        public void AddGanttTask(GanttRow row, GanttTask task)
        {
            if (task.Start < ganttChartData.MaxDate && task.End > ganttChartData.MinDate)
                row.Tasks.Add(task);
        }

        public TimeLine CreateTimeLine(PeriodSplitter.PeriodSplitter splitter, PeriodNameFormatter PeriodNameFormatter)
        {
            if (splitter.MaxDate != GanttData.MaxDate || splitter.MinDate != GanttData.MinDate)
                throw new ArgumentException("The time-line must have the same max and min -date as the chart");

            List<Period> timeLineParts = splitter.Split();

            TimeLine timeline = new TimeLine();
            foreach (Period p in timeLineParts)
            {
                timeline.Items.Add(new TimeLineItem() { Name = PeriodNameFormatter(p), Start = p.Start, End = p.End.AddSeconds(-1) });
            }

            ganttChartData.TimeLines.Add(timeline);
            return timeline;
        }

        public void ClearGantt()
        {
            ganttChartData.RowGroups.Clear();
            ganttChartData.TimeLines.Clear();
            SelectedItems.Clear();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (TaskSelectionMode == SelectionMode.None)
                return;

            if (TaskSelectionMode == SelectionMode.Single)
                DeselectAllTasks();

            GanttTask gantTask = ( (GanttTask)( (FrameworkElement)( sender ) ).DataContext );
            gantTask.IsSelected = !gantTask.IsSelected;

            SelectedItemChanged?.Invoke(this, EventArgs.Empty);
        }

        private void DeselectAllTasks()
        {
            foreach (GanttRowGroup group in ganttChartData.RowGroups)
                foreach (GanttRow row in group.Rows)
                    foreach (GanttTask task in row.Tasks)
                        task.IsSelected = false;
        }

        public GanttRowGroup CreateGanttRowGroup()
        {
            GanttRowGroup rowGroup = new GanttRowGroup();
            ganttChartData.RowGroups.Add(rowGroup);
            return rowGroup;
        }

        public HeaderedGanttRowGroup CreateGanttRowGroup(string name)
        {
            HeaderedGanttRowGroup rowGroup = new HeaderedGanttRowGroup { Name = name };
            ganttChartData.RowGroups.Add(rowGroup);
            return rowGroup;
        }

        public ExpandableGanttRowGroup CreateGanttRowGroup(string name, bool isExpanded)
        {
            ExpandableGanttRowGroup rowGroup = new ExpandableGanttRowGroup { Name = name, IsExpanded = isExpanded };
            ganttChartData.RowGroups.Add(rowGroup);
            return rowGroup;
        }

        public GanttRow CreateGanttRow(GanttRowGroup rowGroup, string name)
        {
            GanttRowHeader rowHeader = new GanttRowHeader { Name = name };
            GanttRow row = new GanttRow { RowHeader = rowHeader, Tasks = new ObservableCollection<GanttTask>() };
            rowGroup.Rows.Add(row);
            return row;
        }

        public void SetGridLinesTimeline(TimeLine timeline)
        {
            if (!ganttChartData.TimeLines.Contains(timeline))
                throw new Exception("Invalid time-line");

            gridLineTimeLine = timeline;
        }

        public void SetGridLinesTimeline(TimeLine timeline, BackgroundFormatter backgroundFormatter)
        {
            if (!ganttChartData.TimeLines.Contains(timeline))
                throw new Exception("Invalid time-line");

            foreach (TimeLineItem item in timeline.Items)
                item.BackgroundColor = backgroundFormatter(item);

            gridLineTimeLines.Clear();
            gridLineTimeLines.Add(timeline);
            //gridLineTimeLine = time-line;
        }


        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!AllowUserSelection)
                return;

            // TODO:: Set visibility to hidden for all selectionRectangles
            Canvas canvas = ( (Canvas)UIHelper.FindVisualParent<Grid>(( (DependencyObject)sender )).FindName("selectionCanvas") );
            Border selectionRectangle = (Border)canvas.FindName("selectionRectangle");
            selectionStartX = e.GetPosition(canvas).X;
            selectionRectangle.Margin = new Thickness(selectionStartX, 0, 0, 5);
            selectionRectangle.Visibility = Visibility.Visible;
            selectionRectangle.IsEnabled = true;
            selectionRectangle.IsHitTestVisible = false;
            selectionRectangle.Width = 0;
        }

        private void selectionRectangle_MouseMove(object sender, MouseEventArgs e)
        {
            ChangeSelectionRectangleSize(sender, e);
        }

        private void ChangeSelectionRectangleSize(object sender, MouseEventArgs e)
        {
            Canvas canvas = ( (Canvas)UIHelper.FindVisualParent<Grid>(( (DependencyObject)sender )).FindName("selectionCanvas") );
            Border selectionRectangle = (Border)canvas.FindName("selectionRectangle");

            if (selectionRectangle.IsEnabled == false)
                return;

            double SelectionEndX = e.GetPosition(canvas).X;
            double selectionWidth = SelectionEndX - selectionStartX;
            if (selectionWidth > 0)
            {
                selectionRectangle.Width = selectionWidth;
            }
            else
            {
                selectionWidth = -selectionWidth;
                double selectionRectangleStartX = selectionStartX - selectionWidth;
                selectionRectangle.Width = selectionWidth;
                selectionRectangle.Margin = new Thickness(selectionRectangleStartX, 0, 0, 5);
            }
            
        }

        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            StopSelection(sender, e);
        }

        private void selectionCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            StopSelection(sender, e);
        }

        private void StopSelection(object sender, MouseEventArgs e)
        {
            Canvas canvas = ( (Canvas)UIHelper.FindVisualParent<Grid>(( (DependencyObject)sender )).FindName("selectionCanvas") );
            Border selectionRectangle = (Border)canvas.FindName("selectionRectangle");

            if (selectionRectangle.IsEnabled == false)
                return;
            
            ChangeSelectionRectangleSize(sender, e);
            selectionRectangle.IsEnabled = false;
            selectionRectangle.IsHitTestVisible = true;

            if (selectionRectangle.Visibility != Visibility.Visible)
                return;

            if (GanttRowAreaSelected == null)
                return;

            if (selectionRectangle.Width <= 0)
                return;

            double totalWidth = canvas.ActualWidth;
            TimeSpan tsTaskStart = new TimeSpan(Convert.ToInt64(( ganttChartData.MaxDate.Ticks - ganttChartData.MinDate.Ticks ) * ( selectionStartX / totalWidth )));
            TimeSpan tsTaskEnd = new TimeSpan(Convert.ToInt64(( ganttChartData.MaxDate.Ticks - ganttChartData.MinDate.Ticks ) * ( ( selectionStartX + selectionRectangle.Width ) / totalWidth )));
            DateTime selctionStartDate = ganttChartData.MinDate.Add(tsTaskStart);
            DateTime selctionEndDate = ganttChartData.MinDate.Add(tsTaskEnd);
            SelectionPeriod.Start = selctionStartDate;
            SelectionPeriod.End = selctionEndDate;
            GanttRowAreaSelected(this, new PeriodEventArgs() { SelectionStart = selctionStartDate, SelectionEnd = selctionEndDate });           
        }


        private void selectionRectangle_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            ( (Border)sender ).ContextMenu.IsOpen = true;
        }
    }
}
