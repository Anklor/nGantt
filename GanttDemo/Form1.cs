using System;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using nGantt.GanttChart;
using nGantt.PeriodSplitter;

namespace GanttDemo
{
    public partial class Form1 : Form
    {
        private int _gantLenght;
        private ObservableCollection<ContextMenuItem> _ganttTaskContextMenuItems = new ObservableCollection<ContextMenuItem>();
        private ObservableCollection<SelectionContextMenuItem> _selectionContextMenuItems = new ObservableCollection<SelectionContextMenuItem>();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _gantLenght = 50;
            dateTimePicker.Value = DateTime.Parse("2012-02-01");
            DateTime minDate = dateTimePicker.Value.Date;
            DateTime maxDate = minDate.AddDays(_gantLenght);

            // Set selection -mode
            ganttControl1.TaskSelectionMode = nGantt.GanttControl.SelectionMode.Single;
            // Enable GanttTasks to be selected
            ganttControl1.AllowUserSelection = true;

            // listen to the GanttRowAreaSelected event
            ganttControl1.GanttRowAreaSelected += new EventHandler<PeriodEventArgs>(ganttControl1_GanttRowAreaSelected);
            
            // define ganttTask context menu and action when each item is clicked
            _ganttTaskContextMenuItems.Add(new ContextMenuItem(ViewClicked, "View..."));
            _ganttTaskContextMenuItems.Add(new ContextMenuItem(EditClicked, "Edit..."));
            _ganttTaskContextMenuItems.Add(new ContextMenuItem(DeleteClicked, "Delete..."));
            ganttControl1.GanttTaskContextMenuItems = _ganttTaskContextMenuItems;

            // define selection context menu and action when each item is clicked
            _selectionContextMenuItems.Add(new SelectionContextMenuItem(NewClicked, "New..."));
            ganttControl1.SelectionContextMenuItems = _selectionContextMenuItems;
        }

        private void NewClicked(Period selectionPeriod)
        {
            MessageBox.Show("New clicked for task " + selectionPeriod.Start.ToString() + " -> " + selectionPeriod.End.ToString());
        }

        private void ViewClicked(GanttTask ganttTask)
        {
            MessageBox.Show("New clicked for task " + ganttTask.Name);
        }

        private void EditClicked(GanttTask ganttTask)
        {
            MessageBox.Show("Edit clicked for task " + ganttTask.Name);
        }

        private void DeleteClicked(GanttTask ganttTask)
        {
            MessageBox.Show("Delete clicked for task " + ganttTask.Name);
        }

        void ganttControl1_GanttRowAreaSelected(object sender, PeriodEventArgs e)
        {
            MessageBox.Show(e.SelectionStart.ToString() + " -> " + e.SelectionEnd.ToString());
        }

        private System.Windows.Media.Brush DetermineBackground(TimeLineItem timeLineItem)
        {
            if (timeLineItem.End.Date.DayOfWeek == DayOfWeek.Saturday || timeLineItem.End.Date.DayOfWeek == DayOfWeek.Sunday)
                return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightBlue);
            else
                return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Transparent);
        }

        private void CreateData(DateTime minDate, DateTime maxDate)
        {
            // Set max and min dates
            ganttControl1.Initialize(minDate, maxDate);

            // Create timelines and define how they should be presented
            ganttControl1.CreateTimeLine(new PeriodYearSplitter(minDate, maxDate), FormatYear);
            ganttControl1.CreateTimeLine(new PeriodMonthSplitter(minDate, maxDate), FormatMonth);
            TimeLine gridLineTimeLine = ganttControl1.CreateTimeLine(new PeriodDaySplitter(minDate, maxDate), FormatDay);
            ganttControl1.CreateTimeLine(new PeriodDaySplitter(minDate, maxDate), FormatDayName);

            // Set the time line to attach grid lines to
            ganttControl1.SetGridLinesTimeline(gridLineTimeLine, DetermineBackground);

            // Create and data
            HeaderedGanttRowGroup rowgroup1 = ganttControl1.CreateGanttRowGroup("HeaderdGanttRowGroup");
            GanttRow row1 = ganttControl1.CreateGanttRow(rowgroup1, "GanttRow 1");
            ganttControl1.AddGanttTask(row1, new GanttTask() { Start = DateTime.Parse("2012-02-01"), End = DateTime.Parse("2012-03-01"), Name = "GanttRow 1:GanttTask 1", TaskProgressVisibility = System.Windows.Visibility.Hidden });
            ganttControl1.AddGanttTask(row1, new GanttTask() { Start = DateTime.Parse("2012-03-05"), End = DateTime.Parse("2012-05-01"), Name = "GanttRow 1:GanttTask 2" });
            ganttControl1.AddGanttTask(row1, new GanttTask() { Start = DateTime.Parse("2012-06-01"), End = DateTime.Parse("2012-06-15"), Name = "GanttRow 1:GanttTask 3" });

            HeaderedGanttRowGroup rowgroup2 = ganttControl1.CreateGanttRowGroup("ExpandableGanttRowGroup", true);
            GanttRow row2 = ganttControl1.CreateGanttRow(rowgroup2, "GanttRow 2");
            GanttRow row3 = ganttControl1.CreateGanttRow(rowgroup2, "GanttRow 3");
            ganttControl1.AddGanttTask(row2, new GanttTask() { Start = DateTime.Parse("2012-02-10"), End = DateTime.Parse("2012-03-10"), Name = "GanttRow 2:GanttTask 1" });
            ganttControl1.AddGanttTask(row2, new GanttTask() { Start = DateTime.Parse("2012-03-25"), End = DateTime.Parse("2012-05-10"), Name = "GanttRow 2:GanttTask 2" });
            ganttControl1.AddGanttTask(row2, new GanttTask() { Start = DateTime.Parse("2012-06-10"), End = DateTime.Parse("2012-09-15"), Name = "GanttRow 2:GanttTask 3", PercentageCompleted = 0.375 });
            ganttControl1.AddGanttTask(row3, new GanttTask() { Start = DateTime.Parse("2012-01-07"), End = DateTime.Parse("2012-09-15"), Name = "GanttRow 3:GanttTask 1", PercentageCompleted = 0.5 });

            GanttRowGroup rowgroup3 = ganttControl1.CreateGanttRowGroup();
            GanttRow row4 = ganttControl1.CreateGanttRow(rowgroup3, "GanttRow 4");
            ganttControl1.AddGanttTask(row4, new GanttTask() { Start = DateTime.Parse("2012-02-14"), End = DateTime.Parse("2012-02-27"), Name = "GanttRow 4:GanttTask 1", PercentageCompleted = 1 });
            ganttControl1.AddGanttTask(row4, new GanttTask() { Start = DateTime.Parse("2012-04-8"), End = DateTime.Parse("2012-09-19"), Name = "GanttRow 4:GanttTask 2" });
        }

        private string FormatYear(Period period) => period.Start.Year.ToString();
        
        private string FormatMonth(Period period) => period.Start.Month.ToString();
        
        private string FormatDay(Period period) => period.Start.Day.ToString();
        
        private string FormatDayName(Period period) => period.Start.DayOfWeek.ToString();
        
        private void buttonPrevious_Click(object sender, EventArgs e)
        {
            dateTimePicker.Value = ganttControl1.GanttData.MinDate.AddDays(-_gantLenght);
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            dateTimePicker.Value = ganttControl1.GanttData.MaxDate;
        }

        private void dateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            DateTime minDate = dateTimePicker.Value.Date;
            ganttControl1.ClearGantt();
            CreateData(minDate, minDate.AddDays(_gantLenght));
        }
    }
}
