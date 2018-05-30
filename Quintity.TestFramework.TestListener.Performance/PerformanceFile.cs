using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using log4net;
using Quintity.TestFramework.Core;
using Quintity.TestFramework.Runtime;

namespace Quintity.TestFramework.TestListener.Performance
{
    public class PerformanceFile : Quintity.TestFramework.Runtime.TestListener
    {
        private static readonly log4net.ILog LogEvent = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private string _metricsFolder;
        private string _metricsFile;
        private string _resultsFolder;
        private string _resultsFile;
        private string _resultsFormat = "{0}|{1}|{2}|{3}|{4}";
        private StringBuilder _resultsContent;
        private StringBuilder _metricsContent;

        private Object _lockObject = new Object();

        #region Constructors

        public PerformanceFile(Dictionary<string, string> args)
            : this()
        {
            _resultsContent = new StringBuilder();
            _metricsContent = new StringBuilder();
            _resultsFolder = args["TestResults"];
        }

        public PerformanceFile() { }

        #endregion

        #region Event handlers

        public override void OnTestExecutionBegin(TestExecutor testExecutor, TestExecutionBeginArgs args)
        {
            LogEvent.Debug($"{MethodInfo.GetCurrentMethod().Name} {args.VirtualUser}");

            if (!TestListenersCache.TryGetValueAsString("MetricsFolder", out _metricsFolder))
            {
                _metricsFolder = string.Format("{0}\\{1}", _resultsFolder, DateTime.Now.ToString("MM.dd.yyyy@hh.mm.sstt"));
                Directory.CreateDirectory(_metricsFolder);

                TestListenersCache.Add("MetricsFolder", _metricsFolder);
                LogEvent.Debug($"Metric folder added listeners cache ({_metricsFile}).");
                _metricsContent.AppendLine("VirtualUser!PerID|Description|StartTime|StopTime|ElapsedTime(sec)|StateArgs");
            }

            _resultsFile = string.Format("{0}\\TestResults.csv", _metricsFolder);
            _metricsFile = string.Format("{0}\\TestMetrics.csv", _metricsFolder);
        }

        public override void OnTestExecutionComplete(TestExecutor testExecutor, TestExecutionCompleteArgs args)
        {
            using (StreamWriter sw = File.AppendText(_resultsFile))
            {
                sw.WriteLine(_resultsContent.ToString());
            }

            using (StreamWriter sw = File.AppendText(_metricsFile))
            {
                sw.WriteLine(_metricsContent.ToString());
            }
        }

        public override void OnTestSuiteExecutionComplete(TestSuite testSuite, TestSuiteResult testSuiteResult)
        {
            var result = string.Format(_resultsFormat, testSuiteResult.VirtualUser, testSuiteResult.StartTime, testSuiteResult.EndTime,
                testSuiteResult.ElapsedTime, testSuiteResult.TestVerdict);
            _resultsContent.AppendLine(result);

            writeTestSuiteResultToFile(testSuiteResult);
        }

        public override void OnTestMetric(string virtualUser, TestMetricEventArgs args)
        {
            if (!args.ElapsedTime.Equals(TimeSpan.Zero))
            {
                var flattenedArgs = args.FlattenStateArgs();

                var metric = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}",
                        args.VirtualUser, args.PerfID, args.Description,
                        formatDateTime(args.StartTime),
                        formatDateTime(args.StopTime),
                        args.ElapsedTime.TotalSeconds,
                        string.IsNullOrEmpty(flattenedArgs) ? "" : string.Format("\"{0}\"", flattenedArgs));

                _metricsContent.AppendLine(metric);
            }
        }

        #region Unused handlers

        public override void OnTestSuiteExecutionBegin(TestSuite testSuite, TestSuiteBeginExecutionArgs args)
        { }

        public override void OnTestPreprocessorBegin(TestSuite testSuite, TestProcessorBeginExecutionArgs args)
        { }

        public override void OnTestPreprocessorComplete(TestSuite testSuite, TestProcessorResult testProcessorResult)
        { }

        public override void OnTestPostprocessorBegin(TestSuite testSuite, TestProcessorBeginExecutionArgs args)
        { }

        public override void OnTestPostprocessorComplete(TestSuite testSuite, TestProcessorResult testProcessorResult)
        { }

        public override void OnTestCaseExecutionBegin(TestCase testCase, TestCaseBeginExecutionArgs args)
        { }

        public override void OnTestCaseExecutionComplete(TestCase testCase, TestCaseResult testCaseResult)
        { }

        public override void OnTestStepExecutionBegin(TestStep testStep, TestStepBeginExecutionArgs args)
        { }

        public override void OnTestStepExecutionComplete(TestStep testStep, TestStepResult testStepResult)
        { }

        public override void OnTestCheck(TestCheck testCheck)
        { }

        public override void OnTestWarning(TestWarning testWarning)
        { }

        public override void OnTestTrace(string virtualUser, string traceMessage)
        { }

        #endregion  Unused handlers

        #endregion Listener event handlers

        #region Private methods

        private string formatDateTime(DateTime dateTime)
        {
            return dateTime.ToString("MM/dd/yyyy hh:mm:ss.fffffff tt");
        }

        private void writeTestSuiteResultToFile(TestSuiteResult testSuiteResult)
        {
            TestSuiteResult.SerializeToFile(testSuiteResult, string.Format("{0}\\{1} - {2}.xml", _metricsFolder,
                testSuiteResult.VirtualUser, testSuiteResult.TestVerdict));
        }

        private void writeTestResultsToFile(TestSuiteResult testSuiteResult)
        {
            var result = string.Format(_resultsFormat, testSuiteResult.VirtualUser,
                testSuiteResult.StartTime, testSuiteResult.EndTime, testSuiteResult.ElapsedTime, testSuiteResult.TestVerdict);

            File.AppendAllLines(_resultsFile, new string[] { result });
        }

        private void writeTestMetricToFile(string metric)
        {
            using (StreamWriter sw = File.AppendText(_metricsFile))
            {
                sw.WriteLine(metric);
            }
        }

        #endregion
    }
}