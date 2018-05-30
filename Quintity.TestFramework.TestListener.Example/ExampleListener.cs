using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Quintity.TestFramework.Core;
using Quintity.TestFramework.Runtime;

namespace Quintity.TestFramework.TestListeners
{
    public class ExampleListener : TestListener
    {
        public override void OnTestExecutionBegin(TestExecutor testExecutor, TestExecutionBeginArgs args)
        {
            //Debug.WriteLine("ExampleListener method:  " + MethodInfo.GetCurrentMethod().Name);
        }

        public override void OnTestSuiteExecutionBegin(TestSuite testSuite, TestSuiteBeginExecutionArgs args)
        {
            //Debug.WriteLine("ExampleListener method:  " + MethodInfo.GetCurrentMethod().Name);
        }

        public override void OnTestPreprocessorBegin(TestSuite testSuite, TestProcessorBeginExecutionArgs args)
        {
            //Debug.WriteLine("ExampleListener method:  " + MethodInfo.GetCurrentMethod().Name);
        }

        public override void OnTestPreprocessorComplete(TestSuite testSuite, TestProcessorResult testProcessorResult)
        {
            //Debug.WriteLine("ExampleListener method:  " + MethodInfo.GetCurrentMethod().Name);
        }

        public override void OnTestCaseExecutionBegin(TestCase testCase, TestCaseBeginExecutionArgs args)
        {
            //Debug.WriteLine("ExampleListener method:  " + MethodInfo.GetCurrentMethod().Name);
        }

        public override void OnTestStepExecutionBegin(TestStep testStep, TestStepBeginExecutionArgs args)
        {
            //Debug.WriteLine("ExampleListener method:  " + MethodInfo.GetCurrentMethod().Name);
        }

        public override void OnTestCheck(TestCheck testCheck)
        {
            //Debug.WriteLine("ExampleListener method:  " + MethodInfo.GetCurrentMethod().Name);
        }

        public override void OnTestWarning(TestWarning testWarning)
        {
            //Debug.WriteLine("ExampleListener method:  " + MethodInfo.GetCurrentMethod().Name);
        }

        public override void OnTestTrace(string virtualString, string traceMessage)
        {
            //Debug.WriteLine("ExampleListener method:  " + MethodInfo.GetCurrentMethod().Name);
        }

        public override void OnTestStepExecutionComplete(TestStep testStep, TestStepResult testStepResult)
        {
            Debug.WriteLine("ExampleListener method:  " + MethodInfo.GetCurrentMethod().Name);
        }

        public override void OnTestCaseExecutionComplete(TestCase testCase, TestCaseResult testCaseResult)
        {
            //Debug.WriteLine("ExampleListener method:  " + MethodInfo.GetCurrentMethod().Name);
            Thread.Sleep(5000);
            Debug.WriteLine("ExampleListener method:  " + MethodInfo.GetCurrentMethod().Name);
        }

        public override void OnTestPostprocessorBegin(TestSuite testSuite, TestProcessorBeginExecutionArgs args)
        {
            //Debug.WriteLine("ExampleListener method:  " + MethodInfo.GetCurrentMethod().Name);
        }

        public override void OnTestPostprocessorComplete(TestSuite testSuite, TestProcessorResult testProcessorResult)
        {
            //Debug.WriteLine("ExampleListener method:  " + MethodInfo.GetCurrentMethod().Name);
        }

        public override void OnTestSuiteExecutionComplete(TestSuite testSuite, TestSuiteResult testSuiteResult)
        {
            Debug.WriteLine("ExampleListener method:  " + MethodInfo.GetCurrentMethod().Name);
        }

        public override void OnTestExecutionComplete(TestExecutor testExecutor, TestExecutionCompleteArgs args)
        {
            Debug.WriteLine("ExampleListener method:  " + MethodInfo.GetCurrentMethod().Name);
        }

        public override void OnTestMetric(string virtualUser, TestMetricEventArgs args)
        {
            Debug.WriteLine("ExampleListener method:  " + MethodInfo.GetCurrentMethod().Name);
        }
    }
}
