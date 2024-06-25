using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TestWpfTimeline
{
    public enum StepUnit
    {
        Millisecond,
        Second,
        Minute,
        Hour,
    }


    public class TimelineControl : Control
    {
        public double SizePerSecond
        {
            get { return (double)GetValue(SizePerSecondProperty); }
            set { SetValue(SizePerSecondProperty, value); }
        }

        public double PreferredStepSize
        {
            get { return (double)GetValue(PreferredStepSizeProperty); }
            set { SetValue(PreferredStepSizeProperty, value); }
        }

        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }
        public double SmallMarkSize
        {
            get { return (double)GetValue(SmallMarkSizeProperty); }
            set { SetValue(SmallMarkSizeProperty, value); }
        }

        public TimeSpan TimeOffset
        {
            get { return (TimeSpan)GetValue(TimeOffsetProperty); }
            set { SetValue(TimeOffsetProperty, value); }
        }


        public static readonly DependencyProperty PreferredStepSizeProperty =
            DependencyProperty.Register("PreferredStepSize", typeof(double), typeof(TimelineControl), new FrameworkPropertyMetadata(50.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty SizePerSecondProperty =
            DependencyProperty.Register("SizePerSecond", typeof(double), typeof(TimelineControl), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register("Scale", typeof(double), typeof(TimelineControl), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty SmallMarkSizeProperty =
            DependencyProperty.Register("SmallMarkSize", typeof(double), typeof(TimelineControl), new FrameworkPropertyMetadata(4.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty TimeOffsetProperty =
            DependencyProperty.Register("TimeOffset", typeof(TimeSpan), typeof(TimelineControl), new FrameworkPropertyMetadata(default(TimeSpan), FrameworkPropertyMetadataOptions.AffectsRender));



        public double MakeNormalValueLarger(double value)
        {
            return value * 2;
        }

        public double MakeNormalValueSmaller(double value)
        {
            return value / 2;
        }

        public double MakeSecondOrMinuteLarger(double value)
        {
            return value switch
            {
                1 => 2,
                2 => 3,
                3 => 5,
                5 => 10,
                10 => 15,
                15 => 30,
                30 => 60,
                _ => value * 2,
            };
        }

        public double MakeSecondOrMinuteSmaller(double value)
        {
            return value switch
            {
                60 => 30,
                30 => 15,
                15 => 10,
                10 => 5,
                5 => 3,
                3 => 2,
                2 => 1,
                _ => value / 2,
            };
        }


        protected override void OnRender(DrawingContext drawingContext)
        {
            double step = 1;
            StepUnit stepUnit = StepUnit.Second;

            double currentStepSize = SizePerSecond * Scale;

            while (currentStepSize > PreferredStepSize * 2)
            {
                if (stepUnit == StepUnit.Hour && step <= 1)
                {
                    step *= 60;
                    stepUnit = StepUnit.Minute;
                }
                else if (stepUnit == StepUnit.Minute && step <= 1)
                {
                    step *= 60;
                    stepUnit = StepUnit.Second;
                }
                else if (stepUnit == StepUnit.Second && step <= 1)
                {
                    step *= 1000;
                    stepUnit = StepUnit.Millisecond;
                }

                double newStep;
                if (stepUnit == StepUnit.Millisecond)
                {
                    newStep = MakeNormalValueSmaller(step);
                }
                else if (stepUnit is StepUnit.Second or StepUnit.Minute)
                {
                    newStep = MakeSecondOrMinuteSmaller(step);
                }
                else
                {
                    newStep = MakeNormalValueSmaller(step);
                }

                currentStepSize *= newStep / step;
                step = newStep;
            }

            while (currentStepSize < PreferredStepSize)
            {
                if (stepUnit == StepUnit.Millisecond && step >= 1000)
                {
                    step /= 1000;
                    stepUnit = StepUnit.Second;
                }
                else if (stepUnit == StepUnit.Second && step >= 60)
                {
                    step /= 60;
                    stepUnit = StepUnit.Minute;
                }
                else if (stepUnit == StepUnit.Minute && step >= 60)
                {
                    step /= 60;
                    stepUnit = StepUnit.Hour;
                }

                double newStep;
                if (stepUnit == StepUnit.Millisecond)
                {
                    newStep = MakeNormalValueLarger(step);
                }
                else if (stepUnit is StepUnit.Second or StepUnit.Minute)
                {
                    newStep = MakeSecondOrMinuteLarger(step);
                }
                else
                {
                    newStep = MakeNormalValueLarger(step);
                }

                currentStepSize *= newStep / step;
                step = newStep;
            }

            var index = 0;
            var pen = new Pen(Foreground, 1);

            var stepTime = stepUnit switch
            {
                StepUnit.Millisecond => TimeSpan.FromMilliseconds(step),
                StepUnit.Second => TimeSpan.FromSeconds(step),
                StepUnit.Minute => TimeSpan.FromMinutes(step),
                StepUnit.Hour => TimeSpan.FromHours(step),
                _ => throw new Exception("This would never happened"),
            };

            var timeOffsetStepCount = TimeOffset / stepTime;
            var timeOffsetStepCountInt = (int)timeOffsetStepCount;
            var timeOffsetStepCountRem = timeOffsetStepCount - timeOffsetStepCountInt;

            var positionOffset = -timeOffsetStepCountRem * currentStepSize;
            var drawTimeOffset = timeOffsetStepCountInt * stepTime;

            while (index * currentStepSize < ActualWidth + currentStepSize)
            {
                var position = positionOffset + index * currentStepSize;
                var time = drawTimeOffset + stepTime * index;

                var timeTextBuilder = new StringBuilder();
                var appendHour = time.Hours != 0;
                var appendMinute = appendHour || time.Minutes != 0;
                var appendSecond = true;
                var appendMillisecond = time.Milliseconds != 0;

                if (!appendMillisecond)
                {
                    appendMinute = true;
                }

                if (appendHour)
                {
                    timeTextBuilder.AppendFormat("{0:00}", time.Hours);
                }

                if (appendMinute)
                {
                    if (appendHour)
                    {
                        timeTextBuilder.Append(':');
                    }

                    timeTextBuilder.AppendFormat("{0:00}", time.Minutes);
                }

                if (appendSecond)
                {
                    if (appendMinute)
                    {
                        timeTextBuilder.Append(':');
                    }

                    timeTextBuilder.AppendFormat("{0:00}", time.Seconds);
                }
                
                if (appendMillisecond)
                {
                    if (appendSecond)
                    {
                        timeTextBuilder.Append('.');
                    }

                    timeTextBuilder.AppendFormat("{0:000}", time.Milliseconds);
                }


                var timeText = timeTextBuilder.ToString();

                var timeFormattedText = new FormattedText(
                    timeText,
                    CultureInfo.GetCultureInfo("en-us"),
                    FlowDirection.LeftToRight,
                    new Typeface("Verdana"),
                    8,
                    Brushes.Black,
                    1);

                drawingContext.DrawLine(pen, new Point(position, 0), new Point(position, ActualHeight));
                drawingContext.DrawText(timeFormattedText, new Point(position + 2, SmallMarkSize));

                for (int i = 1; i < 10; i++)
                {
                    drawingContext.DrawLine(pen, new Point(position + (currentStepSize * i / 10), 0), new Point(position + (currentStepSize * i / 10), SmallMarkSize));
                }

                index++;
            }
        }
    }
}
