using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quintity.TestFramework.Core;
using Quintity.TestFramework.Runtime;

namespace Quintity.TestFramework.TestListeners.Rally
{
    public class RallyListener : TestListener
    {
        public override void OnTestExecutionBegin(TestExecutor testExecutor, TestExecutionBeginArgs args)
        {
            throw new NotImplementedException();
        }

        public override void OnTestExecutionComplete(TestExecutor testExecutor, TestExecutionCompleteArgs args)
        {
            throw new NotImplementedException();
        }

        public override void OnTestSuiteExecutionBegin(TestSuite testSuite, TestSuiteBeginExecutionArgs args)
        {
            throw new NotImplementedException();
        }

        public override void OnTestPreprocessorBegin(TestSuite testSuite, TestProcessorBeginExecutionArgs args)
        {
            throw new NotImplementedException();
        }

        public override void OnTestPreprocessorComplete(TestSuite testSuite, TestProcessorResult testProcessorResult)
        {
            throw new NotImplementedException();
        }

        public override void OnTestPostprocessorBegin(TestSuite testSuite, TestProcessorBeginExecutionArgs args)
        {
            throw new NotImplementedException();
        }

        public override void OnTestPostprocessorComplete(TestSuite testSuite, TestProcessorResult testProcessorResult)
        {
            throw new NotImplementedException();
        }

        public override void OnTestSuiteExecutionComplete(TestSuite testSuite, TestSuiteResult testSuiteResult)
        {
            throw new NotImplementedException();
        }

        public override void OnTestCaseExecutionBegin(TestCase testCase, TestCaseBeginExecutionArgs args)
        {
            throw new NotImplementedException();
        }

        public override void OnTestCaseExecutionComplete(TestCase testCase, TestCaseResult testCaseResult)
        {
            throw new NotImplementedException();
        }

        public override void OnTestStepExecutionBegin(TestStep testStep, TestStepBeginExecutionArgs args)
        {
            throw new NotImplementedException();
        }

        public override void OnTestStepExecutionComplete(TestStep testStep, TestStepResult testStepResult)
        {
            throw new NotImplementedException();
        }

        public override void OnTestCheck(TestCheck testCheck)
        {
            throw new NotImplementedException();
        }

        public override void OnTestWarning(TestWarning testWarning)
        {
            throw new NotImplementedException();
        }

        public override void OnTestTrace(string virtualString, string traceMessage)
        {
            throw new NotImplementedException();
        }

        public override void OnTestMetric(string virtualUser, TestMetricEventArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
