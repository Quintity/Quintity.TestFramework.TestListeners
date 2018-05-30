using System;
using System.IO;
using System.Collections.Generic;
using Quintity.TestFramework.Core;
using Quintity.TestFramework.Runtime;

namespace Quintity.TestFramework.TestListener.TextFile
{
    public class TextFileListener : Quintity.TestFramework.Runtime.TestListener
    {
        #region Data members

        Dictionary<Guid, int> testObjectDictionary = null;
        const string padding = "  ";

        Stack<string> resultFiles;

        string resultsFolder = null;
        string resultFile = null;
        System.Guid testStepKey;

        #endregion

        #region Constructors

        public TextFileListener()
        {
            resultFiles = new Stack<string>();
            //testObjectDictionary = new Dictionary<Guid, int>();
        }

        public TextFileListener(Dictionary<string, string> args)
            : this()
        {
            resultsFolder = args["TestResults"];
        }

        #endregion

        #region Listener event handlers



        public override void OnTestSuiteExecutionBegin(TestSuite testSuite, TestSuiteBeginExecutionArgs args)
        {
            if (testObjectDictionary == null)
            {
                testObjectDictionary = new Dictionary<Guid, int>();
                testObjectDictionary.Add(testSuite.SystemID, 0);
                resultFile = $"{resultsFolder}\\{testSuite.FileName.Replace(".ste", ".txt")}";

                using (StreamWriter sw = File.CreateText(resultFile))
                {
                    sw.WriteLine("Test suite:  " + testSuite.Title);
                }
            }
            else
            {
                int offset = 1;

                testObjectDictionary.Add(testSuite.SystemID, offset);

                string padding = getPadding(offset);

                using (StreamWriter sw = File.AppendText(resultFile))
                {
                    sw.WriteLine(padding + "Test suite:  " + testSuite.Title);
                }
            }
        }

        public override void OnTestCaseExecutionBegin(TestCase testCase, TestCaseBeginExecutionArgs args)
        {
            int offset = testObjectDictionary[testCase.ParentID] + 1;

            testObjectDictionary.Add(testCase.SystemID, offset);

            string padding = getPadding(offset);

            using (StreamWriter sw = File.AppendText(resultFile))
            {
                sw.WriteLine(padding + "Test case:  " + testCase.Title);
            }
        }

        public override void OnTestStepExecutionBegin(TestStep testStep, TestStepBeginExecutionArgs args)
        {
            int offset = testObjectDictionary[testStep.ParentID] + 2;
            testStepKey = testStep.SystemID;
            testObjectDictionary.Add(testStep.SystemID, offset);

            string padding = getPadding(offset);

            using (StreamWriter sw = File.AppendText(resultFile))
            {
                sw.WriteLine(padding + "Test step:  " + testStep.Title);
            }
        }

        public override void OnTestStepExecutionComplete(TestStep testStep, TestStepResult testStepResult)
        {
            string padding = getPadding(testObjectDictionary[testStep.SystemID]);

            using (StreamWriter sw = File.AppendText(resultFile))
            {
                string result = testStepResult.TestVerdict != TestVerdict.DidNotExecute ? testStepResult.TestVerdict.ToString() : "Did not execute";
                sw.WriteLine(padding + "Test step:  " + testStep.Title + ":  " + result);
                sw.WriteLine(padding + "TestMessage:  " + testStepResult.TestMessage);
                sw.WriteLine(string.Format("{0}Start Time: {1}, End Time:  {2}, Duration:  {3}.",
                    padding, testStepResult.StartTime.ToString("T", null), testStepResult.EndTime.ToString("T", null), testStepResult.ElapsedTime));
            }
        }

        public override void OnTestCaseExecutionComplete(TestCase testCase, TestCaseResult testCaseResult)
        {
            string padding = getPadding(testObjectDictionary[testCase.SystemID]);

            using (StreamWriter sw = File.AppendText(resultFile))
            {
                sw.WriteLine(string.Format("{0}Start Time: {1}, End Time:  {2}, Duration:  {3}.",
                    padding, testCaseResult.StartTime.ToString("T", null), testCaseResult.EndTime.ToString("T", null), testCaseResult.ElapsedTime));
            }
        }

        public override void OnTestCheck(TestCheck testCheck)
        {
            string padding = getPadding(testObjectDictionary[testStepKey] + 1);

            using (StreamWriter sw = File.AppendText(resultFile))
            {
                sw.WriteLine($"{padding} TestCheck: {testCheck.Description}, Verdict:  {testCheck.TestVerdict}");
                sw.WriteLine(padding + "  " + testCheck.Comment);
            }
        }

        object fileLock = new object();

        public override void OnTestTrace(string virtualString, string traceMessage)
        {
            int @value = -1;
            string padding = null;

            testObjectDictionary?.TryGetValue(testStepKey, out @value);

            if (@value != -1)
            {
                padding = getPadding(testObjectDictionary[testStepKey] + 1);

                lock (fileLock)
                {
                    using (StreamWriter sw = File.AppendText(resultFile))
                    {
                        sw.WriteLine(padding + "TestTrace: " + traceMessage);
                    }
                }
            }
        }

        public override void OnTestWarning(TestWarning testWarning)
        {
            string padding = getPadding(testObjectDictionary[testStepKey] + 1);

            using (StreamWriter sw = File.AppendText(resultFile))
            {
                sw.WriteLine(padding + "TestWarning: " + testWarning.Comment);
                sw.WriteLine(padding + "  " + "Source: " + testWarning.Source);
            }
        }

        public override void OnTestMetric(string virtualUser, TestMetricEventArgs args)
        {
            string padding = getPadding(testObjectDictionary[testStepKey] + 1);

            using (StreamWriter sw = File.AppendText(resultFile))
            {
                sw.WriteLine($"{padding} TestMetric: {args.Description}, Elapsed time:  {args.ElapsedTime}");
            }
        }

        public override void OnTestExecutionComplete(TestExecutor testExecutor, TestExecutionCompleteArgs args)
        {
            resultFile = null;
            testObjectDictionary = null;
        }

        public override void OnTestSuiteExecutionComplete(TestSuite testSuite, TestSuiteResult testSuiteResult)
        {
            string padding = getPadding(testObjectDictionary[testSuite.SystemID]);

            using (StreamWriter sw = File.AppendText(resultFile))
            {
                sw.WriteLine(string.Format("{0}Passed: {1}, Failed:  {2}, Errors:  {3}, Inactive:  {4}, Total:  {5}.",
                    padding, testSuiteResult.Passed, testSuiteResult.Failed, testSuiteResult.Errored, testSuiteResult.DidNotExecute, testSuiteResult.Total));

                sw.WriteLine(string.Format("{0}Start Time: {1}, End Time:  {2}, Duration:  {3}.",
                    padding, testSuiteResult.StartTime.ToString("T", null), testSuiteResult.EndTime.ToString("T", null), testSuiteResult.ElapsedTime));
            }
        }

        #region Unused handlers

        public override void OnTestExecutionBegin(TestExecutor testExecutor, TestExecutionBeginArgs args) { }

        public override void OnTestPostprocessorBegin(TestSuite testSuite, TestProcessorBeginExecutionArgs args) { }

        public override void OnTestPostprocessorComplete(TestSuite testSuite, TestProcessorResult testProcessorResult) { }

        public override void OnTestPreprocessorBegin(TestSuite testSuite, TestProcessorBeginExecutionArgs args) { }

        public override void OnTestPreprocessorComplete(TestSuite testSuite, TestProcessorResult testProcessorResult) { }

        #endregion

        #endregion

        #region Private methods

        private int getOffset(Guid systemId)
        {
            int offset = -1;

            try
            {
                offset = testObjectDictionary[systemId];
            }
            catch
            {
                ;
            }

            return offset;
        }

        private string getPadding(int offset)
        {
            string padding = null;

            for (int i = 1; i <= offset; i++)
            {
                padding += TextFileListener.padding;
            }

            return padding;
        }

        private void writeLine(string line)
        {
            using (StreamWriter sw = File.AppendText(resultFile))
            {
                sw.WriteLine(line);
            }
        }

        private void writeLine(int offset, string line)
        {
            using (StreamWriter sw = File.AppendText(resultFile))
            {
                sw.WriteLine(getPadding(offset) + line);
            }
        }

        #endregion
    }
}
